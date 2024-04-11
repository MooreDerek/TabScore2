// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using System.Runtime.Serialization;

namespace TabScore2.SharedClasses
{
    [DataContract]
    public class Result
    {
        [DataMember(Order = 1)] public int SectionID { get; set; }
        [DataMember(Order = 2)] public string SectionLetter { get; set; } = string.Empty;
        [DataMember(Order = 3)] public int TableNumber { get; set; }
        [DataMember(Order = 4)] public int RoundNumber { get; set; }
        [DataMember(Order = 5)] public int BoardNumber { get; set; } = 0;
        [DataMember(Order = 6)] public int NumberNorth { get; set; }
        [DataMember(Order = 7)] public int NumberEast { get; set; }
        [DataMember(Order = 8)] public int NumberSouth { get; set; } = 0;
        [DataMember(Order = 9)] public int NumberWest { get; set; } = 0;
        [DataMember(Order = 10)] public string DeclarerNSEW { get; set; } = string.Empty;
        [DataMember(Order = 11)] public bool Vulnerable { get; set; }
        [DataMember(Order = 12)] public int ContractLevel { get; set; } = -999;
        [DataMember(Order = 13)] public string ContractSuit { get; set; } = string.Empty;
        [DataMember(Order = 14)] public string ContractX { get; set; } = string.Empty;
        [DataMember(Order = 15)] public string LeadCard { get; set; } = string.Empty;
        [DataMember(Order = 16)] public int TricksTaken { get; set; } = -1;
        [DataMember(Order = 17)] public string TricksTakenSymbol { get; set; } = string.Empty;
        [DataMember(Order = 18)] public string Remarks { get; set; } = string.Empty;
        [DataMember(Order = 19)] public int Score { get; set; } = 0;
        [DataMember(Order = 20)] public double MatchpointsNS { get; set; }
        [DataMember(Order = 21)] public double MatchpointsEW { get; set; }
    }
}