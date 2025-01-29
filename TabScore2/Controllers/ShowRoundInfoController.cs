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
    public class ShowRoundInfoController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;
        private readonly ISettings settings = iSettings;

        public ActionResult Index()
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");

            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            ViewData["Title"] = utilities.Title("ShowRoundInfo", TitleType.Location, deviceNumber);
            ViewData["Header"] = utilities.Header(HeaderType.Location, deviceNumber);
            Section section = database.GetSection(deviceStatus.SectionID);
            if (deviceStatus.TableNumber == 0)
            {
                ShowRoundInfoSitoutModel showRoundInfoSitoutModel = new(deviceStatus.PairNumber, deviceStatus.RoundNumber, section.DevicesPerTable);
                deviceStatus.AtSitoutTable = true;
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
                return View("Sitout", showRoundInfoSitoutModel);
            }

            // Update player names if not just immediately done in ShowPlayerIDs
            TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
            if (deviceStatus.NamesUpdateRequired)
            {
                Names names = database.GetNamesForRound(tableStatus.SectionID, tableStatus.RoundNumber, tableStatus.RoundData.NumberNorth, tableStatus.RoundData.NumberEast, tableStatus.RoundData.NumberSouth, tableStatus.RoundData.NumberWest);
                tableStatus.RoundData.NameNorth = names.NameNorth;
                tableStatus.RoundData.NameEast = names.NameEast;
                tableStatus.RoundData.NameSouth = names.NameSouth;
                tableStatus.RoundData.NameWest = names.NameWest;
                tableStatus.RoundData.GotAllNames = names.GotAllNames;
            }
            deviceStatus.NamesUpdateRequired = true;

            ShowRoundInfoModel showRoundInfoModel = utilities.CreateShowRoundInfoModel(deviceNumber);
            if (deviceStatus.RoundNumber > 1)
            {
                showRoundInfoModel.BoardsFromTable = utilities.GetBoardsFromTableNumber(tableStatus);
            }
            
            // Check if a sitout table
            if (tableStatus.RoundData.NumberNorth == 0 || tableStatus.RoundData.NumberNorth == section.MissingPair)
            {
                tableStatus.ReadyForNextRoundNorth = true;
                tableStatus.ReadyForNextRoundSouth = true;
                showRoundInfoModel.NSMissing = true;
                deviceStatus.AtSitoutTable = true;
            }
            else if (tableStatus.RoundData.NumberEast == 0 || tableStatus.RoundData.NumberEast == section.MissingPair)
            {
                tableStatus.ReadyForNextRoundEast = true;
                tableStatus.ReadyForNextRoundWest = true;
                showRoundInfoModel.EWMissing = true;
                deviceStatus.AtSitoutTable = true;
            }
            else
            {
                deviceStatus.AtSitoutTable = false;
            }

            if (deviceStatus.RoundNumber == 1 || section.DevicesPerTable > 1)
            {
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            }
            else
            {
                // Back button needed if one tablet device per table, in case EW need to go back to check their move details 
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabledAndBack;
            }
            if (settings.IsIndividual)
            {
                return View("Individual", showRoundInfoModel);
            }
            else
            {
                return View("Pair", showRoundInfoModel);
            }
        }

        public ActionResult BackButtonClick()
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");

            // Only for one tablet device per table.  Reset to the previous round; RoundNumber > 1 else no Back button and cannot get here
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
            int newRoundNumber = deviceStatus.RoundNumber;  // Going back, so new round is current round!
            deviceStatus.RoundNumber--;
            tableStatus.RoundNumber--;
            tableStatus.RoundData = database.GetRound(tableStatus.SectionID, tableStatus.TableNumber, tableStatus.RoundNumber);
            return RedirectToAction("Index", "ShowMove", new { newRoundNumber});
        }
    }
}