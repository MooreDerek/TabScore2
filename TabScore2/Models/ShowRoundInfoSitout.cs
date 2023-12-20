// TabScore, a wireless bridge scoring program.  Copyright(C) 2023 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore.Models
{
    public class ShowRoundInfoSitout()
    {
        public int TabletDeviceNumber { get; private set; }
        public int PairNumber { get; private set; }
        public int RoundNumber { get; private set; }
        public int TabletDevicesPerTable { get; private set; }

        public ShowRoundInfoSitout(int tabletDeviceNumber)
        {
            TabletDeviceNumber = tabletDeviceNumber;
            TabletDeviceStatus tabletDeviceStatus = AppData.TabletDeviceStatusList[tabletDeviceNumber];
            TabletDevicesPerTable = AppData.SectionsList.Find(x => x.SectionID == tabletDeviceStatus.SectionID).TabletDevicesPerTable;
            PairNumber = tabletDeviceStatus.PairNumber;
            RoundNumber = tabletDeviceStatus.RoundNumber;
        }
    }
}