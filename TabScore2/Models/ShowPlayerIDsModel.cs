// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Classes;

namespace TabScore2.Models
{
    public class ShowPlayerIDsModel(int deviceNumber, bool showWarning) : List<PlayerEntry>
    {
        public int TabletDeviceNumber { get; set; } = deviceNumber;
        public int NumberOfBlankEntries { get; set; } = 0;
        public bool ShowWarning { get; set; } = showWarning;
        public bool ShowMessage { get; set; } = false;
    }
}