// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Globals;

namespace TabScore2.Classes
{
    public class PlayerEntry(string name, int number, Direction direction)
    {
        public string Name { get; private set; } = name;
        public int Number { get; private set; } = number;
        public Direction Direction { get; private set; } = direction;
    }
}