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
    public class ShowTravellerController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(int tabletDeviceNumber, int boardNumber, bool fromView = false)
        {
            if (!settings.ShowTraveller)
            {
                return RedirectToAction("Index", "ShowBoards", new { tabletDeviceNumber });
            }

            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;
            
            // If ResultData is null, either from ShowBoards/View or browser 'Back' button, retrieve result
            tableStatus.ResultData ??= database.GetResult(tableStatus.SectionID, tableStatus.TableNumber, tableStatus.RoundNumber, boardNumber);
           
            ShowTraveller traveller = utilities.CreateShowTravellerModel(tabletDeviceNumber);
            traveller.FromView = fromView;

            if (settings.ShowTimer) ViewData["TimerSeconds"] = appData.GetTimerSeconds(tabletDeviceNumber);
            ViewData["Title"] = utilities.Title(tabletDeviceNumber, "ShowTraveller", TitleType.Location);
            ViewData["Header"] = utilities.Header(tabletDeviceNumber, HeaderType.FullColoured);
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
