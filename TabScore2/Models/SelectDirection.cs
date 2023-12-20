// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Classes;
using TabScore2.Globals;
namespace TabScore.Models
{
    public class SelectDirection(TableStatus tableStatus, Section section, Direction direction, bool confirm)
    {
        public int SectionID { get; set; } = tableStatus.SectionID;
        public int TableNumber { get; set; } = tableStatus.TableNumber;
        public Direction Direction { get; set; } = direction;
        public int RoundNumber { get; set; } = tableStatus.RoundNumber;
        public bool NorthSouthMissing { get; set; } = tableStatus.RoundData.NumberNorth == 0 || tableStatus.RoundData.NumberNorth == section.MissingPair;
        public bool EastWestMissing { get; set; } = tableStatus.RoundData.NumberEast == 0 || tableStatus.RoundData.NumberEast == section.MissingPair;
        public bool Confirm { get; set; } = confirm;
    }
}