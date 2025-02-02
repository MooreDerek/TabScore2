// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Classes
{
    public class RoundTimer
    {
        public int SectionId { get; set; }
        public int RoundNumber { get; set; }
        public DateTime StartTime { get; set; }
        public int SecondsPerRound { get; set; }
    }
}
