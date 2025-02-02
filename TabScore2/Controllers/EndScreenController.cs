﻿// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.UtilityServices;

namespace TabScore2.Controllers
{
    public class EndScreenController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;

        public ActionResult Index()
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            ViewData["Header"] = utilities.Header(HeaderType.Location, deviceStatus);
            ViewData["Title"] = utilities.Title("EndScreen", deviceStatus);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            return View();
        }

        public ActionResult OKButtonClick()
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");

            // Check if new round has been added; can't apply to individuals
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            if (deviceStatus.RoundNumber == database.GetNumberOfRoundsInSection(deviceStatus.SectionId, true))   // Force database read
            {
                // Final round, so no new rounds added
                return RedirectToAction("Index", "EndScreen");
            }
            else
            {
                return RedirectToAction("Index", "ShowMove", new { newRoundNumber = deviceStatus.RoundNumber + 1 });
            }
        }
    }
}