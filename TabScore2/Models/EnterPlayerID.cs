// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Globals;

namespace TabScore2.Models
{
    public class EnterPlayerID(int tabletDeviceNumber, Direction direction)
    {
        public int TabletDeviceNumber { get; set; } = tabletDeviceNumber;
        public Direction Direction { get; set; } = direction;
    }
}
