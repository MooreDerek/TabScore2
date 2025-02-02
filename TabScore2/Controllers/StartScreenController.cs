// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Reflection;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Resources;

namespace TabScore2.Controllers
{
    public class StartScreenController(IStringLocalizer<Strings> iLocalizer, IDatabase iDatabase, ISettings iSettings) : Controller
    {
        private readonly IStringLocalizer<Strings> localizer = iLocalizer;
        private readonly IDatabase database = iDatabase;
        private readonly ISettings settings = iSettings;

        public ActionResult Index()
        {
            ViewData["Title"] = localizer["StartScreen"];
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