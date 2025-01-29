// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.UtilityServices;

namespace TabScore2.Controllers
{
    public class SelectSectionController(IDatabase iDatabase, IUtilities iUtilities) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IUtilities utilities = iUtilities;

        public ActionResult Index()
        {
            SelectSectionModel selectSectionModel = [];
            selectSectionModel.AddRange(database.GetSectionsList());
            // Check if only one section - if so use it
            if (selectSectionModel.Count == 1)
            {
                return RedirectToAction("Index", "SelectTableNumber", new { sectionID = selectSectionModel[0].ID });
            }
            else
            // Get section
            {
                ViewData["Title"] = utilities.Title("SelectSection");
                ViewData["Header"] = string.Empty;
                ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
                return View(selectSectionModel);
            }
        }
    }
}