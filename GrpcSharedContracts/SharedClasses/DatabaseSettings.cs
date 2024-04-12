// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using System.Runtime.Serialization;

namespace TabScore2.SharedClasses
{
    [DataContract]
    public class DatabaseSettings
    {
        [DataMember(Order = 1)] public bool UpdateRequired { get; set; }
        [DataMember(Order = 2)] public bool ShowTraveller { get; set; }
        [DataMember(Order = 3)] public bool ShowPercentage { get; set; }
        [DataMember(Order = 4)] public bool EnterLeadCard { get; set; }
        [DataMember(Order = 5)] public bool ValidateLeadCard { get; set; }
        [DataMember(Order = 6)] public int ShowRanking { get; set; }
        [DataMember(Order = 7)] public int EnterResultsMethod { get; set; }
        [DataMember(Order = 8)] public bool ShowHandRecord { get; set; }
        [DataMember(Order = 9)] public bool NumberEntryEachRound { get; set; }
        [DataMember(Order = 10)] public int NameSource { get; set; }
        [DataMember(Order = 11)] public bool ManualHandRecordEntry { get; set; }
    }
}