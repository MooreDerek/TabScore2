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
    public class ShowHandRecordController(IAppData iAppData, IUtilities iUtilities, ISettings iSettings) : Controller
    {
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(int tabletDeviceNumber, int boardNumber, bool fromView)
        {
            ShowHandRecord? showHandRecord = utilities.CreateShowHandRecordModel(tabletDeviceNumber, boardNumber);
            if (showHandRecord == null) return RedirectToAction("Index", "ShowTraveller", new { tabletDeviceNumber, boardNumber });

            showHandRecord.FromView = fromView;
            TabletDeviceStatus tabletDeviceStatus = appData.GetTabletDeviceStatus(tabletDeviceNumber);
            showHandRecord.PerspectiveDirection = tabletDeviceStatus.PerspectiveDirection;
            showHandRecord.PerspectiveButtonOption = tabletDeviceStatus.PerspectiveButtonOption;

            if (settings.ShowTimer) ViewData["TimerSeconds"] = appData.GetTimerSeconds(tabletDeviceNumber);
            ViewData["Title"] = utilities.Title(tabletDeviceNumber, "ShowHandRecord", TitleType.Location);
            ViewData["Header"] = utilities.Header(tabletDeviceNumber, HeaderType.FullColoured);
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            return View(showHandRecord);
        }
    }
}