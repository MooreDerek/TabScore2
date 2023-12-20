// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using System.Data.Odbc;

namespace TabScore2.DataServices
{
    public static class ODBCRetryHelper
    {
        public static void ODBCRetry(Action cmd)
        {
            int attempts = 3;
            while (true)
            {
                try
                {
                    attempts--;
                    cmd();
                    break;
                }
                catch (OdbcException e)
                {
                    // Don't retry if single error is that table, column or field does not exist
                    if (e.Errors.Count == 1 && (e.Errors[0].SQLState == "42S02" || e.Errors[0].SQLState == "42S22" || e.Errors[0].SQLState == "07002")) throw;
                    if (attempts <= 0) throw;
                    Random r = new();
                    Thread.Sleep(r.Next(300, 500));
                }
            }
        }
    }
}
