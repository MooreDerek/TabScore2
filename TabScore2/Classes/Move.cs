// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Globals;

namespace TabScore2.Classes
{
    public class Move(int pairNumber, Direction direction)
    {
        public int NewTableNumber { get; set; }
        public Direction Direction { get; set; } = direction;
        public Direction NewDirection { get; set; }
        public bool Stay { get; set; }
        public bool NewTableIsSitout { get; set; } = false;
        public int PairNumber { get; set; } = pairNumber;
    }
}
