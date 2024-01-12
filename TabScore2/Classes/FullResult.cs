// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Classes
{
    public class FullResult
    {
        public int SectionID { get; set; }
        public string SectionLetter { get; set; } = string.Empty;
        public int TableNumber { get; set; }
        public int RoundNumber { get; set; }
        public int BoardNumber { get; set; }
        public int NumberNorth { get; set; }
        public int NumberEast { get; set; }
        public string DeclarerNSEW { get; set; } = string.Empty;
        public bool Vulnerable { get; set; }
        public int ContractLevel { get; set; }
        public string ContractSuit { get; set; } = string.Empty;
        public string ContractX { get; set; } = string.Empty;
        public string LeadCard { get; set; } = string.Empty;
        public string TricksTakenSymbol { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }
}
