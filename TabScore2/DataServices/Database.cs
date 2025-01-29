// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcMessageClasses;
using GrpcServices;
using TabScore2.SharedClasses;

namespace TabScore2.DataServices
{
    public class Database(IBwsDatabaseService iClient, ISettings iSettings) : IDatabase
    {
        private readonly IBwsDatabaseService client = iClient;
        private readonly ISettings settings = iSettings;

        // ============================
        // Prepare the database for use
        // ============================
        public string Initialize(string pathToDatabase)  // Called from main form when path to database is set
        {
            InitializeReturnMessage initializeReturnMessage = client.Initialize(new InitializeMessage() { 
                PathToDatabase = pathToDatabase,
                DefaultShowTraveller = settings.DefaultShowTraveller,
                DefaultShowPercentage = settings.DefaultShowPercentage,
                DefaultEnterLeadCard = settings.DefaultEnterLeadCard,
                DefaultValidateLeadCard = settings.DefaultValidateLeadCard,
                DefaultShowRanking = settings.DefaultShowRanking,
                DefaultEnterResultsMethod = settings.DefaultEnterResultsMethod,
                DefaultShowHandRecord = settings.DefaultShowHandRecord,
                DefaultNumberEntryEachRound = settings.DefaultNumberEntryEachRound,
                DefaultNameSource = settings.DefaultNameSource,
                DefaultManualHandRecordEntry = settings.DefaultManualHandRecordEntry
            });
            if (initializeReturnMessage.ReturnMessage == string.Empty)
            {
                settings.IsIndividual = initializeReturnMessage.IsIndividual;
                GetDatabaseSettings();
            }
            return initializeReturnMessage.ReturnMessage;
        }

        public void WebappInitialize()  // Called from webapp StartScreen and run just once.  After this point, changing the TabletsMove setting will have no effect
        {
            GetDatabaseSettings();    // Refresh setting as these can be changed by the scoring program
            client.WebappInitialize(new WebappInitializeMessage() { TabletsMove = settings.TabletsMove });
        }

        public bool IsDatabaseConnectionOK()
        {
            return client.IsDatabaseConnectionOK().IsDatabaseConnectionOK;
        } 
        

        // ========================================
        // Implement methods to access the database
        // ========================================

        // SECTION
        public Section GetSection(int sectionID)
        {
            return client.GetSection(new SectionIDMessage() { SectionID = sectionID });
        }

        public List<Section> GetSectionsList()
        {
            return client.GetSectionsList();
        }

        // TABLE
        public void RegisterTable(int sectionID, int tableNumber)
        {
            client.RegisterTable(new SectionTableMessage() { SectionID = sectionID, TableNumber = tableNumber });
        }

        // ROUND
        public int GetNumberOfRoundsInSection(int sectionID, bool forceDatabaseRead = false)
        {
            if (forceDatabaseRead)
            {
                client.UpdateNumberOfRoundsInSection(new SectionIDMessage() { SectionID = sectionID });
            }
            return client.GetSection(new SectionIDMessage() { SectionID = sectionID }).NumberOfRounds;
        }

        public int GetNumberOfLastRoundWithResults(int sectionID, int tableNumber)
        {
            return client.GetNumberOfLastRoundWithResults(new SectionTableMessage() { SectionID = sectionID, TableNumber = tableNumber }).NumberOfLastRoundWithResults;
        }

        public List<Round> GetRoundsList(int sectionID, int roundNumber) 
        {
            return client.GetRoundsList(new SectionRoundMessage() { SectionID = sectionID, RoundNumber = roundNumber });
        }

        public Round GetRound(int sectionID, int tableNumber, int roundNumber)
        {
            return client.GetRound(new SectionTableRoundMessage { SectionID = sectionID, TableNumber = tableNumber, RoundNumber = roundNumber });
        }

        // RECEIVEDDATA
        public Result GetResult(int sectionID, int tableNumber, int roundNumber, int boardNumber)
        {
            return client.GetResult(new SectionTableRoundBoardMessage { SectionID = sectionID, TableNumber = tableNumber, RoundNumber = roundNumber, BoardNumber = boardNumber });
        }

        public void SetResult(Result result)
        {
            client.SetResult(result);
        }

