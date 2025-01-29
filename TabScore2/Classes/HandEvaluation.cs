// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Classes
{
    public class HandEvaluation(int sectionID, int boardNumber)
    {
        public int SectionID { get; set; } = sectionID;
        public int BoardNumber { get; set; } = boardNumber;
        public int NorthSpades { get; set; } = -1;  // Indicates no hand evaluation
        public int NorthHearts { get; set; }
        public int NorthDiamonds { get; set; }
        public int NorthClubs { get; set; }
        public int NorthNotrump { get; set; }
        public int EastSpades { get; set; }
        public int EastHearts { get; set; }
        public int EastDiamonds { get; set; }
        public int EastClubs { get; set; }
        public int EastNotrump { get; set; }
        public int SouthSpades { get; set; }
        public int SouthHearts { get; set; }
        public int SouthDiamonds { get; set; }
        public int SouthClubs { get; set; }
        public int SouthNotrump { get; set; }
        public int WestSpades { get; set; }
        public int WestHearts { get; set; }
        public int WestDiamonds { get; set; }
        public int WestClubs { get; set; }
        public int WestNotrump { get; set; }
        public int NorthHcp { get; set; }
        public int SouthHcp { get; set; }
        public int EastHcp { get; set; }
        public int WestHcp { get; set; }
    }
}
