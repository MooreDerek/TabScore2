using SharedContracts;

namespace TabScore2.DataServices
{
    public class ExternalNamesDatabase(IExternalNamesDatabaseService iClient) : IExternalNamesDatabase
    {
        private readonly IExternalNamesDatabaseService client = iClient;

        public string GetExternalPlayerName(string playerID)
        {
            return client!.GetExternalPlayerName(new GrpcPlayerRequest() { PlayerID = playerID }).PlayerName;
        }
    }
}
