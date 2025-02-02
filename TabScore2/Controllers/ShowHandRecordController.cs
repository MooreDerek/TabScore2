// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.UtilityServices;

namespace TabScore2.Controllers
{
    public class ShowHandRecordController(IAppData iAppData, IUtilities iUtilities) : Controller
    {
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;

        public ActionResult Index(int boardNumber, bool fromView)
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            ShowHandRecordModel? showHandRecordModel = utilities.CreateShowHandRecordModel(deviceStatus, boardNumber);
            if (showHandRecordModel == null) return RedirectToAction("Index", "ShowTraveller", new { boardNumber });

            showHandRecordModel.FromView = fromView;
            ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceStatus);
            ViewData["Title"] = utilities.Title("ShowHandRecord", deviceStatus);
            ViewData["Header"] = utilities.Header(HeaderType.FullColoured, deviceStatus);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            return View(showHandRecordModel);
        }
    }
}