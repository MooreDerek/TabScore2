// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Globals
{
    public static class Global
    {
        private static readonly bool[] NSVulnerability = [false, true, false, true, true, false, true, false, false, true, false, true, true, false, true, false];
        private static readonly bool[] EWVulnerability = [false, false, true, true, false, true, true, false, true, true, false, false, true, false, false, true];

        public static bool IsNSVulnerable(int boardNumber)
        {
            return NSVulnerability[(boardNumber - 1) % 16];
        }

        public static bool IsEWVulnerable(int boardNumber)
        {
            return EWVulnerability[(boardNumber - 1) % 16];
        }
    }
}
