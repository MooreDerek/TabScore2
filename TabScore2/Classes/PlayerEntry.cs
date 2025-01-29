// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Globals;

namespace TabScore2.Classes
{
    public class PlayerEntry(string displayName, int number, Direction direction)
    {
        public string DisplayName { get; private set; } = displayName;
        public int Number { get; private set; } = number;
        public Direction Direction { get; private set; } = direction;
    }
}