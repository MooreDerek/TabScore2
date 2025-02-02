// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.Resources;

namespace TabScore2.Controllers
{
    public class SelectSectionController(IStringLocalizer<Strings> iLocalizer, IDatabase iDatabase) : Controller
    {
        private readonly IStringLocalizer<Strings> localizer = iLocalizer;
        private readonly IDatabase database = iDatabase;

        public ActionResult Index()
        {
            SelectSectionModel selectSectionModel = [];
            selectSectionModel.AddRange(database.GetSectionsList());
            // Check if only one section - if so use it
            if (selectSectionModel.Count == 1)
            {
                return RedirectToAction("Index", "SelectTableNumber", new { sectionId = selectSectionModel[0].SectionId });
            }
            else
            // Get section
            {
                ViewData["Title"] = localizer["SelectSection"];
                ViewData["Header"] = string.Empty;
                ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
                return View(selectSectionModel);
            }
        }
    }
}