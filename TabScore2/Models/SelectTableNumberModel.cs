// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts.SharedClasses;

namespace TabScore2.Models
{
    public class SelectTableNumberModel(Section section, int tableNumber, bool confirm)
    {
        public int SectionId { get; set; } = section.SectionId;
        public int TableNumber { get; set; } = tableNumber;
        public int NumTables { get; set; } = section.NumberOfTables;
        public bool Confirm { get; set; } = confirm;
    }
}