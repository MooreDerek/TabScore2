// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.UtilityServices;

namespace TabScore2.Controllers
{
    public class ShowRankingListController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(int deviceNumber)
        {
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            
            // Only show ranking list settings criteria are met
            if (settings.ShowRanking != 1  
               || deviceStatus.RoundNumber <= settings.SuppressRankingListForFirstXRounds
               || deviceStatus.RoundNumber > database.GetNumberOfRoundsInEvent(deviceStatus.SectionID, deviceStatus.RoundNumber) - settings.SuppressRankingListForLastXRounds)
            {
                return RedirectToAction("Index", "ShowMove", new { deviceNumber, newRoundNumber = deviceStatus.RoundNumber + 1 });
            }

            ShowRankingListModel showRankingListModel = utilities.CreateRankingListModel(deviceNumber);
            // Only show the ranking list if it contains something meaningful
            if (showRankingListModel.Count <= 1 || showRankingListModel[0].ScoreDecimal == 0.0)
            {
                return RedirectToAction("Index", "ShowMove", new { deviceNumber, newRoundNumber = deviceStatus.RoundNumber + 1 });
            }

            ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceNumber);
            ViewData["Title"] = utilities.Title("ShowRankingList", TitleType.Location, deviceNumber);
            ViewData["Header"] = utilities.Header(HeaderType.Round, deviceNumber);
            if (deviceStatus.AtSitoutTable)
            {
                // Can't go back to ShowBoards if it's a sitout and there are no boards to play, so no 'Back' button
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            }
            else
            {
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabledAndBack;
            }

            if (database.IsIndividual)
            {
                return View("Individual", showRankingListModel);
            }
            else if (showRankingListModel.Exists(x => x.Orientation == "E"))
            {
                return View("TwoWinners", showRankingListModel);
            }
            else
            {
                return View("OneWinner", showRankingListModel);
            }
    }

        public ActionResult Final(int deviceNumber)
        {
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            ShowRankingListModel showRankingListModel = utilities.CreateRankingListModel(deviceNumber);
            if (showRankingListModel.Count <= 1 && showRankingListModel[0].ScoreDecimal == 0.0)
            {
                return RedirectToAction("Index", "EndScreen", new { deviceNumber });

            }

            showRankingListModel.FinalRankingList = true;
            ViewData["Title"] = utilities.Title("ShowFinalRankingList", TitleType.Location, deviceNumber);
            ViewData["Header"] = utilities.Header(HeaderType.Round, deviceNumber);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            if (database.IsIndividual)
            {
                return View("Individual", showRankingListModel);
            }
            else if (showRankingListModel.Exists(x => x.Orientation == "E"))
            {
                return View("TwoWinners", showRankingListModel);
            }
            else
            {
                return View("OneWinner", showRankingListModel);
            }
        }

        public JsonResult PollRanking(int deviceNumber)
        {
            int sectionID = appData.GetDeviceStatus(deviceNumber).SectionID;
            List<Ranking> rankingList = utilities.GetRankings(sectionID);
            return Json(rankingList);
        }
    }
}