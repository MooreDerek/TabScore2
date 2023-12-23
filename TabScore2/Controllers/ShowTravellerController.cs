// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.Models;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.UtilityServices;

namespace TabScore2.Controllers
{
    public class ShowTravellerController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(int deviceNumber, int boardNumber, bool fromView = false)
        {
            if (!settings.ShowTraveller)
            {
                return RedirectToAction("Index", "ShowBoards", new { deviceNumber });
            }

            TableStatus tableStatus = appData.GetTableStatus(deviceNumber);

            // If ResultData doesn't exist, either from ShowBoards/View or browser 'Back' button, retrieve result
            if (tableStatus.ResultData.BoardNumber == 0)
            {
                tableStatus.ResultData = database.GetResult(tableStatus.SectionID, tableStatus.TableNumber, tableStatus.RoundNumber, boardNumber);
            }
           
            ShowTraveller traveller = utilities.CreateShowTravellerModel(deviceNumber);
            traveller.FromView = fromView;

            if (settings.ShowTimer) ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceNumber);
            ViewData["Title"] = utilities.Title(deviceNumber, "ShowTraveller", TitleType.Location);
            ViewData["Header"] = utilities.Header(deviceNumber, HeaderType.FullColoured);
            if (fromView)
            {
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            }
            else
            {
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabledAndBack;
            }

            if (database.IsIndividual)
            {
                return View("Individual", traveller);
            }
            else
            {
                return View("Pairs", traveller);
            }
        }
    }
}
