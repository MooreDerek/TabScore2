// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Globals;

namespace TabScore2.Classes
{
    public class DeviceStatus(int sectionID, int tableNumber, int pairNumber, int roundNumber, Direction direction)
    {
        public int SectionID { get; private set; } = sectionID;
        public int TableNumber { get; set; } = tableNumber;
        public int PairNumber { get; set; } = pairNumber;
        public Direction Direction { get; set; } = direction;
        public string? Location { get; set; }
        public int RoundNumber { get; set; } = roundNumber;
        public bool NamesUpdateRequired { get; set; } = true;
        public bool AtSitoutTable { get; set; }
    }
}
