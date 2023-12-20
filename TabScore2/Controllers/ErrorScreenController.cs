// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Resources;

namespace TabScore.Controllers
{
    public class ErrorScreenController(IStringLocalizer<Strings> iLocalizer, IDatabase iDatabase) : Controller
    {
        private readonly IStringLocalizer<Strings> localizer = iLocalizer;
        private readonly IDatabase database = iDatabase;
        
        public ActionResult Index()
        {
            ViewData["Title"] = localizer["ErrorScreen"];
            ViewData["Header"] = string.Empty;
            ViewData["ButtonOptions"] = ButtonOptions.OKEnabled;
            return View();
        }

        public ActionResult OKButtonClick()
        {
           if (database.IsDatabaseConnectionOK())  // Successful database read/write, so must have been a temporary glitch
            {
                return RedirectToAction("Index", "SelectSection");  // Need to re-establish Section/TableNumber/Direction for this tablet device
            }
            else  // Can't read/write to database after the error, so pass error to StartScreen and await database update 
            {
                TempData["WarningMessage"] = localizer["ErrorPermanentDB"].ToString();
                return RedirectToAction("Index", "StartScreen");
            }
        }
    }
}