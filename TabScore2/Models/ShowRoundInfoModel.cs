﻿// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Models
{
    public class ShowRoundInfoModel()
    {
        public int RoundNumber { get; set; }
        public int NumberNorth { get; set; }  // Applies to NS pair in pairs and teams
        public int NumberEast { get; set; }  // Applies to EW pair in pairs and teams
        public int NumberSouth { get; set; }
        public int NumberWest { get; set; }
        public string DisplayNameNorth { get; set; } = string.Empty;
        public string DisplayNameSouth { get; set; } = string.Empty;
        public string DisplayNameEast { get; set; } = string.Empty;
        public string DisplayNameWest { get; set; } = string.Empty;
        public int LowBoard { get; set; }
        public int HighBoard { get; set; }
        public bool NSMissing { get; set; } = false;
        public bool EWMissing { get; set; } = false;
        public int BoardsFromTable { get; set; } = -1;
    }
}