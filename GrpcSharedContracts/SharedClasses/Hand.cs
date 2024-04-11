// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using System.Runtime.Serialization;

namespace TabScore2.SharedClasses
{
    [DataContract]
    public class Hand
    {
        [DataMember(Order = 1)] public int SectionID { get; set; } = 1;  // Default SectionID=1 if hands apply to more than one section
        [DataMember(Order = 2)] public int BoardNumber { get; set; } = 0;
        [DataMember(Order = 3)] public string NorthSpades { get; set; } = "###";  // Indicates no hand record available
        [DataMember(Order = 4)] public string NorthHearts { get; set; } = string.Empty;
        [DataMember(Order = 5)] public string NorthDiamonds { get; set; } = string.Empty;
        [DataMember(Order = 6)] public string NorthClubs { get; set; } = string.Empty;
        [DataMember(Order = 7)] public string EastSpades { get; set; } = string.Empty;
        [DataMember(Order = 8)] public string EastHearts { get; set; } = string.Empty;
        [DataMember(Order = 9)] public string EastDiamonds { get; set; } = string.Empty;
        [DataMember(Order = 10)] public string EastClubs { get; set; } = string.Empty;
        [DataMember(Order = 11)] public string SouthSpades { get; set; } = string.Empty;
        [DataMember(Order = 12)] public string SouthHearts { get; set; } = string.Empty;
        [DataMember(Order = 13)] public string SouthDiamonds { get; set; } = string.Empty;
        [DataMember(Order = 14)] public string SouthClubs { get; set; } = string.Empty;
        [DataMember(Order = 15)] public string WestSpades { get; set; } = string.Empty;
        [DataMember(Order = 16)] public string WestHearts { get; set; } = string.Empty;
        [DataMember(Order = 17)] public string WestDiamonds { get; set; } = string.Empty;
        [DataMember(Order = 18)] public string WestClubs { get; set; } = string.Empty;
    }
}