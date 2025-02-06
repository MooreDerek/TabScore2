// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcSharedContracts.SharedClasses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.Resources;

namespace TabScore2.Controllers
{
    public class SelectTableNumberController(IStringLocalizer<Strings> iLocalizer, IDatabase iDatabase, IAppData iAppData, ISettings iSettings) : Controller
    {
        private readonly IStringLocalizer<Strings> localizer = iLocalizer;
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(int sectionId, int tableNumber = 0, bool confirm = false) 
        {
            Section section = database.GetSection(sectionId);
            SelectTableNumberModel selectTableNumberModel = new(section, tableNumber, confirm);
            ViewData["Title"] = $"{localizer["Section"]} {section.SectionLetter}: {localizer["SelectTableNumber"]}";
            ViewData["Header"] = $"{localizer["Section"]} {section.SectionLetter}";
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
            return View(selectTableNumberModel);   
        }

        public ActionResult OKButtonClick(int sectionId, int tableNumber, bool confirm)
        {
            // Register table in database
            database.RegisterTable(sectionId, tableNumber);

            TableStatus tableStatus = appData.GetTableStatus(sectionId, tableNumber);  // Creates a new table status record if needed
            tableStatus.RoundData = database.GetRound(sectionId, tableNumber, tableStatus.RoundNumber);

            // If devices move, we also need direction.  For 2-winner pairs or teams (when Winners = 2), the devices don't move
            if (settings.DevicesMove && database.GetSection(sectionId).Winners == 1) return RedirectToAction("Index", "SelectDirection", new { sectionId, tableNumber });

            // One tablet device per table, so Direction defaults to North.  Check if tablet device is already registered for this location 
            bool deviceStatusExists = appData.DeviceStatusExists(sectionId, tableNumber);
            if (deviceStatusExists && confirm)
            {
                // Ok to change to this tablet, so set session state
                HttpContext.Session.SetInt32("SectionId", sectionId);
                HttpContext.Session.SetInt32("TableNumber", tableNumber);
            }
            else if (deviceStatusExists)
            {
                // Check if section and table number matches session state - if not go back to confirm
                int savedSectionId = HttpContext.Session.GetInt32("SectionId") ?? 0;
                int savedTableNumber = HttpContext.Session.GetInt32("TableNumber") ?? 0;
                if (sectionId != savedSectionId || tableNumber != savedTableNumber)
                {
                    return RedirectToAction("Index", "SelectTableNumber", new { sectionId, tableNumber, confirm = true });
                }
                // else = session state matches, so this is a re-registration and nothing more to do
            }
            else 
            {
                // Not on list, so need to add it
                appData.AddDeviceStatus(sectionId, tableNumber, tableStatus.RoundData.NumberNorth, tableStatus.RoundNumber);
                HttpContext.Session.SetInt32("SectionId", sectionId);
                HttpContext.Session.SetInt32("TableNumber", tableNumber);
            }
            DeviceStatus deviceStatus = appData.GetDeviceStatus(sectionId, tableNumber);
            deviceStatus.DevicesPerTable = 1;  // Devices not moving

            // DeviceNumber is the key for identifying this particular tablet device and is used throughout the rest of the application
            HttpContext.Session.SetInt32("DeviceNumber", appData.GetDeviceNumber(deviceStatus));

            if (tableStatus.ReadyForNextRoundNorth)
            {
                return RedirectToAction("Index", "ShowMove", new { newRoundNumber = tableStatus.RoundNumber + 1 });
            }
            else if (deviceStatus.RoundNumber == 1 || settings.NumberEntryEachRound)
            {
                return RedirectToAction("Index", "ShowPlayerIds");
            }
            else
            {
                return RedirectToAction("Index", "ShowRoundInfo");
            } 
        }
    }
}