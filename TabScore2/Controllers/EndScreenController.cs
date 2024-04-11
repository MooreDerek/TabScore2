// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.UtilityServices;

namespace TabScore.Controllers
{
    public class EndScreenController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;

        public ActionResult Index(int deviceNumber)
        {
            ViewData["Header"] = utilities.Header(HeaderType.Location, deviceNumber);
            ViewData["Title"] = utilities.Title("EndScreen", TitleType.Location, deviceNumber);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            ViewData["TabletDeviceNumber"] = deviceNumber;
            return View();
        }

        public ActionResult OKButtonClick(int deviceNumber)
        {
            // Check if new round has been added; can't apply to individuals
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            if (deviceStatus.RoundNumber == database.GetNumberOfRoundsInSection(deviceStatus.SectionID, true))   // Force database read
            {
                // Final round, so no new rounds added
                return RedirectToAction("Index", "EndScreen", new { deviceNumber });
            }
            else
            {
                return RedirectToAction("Index", "ShowMove", new { deviceNumber, newRoundNumber = deviceStatus.RoundNumber + 1 });
            }
        }
    }
}