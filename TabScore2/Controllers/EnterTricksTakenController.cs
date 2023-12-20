// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore.Models;
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

        public ActionResult Index(int tabletDeviceNumber)
        {
            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;
            if (tableStatus.ResultData == null)  // Probably from browser 'Back' button.  Don't know boardNumber so go to ShowBoards
            {
                return RedirectToAction("Index", "ShowBoards", new { tabletDeviceNumber });
            }
            
            EnterContract enterContract = new(tabletDeviceNumber, tableStatus.ResultData);

            if (settings.ShowTimer) ViewData["TimerSeconds"] = appData.GetTimerSeconds(tabletDeviceNumber);
            ViewData["Title"] = utilities.Title(tabletDeviceNumber, "EnterTricksTaken", TitleType.Location);
            ViewData["Header"] = utilities.Header(tabletDeviceNumber, HeaderType.FullColoured, tableStatus.ResultData.BoardNumber);
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

        public ActionResult OKButtonClick(int tabletDeviceNumber, int numTricks)
        {
            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;  // tableStatus and result must exist
            Result contractResult = tableStatus.ResultData!;
            contractResult.TricksTaken = numTricks;
            contractResult.CalculateScore();
            return RedirectToAction("Index", "ConfirmResult", new { tabletDeviceNumber });
        }

        public ActionResult BackButtonClick(int tabletDeviceNumber)
        {
            if (settings.EnterLeadCard)
            {
                return RedirectToAction("Index", "EnterLead", new { tabletDeviceNumber, leadValidation = LeadValidationOptions.NoWarning });
            }
            else
            {
                TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;  // tableStatus and result must exist
                Result contractResult = tableStatus.ResultData!;
                return RedirectToAction("Index", "EnterContract", new { tabletDeviceNumber, boardNumber = contractResult.BoardNumber });
            }
        }
    }
}