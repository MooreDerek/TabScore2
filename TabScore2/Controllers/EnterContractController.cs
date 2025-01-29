// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.SharedClasses;
using TabScore2.UtilityServices;

namespace TabScore2.Controllers
{
    public class EnterContractController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;

        public ActionResult Index(int boardNumber)
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            
            TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
            Result result = tableStatus.ResultData;
            if (tableStatus.ResultData.BoardNumber != boardNumber)
            {
                // No result set for this board yet, so get result (if any) from database and set pair/player numbers
                result = database.GetResult(tableStatus.SectionID, tableStatus.TableNumber, tableStatus.RoundNumber, boardNumber);
                result.NumberNorth = tableStatus.RoundData.NumberNorth;
                result.NumberEast = tableStatus.RoundData.NumberEast;
                result.NumberSouth = tableStatus.RoundData.NumberSouth;
                result.NumberWest = tableStatus.RoundData.NumberWest;
                tableStatus.ResultData = result;
            }

            EnterContractModel enterContractModel = utilities.CreateEnterContractModel(result);

            ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceNumber);
            ViewData["Title"] = utilities.Title("EnterContract", TitleType.Location, deviceNumber);
            ViewData["Header"] = utilities.Header(HeaderType.FullColoured, deviceNumber, result.BoardNumber);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabledAndBack;
            return View(enterContractModel);
        }

        public ActionResult OKButtonContract(int contractLevel, string contractSuit, string contractX, string declarerNSEW)
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            
            contractX ??= string.Empty;
            Result result = appData.GetTableStatus(deviceNumber).ResultData;
            result.ContractLevel = contractLevel;
            result.ContractSuit = contractSuit;
            result.ContractX = contractX;
            result.DeclarerNSEW = declarerNSEW;
            result.Remarks = string.Empty;
            return RedirectToAction("Index", "EnterLead", new { leadValidation = LeadValidationOptions.Validate });
        }

        public ActionResult OKButtonPass()
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            
            Result result = appData.GetTableStatus(deviceNumber).ResultData;
            result.ContractLevel = 0;
            result.ContractSuit = string.Empty;
            result.ContractX = string.Empty;
            result.DeclarerNSEW = string.Empty;
            result.LeadCard = string.Empty;
            result.TricksTaken = -1;
            result.Remarks = string.Empty;
            return RedirectToAction("Index", "ConfirmResult");
        }
        
        public ActionResult OKButtonSkip()
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            
            Result result = appData.GetTableStatus(deviceNumber).ResultData;
            result.ContractLevel = -1;
            result.ContractSuit = string.Empty;
            result.ContractX = string.Empty;
            result.DeclarerNSEW = string.Empty;
            result.LeadCard = string.Empty;
            result.TricksTaken = -1;
            result.Remarks = "Not played";
            database.SetResult(result);
            return RedirectToAction("Index", "ShowBoards");
        }
    }
}
