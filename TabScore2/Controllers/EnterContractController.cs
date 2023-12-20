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
    public class EnterContractController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(int tabletDeviceNumber, int boardNumber)
        {
            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;
            
            // Get or create new result as needed
            tableStatus.ResultData ??= database.GetResult(tableStatus.SectionID, tableStatus.TableNumber, tableStatus.RoundNumber, boardNumber);

            LeadValidationOptions leadValidationOptions;
            if (settings.ValidateLeadCard)
            {
                leadValidationOptions = LeadValidationOptions.Validate;
            }
            else
            {
                leadValidationOptions = LeadValidationOptions.NoWarning;
            }
            EnterContract enterContract = new(tabletDeviceNumber, tableStatus.ResultData, leadValidationOptions);

            if (settings.ShowTimer) ViewData["TimerSeconds"] = appData.GetTimerSeconds(tabletDeviceNumber);
            ViewData["Title"] = utilities.Title(tabletDeviceNumber, "EnterContract", TitleType.Location);
            ViewData["Header"] = utilities.Header(tabletDeviceNumber, HeaderType.FullColoured, tableStatus.ResultData.BoardNumber);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabledAndBack;
            return View(enterContract);
        }

        public ActionResult OKButtonContract(int tabletDeviceNumber, int contractLevel, string contractSuit, string contractX, string declarerNSEW)
        {
            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;  // tableStatus and result must exist
            Result result = tableStatus.ResultData!;
            result.ContractLevel = contractLevel;
            result.ContractSuit = contractSuit;
            result.ContractX = contractX;
            result.DeclarerNSEW = declarerNSEW;
            return RedirectToAction("Index", "EnterLead", new { tabletDeviceNumber, leadValidation = LeadValidationOptions.Validate });
        }

        public ActionResult OKButtonPass(int tabletDeviceNumber)
        {
            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;  // tableStatus and result must exist
            Result result = tableStatus.ResultData!;
            result.ContractLevel = 0;
            result.ContractSuit = "";
            result.ContractX = "";
            result.DeclarerNSEW = "";
            result.LeadCard = "";
            result.TricksTaken = -1;
            result.CalculateScore();
            return RedirectToAction("Index", "ConfirmResult", new { tabletDeviceNumber });
        }
        
        public ActionResult OKButtonSkip(int tabletDeviceNumber)
        {
            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;  // tableStatus and result must exist
            Result result = tableStatus.ResultData!;
            result.ContractLevel = -1;
            result.ContractSuit = "";
            result.ContractX = "";
            result.DeclarerNSEW = "";
            result.LeadCard = "";
            result.TricksTaken = -1;
            database.SetResult(tableStatus.SectionID, tableStatus.TableNumber, tableStatus.RoundNumber, result);
            return RedirectToAction("Index", "ShowBoards", new { tabletDeviceNumber });
        }
    }
}
