// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.SharedClasses;
using TabScore2.UtilityServices;

namespace TabScore.Controllers
{
    public class SelectDirectionController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(int sectionID, int tableNumber, Direction direction = Direction.Null, bool confirm = false) 
        {
            TableStatus tableStatus = appData.GetTableStatus(sectionID, tableNumber);
            Section section = database.GetSection(sectionID);
            SelectDirectionModel selectDirectionModel = new(tableStatus, section, direction, confirm);

            ViewData["Title"] = utilities.Title("SelectDirection", TitleType.SectionTable, sectionID, tableNumber);
            ViewData["Header"] = utilities.Header(HeaderType.SectionTable, sectionID, tableNumber);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
            if (settings.IsIndividual)
            {
                return View("Individual", selectDirectionModel);
            }
            else
            {
                return View("Pair", selectDirectionModel);
            }
        }

        public ActionResult OKButtonClick(int sectionID, int tableNumber, Direction direction, int roundNumber, bool confirm)
        {
            TableStatus tableStatus = appData.GetTableStatus(sectionID, tableNumber);

            // Check if device is already registered for this location
            if (appData.DeviceStatusExists(sectionID, tableNumber, direction) && confirm)
            {
                // Ok to change to this tablet, so set session state
                HttpContext.Session.SetInt32("SectionId", sectionID);
                HttpContext.Session.SetInt32("TableNumber", tableNumber);
                HttpContext.Session.SetString("Direction", direction.ToString());
            }
            else if (appData.DeviceStatusExists(sectionID, tableNumber, direction))
            {
                // Check if section and table number matches session state - if so go back to confirm
                if (sectionID == (HttpContext.Session.GetInt32("SectionID") ?? 0) && tableNumber == (HttpContext.Session.GetInt32("TableNumber") ?? 0) && direction.ToString() == (HttpContext.Session.GetString("Direction") ?? string.Empty))
                {
                    return RedirectToAction("Index", "SelectTableNumber", new { sectionID, tableNumber, confirm = true });
                }
                // else = session state matches, so this is a re-registration and nothing more to do
            }
            else
            {
                // Not on list of registered devices, so need to add it
                int pairNumber = 0;
                if (direction == Direction.North)
                {
                    pairNumber = tableStatus.RoundData.NumberNorth;
                }
                else if (direction == Direction.East)
                {
                    pairNumber = tableStatus.RoundData.NumberEast;
                }
                else if (direction == Direction.South)
                {
                    pairNumber = tableStatus.RoundData.NumberSouth;
                }
                else if (direction == Direction.West)
                {
                    pairNumber = tableStatus.RoundData.NumberWest;
                }
                appData.AddDeviceStatus(sectionID, tableNumber, pairNumber, roundNumber, direction);
                HttpContext.Session.SetInt32("SectionId", sectionID);
                HttpContext.Session.SetInt32("TableNumber", tableNumber);
                HttpContext.Session.SetString("Direction", direction.ToString());
            }
            DeviceStatus deviceStatus = appData.GetDeviceStatus(sectionID, tableNumber, direction);

            // DeviceNumber is the key for identifying this particular tablet device and is used throughout the rest of the application
            HttpContext.Session.SetInt32("DeviceNumber", appData.GetDeviceNumber(deviceStatus));

            if (((direction == Direction.North) && tableStatus.ReadyForNextRoundNorth) || ((direction == Direction.East) && tableStatus.ReadyForNextRoundEast) || (direction == Direction.South && tableStatus.ReadyForNextRoundSouth) || (direction == Direction.West && tableStatus.ReadyForNextRoundWest))
            {
                return RedirectToAction("Index", "ShowMove", new { newRoundNumber = roundNumber + 1 });
            }
            else
            {
                return RedirectToAction("Index", "ShowPlayerIDs");
            }
        }
    }
}