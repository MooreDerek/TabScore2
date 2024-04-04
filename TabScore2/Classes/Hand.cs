// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using System.Text;

namespace TabScore2.Classes
{
    public class Hand
    {
        public int SectionID { get; set; } = 1;  // Default SectionID=1 if hands apply to more than one section
        public int BoardNumber { get; set; } = 0;
        public string NorthSpades { get; set; } = "###";  // Indicates no hand record available
        public string NorthHearts { get; set; } = string.Empty;
        public string NorthDiamonds { get; set; } = string.Empty; 
        public string NorthClubs { get; set; } = string.Empty;
        public string EastSpades { get; set; } = string.Empty;
        public string EastHearts { get; set; } = string.Empty;
        public string EastDiamonds { get; set; } = string.Empty;
        public string EastClubs { get; set; } = string.Empty;
        public string SouthSpades { get; set; } = string.Empty;
        public string SouthHearts { get; set; } = string.Empty;
        public string SouthDiamonds { get; set; } = string.Empty;
        public string SouthClubs { get; set; } = string.Empty;
        public string WestSpades { get; set; } = string.Empty;
        public string WestHearts { get; set; } = string.Empty;
        public string WestDiamonds { get; set; } = string.Empty;
        public string WestClubs { get; set; } = string.Empty;
    }
}