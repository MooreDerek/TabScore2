// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using System.Runtime.Serialization;

namespace TabScore2.SharedClasses
{
    [DataContract]
    public class Names
    {
        [DataMember(Order = 1)] public string NameNorth { get; set; } = string.Empty;
        [DataMember(Order = 2)] public string NameSouth { get; set; } = string.Empty;
        [DataMember(Order = 3)] public string NameEast { get; set; } = string.Empty;
        [DataMember(Order = 4)] public string NameWest { get; set; } = string.Empty;
        [DataMember(Order = 5)] public bool GotAllNames { get; set; }
    }
}