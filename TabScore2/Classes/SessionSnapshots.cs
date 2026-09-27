// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Globals;

namespace TabScore2.Classes
{
    // Immutable, point-in-time copies of AppData state for read-only consumers (admin form, scorer web page).
    // They are copied under AppData's lock, so consumers never hold references to objects that request threads are mutating.

    public sealed record TableSnapshot(
        int SectionId,
        int TableNumber,
        int RoundNumber,
        int NumberNorth,
        int NumberEast,
        int NumberSouth,
        int NumberWest,
        int LowBoard,
        int HighBoard,
        bool ReadyNorth,
        bool ReadySouth,
        bool ReadyEast,
        bool ReadyWest);

    public sealed record DeviceSnapshot(
        int DeviceNumber,
        int SectionId,
        string SectionLetter,
        int TableNumber,
        Direction Direction,
        int PairNumber,
        int RoundNumber,
        int DevicesPerTable,
        bool AtSitoutTable,
        string? Location);
}
