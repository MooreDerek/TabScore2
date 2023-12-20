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

        public ActionResult Index(int tabletDeviceNumber)
        {
            TabletDeviceStatus tabletDeviceStatus = appData.GetTabletDeviceStatus(tabletDeviceNumber);
            if (settings.ShowRanking == 1 && tabletDeviceStatus.RoundNumber > 1)  // Show ranking list only from round 2 onwards
            {
                ShowRankingList showRankingList = utilities.CreateRankingListModel(tabletDeviceNumber);
                    
                // Only show the ranking list if it contains something meaningful
                if (showRankingList.Count > 1 && showRankingList[0].ScoreDecimal != 0.0)
                {
                    if (settings.ShowTimer) ViewData["TimerSeconds"] = appData.GetTimerSeconds(tabletDeviceNumber);
                    ViewData["Title"] = utilities.Title(tabletDeviceNumber, "ShowRankingList", TitleType.Location);
                    ViewData["Header"] = utilities.Header(tabletDeviceNumber, HeaderType.Round);
                    if (tabletDeviceStatus.AtSitoutTable)
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
            return RedirectToAction("Index", "ShowMove", new { tabletDeviceNumber, newRoundNumber = tabletDeviceStatus.RoundNumber + 1 });
        }

        public ActionResult Final(int tabletDeviceNumber)
        {
            TabletDeviceStatus tabletDeviceStatus = appData.GetTabletDeviceStatus(tabletDeviceNumber);
            ShowRankingList showRankingList = utilities.CreateRankingListModel(tabletDeviceNumber);
            if (showRankingList.Count <= 1 && showRankingList[0].ScoreDecimal == 0.0)
            {
                return RedirectToAction("Index", "EndScreen", new { tabletDeviceNumber });

            }

            showRankingList.FinalRankingList = true;
            ViewData["Title"] = utilities.Title(tabletDeviceNumber, "ShowFinalRankingList", TitleType.Location);
            ViewData["Header"] = utilities.Header(tabletDeviceNumber, HeaderType.Round);
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

        public JsonResult PollRanking(int tabletDeviceNumber)
        {
            List<Ranking> rankingList = utilities.GetRankings(tabletDeviceNumber);
            return Json(rankingList);
        }
    }
}