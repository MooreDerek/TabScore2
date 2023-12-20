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
    public class ShowRoundInfoController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;

        public ActionResult Index(int tabletDeviceNumber)
        {
            TabletDeviceStatus tabletDeviceStatus = appData.GetTabletDeviceStatus(tabletDeviceNumber);

            ViewData["Title"] = utilities.Title(tabletDeviceNumber, "ShowRoundInfo", TitleType.Location);
            ViewData["Header"] = utilities.Header(tabletDeviceNumber, HeaderType.Location);
            if (tabletDeviceStatus.TableNumber == 0)
            {
                ShowRoundInfoSitout showRoundInfoSitout = new(tabletDeviceNumber);
                tabletDeviceStatus.AtSitoutTable = true;
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
                return View("Sitout", showRoundInfoSitout);
            }

            // Update player names if not just immediately done in ShowPlayerIDs
            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;
            if (tabletDeviceStatus.NamesUpdateRequired) database.GetNamesForRound(tableStatus);
            tabletDeviceStatus.NamesUpdateRequired = true;

            ShowRoundInfo showRoundInfo = new(tabletDeviceNumber, tabletDeviceStatus.RoundNumber, tableStatus.RoundData);
            if (tabletDeviceStatus.RoundNumber > 1)
            {
                showRoundInfo.BoardsFromTable = utilities.GetBoardsFromTableNumber(tableStatus);
            }
            Section section = database.GetSection(tabletDeviceStatus.SectionID);
            
            // Check if a sitout table
            if (tableStatus.RoundData.NumberNorth == 0 || tableStatus.RoundData.NumberNorth == section.MissingPair)
            {
                tableStatus.ReadyForNextRoundNorth = true;
                tableStatus.ReadyForNextRoundSouth = true;
                showRoundInfo.NSMissing = true;
                tabletDeviceStatus.AtSitoutTable = true;
            }
            else if (tableStatus.RoundData.NumberEast == 0 || tableStatus.RoundData.NumberEast == section.MissingPair)
            {
                tableStatus.ReadyForNextRoundEast = true;
                tableStatus.ReadyForNextRoundWest = true;
                showRoundInfo.EWMissing = true;
                tabletDeviceStatus.AtSitoutTable = true;
            }
            else
            {
                tabletDeviceStatus.AtSitoutTable = false;
            }

            if (tabletDeviceStatus.RoundNumber == 1 || section.TabletDevicesPerTable > 1)
            {
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            }
            else
            {
                // Back button needed if one tablet device per table, in case EW need to go back to check their move details 
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabledAndBack;
            }
            if (database.IsIndividual)
            {
                return View("Individual", showRoundInfo);
            }
            else
            {
                return View("Pair", showRoundInfo);
            }
        }

        public ActionResult BackButtonClick(int tabletDeviceNumber)
        {
            // Only for one tablet device per table.  Reset to the previous round; RoundNumber > 1 else no Back button and cannot get here
            TabletDeviceStatus tabletDeviceStatus = appData.GetTabletDeviceStatus(tabletDeviceNumber);
            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;
            int newRoundNumber = tabletDeviceStatus.RoundNumber;  // Going back, so new round is current round!
            tabletDeviceStatus.RoundNumber--;
            tableStatus.RoundNumber--;
            database.GetRoundData(tableStatus);
            return RedirectToAction("Index", "ShowMove", new { tabletDeviceNumber, newRoundNumber});
        }
    }
}