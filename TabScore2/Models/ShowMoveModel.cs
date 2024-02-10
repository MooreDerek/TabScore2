// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Classes;
using TabScore2.Globals;

namespace TabScore2.Models
{
    public class ShowMoveModel : List<Move>
    {
        public int TabletDeviceNumber { get; set; }
        public Direction Direction { get; set; }
        public int NewRoundNumber { get; set; }
        public int LowBoard { get; set; }
        public int HighBoard { get;  set; }
        public int BoardsNewTable { get; set; }
        public int TabletDevicesPerTable { get; set; }
        public int TableNotReadyNumber { get; set; }
    }
}