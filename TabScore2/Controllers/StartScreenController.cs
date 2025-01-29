// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.UtilityServices;

namespace TabScore2.Controllers
{
    public class StartScreenController(IDatabase iDatabase, IUtilities iUtilities, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IUtilities utilities = iUtilities;
        private readonly ISettings settings = iSettings;

        public ActionResult Index()
        {
            ViewData["Title"] = utilities.Title("StartScreen");
            ViewData["Header"] = string.Empty; 
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            ViewData["Version"] = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
            return View();
        }

        public ActionResult OKButtonClick()
        {
            if (!settings.DatabaseReady)
            {
                TempData["WarningMessage"] = "ErrorNoDBSelected";
                return RedirectToAction("Index", "StartScreen");
            }
            if (!settings.SessionStarted) 
            {
                database.WebappInitialize();    // Only runs once at the start of the session
                settings.SessionStarted = true;
            }
            return RedirectToAction("Index", "SelectSection");
        }
    }
}