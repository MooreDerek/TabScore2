﻿// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
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

        public ActionResult Index(int boardNumber, bool fromView = false)
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            if (!settings.ShowTraveller)
            {
                return RedirectToAction("Index", "ShowBoards");
            }

            TableStatus tableStatus = appData.GetTableStatus(deviceStatus.SectionId, deviceStatus.TableNumber);

            // If ResultData doesn't exist, either from ShowBoards/View or browser 'Back' button, retrieve result
            if (tableStatus.ResultData.BoardNumber == 0)
            {
                tableStatus.ResultData = database.GetResult(tableStatus.SectionId, tableStatus.TableNumber, tableStatus.RoundNumber, boardNumber);
            }
           
            ShowTravellerModel showTravellerModel = utilities.CreateShowTravellerModel(deviceStatus);
            showTravellerModel.FromView = fromView;

            ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceStatus);
            ViewData["Title"] = utilities.Title("ShowTraveller", deviceStatus);
            ViewData["Header"] = utilities.Header(HeaderType.FullColoured, deviceStatus);
            if (fromView)
            {
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            }
            else
            {
                ViewData["ButtonOptions"] = ButtonOptions.OKEnabledAndBack;
            }

            if (settings.IsIndividual)
            {
                return View("Individual", showTravellerModel);
            }
            else
            {
                return View("Pairs", showTravellerModel);
            }
        }
    }
}
