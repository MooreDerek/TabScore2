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
    public class EnterLeadController(IAppData iAppData, IUtilities iUtilities, ISettings iSettings) : Controller
    {
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(int tabletDeviceNumber, LeadValidationOptions leadValidation)
        {
            if (!settings.EnterLeadCard)
            {
                return RedirectToAction("Index", "EnterTricksTaken", new { tabletDeviceNumber });
            }

            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;
            if (tableStatus.ResultData == null)  // Probably from browser 'Back' button.  Don't know boardNumber so go to ShowBoards
            {
                return RedirectToAction("Index", "ShowBoards", new { tabletDeviceNumber });
            }

            if (tableStatus.ResultData.LeadCard == "")  // Lead not set, so use leadValidation value as passed to controller
            {
                tableStatus.LeadValidation = leadValidation;
            }
            else  // Lead already set, so must be an edit (ie no validation and no warning)
            {
                tableStatus.LeadValidation = LeadValidationOptions.NoWarning;
            }
            EnterContract enterContract = new(tabletDeviceNumber, tableStatus.ResultData, tableStatus.LeadValidation);

            if (settings.ShowTimer) ViewData["TimerSeconds"] = appData.GetTimerSeconds(tabletDeviceNumber);
            ViewData["Title"] = utilities.Title(tabletDeviceNumber, "EnterLead", TitleType.Location);
            ViewData["Header"] = utilities.Header(tabletDeviceNumber, HeaderType.FullColoured, tableStatus.ResultData.BoardNumber);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabledAndBack;
            return View(enterContract);
        }

        public ActionResult OKButtonClick(int tabletDeviceNumber, string card)
        {
            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;  // tableStatus and result must exist
            Result contractResult = tableStatus.ResultData!;  
            if (tableStatus.LeadValidation != LeadValidationOptions.Validate || !settings.ValidateLeadCard || utilities.ValidateLead(tableStatus, card))
            {
                contractResult.LeadCard = card;
                return RedirectToAction("Index", "EnterTricksTaken", new { tabletDeviceNumber });
            }
            else
            {
                return RedirectToAction("Index", "EnterLead", new { tabletDeviceNumber, leadValidation = LeadValidationOptions.Warning });
            }
        }
    }
}