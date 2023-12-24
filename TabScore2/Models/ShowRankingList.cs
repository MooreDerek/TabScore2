// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Classes;

namespace TabScore2.Models
{
    public class ShowRankingList : List<Ranking>
    {
        public int TabletDeviceNumber { get; set; }
        public int RoundNumber { get; set; }
        public int NumberNorth { get; set; } = 0;
        public int NumberEast { get; set; } = 0;
        public int NumberSouth { get; set; } = 0;
        public int NumberWest { get; set; } = 0;
        public bool FinalRankingList { get; set; } = false;
    }
}
