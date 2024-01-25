// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using System.Data.Odbc;
using System.Net.NetworkInformation;
using System.Text;
using TabScore2.Classes;
using TabScore2.Globals;

namespace TabScore2.DataServices
{
    // BwsDatabase implements the TabScore2 IDatabase interface for a Microsoft Access 98 scoring database
    public class BwsDatabase : IDatabase
    {
        private static string? connectionString = null;

        private static string pathToDatabase = string.Empty;
        public string PathToDatabase
        {
            get
            {
                if (pathToDatabase == string.Empty)
                {
                    string argsString = string.Empty;
                    string[] arguments = Environment.GetCommandLineArgs();

                    // Parse command line args correctly to get database path
                    foreach (string s in arguments)
                    {
                        argsString = argsString + s + " ";
                    }
                    arguments = argsString.Split(['/']);
                    foreach (string s in arguments)
                    {
                        if (s.StartsWith("f:["))
                        {
                            pathToDatabase = s.Split(['[', ']'])[1];
                            break;
                        }
                    }
                }
                return pathToDatabase;
            }
            set
            {
                pathToDatabase = value;
            }
        }

        private static bool initializationComplete = false;
        public bool InitializationComplete
        {
            get
            {
                return initializationComplete;
            }
            set
            {
                initializationComplete = value;
            }
        }
        
