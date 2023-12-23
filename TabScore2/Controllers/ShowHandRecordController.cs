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
    public class ShowHandRecordController(IAppData iAppData, IUtilities iUtilities, ISettings iSettings) : Controller
    {
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(int deviceNumber, int boardNumber, bool fromView)
        {
            ShowHandRecord? showHandRecord = utilities.CreateShowHandRecordModel(deviceNumber, boardNumber);
            if (showHandRecord == null) return RedirectToAction("Index", "ShowTraveller", new { deviceNumber, boardNumber });

            showHandRecord.FromView = fromView;
            DeviceStatus deviceStatus = appData.GetTabletDeviceStatus(deviceNumber);
            showHandRecord.PerspectiveDirection = deviceStatus.PerspectiveDirection;
            showHandRecord.PerspectiveButtonOption = deviceStatus.PerspectiveButtonOption;

            if (settings.ShowTimer) ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceNumber);
            ViewData["Title"] = utilities.Title(deviceNumber, "ShowHandRecord", TitleType.Location);
            ViewData["Header"] = utilities.Header(deviceNumber, HeaderType.FullColoured);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            return View(showHandRecord);
        }
    }
}