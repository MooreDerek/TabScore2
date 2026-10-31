using TabScore2.DataServices;
using TabScore2.Globals;

namespace TabScore2.Tests.Support;

internal sealed class TestSettings : ISettings
{
    public bool ShowTraveller { get; set; }
    public bool ShowPercentage { get; set; }
    public bool EnterLeadCard { get; set; }
    public bool ValidateLeadCard { get; set; }
    public int ShowRanking { get; set; }
    public bool ShowHandRecord { get; set; }
    public bool NumberEntryEachRound { get; set; }
    public int NameSource { get; set; }
    public int EnterResultsMethod { get; set; }
    public bool ManualHandRecordEntry { get; set; }
    public bool DefaultShowTraveller { get; set; }
    public bool DefaultShowPercentage { get; set; }
    public bool DefaultEnterLeadCard { get; set; }
    public bool DefaultValidateLeadCard { get; set; }
    public int DefaultShowRanking { get; set; }
    public bool DefaultShowHandRecord { get; set; }
    public bool DefaultNumberEntryEachRound { get; set; }
    public int DefaultNameSource { get; set; }
    public int DefaultEnterResultsMethod { get; set; }
    public bool DefaultManualHandRecordEntry { get; set; }
    public Mode Mode { get; set; } = Mode.Personal;
    public string ShowHandRecordFromDirection { get; set; } = "South";
    public bool ShowTimer { get; set; }
    public int SecondsPerBoard { get; set; } = 390;
    public int AdditionalSecondsPerRound { get; set; } = 60;
    public bool MasterTableForTimer { get; set; }
    public bool DoubleDummy { get; set; }
    public int SuppressRankingListForFirstXRounds { get; set; }
    public int SuppressRankingListForLastXRounds { get; set; }
    public bool ShowSplashScreen { get; set; }
    public bool RegisterByContestantNumber { get; set; }
    public bool DatabaseReady { get; set; } = true;
    public bool IsIndividual { get; set; }
    public bool SessionStarted { get; set; } = true;
}
