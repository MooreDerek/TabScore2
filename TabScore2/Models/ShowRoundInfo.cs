// TabScore, a wireless bridge scoring program.  Copyright(C) 2023 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Classes;

namespace TabScore.Models
{
    public class ShowRoundInfo(int tabletDeviceNumber, int roundNumber, Round roundData)
    {
        public int TabletDeviceNumber { get; private set; } = tabletDeviceNumber;
        public int RoundNumber { get; private set; } = roundNumber;
        public Round RoundData { get; private set; } = roundData;
        public bool NSMissing { get; set; } = false;
        public bool EWMissing { get; set; } = false;
        public int BoardsFromTable { get; set; } = -1;
    }
}