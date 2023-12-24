// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.UtilityServices;

namespace TabScore.Controllers
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
            DeviceStatus deviceStatus = appData.GetTabletDeviceStatus(deviceNumber);

            if (database.GetHand(deviceStatus.SectionID, boardNumber) != null)
            {
                // Hand record already exists, so no need to enter it
                return RedirectToAction("Index", "ShowTraveller", new { deviceNumber, boardNumber });
            }
            EnterHandRecord enterHandRecord = new(deviceNumber, deviceStatus.SectionID, boardNumber);
            
            if (settings.ShowTimer) ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceNumber);
            ViewData["Title"] = utilities.Title(deviceNumber, "EnterHandRecord", TitleType.Plain);
            ViewData["Header"] = utilities.Header(deviceNumber, HeaderType.FullColoured, boardNumber);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabledAndBack;
            return View(enterHandRecord);
        }

        public ActionResult OKButtonClick(int deviceNumber, string NS, string NH, string ND, string NC, string SS, string SH, string SD, string SC, string ES, string EH, string ED, string EC, string WS, string WH, string WD, string WC)
        {
            int boardNumber = appData.GetTableStatus(deviceNumber).ResultData.BoardNumber;
            Hand hand = new()
            {
                SectionID = appData.GetTabletDeviceStatus(deviceNumber).SectionID,
                BoardNumber = boardNumber,
                NorthSpades = NS,
                NorthHearts = NH,
                NorthDiamonds = ND,
                NorthClubs = NC,
                SouthSpades = SS,
                SouthHearts = SH,
                SouthDiamonds = SD,
                SouthClubs = SC,
                EastSpades = ES,
                EastHearts = EH,
                EastDiamonds = ED,
                EastClubs = EC,
                WestSpades = WS,
                WestHearts = WH,
                WestDiamonds = WD,
                WestClubs = WC
            };
            database.AddHand(hand);
            if (settings.DoubleDummy) appData.AddHandEvaluation(hand);
            return RedirectToAction("Index", "ShowTraveller", new { deviceNumber, boardNumber });
        }
    }
}
