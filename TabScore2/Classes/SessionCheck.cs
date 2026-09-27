// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Globals;

namespace TabScore2.Classes
{
    // Start-of-play layout for one table, built from round 1 of the movement plus the player names held in the database.
    // A pair/player number is 0 where that side is absent (0 in RoundData, or equal to Section.MissingPair).
    // Names are as returned by GetNamesForRound: empty means no ID entered, or the ID was entered as 'Unknown'.
    public sealed record LayoutTable(
        int SectionId,
        string SectionLetter,
        int TableNumber,
        bool DevicesMove,
        bool IsIndividual,
        int NumberNorth,
        int NumberEast,
        int NumberSouth,
        int NumberWest,
        string NameNorth,
        string NameEast,
        string NameSouth,
        string NameWest)
    {
        public bool NorthSouthAbsent => NumberNorth == 0;
        public bool EastWestAbsent => NumberEast == 0;
        public bool IsSitoutTable => NorthSouthAbsent || EastWestAbsent;

        // The device registrations this table needs at the start of play.  Keys match AppData.DeviceStatusExists
        public IReadOnlyList<Direction> ExpectedDevices
        {
            get
            {
                if (!DevicesMove)
                {
                    // One device per table, always registered as North.  This includes a sit-out table: the pair that
                    // is present (normally E/W) must register there to log the table on and enter their player IDs,
                    // even though the device then leaves because no scoring is done at that table
                    return [Direction.North];
                }
                List<Direction> expected = [];
                if (!NorthSouthAbsent)
                {
                    expected.Add(Direction.North);
                    if (IsIndividual) expected.Add(Direction.South);
                }
                if (!EastWestAbsent)
                {
                    expected.Add(Direction.East);
                    if (IsIndividual) expected.Add(Direction.West);
                }
                return expected;
            }
        }
    }

    public sealed record SessionLayout(DateTime LoadedAt, IReadOnlyList<LayoutTable> Tables, string? ErrorMessage);

    public enum NamesState
    {
        NotApplicable,  // This side is absent (sit-out)
        Complete,
        Missing
    }

    public sealed record TableCheckRow(
        LayoutTable Table,
        IReadOnlyList<Direction> MissingDevices,
        NamesState NorthSouthNames,
        NamesState EastWestNames)
    {
        public bool AllDevicesRegistered => MissingDevices.Count == 0;
        public bool NamesComplete => NorthSouthNames != NamesState.Missing && EastWestNames != NamesState.Missing;
        public bool IsOk => AllDevicesRegistered && NamesComplete;
    }

    public sealed record SessionCheck(
        DateTime LayoutLoadedAt,
        DateTime CheckedAt,
        IReadOnlyList<TableCheckRow> Rows,
        IReadOnlyList<DeviceSnapshot> UnexpectedDevices,
        string? ErrorMessage)
    {
        public int TablesNeedingAttention => Rows.Count(x => !x.IsOk);
    }
}
