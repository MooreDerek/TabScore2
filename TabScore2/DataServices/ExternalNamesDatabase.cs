// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcMessageClasses;
using GrpcServices;

namespace TabScore2.DataServices
{
    public class ExternalNamesDatabase(IExternalNamesDatabaseService iClient) : IExternalNamesDatabase
    {
        private readonly IExternalNamesDatabaseService client = iClient;

        public string GetExternalPlayerName(string playerID)
        {
            return client!.GetExternalPlayerName(new PlayerMessage() { PlayerID = playerID }).PlayerName;
        }
    }
}
