// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.SharedClasses;

namespace TabScore2.Models
{
    public class SelectTableNumberModel(Section section, int tableNumber, bool confirm)
    {
        public int SectionID { get; set; } = section.ID;
        public int TableNumber { get; set; } = tableNumber;
        public int NumTables { get; set; } = section.Tables;
        public bool Confirm { get; set; } = confirm;
    }
}