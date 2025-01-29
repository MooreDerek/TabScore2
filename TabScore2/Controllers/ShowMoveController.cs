// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.SharedClasses;
using TabScore2.UtilityServices;

namespace TabScore2.Controllers
{
    public class ShowMoveController(IDatabase iDatabase, IUtilities iUtilities, IAppData iAppData, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IUtilities utilities = iUtilities;
        private readonly IAppData appData = iAppData;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(int newRoundNumber, int tableNotReadyNumber = -1)
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            if (newRoundNumber > database.GetNumberOfRoundsInSection(deviceStatus.SectionID))  // Session complete
            {
                if (settings.ShowRanking == 2)
                {
                    return RedirectToAction("Final", "ShowRankingList");
                }
                else
                {
                    return RedirectToAction("Index", "EndScreen");
                }
            }

            if (deviceStatus.TableNumber != 0)  // Not at phantom table, so update table status
            {
                TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
                if (tableStatus.RoundNumber < newRoundNumber)
                {
                    // No tablet device has yet advanced this table to the next round, so show that this one is ready to do so
                    switch (deviceStatus.Direction)
                    {
                        case Direction.North:
                            tableStatus.ReadyForNextRoundNorth = true;
                            break;
                        case Direction.South:
                            tableStatus.ReadyForNextRoundSouth = true;
                            break;
                        case Direction.East:
                            tableStatus.ReadyForNextRoundEast = true;
                            break;
                        case Direction.West:
                            tableStatus.ReadyForNextRoundWest = true;
                            break;
                    }
                }
            }

            ShowMoveModel showMoveModel = utilities.CreateShowMoveModel(deviceNumber, newRoundNumber, tableNotReadyNumber);

            ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceNumber);
            ViewData["Title"] = utilities.Title("ShowMove", TitleType.Location, deviceNumber);
            ViewData["Header"] = utilities.Header(HeaderType.Location, deviceNumber);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;

            return View(showMoveModel);
        }

        public ActionResult OKButtonClick(int newRoundNumber)
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            Section section = database.GetSection(deviceStatus.SectionID);
            if (section.DevicesPerTable > 1)  // Tablet devices are moving, so need to check if new table is ready
            {
                // Get the move for this tablet device
                List<Round> roundsList = database.GetRoundsList(deviceStatus.SectionID, newRoundNumber);
                Move move = utilities.GetMove(roundsList, deviceStatus.TableNumber, deviceStatus.PairNumber, deviceStatus.Direction);

                if (move.NewTableNumber == 0)  // Move is to phantom table, so update and go straight to RoundInfo
                {
                    appData.UpdateDeviceStatus(deviceNumber, 0, newRoundNumber, Direction.Sitout);
                    HttpContext.Session.SetInt32("TableNumber", 0);
                    HttpContext.Session.SetString("Direction", Direction.Sitout.ToString());
                    return RedirectToAction("Index", "ShowRoundInfo");
                }

                // Check if the new table (the one we're trying to move to) is ready.  Expanded code here to make it easier to understand
                bool newTableReady;
                TableStatus newTableStatus = appData.GetTableStatus(section.ID, move.NewTableNumber);
                if (newTableStatus.RoundNumber == newRoundNumber)
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
                    if (section.DevicesPerTable == 2 && newTableStatus.ReadyForNextRoundNorth && newTableStatus.ReadyForNextRoundEast)
                    {
                        newTableReady = true;
                    }
                    else if (section.DevicesPerTable == 4 && newTableStatus.ReadyForNextRoundNorth && newTableStatus.ReadyForNextRoundSouth && newTableStatus.ReadyForNextRoundEast && newTableStatus.ReadyForNextRoundWest)
                    {
                        newTableReady = true;
                    }
                    else
                    {
                        newTableReady = false;
                    }
                }

                if (newTableReady)  // Reset tablet device and table statuses for new round, and update session state
                {
                    appData.UpdateDeviceStatus(deviceNumber, move.NewTableNumber, newRoundNumber, move.NewDirection);
                    appData.UpdateTableStatus(section.ID, move.NewTableNumber, newRoundNumber);
                    HttpContext.Session.SetInt32("TableNumber", move.NewTableNumber);
                    HttpContext.Session.SetString("Direction", move.NewDirection.ToString());
                }
                else  // Go back and wait
                {
                    return RedirectToAction("Index", "ShowMove", new { newRoundNumber, tableNotReadyNumber = move.NewTableNumber });
                }
            }
            else  // Tablet device not moving and is the only tablet device at this table
            {
                deviceStatus.RoundNumber = newRoundNumber;
                appData.UpdateTableStatus(section.ID, deviceStatus.TableNumber, newRoundNumber);
            }

            // Refresh settings for the start of the round.  Only done once per round.
            database.GetDatabaseSettings(section.ID, newRoundNumber);
            return RedirectToAction("Index", "ShowPlayerIDs");
        }
    }
}