// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.Models;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.UtilityServices;

namespace TabScore2.Controllers
{
    public class ConfirmResultController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;

        public ActionResult Index(int deviceNumber)
        {
            TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
            if (tableStatus.ResultData.BoardNumber == 0)  // Probably from browser 'Back' button.  Don't know boardNumber so go to ShowBoards
            {
                return RedirectToAction("Index", "ShowBoards", new { deviceNumber });
            }

            EnterContract enterContract = utilities.CreateEnterContractModel(deviceNumber, tableStatus.ResultData, true);

            ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceNumber);
            ViewData["Title"] = utilities.Title("ConfirmResult", TitleType.Location, deviceNumber);
            ViewData["Header"] = utilities.Header(HeaderType.FullColoured, deviceNumber, tableStatus.ResultData.BoardNumber);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabledAndBack;
            return View(enterContract);
        }

        public ActionResult OKButtonClick(int deviceNumber)
        {
            Result result = appData.GetTableStatus(deviceNumber).ResultData;
            database.SetResult(result);
            return RedirectToAction("Index", "EnterHandRecord", new { deviceNumber, boardNumber = result.BoardNumber });
        }

        public ActionResult BackButtonClick(int deviceNumber)
        {
            Result result = appData.GetTableStatus(deviceNumber).ResultData;
            if (result.ContractLevel == 0)  // This was passed out, so Back goes all the way to Enter Contract screen
            {
                return RedirectToAction("Index", "EnterContract", new { deviceNumber, boardNumber = result.BoardNumber });
            }
            else
            {
                return RedirectToAction("Index", "EnterTricksTaken", new { deviceNumber });
            }
        }
    }
}
