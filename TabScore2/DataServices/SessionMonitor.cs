// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts.SharedClasses;
using TabScore2.Classes;

namespace TabScore2.DataServices
{
    public class SessionMonitor(IDatabase iDatabase, IAppData iAppData, ISettings iSettings) : ISessionMonitor
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly ISettings settings = iSettings;

        // The layout needs one database call per table for names (about 13 calls for a 12-table session), so it is
        // cached briefly.  This keeps the scorer's auto-refreshing web page and the admin form's timer cheap, while
        // still picking up newly entered names within half a minute
        private static readonly TimeSpan LayoutMaxAge = TimeSpan.FromSeconds(30);
        private readonly object layoutSync = new();
        private SessionLayout? cachedLayout;

        public SessionCheck GetStartOfPlayCheck(bool reloadLayout = false)
        {
            SessionLayout layout = GetLayout(reloadLayout);
            return SessionCheckEvaluator.Evaluate(layout, appData.GetDeviceStatusSnapshot(), DateTime.Now);
        }

        private SessionLayout GetLayout(bool forceReload)
        {
            lock (layoutSync)
            {
                if (!forceReload && cachedLayout != null && DateTime.Now - cachedLayout.LoadedAt < LayoutMaxAge)
                {
                    return cachedLayout;
                }
            }

            // Load outside the lock.  Two concurrent reloads are harmless; the later one simply replaces the earlier one
            SessionLayout freshLayout = LoadLayout();
            if (freshLayout.ErrorMessage == null)  // Don't cache failures, so the next request tries again
            {
                lock (layoutSync)
                {
                    cachedLayout = freshLayout;
                }
            }
            return freshLayout;
        }

        private SessionLayout LoadLayout()
        {
            if (!settings.DatabaseReady)
            {
                return new SessionLayout(DateTime.Now, [], "MonitorDatabaseNotReady");
            }

            try
            {
                bool isIndividual = settings.IsIndividual;
                List<LayoutTable> tables = [];
                foreach (Section section in database.GetSectionsList())
                {
                    // Same rule the web app uses to decide between one device per table and devices that move
                    bool devicesMove = settings.DevicesMove && section.Winners == 1;

                    foreach (Round round in database.GetRoundsList(section.SectionId, 1).OrderBy(x => x.TableNumber))
                    {
                        if (round.TableNumber == 0) continue;  // Not a physical table

                        int numberNorth = PresentNumber(round.NumberNorth, section.MissingPair);
                        int numberEast = PresentNumber(round.NumberEast, section.MissingPair);
                        // For pairs, GetRoundsList doesn't set South/West; the pair number applies to both players
                        int numberSouth = isIndividual ? PresentNumber(round.NumberSouth, section.MissingPair) : numberNorth;
                        int numberWest = isIndividual ? PresentNumber(round.NumberWest, section.MissingPair) : numberEast;

                        // Absent sides are passed as 0, so they are not looked up (a missing pair has no names by definition)
                        Names names = database.GetNamesForRound(section.SectionId, 1, numberNorth, numberEast, numberSouth, numberWest);

                        tables.Add(new LayoutTable(
                            section.SectionId,
                            section.SectionLetter,
                            round.TableNumber,
                            devicesMove,
                            isIndividual,
                            numberNorth,
                            numberEast,
                            numberSouth,
                            numberWest,
                            names.NameNorth ?? string.Empty,
                            names.NameEast ?? string.Empty,
                            names.NameSouth ?? string.Empty,
                            names.NameWest ?? string.Empty));
                    }
                }
                return new SessionLayout(DateTime.Now, tables, null);
            }
            catch (Exception)
            {
                // gRPC server unavailable, database locked, etc.  Report it rather than crash the admin form or web page
                return new SessionLayout(DateTime.Now, [], "MonitorDatabaseError");
            }
        }

        // Same test as ShowRoundInfoController: a number of 0, or the section's missing pair, means that side is absent
        private static int PresentNumber(int number, int missingPair)
        {
            return (number == 0 || number == missingPair) ? 0 : number;
        }
    }
}
