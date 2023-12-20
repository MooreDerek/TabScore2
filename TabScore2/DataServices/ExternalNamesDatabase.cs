using System.Data.Odbc;

namespace TabScore2.DataServices
{
    public class ExternalNamesDatabase : IExternalNamesDatabase
    {
        public string GetExternalPlayerName(string playerID)
        {
            string name = "Unknown";
            OdbcConnectionStringBuilder externalDB = new() { Driver = "Microsoft Access Driver (*.mdb)" };
            externalDB.Add("Dbq", @"C:\Bridgemate\BMPlayerDB.mdb");
            externalDB.Add("Uid", "Admin");
            using (OdbcConnection connection = new(externalDB.ToString()))
            {
                object? queryResult = null;
                string SQLString = $"SELECT Name FROM PlayerNameDatabase WHERE ID={playerID}";
                OdbcCommand cmd = new(SQLString, connection);
                try
                {
                    connection.Open();
                    ODBCRetryHelper.ODBCRetry(() =>
                    {
                        queryResult = cmd.ExecuteScalar();
                        if (queryResult != null)
                        {
                            string? tempName = queryResult.ToString();
                            if (tempName != null) name = tempName;
                        }
                    });
                }
                catch (OdbcException)  // If we can't read the external database for whatever reason, just return "Unknown"
                {
                }
                finally
                {
                    cmd.Dispose();
                }
            }
            return name;
        }
    }
}
