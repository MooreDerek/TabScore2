// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Models
{
    public class EnterHandRecordModel(int deviceNumber, int sectionID, int boardNumber)
    {
        public int TabletDeviceNumber { get; private set; } = deviceNumber;
        public int SectionID { get; private set; } = sectionID;
        public int BoardNumber { get; private set; } = boardNumber;
    }
}