// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
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
    public class EnterHandRecordController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(int deviceNumber, int boardNumber)
        {
            if (!settings.ManualHandRecordEntry)
            {
                return RedirectToAction("Index", "ShowTraveller", new { deviceNumber, boardNumber });
            }

            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            if (database.GetHand(deviceStatus.SectionID, boardNumber).NorthSpades != "###")
            {
                // Hand record already exists, so no need to enter it
                return RedirectToAction("Index", "ShowTraveller", new { deviceNumber, boardNumber });
            }
            EnterHandRecordModel enterHandRecordModel = new(deviceNumber, deviceStatus.SectionID, boardNumber);
            
            ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceNumber);
            ViewData["Title"] = utilities.Title("EnterHandRecord", TitleType.Location, deviceNumber);
            ViewData["Header"] = utilities.Header(HeaderType.FullColoured, deviceNumber, boardNumber);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabledAndBack;
            return View(enterHandRecordModel);
        }

        public ActionResult OKButtonClick(int deviceNumber, string NS, string NH, string ND, string NC, string SS, string SH, string SD, string SC, string ES, string EH, string ED, string EC, string WS, string WH, string WD, string WC)
        {
            int boardNumber = appData.GetTableStatus(deviceNumber).ResultData.BoardNumber;
            Hand hand = new()
            {
                SectionID = appData.GetDeviceStatus(deviceNumber).SectionID,
                BoardNumber = boardNumber,
                NorthSpades = NS ?? "###",
                NorthHearts = NH ?? string.Empty,
                NorthDiamonds = ND ?? string.Empty,
                NorthClubs = NC ?? string.Empty,
                SouthSpades = SS ?? string.Empty,
                SouthHearts = SH ?? string.Empty,
                SouthDiamonds = SD ?? string.Empty,
                SouthClubs = SC ?? string.Empty,
                EastSpades = ES ?? string.Empty,
                EastHearts = EH ?? string.Empty,
                EastDiamonds = ED ?? string.Empty,
                EastClubs = EC ?? string.Empty,
                WestSpades = WS ?? string.Empty,
                WestHearts = WH ?? string.Empty,
                WestDiamonds = WD ?? string.Empty,
                WestClubs = WC ?? string.Empty
            };
            database.AddHand(hand);
            if (settings.DoubleDummy) appData.AddHandEvaluation(hand);
            return RedirectToAction("Index", "ShowTraveller", new { deviceNumber, boardNumber });
        }
    }
}
