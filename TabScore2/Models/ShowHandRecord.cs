// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Globals;

namespace TabScore2.Models
{
    public class ShowHandRecord(int deviceNumber, int boardNumber, string dealer)
    {
        public int TabletDeviceNumber { get; set; } = deviceNumber;
        public int BoardNumber { get; set; } = boardNumber;
        public string Dealer { get; set; } = dealer;
        public bool FromView { get; set; } = false;
        public Direction PerspectiveDirection { get; set; }
        public HandRecordPerspectiveButtonOptions PerspectiveButtonOption { get; set; }

        public string NorthSpades { get; set; } = "###";
        public string NorthHearts { get; set; } = string.Empty;
        public string NorthDiamonds { get; set; } = string.Empty;
        public string NorthClubs { get; set; } = string.Empty;
        public string EastSpades { get; set; } = string.Empty;
        public string EastHearts { get; set; } = string.Empty;
        public string EastDiamonds { get; set; } = string.Empty;
        public string EastClubs { get; set; } = string.Empty;
        public string SouthSpades { get; set; } = string.Empty;
        public string SouthHearts { get; set; } = string.Empty;
        public string SouthDiamonds { get; set; } = string.Empty;
        public string SouthClubs { get; set; } = string.Empty;
        public string WestSpades { get; set; } = string.Empty;
        public string WestHearts { get; set; } = string.Empty;
        public string WestDiamonds { get; set; } = string.Empty;
        public string WestClubs { get; set; } = string.Empty;

        public int EvalNorthNT { get; set; } = -1;
        public int EvalNorthSpades { get; set; }
        public int EvalNorthHearts { get; set; }
        public int EvalNorthDiamonds { get; set; }
        public int EvalNorthClubs { get; set; }
        public int EvalEastNT { get; set; }
        public int EvalEastSpades { get; set; }
        public int EvalEastHearts { get; set; }
        public int EvalEastDiamonds { get; set; }
        public int EvalEastClubs { get; set; }
        public int EvalSouthNT { get; set; }
        public int EvalSouthSpades { get; set; }
        public int EvalSouthHearts { get; set; }
        public int EvalSouthDiamonds { get; set; }
        public int EvalSouthClubs { get; set; }
        public int EvalWestSpades { get; set; }
        public int EvalWestNT { get; set; }
        public int EvalWestHearts { get; set; }
        public int EvalWestDiamonds { get; set; }
        public int EvalWestClubs { get; set; }

        public int HCPNorth { get; set; }
        public int HCPSouth { get; set; }
        public int HCPEast { get; set; }
        public int HCPWest { get; set; }
    }
}