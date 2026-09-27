// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Globals;

namespace TabScore2.Classes
{
    // The start-of-play comparison, kept free of any database or AppData access so it can be unit tested directly
    public static class SessionCheckEvaluator
    {
        public static SessionCheck Evaluate(SessionLayout layout, IReadOnlyList<DeviceSnapshot> devices, DateTime checkedAt)
        {
            if (layout.ErrorMessage != null)
            {
                return new SessionCheck(layout.LoadedAt, checkedAt, [], [], layout.ErrorMessage);
            }

            HashSet<(int SectionId, int TableNumber, Direction Direction)> registered =
                devices.Select(x => (x.SectionId, x.TableNumber, x.Direction)).ToHashSet();
            HashSet<(int SectionId, int TableNumber, Direction Direction)> expected = [];

            List<TableCheckRow> rows = [];
            foreach (LayoutTable table in layout.Tables)
            {
                List<Direction> missingDevices = [];
                foreach (Direction direction in table.ExpectedDevices)
                {
                    expected.Add((table.SectionId, table.TableNumber, direction));
                    if (!registered.Contains((table.SectionId, table.TableNumber, direction)))
                    {
                        missingDevices.Add(direction);
                    }
                }
                rows.Add(new TableCheckRow(
                    table,
                    missingDevices,
                    GetNamesState(table.NorthSouthAbsent, table.NameNorth, table.NameSouth),
                    GetNamesState(table.EastWestAbsent, table.NameEast, table.NameWest)));
            }

            // Devices registered somewhere the round 1 movement doesn't expect one, eg the pair at a sit-out table
            // registering as the absent side.  Devices at table 0 (sitting out with no table) are not flagged
            List<DeviceSnapshot> unexpectedDevices = devices
                .Where(x => x.TableNumber != 0 && !expected.Contains((x.SectionId, x.TableNumber, x.Direction)))
                .ToList();

            return new SessionCheck(layout.LoadedAt, checkedAt, rows, unexpectedDevices, null);
        }

        // An empty name means no ID has been entered (or the ID was entered as 'Unknown').
        // An ID with no name in the names database comes back as '#number', which counts as entered
        private static NamesState GetNamesState(bool sideAbsent, string name1, string name2)
        {
            if (sideAbsent) return NamesState.NotApplicable;
            return (name1 != string.Empty && name2 != string.Empty) ? NamesState.Complete : NamesState.Missing;
        }
    }
}
