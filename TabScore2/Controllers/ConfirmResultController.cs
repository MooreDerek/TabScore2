﻿// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore.Models;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.UtilityServices;

namespace TabScore2.Controllers
{
    public class ConfirmResultController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(int tabletDeviceNumber)
        {
            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;
            if (tableStatus.ResultData == null)  // Probably from browser 'Back' button.  Don't know boardNumber so go to ShowBoards
            {
                return RedirectToAction("Index", "ShowBoards", new { tabletDeviceNumber });
            }

            EnterContract enterContract = new(tabletDeviceNumber, tableStatus.ResultData);

            if (settings.ShowTimer) ViewData["TimerSeconds"] = appData.GetTimerSeconds(tabletDeviceNumber);
            ViewData["Title"] = utilities.Title(tabletDeviceNumber, "ConfirmResult", TitleType.Location);
            ViewData["Header"] = utilities.Header(tabletDeviceNumber, HeaderType.FullPlain, tableStatus.ResultData.BoardNumber);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabledAndBack;
            return View(enterContract);
        }

        public ActionResult OKButtonClick(int tabletDeviceNumber)
        {
            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;  // tableStatus and result must exist
            Result result = tableStatus.ResultData!;
            database.SetResult(tableStatus.SectionID, tableStatus.TableNumber, tableStatus.RoundNumber, result);
            return RedirectToAction("Index", "EnterHandRecord", new { tabletDeviceNumber, boardNumber = result.BoardNumber });
        }

        public ActionResult BackButtonClick(int tabletDeviceNumber)
        {
            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;  // tableStatus and result must exist
            Result result = tableStatus.ResultData!;
            if (result.ContractLevel == 0)  // This was passed out, so Back goes all the way to Enter Contract screen
            {
                return RedirectToAction("Index", "EnterContract", new { tabletDeviceNumber, boardNumber = result.BoardNumber });
            }
            else
            {
                return RedirectToAction("Index", "EnterTricksTaken", new { tabletDeviceNumber });
            }
        }
    }
}