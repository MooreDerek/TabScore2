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
    public class ShowPlayerIDsController(IDatabase iDatabase, IAppData iAppData, ISettings iSettings, IUtilities iUtilities) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly ISettings settings = iSettings;
        private readonly IUtilities utilities = iUtilities;

        public ActionResult Index(int deviceNumber, bool showWarning = false)
        {
            DeviceStatus deviceStatus = appData.GetTabletDeviceStatus(deviceNumber);
            TableStatus tableStatus = appData.GetTableStatus(deviceNumber);

            if (deviceStatus.NamesUpdateRequired) {
                database.GetNamesForRound(tableStatus);  // Update names from database if not done very recently
            }

            if (tableStatus.RoundData.GotAllNames && deviceStatus.RoundNumber > 1 && !settings.NumberEntryEachRound)
            {
                // Player numbers not needed if all names have already been entered and names are not being updated each round
                deviceStatus.NamesUpdateRequired = false;  // No round update required in RoundInfo as it's just been done
                return RedirectToAction("Index", "ShowRoundInfo", new { deviceNumber });
            }
            deviceStatus.NamesUpdateRequired = true;  // We'll now need to update when we get to RoundInfo in case names change in the mean time

            ShowPlayerIDs showplayerIDs = utilities.CreateShowPlayerIDsModel(deviceNumber, showWarning);
            ViewData["Title"] = utilities.Title(deviceNumber, "ShowPlayerIDs", TitleType.Location);
            ViewData["Header"] = utilities.Header(deviceNumber, HeaderType.Round);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;

            if (database.IsIndividual)
            {
                return View("Individual", showplayerIDs);
            }
            else
            {
                return View("Pair", showplayerIDs);
            }
        }

        public ActionResult OKButtonClick(int deviceNumber)
        {
            TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
            database.GetNamesForRound(tableStatus);
            appData.GetTabletDeviceStatus(deviceNumber).NamesUpdateRequired = false;  // No names update required on next screen as it's only just been done

            // Check if all required names have been entered, and if not go back and wait
            if (tableStatus.RoundData.GotAllNames)
            {
                return RedirectToAction("Index", "ShowRoundInfo", new { deviceNumber });
            }
            else
            {
                return RedirectToAction("Index", "ShowPlayerIDs", new { deviceNumber, showWarning = true });
            }
        }
    }
}