        public bool IsDatabaseConnectionOK()
        {
            // Test read and write to the scoring database
            try
            {
                using OdbcConnection connection = new(connectionString);
                connection.Open();
                int logOnOff = 0;
                string SQLString = $"SELECT LogOnOff FROM Tables WHERE Section=1 AND [Table]=1";
                OdbcCommand cmd = new(SQLString, connection);
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    logOnOff = Convert.ToInt32(cmd.ExecuteScalar());
                });
                SQLString = $"UPDATE Tables SET LogOnOff={logOnOff} WHERE Section=1 AND [Table]=1";
                cmd = new OdbcCommand(SQLString, connection);
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    cmd.ExecuteNonQuery();
                });
                cmd.Dispose();
            }
            catch
            {
                return false;
            }
            return true;
        }

        private static bool? isIndividual = null;
        public bool IsIndividual
        {
            get
            {
                if (isIndividual == null)
                {
                    // Determine if event is an 'Individual', in which case RoundData will contain a filled 'South' field.
                    isIndividual = true;
                    using OdbcConnection connection = new(connectionString);
                    connection.Open();
                    String SQLString = $"SELECT TOP 1 South FROM RoundData";
                    OdbcCommand cmd = new(SQLString, connection);
                    try
                    {
                        ODBCRetryHelper.ODBCRetry(() =>
                        {
                            object? queryResult = cmd.ExecuteScalar();
                            if (queryResult == null || queryResult == DBNull.Value || Convert.ToString(queryResult) == string.Empty) isIndividual = false;
                        });
                    }
                    catch (OdbcException e)
                    {
                        if (e.Errors.Count > 1 || e.Errors[0].SQLState != "07002")   // Error other than field 'South' doesn't exist
                        {
                            throw;
                        }
                        else
                        {
                            isIndividual = false;
                        }
                    }
                    finally
                    {
                        cmd.Dispose();
                    }
                }
                return isIndividual.Value;
            }
        }

        // ==========================================================
        // Prepare the database for use by checking tables and fields
        // ==========================================================
        public string? Initialize()
        {
            // Set connection string for ODBC
            if (pathToDatabase == string.Empty) return "NoDatabasePath";
            if (!File.Exists(pathToDatabase)) return "DatabaseNotExist";
            OdbcConnectionStringBuilder cs = new() { Driver = "Microsoft Access Driver (*.mdb)" };
            cs.Add("Dbq", pathToDatabase);
            cs.Add("Uid", "Admin");
            cs.Add("Pwd", string.Empty);
            connectionString = cs.ToString();

            // Check a number of features in the Access scoring database to ensure that TabScore2 will work correctly
            // This mainly concerns tables that TabScore2 will need to write to 
            using OdbcConnection connection = new(connectionString);
            try
            {
                connection.Open();
            }
            catch
            {
                return "DatabaseNotAccessible";
            }

            // Validate SECTION Table
            // Add field 'Winners' to table 'Section' if it doesn't already exist
            String SQLString = "ALTER TABLE Section ADD Winners SHORT";
            OdbcCommand cmd = new(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            // Read sections
            sectionsList.Clear();
            SQLString = "SELECT ID, Letter, [Tables], Winners FROM Section";
            cmd = new OdbcCommand(SQLString, connection);
            OdbcDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int sectionID = reader.GetInt32(0);
                string sectionLetter = reader.GetString(1);
                int numTables = reader.GetInt32(2);
                int winners = 0;
                if (!reader.IsDBNull(3))
                {
                    object tempWinners = reader.GetValue(3);
                    if (tempWinners != null) winners = Convert.ToInt32(tempWinners);
                }
                sectionsList.Add(new Section() { ID = sectionID, Letter = sectionLetter, Tables = numTables, Winners = winners });
            }
            reader.Close();

            // Check that a section exists
            if (sectionsList.Count == 0)
            {
                return "DatabaseNoSections";
            }

            foreach (Section section in sectionsList)
            {
                // Check section letters, and number of tables per section.  These are TabScore constraints
                section.Letter = section.Letter.Trim();  // Remove any spurious characters
                if (section.ID < 1 || section.ID > 4 || (section.Letter != "A" && section.Letter != "B" && section.Letter != "C" && section.Letter != "D"))
                {
                    return "DatabaseIncorrectSections";
                }
                if (section.Tables > 30)
                {
                    return "DatabaseTooManyTables";
                }

                if (section.Winners == 0)
                {
                    // Set Winners field based on data from RoundData table.  If the maximum pair number > number of tables + 1, we can assume a one-winner movement.
                    // The +1 is to take account of a rover in a two-winner movement. 

                    SQLString = $"SELECT NSpair, EWpair FROM RoundData WHERE Section={section.ID}";
                    int maxPairNumber = 0;
                    cmd = new OdbcCommand(SQLString, connection);
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        int newNSpair = reader.GetInt32(0);
                        int newEWpair = reader.GetInt32(1);
                        if (newNSpair > maxPairNumber) maxPairNumber = newNSpair;
                        if (newEWpair > maxPairNumber) maxPairNumber = newEWpair;
                    }
                    reader.Close();
                    if (maxPairNumber == 0)  // No round data for this section! 
                    {
                        section.Winners = 0;
                    }
                    else if (maxPairNumber > section.Tables + 1)
                    {
                        section.Winners = 1;
                    }
                    else
                    {
                        section.Winners = 2;
                    }
                    SQLString = $"UPDATE Section SET Winners={section.Winners} WHERE ID={section.ID}";
                    cmd = new OdbcCommand(SQLString, connection);
                    cmd.ExecuteNonQuery();
                }
            }

            // Validate RECEIVEDDATA Table
            // If this is an individual event, add extra fields South and West to ReceivedData if they don't exist
            if (IsIndividual)
            {
                SQLString = "ALTER TABLE ReceivedData ADD South SHORT";
                cmd = new OdbcCommand(SQLString, connection);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (OdbcException e)
                {
                    if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                    {
                        throw;
                    }
                }
                SQLString = "ALTER TABLE ReceivedData ADD West SHORT";
                cmd = new OdbcCommand(SQLString, connection);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (OdbcException e)
                {
                    if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                    {
                        throw;
                    }
                }
            }

            // Validate PLAYERNUMBERS Table
            // Add field 'Name' to table 'PlayerNumbers' if it doesn't already exist
            SQLString = "ALTER TABLE PlayerNumbers ADD [Name] VARCHAR(30)";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            // Add field 'Processed' to table 'PlayerNumbers' if it doesn't already exist
            SQLString = "ALTER TABLE PlayerNumbers ADD Processed YESNO";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                SQLString = $"UPDATE PlayerNumbers SET Processed=False";
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            // Add field 'Updated' to table 'PlayerNumbers' if it doesn't already exist
            SQLString = "ALTER TABLE PlayerNumbers ADD Updated YESNO";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                SQLString = $"UPDATE PlayerNumbers SET Updated=False";
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            // Add field 'TimeLog' to table 'PlayerNumbers' if it doesn't already exist
            SQLString = "ALTER TABLE PlayerNumbers ADD TimeLog DATETIME";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            // Add field 'Round' to table 'PlayerNumbers' if it doesn't already exist
            SQLString = "ALTER TABLE PlayerNumbers ADD [Round] SHORT";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            // Ensure that all Round values are set to 0 to start with
            SQLString = "UPDATE PlayerNumbers SET [Round]=0 WHERE [Round] IS NULL";
            cmd = new OdbcCommand(SQLString, connection);
            cmd.ExecuteNonQuery();

            // Add a new field 'TabScorePairNo' to table 'PlayerNumbers' if it doesn't exist and populate it if possible
            SQLString = "ALTER TABLE PlayerNumbers ADD TabScorePairNo SHORT";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }
            SQLString = "SELECT Section, [Table], Direction FROM PlayerNumbers";
            cmd = new OdbcCommand(SQLString, connection);
            reader = cmd.ExecuteReader();
            OdbcCommand cmd2 = new();
            while (reader.Read())
            {
                int section = reader.GetInt32(0);
                int table = reader.GetInt32(1);
                string direction = reader.GetString(2);
                if (IsIndividual)
                {
                    switch (direction)
                    {
                        case "N":
                            SQLString = $"SELECT NSPair FROM RoundData WHERE Section={section} AND [Table]={table} AND ROUND=1";
                            break;
                        case "S":
                            SQLString = $"SELECT South FROM RoundData WHERE Section={section} AND [Table]={table} AND ROUND=1";
                            break;
                        case "E":
                            SQLString = $"SELECT EWPair FROM RoundData WHERE Section={section} AND [Table]={table} AND ROUND=1";
                            break;
                        case "W":
                            SQLString = $"SELECT West FROM RoundData WHERE Section={section} AND [Table]={table} AND ROUND=1";
                            break;
                    }
                }
                else
                {
                    switch (direction)
                    {
                        case "N":
                        case "S":
                            SQLString = $"SELECT NSPair FROM RoundData WHERE Section={section} AND [Table]={table} AND ROUND=1";
                            break;
                        case "E":
                        case "W":
                            SQLString = $"SELECT EWPair FROM RoundData WHERE Section={section} AND [Table]={table} AND ROUND=1";
                            break;
                    }
                }
                cmd2 = new OdbcCommand(SQLString, connection);
                object? queryResult = cmd2.ExecuteScalar();
                if (queryResult != null)
                {
                    string? pairNo = queryResult.ToString();
                    if (pairNo != null)
                    {
                        SQLString = $"UPDATE PlayerNumbers SET TabScorePairNo={pairNo} WHERE Section={section} AND [Table]={table} AND Direction='{direction}'";
                        cmd2 = new OdbcCommand(SQLString, connection);
                        cmd2.ExecuteNonQuery();
                    }
                }
            }
            cmd2.Dispose();

            // Validate PLAYERNAMES Table
            SQLString = "CREATE TABLE PlayerNames (ID LONG, [Name] VARCHAR(40), strID VARCHAR(8))";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "42S01")  // Error other than PlayerNames table already exists
                {
                    throw;
                }
            }

            // Add field 'strID' to table 'PlayerNames' if it doesn't already exist
            SQLString = "ALTER TABLE PlayerNames ADD [strID] VARCHAR(18)";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            // Read PlayerNames table
            // Cater for possibility that one or both of ID and strID could be null/blank.  Prefer strID
            internalPlayerRecordsList.Clear();
            SQLString = $"SELECT ID, Name, strID FROM PlayerNames";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        object? readerID = null;
                        if (!reader.IsDBNull(0))
                        {
                            readerID = reader.GetValue(0);
                        }
                        object? readerStrID = null;
                        if (!reader.IsDBNull(2))
                        {
                            readerStrID = reader.GetValue(2);
                        }
                        string? tempID = null;
                        if (readerStrID != null) tempID = Convert.ToString(readerStrID);
                        if ((tempID == null || tempID == string.Empty) && readerID != null) tempID = Convert.ToString(Convert.ToInt32(readerID));

                        if (tempID != null)
                        {
                            InternalPlayerRecord playerRecord = new()
                            {
                                ID = tempID,
                                Name = reader.GetString(1),
                            };
                            internalPlayerRecordsList.Add(playerRecord);
                        }
                    };
                });
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count > 1 || e.Errors[0].SQLState != "42S02")  // Error other than PlayerNames table does not exist
                {
                    throw;
                }
            }
            finally
            {
                reader.Close();
            }

            // Validate and read HANDRECORD Table
            SQLString = "CREATE TABLE HandRecord (Section SHORT, Board SHORT, NorthSpades VARCHAR(13), NorthHearts VARCHAR(13), NorthDiamonds VARCHAR(13), NorthClubs VARCHAR(13), EastSpades VARCHAR(13), EastHearts VARCHAR(13), EastDiamonds VARCHAR(13), EastClubs VARCHAR(13), SouthSpades VARCHAR(13), SouthHearts VARCHAR(13), SouthDiamonds VARCHAR(13), SouthClubs VARCHAR(13), WestSpades VARCHAR(13), WestHearts VARCHAR(13), WestDiamonds VARCHAR(13), WestClubs VARCHAR(13))";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count > 1 || e.Errors[0].SQLState != "42S01")  // Error other than HandRecord table already exists
                {
                    throw;
                }
            }

            SQLString = $"SELECT Section, Board, NorthSpades, NorthHearts, NorthDiamonds, NorthClubs, EastSpades, EastHearts, EastDiamonds, EastClubs, SouthSpades, SouthHearts, SouthDiamonds, SouthClubs, WestSpades, WestHearts, WestDiamonds, WestClubs FROM HandRecord";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Hand hand = new()
                        {
                            SectionID = reader.GetInt16(0),
                            BoardNumber = reader.GetInt16(1),
                            NorthSpades = reader.GetString(2),
                            NorthHearts = reader.GetString(3),
                            NorthDiamonds = reader.GetString(4),
                            NorthClubs = reader.GetString(5),
                            EastSpades = reader.GetString(6),
                            EastHearts = reader.GetString(7),
                            EastDiamonds = reader.GetString(8),
                            EastClubs = reader.GetString(9),
                            SouthSpades = reader.GetString(10),
                            SouthHearts = reader.GetString(11),
                            SouthDiamonds = reader.GetString(12),
                            SouthClubs = reader.GetString(13),
                            WestSpades = reader.GetString(14),
                            WestHearts = reader.GetString(15),
                            WestDiamonds = reader.GetString(16),
                            WestClubs = reader.GetString(17)
                        };
                        handsList.Add(hand);
                    }
                    reader.Close();
                });
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count > 1 || e.Errors[0].SQLState != "42S02")  // Error other than HandRecord table does not exist
                {
                    throw;
                }
            }

            // Validate SETTINGS Table
            SQLString = "ALTER TABLE Settings ADD ShowResults YESNO";
            cmd = new(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                SQLString = "UPDATE Settings SET ShowResults=YES";
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            SQLString = "ALTER TABLE Settings ADD ShowPercentage YESNO";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                SQLString = "UPDATE Settings SET ShowPercentage=YES";
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            SQLString = "ALTER TABLE Settings ADD LeadCard YESNO";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                SQLString = "UPDATE Settings SET LeadCard=YES";
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            SQLString = "ALTER TABLE Settings ADD BM2ValidateLeadCard YESNO";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                SQLString = "UPDATE Settings SET BM2ValidateLeadCard=YES";
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            SQLString = "ALTER TABLE Settings ADD BM2NumberEntryEachRound YESNO";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                SQLString = "UPDATE Settings SET BM2NumberEntryEachRound=NO";
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            SQLString = "ALTER TABLE Settings ADD BM2ViewHandRecord YESNO";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                SQLString = "UPDATE Settings SET BM2ViewHandRecord=YES";
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            SQLString = "ALTER TABLE Settings ADD BM2Ranking SHORT";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                SQLString = "UPDATE Settings SET BM2Ranking=1";
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            SQLString = "ALTER TABLE Settings ADD BM2NameSource SHORT";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                SQLString = "UPDATE Settings SET BM2NameSource=0";
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            SQLString = "ALTER TABLE Settings ADD EnterResultsMethod SHORT";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                SQLString = "UPDATE Settings SET EnterResultsMethod=1";
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }

            SQLString = "ALTER TABLE Settings ADD BM2EnterHandRecord YESNO";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                cmd.ExecuteNonQuery();
                SQLString = "UPDATE Settings SET BM2EnterHandRecord=NO";
                cmd = new OdbcCommand(SQLString, connection);
                cmd.ExecuteNonQuery();
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count != 1 || e.Errors[0].SQLState != "HYS21")
                {
                    throw;
                }
            }
            finally
            {
                cmd.Dispose();
            }
            return null;
        }

        // ========================================
        // Implement methods to access the database
        // ========================================

        // SECTION
        private static readonly List<Section> sectionsList = [];

        public Section GetSection(int sectionID)
        {
            Section? section = sectionsList.Find(x => x.ID == sectionID);
            section ??= sectionsList[0];
            return section;
        }

        public List<Section> GetSectionsList()
        {
            return sectionsList;
        }

        // TABLE
        public void RegisterTable(int sectionID, int tableNumber)
        {
            // Set table status in "Tables" table.  Not needed in TabScore, but complies with BridgeMate spec
            using OdbcConnection connection = new(connectionString);
            connection.Open();
            string SQLString = $"UPDATE Tables SET LogOnOff=1 WHERE Section={sectionID} AND [Table]={tableNumber}";
            OdbcCommand cmd = new(SQLString, connection);
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    cmd.ExecuteNonQuery();
                });
            }
            catch { }
            cmd.Dispose();
        }

        // ROUND
        private static int roundsInEventRoundNumber = 0;
        private static int numberOfRoundsInEvent = 1;
        public int GetNumberOfRoundsInEvent(int sectionID, int roundNumber = 999)
        {
            // Need to re-query the database each round in case rounds are added/removed by scoring program
            if (roundNumber >= roundsInEventRoundNumber) 
            {
                object? queryResult = null;
                using (OdbcConnection connection = new(connectionString))
                {
                    connection.Open();
                    string SQLString = $"SELECT MAX(Round) FROM RoundData WHERE Section={sectionID}";
                    OdbcCommand cmd = new(SQLString, connection);
                    try
                    {
                        ODBCRetryHelper.ODBCRetry(() =>
                        {
                            queryResult = cmd.ExecuteScalar();
                        });
                    }
                    catch { }
                    cmd.Dispose();
                }
                roundsInEventRoundNumber++;
                numberOfRoundsInEvent = Convert.ToInt32(queryResult);
            }
            return numberOfRoundsInEvent;
        }

        public int GetNumberOfLastRoundWithResults(int sectionID, int tableNumber)
        {
            object? queryResult = null;
            using (OdbcConnection connection = new(connectionString))
            {
                connection.Open();
                string SQLString = $"SELECT MAX(Round) FROM ReceivedData WHERE Section={sectionID} AND [Table]={tableNumber}";
                OdbcCommand cmd = new(SQLString, connection);
                try
                {
                    ODBCRetryHelper.ODBCRetry(() =>
                    {
                        queryResult = cmd.ExecuteScalar();
                    });
                }
                catch { }
                cmd.Dispose();
            }
            if (queryResult == null || queryResult == DBNull.Value)
            {
                return 1;
            }
            else
            {
                return Convert.ToInt32(queryResult);
            }
        }

        public List<Round> GetRoundsList(int sectionID, int roundNumber) 
        {
            List<Round> roundsList = [];
            using OdbcConnection connection = new(connectionString);
            connection.Open();
            if (IsIndividual)
            {
                string SQLString = $"SELECT [Table], NSPair, EWPair, LowBoard, HighBoard, South, West FROM RoundData WHERE Section={sectionID} AND Round={roundNumber}";
                OdbcCommand cmd = new(SQLString, connection);
                OdbcDataReader? reader = null;
                try
                {
                    ODBCRetryHelper.ODBCRetry(() =>
                    {
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            Round round = new()
                            {
                                TableNumber = reader.GetInt32(0),
                                NumberNorth = reader.GetInt32(1),
                                NumberEast = reader.GetInt32(2),
                                LowBoard = reader.GetInt32(3),
                                HighBoard = reader.GetInt32(4),
                                NumberSouth = reader.GetInt32(5),
                                NumberWest = reader.GetInt32(6)
                            };
                            roundsList.Add(round);
                        }
                    });
                }
                finally
                {
                    reader!.Close();
                    cmd.Dispose();
                }
            }
            else  // Not individual
            {
                string SQLString = $"SELECT [Table], NSPair, EWPair, LowBoard, HighBoard FROM RoundData WHERE Section={sectionID} AND Round={roundNumber}";
                OdbcCommand cmd = new(SQLString, connection);
                OdbcDataReader? reader = null;
                try
                {
                    ODBCRetryHelper.ODBCRetry(() =>
                    {
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            Round round = new()
                            {
                                TableNumber = reader.GetInt32(0),
                                NumberNorth = reader.GetInt32(1),
                                NumberEast = reader.GetInt32(2),
                                LowBoard = reader.GetInt32(3),
                                HighBoard = reader.GetInt32(4),
                            };
                            roundsList.Add(round);
                        }
                    });
                }
                finally
                {
                    reader!.Close();
                    cmd.Dispose();
                }
            }
            return roundsList;
        }

        public void GetRoundData(TableStatus tableStatus)
        {
            using OdbcConnection connection = new(connectionString);
            Round round = new();
            connection.Open();
            if (IsIndividual)
            {
                string SQLString = $"SELECT NSPair, EWPair, South, West, LowBoard, HighBoard FROM RoundData WHERE Section={tableStatus.SectionID} AND Table={tableStatus.TableNumber} AND Round={tableStatus.RoundNumber}";
                OdbcCommand cmd = new(SQLString, connection);
                OdbcDataReader? reader = null;
                try
                {
                    ODBCRetryHelper.ODBCRetry(() =>
                    {
                        reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            round.NumberNorth = reader.GetInt32(0);
                            round.NumberEast = reader.GetInt32(1);
                            round.NumberSouth = reader.GetInt32(2);
                            round.NumberWest = reader.GetInt32(3);
                            round.LowBoard = reader.GetInt32(4);
                            round.HighBoard = reader.GetInt32(5);
                        }
                    });
                }
                finally
                {
                    reader?.Close();
                    cmd.Dispose();
                }
            }
            else  // Not individual
            {
                string SQLString = $"SELECT NSPair, EWPair, LowBoard, HighBoard FROM RoundData WHERE Section={tableStatus.SectionID} AND Table={tableStatus.TableNumber} AND Round={tableStatus.RoundNumber}";
                OdbcCommand cmd = new(SQLString, connection);
                OdbcDataReader? reader = null;
                try
                {
                    ODBCRetryHelper.ODBCRetry(() =>
                    {
                        reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            round.NumberNorth = round.NumberSouth = reader.GetInt32(0);
                            round.NumberEast = round.NumberWest = reader.GetInt32(1);
                            round.LowBoard = reader.GetInt32(2);
                            round.HighBoard = reader.GetInt32(3);
                        }
                    });
                }
                finally
                {
                    reader?.Close();
                    cmd.Dispose();
                }
            }
            
            // Check for use of missing pair in Section table and set player numbers to 0 if necessary
            Section? section = sectionsList.Find(x => x.ID == tableStatus.SectionID);
            if (section != null)
            {
                int missingPair = section.MissingPair;
                if (round.NumberNorth == missingPair) round.NumberNorth = round.NumberSouth = 0;
                if (round.NumberEast == missingPair) round.NumberEast = round.NumberWest = 0;
            }

            tableStatus.RoundData = round;
        }

        // RECEIVEDDATA
        public Result GetResult(int sectionID, int tableNumber, int roundNumber, int boardNumber)
        {
            Result result = new()
            {
                BoardNumber = boardNumber
            };
            if (boardNumber == 0) return result;

            using OdbcConnection connection = new(connectionString);
            connection.Open();
            string SQLString = $"SELECT [NS/EW], Contract, Result, LeadCard, Remarks FROM ReceivedData WHERE Section={sectionID} AND [Table]={tableNumber} AND Round={roundNumber} AND Board={boardNumber}";
            OdbcCommand cmd = new(SQLString, connection);
            OdbcDataReader? reader = null;
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        result.Remarks = reader.GetString(4);
                        string tempContract = reader.GetString(1);
                        if ((result.Remarks == string.Empty || result.Remarks == "Wrong direction") && tempContract.Length > 2)
                            if (tempContract == "PASS")
                            {
                                result.ContractLevel = 0;
                            }
                            else  // Hopefully the database contains a valid contract
                            {
                                string[] temp = tempContract.Split(' ');
                                result.ContractLevel = Convert.ToInt32(temp[0]);
                                result.ContractSuit = temp[1];
                                if (temp.Length > 2) result.ContractX = temp[2];
                                result.DeclarerNSEW = reader.GetString(0);
                                result.LeadCard = reader.GetString(3).Replace("10", "T");
                                string tricksTakenSymbol = reader.GetString(2);
                                if (tricksTakenSymbol == string.Empty)
                                {
                                    result.TricksTaken = -1;
                                }
                                else if (tricksTakenSymbol == "=")
                                {
                                    result.TricksTaken = result.ContractLevel + 6;
                                }
                                else
                                {
                                    result.TricksTaken = result.ContractLevel + Convert.ToInt32(tricksTakenSymbol) + 6;
                                }
                            }
                        else
                        {
                            result.ContractLevel = -1;  // Board not played
                        }
                    }
                });
            }
            finally
            {
                reader!.Close();
                cmd.Dispose();
            }
            return result;
        }

        public void SetResult(Result result)
        {
            using OdbcConnection connection = new(connectionString);
            // Delete any previous result
            connection.Open();
            string SQLString = $"DELETE FROM ReceivedData WHERE Section={result.SectionID} AND [Table]={result.TableNumber} AND Round={result.RoundNumber} AND Board={result.BoardNumber}";
            OdbcCommand cmd = new(SQLString, connection);
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    cmd.ExecuteNonQuery();
                });
            }
            catch { }

            // Set database fields in correct format
            int declarer;
            if (result.ContractLevel <= 0)
            {
                declarer = 0;
            }
            else
            {
                if (IsIndividual)
                {
                    declarer = result.DeclarerNSEW switch
                    {
                        "N" => result.NumberNorth,
                        "E" => result.NumberEast,
                        "S" => result.NumberSouth,
                        "W" => result.NumberWest,
                        _ => 0
                    };
                }
                else
                {
                    declarer = result.DeclarerNSEW switch
                    {
                        "N" => result.NumberNorth,
                        "S" => result.NumberNorth,
                        "NS" => result.NumberNorth,
                        "E" => result.NumberEast,
                        "W" => result.NumberEast,
                        "EW" => result.NumberEast,
                        _ => 0
                    };
                }
            }
            
            string leadCard;
            if (result.LeadCard == null || result.LeadCard == string.Empty || result.LeadCard == "SKIP")
            {
                leadCard = string.Empty;
            }
            else
            {
                leadCard = result.LeadCard.Replace("T", "10");
            }
            
            string contract;
            if (result.ContractLevel < 0)  // No result or board not played
            {
                contract = string.Empty;
            }
            else if (result.ContractLevel == 0)
            {
                contract = "PASS";
            }
            else
            {
                contract = $"{result.ContractLevel} {result.ContractSuit}";
                if (result.ContractX != string.Empty)
                {
                    contract = $"{contract} {result.ContractX}";
                }
            }

            if (IsIndividual)
            {
                SQLString = $"INSERT INTO ReceivedData (Section, [Table], Round, Board, PairNS, PairEW, South, West, Declarer, [NS/EW], Contract, Result, LeadCard, Remarks, DateLog, TimeLog, Processed, Processed1, Processed2, Processed3, Processed4, Erased) VALUES ({result.SectionID}, {result.TableNumber}, {result.RoundNumber}, {result.BoardNumber}, {result.NumberNorth}, {result.NumberEast}, {result.NumberSouth}, {result.NumberWest}, {declarer}, '{result.DeclarerNSEW}', '{contract}', '{result.TricksTakenSymbol}', '{leadCard}', '{result.Remarks}', #{DateTime.Now:yyyy-MM-dd}#, #{DateTime.Now:yyyy-MM-dd hh:mm:ss}#, False, False, False, False, False, False)";
            }
            else
            {
                SQLString = $"INSERT INTO ReceivedData (Section, [Table], Round, Board, PairNS, PairEW, Declarer, [NS/EW], Contract, Result, LeadCard, Remarks, DateLog, TimeLog, Processed, Processed1, Processed2, Processed3, Processed4, Erased) VALUES ({result.SectionID}, {result.TableNumber}, {result.RoundNumber}, {result.BoardNumber}, {result.NumberNorth}, {result.NumberEast}, {declarer}, '{result.DeclarerNSEW}', '{contract}', '{result.TricksTakenSymbol}', '{leadCard}', '{result.Remarks}', #{DateTime.Now:yyyy-MM-dd}#, #{DateTime.Now:yyyy-MM-dd hh:mm:ss}#, False, False, False, False, False, False)";
            }
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    cmd.ExecuteNonQuery();
                });
            }
            catch { }
            cmd.Dispose();
        }

        public List<Result> GetResultsList(int sectionID = 0, int lowBoard = 0, int highBoard = 0, int tableNumber = 0, int roundNumber = 0)
        {
            string SQLString;
            if (sectionID == 0)  // Need all results
            {
                SQLString = $"SELECT Section, [Table], Round, Board, [NS/EW], Contract, LeadCard, Result, Remarks, PairNS, PairEW FROM ReceivedData";
            }
            else if (lowBoard == 0)  // Need all results for section
            {
                if (IsIndividual)
                {
                    SQLString = $"SELECT Section, [Table], Round, Board, [NS/EW], Contract, LeadCard, Result, Remarks, PairNS, PairEW, South, West FROM ReceivedData WHERE Section={sectionID}";
                }
                else
                {
                    SQLString = $"SELECT Section, [Table], Round, Board, [NS/EW], Contract, LeadCard, Result, Remarks, PairNS, PairEW FROM ReceivedData WHERE Section={sectionID}";
                }
            }
            else if (highBoard == 0)  // Need all results for board = lowBoard
            {
                if (IsIndividual)
                {
                    SQLString = $"SELECT Section, [Table], Round, Board, [NS/EW], Contract, LeadCard, Result, Remarks, PairNS, PairEW, South, West FROM ReceivedData WHERE Section={sectionID} AND Board={lowBoard}";
                }
                else
                {
                    SQLString = $"SELECT Section, [Table], Round, Board, [NS/EW], Contract, LeadCard, Result, Remarks, PairNS, PairEW FROM ReceivedData WHERE Section={sectionID} AND Board={lowBoard}";
                }
            }
            else  // Need just the results for this table and round
            {
                if (IsIndividual)
                {
                    SQLString = $"SELECT Section, [Table], Round, Board, [NS/EW], Contract, LeadCard, Result, Remarks, PairNS, PairEW, South, West FROM ReceivedData WHERE Section={sectionID} AND [Table]={tableNumber} AND Round={roundNumber} AND Board>={lowBoard} AND Board<={highBoard}";
                }
                else
                {
                    SQLString = $"SELECT Section, [Table], Round, Board, [NS/EW], Contract, LeadCard, Result, Remarks, PairNS, PairEW FROM ReceivedData WHERE Section={sectionID} AND [Table]={tableNumber} AND Round={roundNumber} AND Board>={lowBoard} AND Board<={highBoard}";
                }
            }
            List<Result> resultsList = [];
            using OdbcConnection connection = new(connectionString);
            connection.Open();
            OdbcCommand? cmd = null;
            OdbcDataReader? reader = null;
            try
            {
                cmd = new OdbcCommand(SQLString, connection);
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Result result = new()
                        {
                            SectionID = reader.GetInt32(0),
                            TableNumber = reader.GetInt32(1),
                            RoundNumber = reader.GetInt32(2),
                            BoardNumber = reader.GetInt32(3),
                            Remarks = reader.GetString(8),
                            NumberNorth = reader.GetInt32(9),
                            NumberEast = reader.GetInt32(10)
                        };
                        if (IsIndividual && sectionID != 0)
                        {
                            result.NumberSouth = reader.GetInt32(11);
                            result.NumberWest = reader.GetInt32(12);
                        }
                        result.SectionLetter = GetSection(result.SectionID).Letter;

                        string tempContract = reader.GetString(5);
                        if ((result.Remarks == string.Empty || result.Remarks == "Wrong direction") && tempContract.Length > 2)
                            if (tempContract == "PASS")
                            {
                                result.ContractLevel = 0;
                            }
                            else  // Hopefully the database contains a valid contract
                            {
                                result.DeclarerNSEW = reader.GetString(4);
                                string[] temp = tempContract.Split(' ');
                                result.ContractLevel = Convert.ToInt32(temp[0]);
                                result.ContractSuit = temp[1];
                                if (temp.Length > 2) result.ContractX = temp[2];
                                result.LeadCard = reader.GetString(6).Replace("10", "T");  // Use T for ten internally
                                result.TricksTakenSymbol = reader.GetString(7);
                                if (result.TricksTakenSymbol == string.Empty)
                                {
                                    result.TricksTaken = -1;
                                }
                                else if (result.TricksTakenSymbol == "=")
                                {
                                    result.TricksTaken = result.ContractLevel + 6;
                                }
                                else
                                {
                                    result.TricksTaken = result.ContractLevel + Convert.ToInt32(result.TricksTakenSymbol) + 6;
                                }
                            }
                        else
                        {
                            result.ContractLevel = -1;  // Board not played
                        }
                        resultsList.Add(result);
                    }
                });
            }
            finally
            {
                reader!.Close();
                cmd!.Dispose();
            }
            return resultsList;
        }

        // PLAYERNAMES
        private static readonly List<InternalPlayerRecord> internalPlayerRecordsList = [];

        public string GetInternalPlayerName(string playerID)
        {
            if (internalPlayerRecordsList.Count == 0)
            {
                return "#" + playerID;
            }
            InternalPlayerRecord? playerRecord = internalPlayerRecordsList.Find(x => (x.ID == playerID));
            if (playerRecord == null)
            {
                return "Unknown #" + playerID;
            }
            else
            {
                return playerRecord.Name;
            }
        }

        // PLAYERNUMBERS
        public void GetNamesForRound(TableStatus tableStatus)
        {
            Round round = tableStatus.RoundData;
            using OdbcConnection connection = new(connectionString);
            connection.Open();
            CheckTabScorePairNos(connection);
            if (IsIndividual)
            {
                round.NameNorth = GetNameFromPlayerNumbersTableIndividual(connection, tableStatus, round.NumberNorth);
                round.NameSouth = GetNameFromPlayerNumbersTableIndividual(connection, tableStatus, round.NumberSouth);
                round.NameEast = GetNameFromPlayerNumbersTableIndividual(connection, tableStatus, round.NumberEast);
                round.NameWest = GetNameFromPlayerNumbersTableIndividual(connection, tableStatus, round.NumberWest);
            }
            else  // Not individual
            {
                round.NameNorth = GetNameFromPlayerNumbersTable(connection, tableStatus, round.NumberNorth, "N");
                round.NameSouth = GetNameFromPlayerNumbersTable(connection, tableStatus, round.NumberNorth, "S");
                round.NameEast = GetNameFromPlayerNumbersTable(connection, tableStatus, round.NumberEast, "E");
                round.NameWest = GetNameFromPlayerNumbersTable(connection, tableStatus, round.NumberEast, "W");
            }

            round.GotAllNames = (round.NumberNorth == 0 || (round.NameNorth != string.Empty && round.NameSouth != string.Empty)) && (round.NumberEast == 0 || (round.NameEast != string.Empty && round.NameWest != string.Empty));
            return;
        }

        private void CheckTabScorePairNos(OdbcConnection conn)
        {
            object? queryResult = null;

            // Check to see if TabScorePairNo exists (it may get overwritten if the scoring program recreates the PlayerNumbers table)
            string SQLString = $"SELECT 1 FROM PlayerNumbers WHERE TabScorePairNo IS NULL";
            OdbcCommand cmd1 = new(SQLString, conn);
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    queryResult = cmd1.ExecuteScalar();
                });
            }
            finally
            {
                cmd1.Dispose();
            }

            if (queryResult != null)
            {
                // TabScorePairNo doesn't exist, so recreate it
                SQLString = "SELECT Section, [Table], Direction, Round FROM PlayerNumbers";
                OdbcCommand cmd2 = new(SQLString, conn);
                OdbcDataReader? reader2 = null;
                try
                {
                    ODBCRetryHelper.ODBCRetry(() =>
                    {
                        reader2 = cmd2.ExecuteReader();
                        while (reader2.Read())
                        {
                            int tempSectionID = reader2.GetInt32(0);
                            int tempTable = reader2.GetInt32(1);
                            string tempDirection = reader2.GetString(2);
                            int tempRoundNumber = reader2.GetInt32(3);
                            int queryRoundNumber = tempRoundNumber;
                            if (queryRoundNumber == 0) queryRoundNumber = 1;
                            if (IsIndividual)
                            {
                                switch (tempDirection)
                                {
                                    case "N":
                                        SQLString = $"SELECT NSPair FROM RoundData WHERE Section={tempSectionID} AND [Table]={tempTable} AND ROUND={queryRoundNumber}";
                                        break;
                                    case "S":
                                        SQLString = $"SELECT South FROM RoundData WHERE Section={tempSectionID} AND [Table]={tempTable} AND ROUND={queryRoundNumber}";
                                        break;
                                    case "E":
                                        SQLString = $"SELECT EWPair FROM RoundData WHERE Section={tempSectionID} AND [Table]={tempTable} AND ROUND={queryRoundNumber}";
                                        break;
                                    case "W":
                                        SQLString = $"SELECT West FROM RoundData WHERE Section={tempSectionID} AND [Table]={tempTable} AND ROUND={queryRoundNumber}";
                                        break;
                                }
                            }
                            else
                            {
                                switch (tempDirection)
                                {
                                    case "N":
                                    case "S":
                                        SQLString = $"SELECT NSPair FROM RoundData WHERE Section={tempSectionID} AND [Table]={tempTable} AND ROUND={queryRoundNumber}";
                                        break;
                                    case "E":
                                    case "W":
                                        SQLString = $"SELECT EWPair FROM RoundData WHERE Section={tempSectionID} AND [Table]={tempTable} AND ROUND={queryRoundNumber}";
                                        break;
                                }
                            }
                            OdbcCommand cmd3 = new(SQLString, conn);
                            try
                            {
                                ODBCRetryHelper.ODBCRetry(() =>
                                {
                                    queryResult = cmd3.ExecuteScalar();
                                });
                            }
                            finally
                            {
                                cmd3.Dispose();
                            }
                            string? TSpairNo = queryResult.ToString();
                            SQLString = $"UPDATE PlayerNumbers SET TabScorePairNo={TSpairNo} WHERE Section={tempSectionID} AND [Table]={tempTable} AND Direction='{tempDirection}' AND Round={tempRoundNumber}";
                            OdbcCommand cmd4 = new(SQLString, conn);
                            try
                            {
                                ODBCRetryHelper.ODBCRetry(() =>
                                {
                                    cmd4.ExecuteNonQuery();
                                });
                            }
                            finally
                            {
                                cmd4.Dispose();
                            }
                        }
                    });
                }
                finally
                {
                    reader2!.Close();
                    cmd2.Dispose();
                }
            }
        }

        private static string GetNameFromPlayerNumbersTable(OdbcConnection conn, TableStatus tableStatus, int pairNo, string direction)
        {
            if (pairNo == 0) return string.Empty;
            string number = string.Empty;
            string name = string.Empty;
            DateTime latestTimeLog = new(2010, 1, 1);

            // First look for entries in the same direction
            string SQLString = $"SELECT Number, Name, Round, TimeLog FROM PlayerNumbers WHERE Section={tableStatus.SectionID} AND TabScorePairNo={pairNo} AND Direction='{direction}'";
            OdbcCommand cmd = new(SQLString, conn);
            OdbcDataReader? reader = null;
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        try
                        {
                            int readerRoundNumber = reader.GetInt32(2);
                            DateTime timeLog;
                            if (reader.IsDBNull(3))
                            {
                                timeLog = new DateTime(2010, 1, 1);
                            }
                            else
                            {
                                timeLog = reader.GetDateTime(3);
                            }
                            if (readerRoundNumber <= tableStatus.RoundNumber && timeLog >= latestTimeLog)
                            {
                                number = reader.GetString(0);
                                name = reader.GetString(1);
                                latestTimeLog = timeLog;
                            }
                        }
                        catch { }  // Record found, but format cannot be parsed
                    }
                });
            }
            finally
            {
                reader!.Close();
            }

            Section? section = sectionsList.Find(x => x.ID == tableStatus.SectionID);
            if (section != null && section.Winners == 1)  // If a one-winner pairs movement, we also need to check the other direction 
            {
                string otherDir = direction switch
                {
                    "N" => "E",
                    "S" => "W",
                    "E" => "N",
                    "W" => "S",
                    _ => string.Empty,
                };
                SQLString = $"SELECT Number, Name, Round, TimeLog FROM PlayerNumbers WHERE Section={tableStatus.SectionID} AND TabScorePairNo={pairNo} AND Direction='{otherDir}'";
                cmd = new OdbcCommand(SQLString, conn);
                try
                {
                    ODBCRetryHelper.ODBCRetry(() =>
                    {
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            try
                            {
                                int readerRoundNumber = reader.GetInt32(2);
                                DateTime timeLog;
                                if (reader.IsDBNull(3))
                                {
                                    timeLog = new DateTime(2010, 1, 1);
                                }
                                else
                                {
                                    timeLog = reader.GetDateTime(3);
                                }
                                if (readerRoundNumber <= tableStatus.RoundNumber && timeLog >= latestTimeLog)
                                {
                                    number = reader.GetString(0);
                                    name = reader.GetString(1);
                                    latestTimeLog = timeLog;
                                }
                            }
                            catch { } // Record found, but format cannot be parsed
                        }
                    });
                }
                finally
                {
                    reader.Close();
                }
            }
            cmd.Dispose();
            if (name == string.Empty && number != string.Empty && number != "0")
            {
                return "#" + number;
            }
            else
            {
                return name;
            }
        }

        private static string GetNameFromPlayerNumbersTableIndividual(OdbcConnection conn, TableStatus tableStatus, int playerNo)
        {
            if (playerNo == 0) return string.Empty;
            string number = string.Empty;
            string name = string.Empty;
            DateTime latestTimeLog = new(2010, 1, 1);

            string SQLString = $"SELECT Number, Name, Round, TimeLog FROM PlayerNumbers WHERE Section={tableStatus.SectionID} AND TabScorePairNo={playerNo}";
            OdbcCommand cmd = new(SQLString, conn);
            OdbcDataReader? reader = null;
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        try
                        {
                            int readerRoundNumber = reader.GetInt32(2);
                            DateTime timeLog;
                            if (reader.IsDBNull(3))
                            {
                                timeLog = new DateTime(2010, 1, 1);
                            }
                            else
                            {
                                timeLog = reader.GetDateTime(3);
                            }
                            if (readerRoundNumber <= tableStatus.RoundNumber && timeLog >= latestTimeLog)
                            {
                                number = reader.GetString(0);
                                name = reader.GetString(1);
                                latestTimeLog = timeLog;
                            }
                        }
                        catch { } // Record found, but format cannot be parsed
                    }
                });
            }
            finally
            {
                reader!.Close();
            }
            cmd.Dispose();
            if (name == string.Empty && number != string.Empty && number != "0")
            {
                return "#" + number;
            }
            else
            {
                return name;
            }
        }

        public void UpdatePlayer(TableStatus tableStatus, Direction direction, string playerID, string playerName)
        {
            // Numbers entered at the start (when round = 1) need to be set as round 0 in the database
            int roundNumber = tableStatus.RoundNumber;
            if (roundNumber == 1) roundNumber = 0;
            
            if (playerName.Contains("Unknown") || playerName.Contains('#')) playerName = string.Empty;
            playerName = playerName.Replace("'", "''");    // Deal with apostrophes in names, eg O'Connor

            string directionLetter = direction.ToString()[..1];    // Need just N, S, E or W
            int pairNumber = 0;
            switch (direction)
            {
                case Direction.North:
                    pairNumber = tableStatus.RoundData.NumberNorth;
                    break;
                case Direction.South:
                    pairNumber = tableStatus.RoundData.NumberSouth;
                    break;
                case Direction.East:
                    pairNumber = tableStatus.RoundData.NumberEast;
                    break;
                case Direction.West:
                    pairNumber = tableStatus.RoundData.NumberWest;
                    break;
            }

            using OdbcConnection connection = new(connectionString);
            connection.Open();
            object? queryResult = null;

            // Check if PlayerNumbers entry exists already; if it does update it, if not create it
            string SQLString = $"SELECT Section FROM PlayerNumbers WHERE Section={tableStatus.SectionID} AND [Table]={tableStatus.TableNumber} AND Round={roundNumber} AND Direction='{directionLetter}'";
            OdbcCommand cmd = new(SQLString, connection);
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    queryResult = cmd.ExecuteScalar();
                });
            }
            catch { }
            if (queryResult == null)
            {
                SQLString = $"INSERT INTO PlayerNumbers (Section, [Table], Direction, [Number], Name, Round, Processed, TimeLog, TabScorePairNo) VALUES ({tableStatus.SectionID}, {tableStatus.TableNumber}, '{directionLetter}', '{playerID}', '{playerName}', {roundNumber}, False, #{DateTime.Now:yyyy-MM-dd hh:mm:ss}#, {pairNumber})";
            }
            else
            {
                SQLString = $"UPDATE PlayerNumbers SET [Number]='{playerID}', [Name]='{playerName}', Processed=False, TimeLog=#{DateTime.Now:yyyy-MM-dd hh:mm:ss}#, TabScorePairNo={pairNumber} WHERE Section={tableStatus.SectionID} AND [Table]={tableStatus.TableNumber} AND Round={roundNumber} AND Direction='{directionLetter}'";
            }
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    cmd.ExecuteNonQuery();
                });
            }
            catch { }
            cmd.Dispose();
            return;
        }

        // HANDRECORD
        private static readonly List<Hand> handsList = [];

        public int HandsCount { get { return handsList.Count; } }

        public List<Hand> HandsList { get { return handsList; } }

        public Hand? GetHand(int sectionID, int boardNumber)
        {
            return handsList.Find(x => x.SectionID == sectionID && x.BoardNumber == boardNumber);
        }

        public void AddHand(Hand hand)
        {
            using OdbcConnection connection = new(connectionString);
            // Delete any previous hand record
            connection.Open();
            string SQLString = $"DELETE FROM HandRecord WHERE Section={hand.SectionID} AND Board={hand.BoardNumber}";
            OdbcCommand cmd = new(SQLString, connection);
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    cmd.ExecuteNonQuery();
                });
                handsList.RemoveAll(x => x.SectionID == hand.SectionID && x.BoardNumber == hand.BoardNumber);
            }
            catch { }

            SQLString = $"INSERT INTO HandRecord (Section, Board, NorthSpades, NorthHearts, NorthDiamonds, NorthClubs, EastSpades, EastHearts, EastDiamonds, EastClubs, SouthSpades, SouthHearts, SouthDiamonds, SouthClubs, WestSpades, WestHearts, WestDiamonds, WestClubs) VALUES ({hand.SectionID}, {hand.BoardNumber}, '{hand.NorthSpades}', '{hand.NorthHearts}', '{hand.NorthDiamonds}', '{hand.NorthClubs}', '{hand.EastSpades}', '{hand.EastHearts}', '{hand.EastDiamonds}', '{hand.EastClubs}', '{hand.SouthSpades}', '{hand.SouthHearts}', '{hand.SouthDiamonds}', '{hand.SouthClubs}', '{hand.WestSpades}', '{hand.WestHearts}', '{hand.WestDiamonds}', '{hand.WestClubs}')";
            cmd = new OdbcCommand(SQLString, connection);
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    cmd.ExecuteNonQuery();
                });
                handsList.Add(hand);
            }
            catch { }
            cmd.Dispose();
        }

        public void GetHandsFromFile(StreamReader file)
        {
            bool newBoard = false;
            string? line = null;
            char[] quoteDelimiter = ['"'];

            handsList.Clear();

            if (!file.EndOfStream)
            {
                line = file.ReadLine();
                newBoard = line != null && line.Length > 7 && line[..7] == "[Board ";
            }
            while (!file.EndOfStream)
            {
                if (newBoard)
                {
                    newBoard = false;
                    Hand hand = new() { BoardNumber = Convert.ToInt32(line!.Split(quoteDelimiter)[1]) };
                    while ((line = file.ReadLine()) != null)
                    {
                        if (line.Length > 6 && line[..6] == "[Deal ")
                        {
                            hand.PBN = line.Split(quoteDelimiter)[1];
                        }
                        else if (line.Length > 7 && line[..7] == "[Board ")
                        {
                            newBoard = true;
                            if (hand.NorthSpades != "###") handsList.Add(hand);
                            break;
                        }
                    }
                    if (file.EndOfStream)
                    {
                        if (hand.NorthSpades != "###") handsList.Add(hand);
                    }
                }
                else if (!file.EndOfStream)
                {
                    line = file.ReadLine();
                    newBoard = line != null && line.Length > 7 && line[..7] == "[Board ";
                }
            }
            file.Close();

            using OdbcConnection connection = new(connectionString);
            connection.Open();
            string SQLString = "DELETE FROM HandRecord";
            OdbcCommand cmd = new(SQLString, connection);
            cmd.ExecuteNonQuery();

            foreach (Hand hand in handsList)
            {
                if (hand.NorthSpades != "###")
                {
                    SQLString = $"INSERT INTO HandRecord (Section, Board, NorthSpades, NorthHearts, NorthDiamonds, NorthClubs, EastSpades, EastHearts, EastDiamonds, EastClubs, SouthSpades, SouthHearts, SouthDiamonds, SouthClubs, WestSpades, WestHearts, WestDiamonds, WestClubs) VALUES ({hand.SectionID}, {hand.BoardNumber}, '{hand.NorthSpades}', '{hand.NorthHearts}', '{hand.NorthDiamonds}', '{hand.NorthClubs}', '{hand.EastSpades}', '{hand.EastHearts}', '{hand.EastDiamonds}', '{hand.EastClubs}', '{hand.SouthSpades}', '{hand.SouthHearts}', '{hand.SouthDiamonds}', '{hand.SouthClubs}', '{hand.WestSpades}', '{hand.WestHearts}', '{hand.WestDiamonds}', '{hand.WestClubs}')";
                    cmd = new OdbcCommand(SQLString, connection);
                    cmd.ExecuteNonQuery();
                }
            }
            cmd.Dispose();
        }

        // SETTINGS
        public DatabaseSettings GetDatabaseSettings()
        {
            DatabaseSettings databaseSettings = new();
            using OdbcConnection connection = new(connectionString);
            connection.Open();
            string SQLString = "SELECT ShowResults, ShowPercentage, LeadCard, BM2ValidateLeadCard, BM2Ranking, EnterResultsMethod, BM2ViewHandRecord, BM2NumberEntryEachRound, BM2NameSource, BM2EnterHandRecord FROM Settings";
            OdbcCommand cmd = new(SQLString, connection);
            OdbcDataReader? reader = null;
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        databaseSettings.ShowTraveller = reader.GetBoolean(0);
                        databaseSettings.ShowPercentage = reader.GetBoolean(1);
                        databaseSettings.EnterLeadCard = reader.GetBoolean(2);
                        databaseSettings.ValidateLeadCard = reader.GetBoolean(3);
                        databaseSettings.ShowRanking = reader.GetInt32(4);
                        databaseSettings.EnterResultsMethod = reader.GetInt32(5);
                        if (databaseSettings.EnterResultsMethod != 1) databaseSettings.EnterResultsMethod = 0;
                        databaseSettings.ShowHandRecord = reader.GetBoolean(6);
                        databaseSettings.NumberEntryEachRound = reader.GetBoolean(7);
                        databaseSettings.NameSource = reader.GetInt32(8);
                        databaseSettings.ManualHandRecordEntry = reader.GetBoolean(9);
                    }
                    reader.Close();
                });
            }
            catch
            {
                // In case of error, use defaults
            }
            finally
            {
                cmd.Dispose();
            }
            return databaseSettings;
        }

        public void UpdateDatabaseSettings(DatabaseSettings databaseSettings)
        {
            using OdbcConnection connection = new(connectionString);
            StringBuilder SQLString = new();
            SQLString.Append($"UPDATE Settings SET");
            if (databaseSettings.ShowTraveller)
            {
                SQLString.Append(" ShowResults=YES,");
            }
            else
            {
                SQLString.Append(" ShowResults=NO,");
            }
            if (databaseSettings.ShowPercentage)
            {
                SQLString.Append(" ShowPercentage=YES,");
            }
            else
            {
                SQLString.Append(" ShowPercentage=NO,");
            }
            if (databaseSettings.EnterLeadCard)
            {
                SQLString.Append(" LeadCard=YES,");
            }
            else
            {
                SQLString.Append(" LeadCard=NO,");
            }
            if (databaseSettings.ValidateLeadCard)
            {
                SQLString.Append(" BM2ValidateLeadCard=YES,");
            }
            else
            {
                SQLString.Append(" BM2ValidateLeadCard=NO,");
            }
            SQLString.Append($" BM2Ranking={databaseSettings.ShowRanking},");
            if (databaseSettings.ShowHandRecord)
            {
                SQLString.Append(" BM2ViewHandRecord=YES,");
            }
            else
            {
                SQLString.Append(" BM2ViewHandRecord=NO,");
            }
            if (databaseSettings.NumberEntryEachRound)
            {
                SQLString.Append(" BM2NumberEntryEachRound=YES,");
            }
            else
            {
                SQLString.Append(" BM2NumberEntryEachRound=NO,");
            }
            SQLString.Append($" BM2NameSource={databaseSettings.NameSource},");
            SQLString.Append($" EnterResultsMethod={databaseSettings.EnterResultsMethod}");

            connection.Open();
            OdbcCommand cmd = new(SQLString.ToString(), connection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        // RANKINGLIST
        public List<Ranking> GetRankingList(int sectionID)
        {
            List<Ranking> rankingList = [];
            using OdbcConnection connection = new(connectionString);
            connection.Open();
            string SQLString = $"SELECT Orientation, Number, Score, Rank FROM Results WHERE Section={sectionID}";

            OdbcCommand cmd = new(SQLString, connection);
            OdbcDataReader? reader1 = null;
            try
            {
                ODBCRetryHelper.ODBCRetry(() =>
                {
                    reader1 = cmd.ExecuteReader();
                    while (reader1.Read())
                    {
                        Ranking ranking = new(reader1.GetString(0), reader1.GetInt32(1), reader1.GetString(2), reader1.GetString(3));
                        ranking.ScoreDecimal = Convert.ToDouble(ranking.Score);
                        rankingList.Add(ranking);
                    }
                    reader1.Close();
                });
            }
            catch (OdbcException e)
            {
                if (e.Errors.Count > 1 || e.Errors[0].SQLState != "42S02")  // Any error other than results table doesn't exist
                {
                    throw;
                }
            }
            cmd.Dispose();
            return rankingList;
        }
    }
}
