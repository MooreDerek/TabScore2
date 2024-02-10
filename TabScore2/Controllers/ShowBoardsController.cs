// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.UtilityServices;

namespace TabScore2.Controllers
{
    public class ShowBoardsController(IAppData iAppData, IUtilities iUtilities) : Controller
    {
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;

        public ActionResult Index(int deviceNumber)
       {
            ShowBoardsModel showBoardsModel = utilities.CreateShowBoardsModel(deviceNumber);
            
            ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceNumber);
            ViewData["Title"] = utilities.Title("ShowBoards", TitleType.Location, deviceNumber);
            ViewData["Header"] = utilities.Header(HeaderType.FullPlain, deviceNumber);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;

            if (appData.GetDeviceStatus(deviceNumber).Direction == Direction.North)
            {
                return View("Scoring", showBoardsModel);
            }
            else
            {
                showBoardsModel.Message = "NOMESSAGE";
                return View("ViewOnly", showBoardsModel);
            }
       }

        public ActionResult ViewResult(int deviceNumber, int boardNumber)
        {
            // Only used by ViewOnly view, for tablet device that is not being used for scoring, to check if result has been entered for this board
            ShowBoardsModel showBoardsModel = utilities.CreateShowBoardsModel(deviceNumber);
                        
            if (showBoardsModel.First(x => x.BoardNumber == boardNumber).ContractLevel < 0)
            {
                showBoardsModel.Message = "NORESULT";
                ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceNumber);
                ViewData["Title"] = utilities.Title("ShowBoards", TitleType.Location, deviceNumber);
                ViewData["Header"] = utilities.Header(HeaderType.FullPlain, deviceNumber);
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
                return View("ViewOnly", showBoardsModel);
            }
            else
            {
                return RedirectToAction("Index", "ShowTraveller", new { deviceNumber, boardNumber, fromView = true });
            }
        }

        public ActionResult OKButtonClick(int deviceNumber)
        {
            // Only used by ViewOnly view, for tablet device that is not being used for scoring, to check if all results have been entered
            ShowBoardsModel showBoardsModel = utilities.CreateShowBoardsModel(deviceNumber);

            if (!showBoardsModel.GotAllResults)
            {
                showBoardsModel.Message = "NOTALLRESULTS";
                ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceNumber);
                ViewData["Title"] = utilities.Title("ShowBoards", TitleType.Location, deviceNumber);
                ViewData["Header"] = utilities.Header(HeaderType.FullPlain, deviceNumber);
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
                return View("ViewOnly", showBoardsModel);
            }
            else
            {
                return RedirectToAction("Index", "ShowRankingList", new { deviceNumber });
            }
        }
    }
}