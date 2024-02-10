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
    public class EnterLeadController(IAppData iAppData, IUtilities iUtilities, ISettings iSettings) : Controller
    {
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(int deviceNumber, LeadValidationOptions leadValidation)
        {
            if (!settings.EnterLeadCard)
            {
                return RedirectToAction("Index", "EnterTricksTaken", new { deviceNumber });
            }

            TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
            if (tableStatus.ResultData.BoardNumber == 0)  // Probably from browser 'Back' button.  Don't know boardNumber so go to ShowBoards
            {
                return RedirectToAction("Index", "ShowBoards", new { deviceNumber });
            }

            if (tableStatus.ResultData.LeadCard == string.Empty)  // Lead not set, so use leadValidation value as passed to controller
            {
                tableStatus.LeadValidation = leadValidation;
            }
            else  // Lead already set, so must be an edit (ie no validation and no warning)
            {
                tableStatus.LeadValidation = LeadValidationOptions.NoWarning;
            }
            EnterContractModel enterContractModel = utilities.CreateEnterContractModel(deviceNumber, tableStatus.ResultData, false, tableStatus.LeadValidation);

            ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceNumber);
            ViewData["Title"] = utilities.Title("EnterLead", TitleType.Location, deviceNumber);
            ViewData["Header"] = utilities.Header(HeaderType.FullColoured, deviceNumber, tableStatus.ResultData.BoardNumber);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabledAndBack;
            return View(enterContractModel);
        }

        public ActionResult OKButtonClick(int deviceNumber, string card)
        {
            TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
            if (tableStatus.LeadValidation != LeadValidationOptions.Validate || !settings.ValidateLeadCard || utilities.ValidateLead(tableStatus, card))
            {
                tableStatus.ResultData.LeadCard = card;
                return RedirectToAction("Index", "EnterTricksTaken", new { deviceNumber });
            }
            else
            {
                return RedirectToAction("Index", "EnterLead", new { deviceNumber, leadValidation = LeadValidationOptions.Warning });
            }
        }
    }
}