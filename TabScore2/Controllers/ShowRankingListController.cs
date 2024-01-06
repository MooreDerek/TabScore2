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
            if (settings.ShowRanking == 1 && deviceStatus.RoundNumber > 1)  // Show ranking list only from round 2 onwards
            {
                ShowRankingList showRankingList = utilities.CreateRankingListModel(deviceNumber);
                    
                // Only show the ranking list if it contains something meaningful
                if (showRankingList.Count > 1 && showRankingList[0].ScoreDecimal != 0.0)
                {
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
                        return View("Individual", showRankingList);
                    }
                    else if (showRankingList.Exists(x => x.Orientation == "E"))
                    {
                        return View("TwoWinners", showRankingList);
                    }
                    else
                    {
                        return View("OneWinner", showRankingList);
                    }
                }
            }
            return RedirectToAction("Index", "ShowMove", new { deviceNumber, newRoundNumber = deviceStatus.RoundNumber + 1 });
        }

        public ActionResult Final(int deviceNumber)
        {
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            ShowRankingList showRankingList = utilities.CreateRankingListModel(deviceNumber);
            if (showRankingList.Count <= 1 && showRankingList[0].ScoreDecimal == 0.0)
            {
                return RedirectToAction("Index", "EndScreen", new { deviceNumber });

            }

            showRankingList.FinalRankingList = true;
            ViewData["Title"] = utilities.Title("ShowFinalRankingList", TitleType.Location, deviceNumber);
            ViewData["Header"] = utilities.Header(HeaderType.Round, deviceNumber);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            if (database.IsIndividual)
            {
                return View("Individual", showRankingList);
            }
            else if (showRankingList.Exists(x => x.Orientation == "E"))
            {
                return View("TwoWinners", showRankingList);
            }
            else
            {
                return View("OneWinner", showRankingList);
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