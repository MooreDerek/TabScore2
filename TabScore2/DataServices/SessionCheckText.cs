// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.Extensions.Localization;
using TabScore2.Classes;
using TabScore2.Globals;
using TabScore2.Resources;

namespace TabScore2.DataServices
{
    // Display text for a SessionCheck.  Shared by the admin form and the scorer's web page so that both say the same thing
    public static class SessionCheckText
    {
        private const string Absent = "–";

        public static string Summary(SessionCheck check, IStringLocalizer<Strings> localizer)
        {
            if (check.ErrorMessage != null) return localizer[check.ErrorMessage];
            if (check.Rows.Count == 0) return localizer["MonitorNoTables"];
            int count = check.TablesNeedingAttention;
            if (count == 0) return localizer["MonitorAllReady"];
            return localizer["MonitorTablesNeedAttention", count];
        }

        public static string PairNumbers(TableCheckRow row, bool northSouth)
        {
            LayoutTable table = row.Table;
            if (northSouth)
            {
                if (table.NorthSouthAbsent) return Absent;
                return table.IsIndividual ? $"{table.NumberNorth} & {table.NumberSouth}" : table.NumberNorth.ToString();
            }
            if (table.EastWestAbsent) return Absent;
            return table.IsIndividual ? $"{table.NumberEast} & {table.NumberWest}" : table.NumberEast.ToString();
        }

        public static string Registration(TableCheckRow row, IStringLocalizer<Strings> localizer)
        {
            LayoutTable table = row.Table;
            if (row.AllDevicesRegistered)
            {
                return table.IsSitoutTable && !table.DevicesMove ? localizer["MonitorRegisteredSitout"] : localizer["MonitorRegistered"];
            }
            if (!table.DevicesMove)
            {
                if (table.IsSitoutTable)
                {
                    string presentPair = table.NorthSouthAbsent ? localizer["MonitorEW"] : localizer["MonitorNS"];
                    return localizer["MonitorSitoutNotRegistered", presentPair];
                }
                return localizer["MonitorNotRegistered"];
            }
            string directions = string.Join(", ", row.MissingDevices.Select(x => DirectionLetter(x, localizer)));
            return localizer["MonitorNotRegisteredDirections", directions];
        }

        public static string Names(TableCheckRow row, bool northSouth, IStringLocalizer<Strings> localizer)
        {
            LayoutTable table = row.Table;
            NamesState state = northSouth ? row.NorthSouthNames : row.EastWestNames;
            if (state == NamesState.NotApplicable) return Absent;
            string name1 = northSouth ? table.NameNorth : table.NameEast;
            string name2 = northSouth ? table.NameSouth : table.NameWest;
            if (name1 == string.Empty && name2 == string.Empty) return localizer["MonitorNoIds"];
            return $"{(name1 == string.Empty ? "?" : name1)} & {(name2 == string.Empty ? "?" : name2)}";
        }

        public static string Unexpected(SessionCheck check, IStringLocalizer<Strings> localizer)
        {
            if (check.UnexpectedDevices.Count == 0) return string.Empty;
            string locations = string.Join(", ", check.UnexpectedDevices.Select(x =>
                string.IsNullOrEmpty(x.Location) ? $"{x.SectionLetter}{x.TableNumber}" : x.Location));
            return localizer["MonitorUnexpected", locations];
        }

        private static string DirectionLetter(Direction direction, IStringLocalizer<Strings> localizer)
        {
            return direction switch
            {
                Direction.North => localizer["N"],
                Direction.South => localizer["S"],
                Direction.East => localizer["E"],
                Direction.West => localizer["W"],
                _ => direction.ToString()
            };
        }
    }
}
