// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Classes
{
    public class DatabaseSettings
    {
        public bool ShowTraveller { get; set; } = true;
        public bool ShowPercentage { get; set; } = true;
        public bool EnterLeadCard { get; set; } = true;
        public bool ValidateLeadCard { get; set; } = true;
        public int ShowRanking { get; set; } = 1;
        public int EnterResultsMethod { get; set; } = 1;
        public bool ShowHandRecord { get; set; } = true;
        public bool NumberEntryEachRound { get; set; } = false;
        public int NameSource { get; set; } = 0;
        public bool ManualHandRecordEntry { get; set; } = false;
    }
}