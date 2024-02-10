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

            EnterContractModel enterContractModel = utilities.CreateEnterContractModel(deviceNumber, tableStatus.ResultData);

            ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceNumber);
            ViewData["Title"] = utilities.Title("EnterTricksTaken", TitleType.Location, deviceNumber);
            ViewData["Header"] = utilities.Header(HeaderType.FullColoured, deviceNumber, tableStatus.ResultData.BoardNumber);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabledAndBack;
            if (settings.EnterResultsMethod == 1)
            {
                return View("TotalTricks", enterContractModel);
            }
            else
            {
                return View("TricksPlusMinus", enterContractModel);
            }
        }

        public ActionResult OKButtonClick(int deviceNumber, int tricksTaken)
        {
            Result result = appData.GetTableStatus(deviceNumber).ResultData;
            result.TricksTaken = tricksTaken;
            if (tricksTaken == -1)
            {
                result.TricksTakenSymbol = string.Empty;
            }
            else
            {
                int tricksTakenLevel = tricksTaken - result.ContractLevel - 6;
                if (tricksTakenLevel == 0)
                {
                    result.TricksTakenSymbol = "=";
                }
                else
                {
                    result.TricksTakenSymbol = tricksTakenLevel.ToString("+#;-#;0");
                }
            }
            result.CalculateScore();
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
                Result result = tableStatus.ResultData!;
                return RedirectToAction("Index", "EnterContract", new { deviceNumber, boardNumber = result.BoardNumber });
            }
        }
    }
}