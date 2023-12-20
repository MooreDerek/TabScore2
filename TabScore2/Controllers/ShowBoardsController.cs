// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore.Models;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.UtilityServices;

namespace TabScore2.Controllers
{
    public class ShowBoardsController(IAppData iAppData, IUtilities iUtilities, ISettings iSettings) : Controller
    {
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(int tabletDeviceNumber)
       {
            ShowBoards showBoards = utilities.CreateShowBoardsModel(tabletDeviceNumber);
            
            if (settings.ShowTimer) ViewData["TimerSeconds"] = appData.GetTimerSeconds(tabletDeviceNumber);
            ViewData["Title"] = utilities.Title(tabletDeviceNumber, "ShowBoards", TitleType.Location);
            ViewData["Header"] = utilities.Header(tabletDeviceNumber, HeaderType.FullPlain);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;

            if (appData.GetTabletDeviceStatus(tabletDeviceNumber).Direction == Direction.North)
            {
                return View("Scoring", showBoards);
            }
            else
            {
                showBoards.Message = "NOMESSAGE";
                return View("ViewOnly", showBoards);
            }
       }

        public ActionResult ViewResult(int tabletDeviceNumber, int boardNumber)
        {
            // Only used by ViewOnly view, for tablet device that is not being used for scoring, to check if result has been entered for this board
            ShowBoards showBoards = utilities.CreateShowBoardsModel(tabletDeviceNumber);
                        
            if (showBoards.First(x => x.BoardNumber == boardNumber).ContractLevel < 0)
            {
                showBoards.Message = "NORESULT";
                if (settings.ShowTimer) ViewData["TimerSeconds"] = appData.GetTimerSeconds(tabletDeviceNumber);
                ViewData["Title"] = utilities.Title(tabletDeviceNumber, "ShowBoards", TitleType.Location);
                ViewData["Header"] = utilities.Header(tabletDeviceNumber, HeaderType.FullPlain);
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
                return View("ViewOnly", showBoards);
            }
            else
            {
                return RedirectToAction("Index", "ShowTraveller", new { tabletDeviceNumber, boardNumber, fromView = true });
            }
        }

        public ActionResult OKButtonClick(int tabletDeviceNumber)
        {
            // Only used by ViewOnly view, for tablet device that is not being used for scoring, to check if all results have been entered
            ShowBoards showBoards = utilities.CreateShowBoardsModel(tabletDeviceNumber);

            if (!showBoards.GotAllResults)
            {
                showBoards.Message = "NOTALLRESULTS";
                if (settings.ShowTimer) ViewData["TimerSeconds"] = appData.GetTimerSeconds(tabletDeviceNumber);
                ViewData["Title"] = utilities.Title(tabletDeviceNumber, "ShowBoards", TitleType.Location);
                ViewData["Header"] = utilities.Header(tabletDeviceNumber, HeaderType.FullPlain);
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
                return View("ViewOnly", showBoards);
            }
            else
            {
                return RedirectToAction("Index", "ShowRankingList", new { tabletDeviceNumber });
            }
        }
    }
}