// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.UtilityServices;

namespace TabScore.Controllers
{
    public class ErrorScreenController(IDatabase iDatabase, IUtilities iUtilities) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IUtilities utilities = iUtilities;
        
        public ActionResult Index()
        {
            ViewData["Title"] = utilities.Title("ErrorScreen");
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
                TempData["WarningMessage"] = "ErrorPermanentDB";
                return RedirectToAction("Index", "StartScreen");
            }
        }
    }
}