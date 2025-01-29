// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Classes
{
    public class TravellerResult
    {
        public int NumberNorth { get; set; }
        public int NumberEast { get; set; }
        public int NumberSouth { get; set; }
        public int NumberWest{ get; set; }
        public string DisplayContract { get; set; } = string.Empty;
        public string DisplayDeclarerNSEW { get; set; } = string.Empty;
        public string DisplayLeadCard { get; set; } = string.Empty;
        public string ScoreNS { get; set; } = string.Empty;
        public string ScoreEW { get; set; } = string.Empty;
        public double SortPercentage { get; set; }
        public bool Highlight { get; set; } = false;
    }
}
