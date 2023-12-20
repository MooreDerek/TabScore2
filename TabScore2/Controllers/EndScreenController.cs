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

        public ActionResult Index(int tabletDeviceNumber)
        {
            ViewData["Header"] = utilities.Header(tabletDeviceNumber, HeaderType.Location);
            ViewData["Title"] = utilities.Title(tabletDeviceNumber, "EndScreen", TitleType.Location);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            ViewData["TabletDeviceNumber"] = tabletDeviceNumber;
            return View();
        }

        public ActionResult OKButtonClick(int tabletDeviceNumber)
        {
            // Check if new round has been added; can't apply to individuals
            TabletDeviceStatus tabletDeviceStatus = appData.GetTabletDeviceStatus(tabletDeviceNumber);
            if (tabletDeviceStatus.RoundNumber == database.GetNumberOfRoundsInEvent(tabletDeviceStatus.SectionID))  
            {
                // Final round, so no new rounds added
                return RedirectToAction("Index", "EndScreen", new { tabletDeviceNumber });
            }
            else
            {
                return RedirectToAction("Index", "ShowMove", new { tabletDeviceNumber, newRoundNumber = tabletDeviceStatus.RoundNumber + 1 });
            }
        }
    }
}