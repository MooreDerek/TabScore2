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
    public class EnterTricksTakenController(IAppData iAppData, IUtilities iUtilities, ISettings iSettings) : Controller
    {
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(int deviceNumber)
        {
            TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
            if (tableStatus.ResultData.BoardNumber == 0)  // Probably from browser 'Back' button.  Don't know boardNumber so go to ShowBoards
            {
                return RedirectToAction("Index", "ShowBoards", new { deviceNumber });
            }

            EnterContract enterContract = utilities.CreateEnterContractModel(deviceNumber, tableStatus.ResultData);

            if (settings.ShowTimer) ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceNumber);
            ViewData["Title"] = utilities.Title(deviceNumber, "EnterTricksTaken", TitleType.Location);
            ViewData["Header"] = utilities.Header(deviceNumber, HeaderType.FullColoured, tableStatus.ResultData.BoardNumber);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabledAndBack;
            if (settings.EnterResultsMethod == 1)
            {
                return View("TotalTricks", enterContract);
            }
            else
            {
                return View("TricksPlusMinus", enterContract);
            }
        }

        public ActionResult OKButtonClick(int deviceNumber, int tricksTaken)
        {
            TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
            Result contractResult = tableStatus.ResultData;
            contractResult.TricksTaken = tricksTaken;
            contractResult.CalculateScore();
            return RedirectToAction("Index", "ConfirmResult", new { deviceNumber });
        }

        public ActionResult BackButtonClick(int deviceNumber)
        {
            if (settings.EnterLeadCard)
            {
                return RedirectToAction("Index", "EnterLead", new { deviceNumber, leadValidation = LeadValidationOptions.NoWarning });
            }
            else
            {
                TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
                Result contractResult = tableStatus.ResultData!;
                return RedirectToAction("Index", "EnterContract", new { deviceNumber, boardNumber = contractResult.BoardNumber });
            }
        }
    }
}