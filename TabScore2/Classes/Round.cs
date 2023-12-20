// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Classes
{
    public class Round
    {
        public int TableNumber { get; set; }
        public int NumberNorth { get; set; } = 0;  // Applies to NS pair in pairs and teams
        public int NumberEast { get; set; } = 0;  // Applies to EW pair in pairs and teams
        public int NumberSouth { get; set; } = 0;
        public int NumberWest { get; set; } = 0;
        public string PlayerIDNorth { get; set; } = string.Empty;
        public string PlayerIDSouth { get; set; } = string.Empty;
        public string PlayerIDEast { get; set; } = string.Empty;
        public string PlayerIDWest { get; set; } = string.Empty;
        public string NameNorth { get; set; } = string.Empty;
        public string NameSouth { get; set; } = string.Empty;
        public string NameEast { get; set; } = string.Empty;
        public string NameWest { get; set; } = string.Empty;
        public bool GotAllNames { get; set; } = false;
        public int LowBoard { get; set; }
        public int HighBoard { get; set; }
    }
}