        public List<Result> GetResultsList(int sectionID = 0, int lowBoard = 0, int highBoard = 0, int tableNumber = 0, int roundNumber = 0)
        {
            return client.GetResultsList(new ResultsListMessage() { SectionID = sectionID, LowBoard = lowBoard, HighBoard = highBoard, TableNumber = tableNumber, RoundNumber = roundNumber });
        }

        // PLAYERNAMES
        public string GetInternalPlayerName(string PlayerID)
        {
            string name = client.GetInternalPlayerName(new PlayerMessage() { PlayerID = PlayerID }).PlayerName;
            if (name == "Unknown")
            {
                return "#" + PlayerID;
            }
            else
            {
                return name;
            }
        }

        // PLAYERNUMBERS
        public Names GetNamesForRound(int sectionID, int roundNumber, int numberNorth, int numberEast, int numberSouth, int numberWest)
        {
            return client.GetNamesForRound(new NamesForRoundMessage { SectionID = sectionID, RoundNumber = roundNumber, NumberNorth = numberNorth, NumberEast = numberEast, NumberSouth = numberSouth, NumberWest = numberWest });
        }

        public void UpdatePlayer(int sectionID, int tableNumber, int roundNumber, string directionLetter, int pairNumber, string playerID, string playerName)
        {
            client.UpdatePlayer(new UpdatePlayerNumberMessage() { SectionID = sectionID, TableNumber = tableNumber, RoundNumber = roundNumber, DirectionLetter = directionLetter, PairNumber = pairNumber, PlayerID = playerID, PlayerName = playerName });
        }

        // HANDRECORD
        public int GetHandsCount() 
        {
            return client.GetHandsCount().HandsCount;
        }

        public List<Hand> GetHandsList()
        {
            return client.GetHandsList();
        }

        public Hand GetHand(int sectionID, int boardNumber)
        {
            return client.GetHand(new SectionBoardMessage { SectionID = sectionID, BoardNumber = boardNumber });
        }

        public void AddHand(Hand hand)
        {
            if (hand.NorthSpades == "###") return;
            client.AddHand(hand);
        }

        public void AddHands(List<Hand> newHandsList)
        {
            client.AddHands(newHandsList);
        }

        // SETTINGS
        public void GetDatabaseSettings(int sectionID = 1, int roundNumber = 0)
        { 
            DatabaseSettings databaseSettings = client.GetDatabaseSettings(new SectionRoundMessage() { SectionID = sectionID, RoundNumber = roundNumber });
            if (databaseSettings.UpdateRequired)
            {
                settings.ShowTraveller = databaseSettings.ShowTraveller;
                settings.ShowPercentage = databaseSettings.ShowPercentage;
                settings.EnterLeadCard = databaseSettings.EnterLeadCard;
                settings.ValidateLeadCard = databaseSettings.ValidateLeadCard;
                settings.ShowRanking = databaseSettings.ShowRanking;
                settings.ShowHandRecord = databaseSettings.ShowHandRecord;
                settings.NumberEntryEachRound = databaseSettings.NumberEntryEachRound;
                settings.NameSource = databaseSettings.NameSource;
                settings.EnterResultsMethod = databaseSettings.EnterResultsMethod;
                settings.ManualHandRecordEntry = databaseSettings.ManualHandRecordEntry;
            }
        }

        public void SetDatabaseSettings()
        {
            DatabaseSettings databaseSettings = new()
            {
                ShowTraveller = settings.ShowTraveller,
                ShowPercentage = settings.ShowPercentage,
                EnterLeadCard = settings.EnterLeadCard,
                ValidateLeadCard = settings.ValidateLeadCard,
                ShowRanking = settings.ShowRanking,
                ShowHandRecord = settings.ShowHandRecord,
                NumberEntryEachRound = settings.NumberEntryEachRound,
                NameSource = settings.NameSource,
                EnterResultsMethod = settings.EnterResultsMethod,
                ManualHandRecordEntry = settings.ManualHandRecordEntry
            };
            client.SetDatabaseSettings(databaseSettings);
        }

        // RANKINGLIST
        public List<Ranking> GetRankingList(int sectionID)
        {
            return client.GetRankingList(new SectionIDMessage() { SectionID = sectionID });
        }
    }
}
