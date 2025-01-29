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
    public class SelectTableNumberController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(int sectionID, int tableNumber = 0, bool confirm = false) 
        {
            Section section = database.GetSection(sectionID);
            SelectTableNumberModel selectTableNumberModel = new(section, tableNumber, confirm);
            ViewData["Title"] = utilities.Title("SelectTableNumber", TitleType.Section, sectionID);
            ViewData["Header"] = utilities.Header(HeaderType.Section, sectionID);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
            return View(selectTableNumberModel);   
        }

        public ActionResult OKButtonClick(int sectionID, int tableNumber, bool confirm)
        {
            // Register table in database
            database.RegisterTable(sectionID, tableNumber);

            TableStatus tableStatus = appData.GetTableStatus(sectionID, tableNumber);  // Return value cannot be null as we've just set it
            tableStatus.RoundData = database.GetRound(sectionID, tableNumber, tableStatus.RoundNumber);

            if (database.GetSection(sectionID).DevicesPerTable == 1)
            {
                // Check if tablet device is already registered for this location. One tablet device per table, so Direction defaults to North
                bool deviceStatusExists = appData.DeviceStatusExists(sectionID, tableNumber);
                if (deviceStatusExists && confirm)
                {
                    // Ok to change to this tablet, so set session state
                    HttpContext.Session.SetInt32("SectionId", sectionID);
                    HttpContext.Session.SetInt32("TableNumber", tableNumber);
                }
                else if (deviceStatusExists)
                {
                    // Check if section and table number matches session state - if so go back to confirm
                    if (sectionID == (HttpContext.Session.GetInt32("SectionID") ?? 0) && tableNumber == (HttpContext.Session.GetInt32("TableNumber") ?? 0))
                    {
                        return RedirectToAction("Index", "SelectTableNumber", new { sectionID, tableNumber, confirm = true });
                    }
                    // else = session state matches, so this is a re-registration and nothing more to do
                }
                else 
                {
                    // Not on list, so need to add it
                    appData.AddDeviceStatus(sectionID, tableNumber, tableStatus.RoundData.NumberNorth, tableStatus.RoundNumber);
                    HttpContext.Session.SetInt32("SectionId", sectionID);
                    HttpContext.Session.SetInt32("TableNumber", tableNumber);
                }
                DeviceStatus deviceStatus = appData.GetDeviceStatus(sectionID, tableNumber);

                // DeviceNumber is the key for identifying this particular tablet device and is used throughout the rest of the application
                HttpContext.Session.SetInt32("DeviceNumber", appData.GetDeviceNumber(deviceStatus));

                if (tableStatus.ReadyForNextRoundNorth)
                {
                    return RedirectToAction("Index", "ShowMove", new { newRoundNumber = tableStatus.RoundNumber + 1 });
                }
                else if (deviceStatus.RoundNumber == 1 || settings.NumberEntryEachRound)
                {
                    return RedirectToAction("Index", "ShowPlayerIDs");
                }
                else
                {
                    return RedirectToAction("Index", "ShowRoundInfo");
                } 
            }
            else   // More than one tablet device per table, so need to know direction for this tablet device
            {
                return RedirectToAction("Index", "SelectDirection", new { sectionID, tableNumber });
            }
        }
    }
}