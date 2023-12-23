// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.UtilityServices;

namespace TabScore2.Controllers
{
    public class ShowBoardsController(IAppData iAppData, IUtilities iUtilities, ISettings iSettings) : Controller
    {
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(int deviceNumber)
       {
            ShowBoards showBoards = utilities.CreateShowBoardsModel(deviceNumber);
            
            if (settings.ShowTimer) ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceNumber);
            ViewData["Title"] = utilities.Title(deviceNumber, "ShowBoards", TitleType.Location);
            ViewData["Header"] = utilities.Header(deviceNumber, HeaderType.FullPlain);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;

            if (appData.GetTabletDeviceStatus(deviceNumber).Direction == Direction.North)
            {
                return View("Scoring", showBoards);
            }
            else
            {
                showBoards.Message = "NOMESSAGE";
                return View("ViewOnly", showBoards);
            }
       }

        public ActionResult ViewResult(int deviceNumber, int boardNumber)
        {
            // Only used by ViewOnly view, for tablet device that is not being used for scoring, to check if result has been entered for this board
            ShowBoards showBoards = utilities.CreateShowBoardsModel(deviceNumber);
                        
            if (showBoards.First(x => x.BoardNumber == boardNumber).ContractLevel < 0)
            {
                showBoards.Message = "NORESULT";
                if (settings.ShowTimer) ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceNumber);
                ViewData["Title"] = utilities.Title(deviceNumber, "ShowBoards", TitleType.Location);
                ViewData["Header"] = utilities.Header(deviceNumber, HeaderType.FullPlain);
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
                return View("ViewOnly", showBoards);
            }
            else
            {
                return RedirectToAction("Index", "ShowTraveller", new { deviceNumber, boardNumber, fromView = true });
            }
        }

        public ActionResult OKButtonClick(int deviceNumber)
        {
            // Only used by ViewOnly view, for tablet device that is not being used for scoring, to check if all results have been entered
            ShowBoards showBoards = utilities.CreateShowBoardsModel(deviceNumber);

            if (!showBoards.GotAllResults)
            {
                showBoards.Message = "NOTALLRESULTS";
                if (settings.ShowTimer) ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceNumber);
                ViewData["Title"] = utilities.Title(deviceNumber, "ShowBoards", TitleType.Location);
                ViewData["Header"] = utilities.Header(deviceNumber, HeaderType.FullPlain);
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
                return View("ViewOnly", showBoards);
            }
            else
            {
                return RedirectToAction("Index", "ShowRankingList", new { deviceNumber });
            }
        }
    }
}