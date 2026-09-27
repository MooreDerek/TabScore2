// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.Classes;
using TabScore2.DataServices;

namespace TabScore2.Controllers
{
    // Scorer's view of the start-of-play check, at http://<TabScore2 PC>:5213/SessionStatus
    //
    // This page is strictly READ-ONLY and has no authentication: anyone on the club network who knows the address can
    // see table numbers, pair numbers and player names.  It must never gain actions that change AppData or the database.
    // Admin actions belong on the desktop SessionStatusForm, unless proper authentication is added here first.
    //
    // It deliberately uses nothing from the tablet flow: no session DeviceNumber, and no AppData method that creates state
    public class SessionStatusController(ISessionMonitor iSessionMonitor) : Controller
    {
        private readonly ISessionMonitor sessionMonitor = iSessionMonitor;

        public ActionResult Index(bool refresh = false)
        {
            SessionCheck check = sessionMonitor.GetStartOfPlayCheck(reloadLayout: refresh);
            return View(check);
        }
    }
}
