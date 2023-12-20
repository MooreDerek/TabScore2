// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.UtilityServices;

namespace TabScore2.Controllers
{
    public class ShowMoveController(IDatabase iDatabase, IUtilities iUtilities, IAppData iAppData, ISettings iSettings, IHttpContextAccessor iHttpContextAccessor) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IUtilities utilities = iUtilities;
        private readonly IAppData appData = iAppData;
        private readonly ISettings settings = iSettings;
        private readonly IHttpContextAccessor httpContextAccessor = iHttpContextAccessor;

        public ActionResult Index(int tabletDeviceNumber, int newRoundNumber, int tableNotReadyNumber = -1)
        {
            TabletDeviceStatus tabletDeviceStatus = appData.GetTabletDeviceStatus(tabletDeviceNumber);
            if (newRoundNumber > database.GetNumberOfRoundsInEvent(tabletDeviceStatus.SectionID))  // Session complete
            {
                if (settings.ShowRanking == 2)
                {
                    return RedirectToAction("Final", "ShowRankingList", new { tabletDeviceNumber });
                }
                else
                {
                    return RedirectToAction("Index", "EndScreen", new { tabletDeviceNumber });
                }
            }

            TableStatus? tableStatus = appData.GetTableStatus(tabletDeviceNumber);
            if (tableStatus != null && tableStatus.RoundNumber < newRoundNumber)
            {
                // No tablet device has yet advanced this table to the next round, so show that this one is ready to do so
                if (tabletDeviceStatus.Direction == Direction.North)
                {
                    tableStatus.ReadyForNextRoundNorth = true;
                }
                else if (tabletDeviceStatus.Direction == Direction.East)
                {
                    tableStatus.ReadyForNextRoundEast = true;
                }
                else if (tabletDeviceStatus.Direction == Direction.South)
                {
                    tableStatus.ReadyForNextRoundSouth = true;
                }
                else if (tabletDeviceStatus.Direction == Direction.West)
                {
                    tableStatus.ReadyForNextRoundWest = true;
                }
            }

            ShowMove showMove = utilities.CreateShowMoveModel(tabletDeviceNumber, newRoundNumber, tableNotReadyNumber);

            ViewData["Title"] = utilities.Title(tabletDeviceNumber, "ShowMove", TitleType.Location);
            ViewData["Header"] = utilities.Header(tabletDeviceNumber, HeaderType.Location);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            if (settings.ShowTimer) ViewData["TimerSeconds"] = appData.GetTimerSeconds(tabletDeviceNumber);

            return View(showMove);
        }

        public ActionResult OKButtonClick(int tabletDeviceNumber, int newRoundNumber)
        {
            TabletDeviceStatus tabletDeviceStatus = appData.GetTabletDeviceStatus(tabletDeviceNumber);
            Section section = database.GetSection(tabletDeviceStatus.SectionID);
            if (section.TabletDevicesPerTable > 1)  // Tablet devices are moving, so need to check if new table is ready
            {
                // Get the move for this tablet device
                List<Round> roundsList = database.GetRoundsList(tabletDeviceStatus.SectionID, newRoundNumber);
                Move move = utilities.GetMove(roundsList, tabletDeviceStatus.TableNumber, tabletDeviceStatus.PairNumber, tabletDeviceStatus.Direction);

                if (move.NewTableNumber == 0)  // Move is to phantom table, so go straight to RoundInfo
                {
                    appData.UpdateTabletDeviceStatus(tabletDeviceNumber, 0, newRoundNumber, Direction.Sitout);
                    return RedirectToAction("Index", "ShowRoundInfo", new { tabletDeviceNumber });
                }

                // Check if the new table (the one we're trying to move to) is ready.  Expanded code here to make it easier to understand
                bool newTableReady;
                TableStatus? newTableStatus = appData.GetTableStatus(section.ID, move.NewTableNumber);
                if (newTableStatus == null)
                {
                    newTableReady = false;  // New table not yet registered (unlikely but possible)
                }
                else if (newTableStatus.RoundNumber == newRoundNumber)
                {
                    newTableReady = true;  // New table has already been advanced to next round by another tablet device, so is ready
                }
                else if (newTableStatus.RoundNumber < newRoundNumber - 1)
                {
                    newTableReady = false;  // New table hasn't yet reached the previous round (unlikely but possible)
                }
                else
                {
                    // New table is on the previous round
                    // It is ready for the move if all tablet device locations are ready.  Sitout locations were set to 'ready' previously 
                    if (section.TabletDevicesPerTable == 2 && newTableStatus.ReadyForNextRoundNorth && newTableStatus.ReadyForNextRoundEast)
                    {
                        newTableReady = true;
                    }
                    else if (section.TabletDevicesPerTable == 4 && newTableStatus.ReadyForNextRoundNorth && newTableStatus.ReadyForNextRoundSouth && newTableStatus.ReadyForNextRoundEast && newTableStatus.ReadyForNextRoundWest)
                    {
                        newTableReady = true;
                    }
                    else
                    {
                        newTableReady = false;
                    }
                }

                if (newTableReady)  // Reset tablet device and table statuses for new round, and update cookie
                {
                    appData.UpdateTabletDeviceStatus(tabletDeviceNumber, move.NewTableNumber, newRoundNumber, move.NewDirection);

                    appData.UpdateTableStatus(section.ID, move.NewTableNumber, newRoundNumber);
                    SetCookie(section.ID, move.NewTableNumber, move.NewDirection);
                }
                else  // Go back and wait
                {
                    return RedirectToAction("Index", "ShowMove", new { tabletDeviceNumber, newRoundNumber, tableNotReadyNumber = move.NewTableNumber });
                }
            }
            else  // Tablet device not moving and is the only tablet device at this table
            {
                tabletDeviceStatus.RoundNumber = newRoundNumber;
                appData.UpdateTableStatus(section.ID, tabletDeviceStatus.TableNumber, newRoundNumber);
            }

            // Refresh settings for the start of the round.  Only done once per round.
            settings.DatabaseRefresh(newRoundNumber);
            return RedirectToAction("Index", "ShowPlayerIDs", new { tabletDeviceNumber });
        }

        // Set a cookie for this device
        private void SetCookie(int sectionID, int tableNumber, Direction direction)
        {
            HttpContext? httpContext = httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Response.Cookies.Append("sectionID", sectionID.ToString());
                httpContext.Response.Cookies.Append("tableNumber", tableNumber.ToString());
                httpContext.Response.Cookies.Append("direction", direction.ToString());
            }
        }
    }
}