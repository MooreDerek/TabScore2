// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcMessageClasses;
using System.Data.Odbc;

namespace GrpcServices
{
    public class ExternalNamesDatabaseService : IExternalNamesDatabaseService
    {
        public PlayerNameMessage GetExternalPlayerName(PlayerMessage request)
        {
            string name = "Unknown";
            OdbcConnectionStringBuilder externalDB = new() { Driver = "Microsoft Access Driver (*.mdb)" };
            externalDB.Add("Dbq", @"C:\Bridgemate\BMPlayerDB.mdb");
            externalDB.Add("Uid", "Admin");
            using (OdbcConnection connection = new(externalDB.ToString()))
            {
                object? queryResult = null;
                string SQLString = $"SELECT Name FROM PlayerNameDatabase WHERE ID={request.PlayerID}";
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
            return new PlayerNameMessage() { PlayerName = name };
        }
    }
}
