// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using System.Runtime.Serialization;

namespace TabScore2.SharedClasses
{
    [DataContract] 
    public class Section
    {
        [DataMember(Order = 1)] public int ID { get; set; }
        [DataMember(Order = 2)] public string Letter { get; set; } = string.Empty;
        [DataMember(Order = 3)] public int Tables { get; set; }
        [DataMember(Order = 4)] public int MissingPair { get; set; }
        [DataMember(Order = 5)] public int Winners { get; set; }
        [DataMember(Order = 6)] public int DevicesPerTable { get; set; }
        [DataMember(Order = 7)] public int CurrentRoundNumber { get; set; }
        [DataMember(Order = 8)] public int NumberOfRounds { get; set; }
    }
}