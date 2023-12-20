// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using System.Text;

namespace TabScore2.Classes
{
    public class Hand
    {
        public int SectionID { get; set; } = 1;  // Default SectionID=1 if hands apply to more than one section
        public int BoardNumber { get; set; } = 0;
        public string NorthSpades { get; set; } = "###";  // Indicates no hand record available
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

        private string pbn = string.Empty;
        public string PBN
        {
            get
            {
                if (pbn != string.Empty)
                {
                    return pbn;
                }
                StringBuilder pbnString = new();
                switch ((BoardNumber - 1) % 4)
                {
                    case 0:
                        pbnString.Append("N:");
                        pbnString.Append(NorthSpades);
                        pbnString.Append('.');
                        pbnString.Append(NorthHearts);
                        pbnString.Append('.');
                        pbnString.Append(NorthDiamonds);
                        pbnString.Append('.');
                        pbnString.Append(NorthClubs);
                        pbnString.Append(' ');
                        pbnString.Append(EastSpades);
                        pbnString.Append('.');
                        pbnString.Append(EastHearts);
                        pbnString.Append('.');
                        pbnString.Append(EastDiamonds);
                        pbnString.Append('.');
                        pbnString.Append(EastClubs);
                        pbnString.Append(' ');
                        pbnString.Append(SouthSpades);
                        pbnString.Append('.');
                        pbnString.Append(SouthHearts);
                        pbnString.Append('.');
                        pbnString.Append(SouthDiamonds);
                        pbnString.Append('.');
                        pbnString.Append(SouthClubs);
                        pbnString.Append(' ');
                        pbnString.Append(WestSpades);
                        pbnString.Append('.');
                        pbnString.Append(WestHearts);
                        pbnString.Append('.');
                        pbnString.Append(WestDiamonds);
                        pbnString.Append('.');
                        pbnString.Append(WestClubs);
                        break;
                    case 1:
                        pbnString.Append("E:");
                        pbnString.Append(EastSpades);
                        pbnString.Append('.');
                        pbnString.Append(EastHearts);
                        pbnString.Append('.');
                        pbnString.Append(EastDiamonds);
                        pbnString.Append('.');
                        pbnString.Append(EastClubs);
                        pbnString.Append(' ');
                        pbnString.Append(SouthSpades);
                        pbnString.Append('.');
                        pbnString.Append(SouthHearts);
                        pbnString.Append('.');
                        pbnString.Append(SouthDiamonds);
                        pbnString.Append('.');
                        pbnString.Append(SouthClubs);
                        pbnString.Append(' ');
                        pbnString.Append(WestSpades);
                        pbnString.Append('.');
                        pbnString.Append(WestHearts);
                        pbnString.Append('.');
                        pbnString.Append(WestDiamonds);
                        pbnString.Append('.');
                        pbnString.Append(WestClubs);
                        pbnString.Append(' ');
                        pbnString.Append(NorthSpades);
                        pbnString.Append('.');
                        pbnString.Append(NorthHearts);
                        pbnString.Append('.');
                        pbnString.Append(NorthDiamonds);
                        pbnString.Append('.');
                        pbnString.Append(NorthClubs);
                        break;
                    case 2:
                        pbnString.Append("S:");
                        pbnString.Append(SouthSpades);
                        pbnString.Append('.');
                        pbnString.Append(SouthHearts);
                        pbnString.Append('.');
                        pbnString.Append(SouthDiamonds);
                        pbnString.Append('.');
                        pbnString.Append(SouthClubs);
                        pbnString.Append(' ');
                        pbnString.Append(WestSpades);
                        pbnString.Append('.');
                        pbnString.Append(WestHearts);
                        pbnString.Append('.');
                        pbnString.Append(WestDiamonds);
                        pbnString.Append('.');
                        pbnString.Append(WestClubs);
                        pbnString.Append(' ');
                        pbnString.Append(NorthSpades);
                        pbnString.Append('.');
                        pbnString.Append(NorthHearts);
                        pbnString.Append('.');
                        pbnString.Append(NorthDiamonds);
                        pbnString.Append('.');
                        pbnString.Append(NorthClubs);
                        pbnString.Append(' ');
                        pbnString.Append(EastSpades);
                        pbnString.Append('.');
                        pbnString.Append(EastHearts);
                        pbnString.Append('.');
                        pbnString.Append(EastDiamonds);
                        pbnString.Append('.');
                        pbnString.Append(EastClubs);
                        break;
                    case 3:
                        pbnString.Append("W:");
                        pbnString.Append(WestSpades);
                        pbnString.Append('.');
                        pbnString.Append(WestHearts);
                        pbnString.Append('.');
                        pbnString.Append(WestDiamonds);
                        pbnString.Append('.');
                        pbnString.Append(WestClubs);
                        pbnString.Append(' ');
                        pbnString.Append(NorthSpades);
                        pbnString.Append('.');
                        pbnString.Append(NorthHearts);
                        pbnString.Append('.');
                        pbnString.Append(NorthDiamonds);
                        pbnString.Append('.');
                        pbnString.Append(NorthClubs);
                        pbnString.Append(' ');
                        pbnString.Append(EastSpades);
                        pbnString.Append('.');
                        pbnString.Append(EastHearts);
                        pbnString.Append('.');
                        pbnString.Append(EastDiamonds);
                        pbnString.Append('.');
                        pbnString.Append(EastClubs);
                        pbnString.Append(' ');
                        pbnString.Append(SouthSpades);
                        pbnString.Append('.');
                        pbnString.Append(SouthHearts);
                        pbnString.Append('.');
                        pbnString.Append(SouthDiamonds);
                        pbnString.Append('.');
                        pbnString.Append(SouthClubs);
                        break;
                }
                pbn = pbnString.ToString();
                return pbn;
            }

            set
            {
                pbn = value;
                char[] pbnDelimiter = [':', '.', ' '];
                string[] pbnArray = pbn.Split(pbnDelimiter);
                switch (pbnArray[0])
                {
                    case "N":
                        NorthSpades = pbnArray[1];
                        NorthHearts = pbnArray[2];
                        NorthDiamonds = pbnArray[3];
                        NorthClubs = pbnArray[4];
                        EastSpades = pbnArray[5];
                        EastHearts = pbnArray[6];
                        EastDiamonds = pbnArray[7];
                        EastClubs = pbnArray[8];
                        SouthSpades = pbnArray[9];
                        SouthHearts = pbnArray[10];
                        SouthDiamonds = pbnArray[11];
                        SouthClubs = pbnArray[12];
                        WestSpades = pbnArray[13];
                        WestHearts = pbnArray[14];
                        WestDiamonds = pbnArray[15];
                        WestClubs = pbnArray[16];
                        break;
                    case "E":
                        EastSpades = pbnArray[1];
                        EastHearts = pbnArray[2];
                        EastDiamonds = pbnArray[3];
                        EastClubs = pbnArray[4];
                        SouthSpades = pbnArray[5];
                        SouthHearts = pbnArray[6];
                        SouthDiamonds = pbnArray[7];
                        SouthClubs = pbnArray[8];
                        WestSpades = pbnArray[9];
                        WestHearts = pbnArray[10];
                        WestDiamonds = pbnArray[11];
                        WestClubs = pbnArray[12];
                        NorthSpades = pbnArray[13];
                        NorthHearts = pbnArray[14];
                        NorthDiamonds = pbnArray[15];
                        NorthClubs = pbnArray[16];
                        break;
                    case "S":
                        SouthSpades = pbnArray[1];
                        SouthHearts = pbnArray[2];
                        SouthDiamonds = pbnArray[3];
                        SouthClubs = pbnArray[4];
                        WestSpades = pbnArray[5];
                        WestHearts = pbnArray[6];
                        WestDiamonds = pbnArray[7];
                        WestClubs = pbnArray[8];
                        NorthSpades = pbnArray[9];
                        NorthHearts = pbnArray[10];
                        NorthDiamonds = pbnArray[11];
                        NorthClubs = pbnArray[12];
                        EastSpades = pbnArray[13];
                        EastHearts = pbnArray[14];
                        EastDiamonds = pbnArray[15];
                        EastClubs = pbnArray[16];
                        break;
                    case "W":
                        WestSpades = pbnArray[1];
                        WestHearts = pbnArray[2];
                        WestDiamonds = pbnArray[3];
                        WestClubs = pbnArray[4];
                        NorthSpades = pbnArray[5];
                        NorthHearts = pbnArray[6];
                        NorthDiamonds = pbnArray[7];
                        NorthClubs = pbnArray[8];
                        EastSpades = pbnArray[9];
                        EastHearts = pbnArray[10];
                        EastDiamonds = pbnArray[11];
                        EastClubs = pbnArray[12];
                        SouthSpades = pbnArray[13];
                        SouthHearts = pbnArray[14];
                        SouthDiamonds = pbnArray[15];
                        SouthClubs = pbnArray[16];
                        break;
                }
            }
        }
    }
}