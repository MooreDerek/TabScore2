// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Globals;

namespace TabScore2.Models
{
    public class ShowHandRecord(int tabletDeviceNumber, string dealer)
    {
        public int TabletDeviceNumber = tabletDeviceNumber;
        public string Dealer = dealer;
        public bool FromView = false;
        public Direction PerspectiveDirection;
        public HandRecordPerspectiveButtonOptions PerspectiveButtonOption;

        public string NorthSpades = "###";
        public string NorthHearts = string.Empty;
        public string NorthDiamonds = string.Empty;
        public string NorthClubs = string.Empty;
        public string EastSpades = string.Empty;
        public string EastHearts = string.Empty;
        public string EastDiamonds = string.Empty;
        public string EastClubs = string.Empty;
        public string SouthSpades = string.Empty;
        public string SouthHearts = string.Empty;
        public string SouthDiamonds = string.Empty;
        public string SouthClubs = string.Empty;
        public string WestSpades = string.Empty;
        public string WestHearts = string.Empty;
        public string WestDiamonds = string.Empty;
        public string WestClubs = string.Empty;

        public int EvalNorthNT = -1;
        public int EvalNorthSpades;
        public int EvalNorthHearts;
        public int EvalNorthDiamonds;
        public int EvalNorthClubs;
        public int EvalEastNT;
        public int EvalEastSpades;
        public int EvalEastHearts;
        public int EvalEastDiamonds;
        public int EvalEastClubs;
        public int EvalSouthNT;
        public int EvalSouthSpades;
        public int EvalSouthHearts;
        public int EvalSouthDiamonds;
        public int EvalSouthClubs;
        public int EvalWestSpades;
        public int EvalWestNT;
        public int EvalWestHearts;
        public int EvalWestDiamonds;
        public int EvalWestClubs;

        public int HCPNorth;
        public int HCPSouth;
        public int HCPEast;
        public int HCPWest;
    }
}