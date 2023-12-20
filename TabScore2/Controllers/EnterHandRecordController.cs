// TabScore, a wireless bridge scoring program.  Copyright(C) 2023 by Peter Flippant
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

        public ActionResult Index(int tabletDeviceNumber, int boardNumber)
        {
            if (!settings.ManualHandRecordEntry)
            {
                return RedirectToAction("Index", "ShowTraveller", new { tabletDeviceNumber, boardNumber });
            }
            TabletDeviceStatus tabletDeviceStatus = appData.GetTabletDeviceStatus(tabletDeviceNumber);
            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;

            if (database.GetHand(tabletDeviceStatus.SectionID, boardNumber) != null)
            {
                // Hand record already exists, so no need to enter it
                return RedirectToAction("Index", "ShowTraveller", new { tabletDeviceNumber, boardNumber });
            }
            EnterHandRecord enterHandRecord = new(tabletDeviceNumber, tableStatus.SectionID, boardNumber);
            
            if (settings.ShowTimer) ViewData["TimerSeconds"] = appData.GetTimerSeconds(tabletDeviceNumber);
            ViewData["Title"] = utilities.Title(tabletDeviceNumber, "EnterHandRecord", TitleType.Plain);
            ViewData["Header"] = utilities.Header(tabletDeviceNumber, HeaderType.FullColoured, tableStatus.ResultData!.BoardNumber);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabledAndBack;
            return View(enterHandRecord);
        }

        public ActionResult OKButtonClick(int tabletDeviceNumber, string NS, string NH, string ND, string NC, string SS, string SH, string SD, string SC, string ES, string EH, string ED, string EC, string WS, string WH, string WD, string WC)
        {
            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;
            int boardNumber = tableStatus.ResultData!.BoardNumber;
            Hand hand = new()
            {
                SectionID = tableStatus.SectionID,
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
            return RedirectToAction("Index", "ShowTraveller", new { tabletDeviceNumber, boardNumber });
        }
    }
}
