namespace TabScore2.DataServices
{
    public interface ISettings
    {
        void SetTabletDevicesPerTable();
        void GetFromDatabase(int roundNumber = 1);
        void UpdateDatabase();

        bool ShowTraveller { get; set; }
        bool ShowPercentage { get; set; }
        bool EnterLeadCard { get; set; }
        bool ValidateLeadCard { get; set; }
        int ShowRanking { get; set; }
        bool ShowHandRecord { get; set; }
        bool NumberEntryEachRound { get; set; }
        int NameSource { get; set; }
        int EnterResultsMethod { get; set; }
        bool TabletsMove { get; set; }
        string ShowHandRecordFromDirection { get; set; }
        bool ShowTimer { get; set; }
        int SecondsPerBoard { get; set; }
        int AdditionalSecondsPerRound { get; set; }
        bool ManualHandRecordEntry { get; set; }
        bool DoubleDummy { get; set; }
        int SuppressRankingListForFirstXRounds { get; set; }
        int SuppressRankingListForLastXRounds { get; set; }
    }
}
