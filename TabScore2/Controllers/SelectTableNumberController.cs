// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.UtilityServices;

namespace TabScore2.Controllers
{
    public class SelectTableNumberController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities, ISettings iSettings, IHttpContextAccessor iHttpContextAccessor) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;
        private readonly ISettings settings = iSettings;
        private readonly IHttpContextAccessor httpContextAccessor = iHttpContextAccessor;

        public ActionResult Index(int sectionID, int tableNumber = 0, bool confirm = false) 
        {
            Section section = database.GetSection(sectionID);
            SelectTableNumberModel selectTableNumberModel = new(section, tableNumber, confirm);
            ViewData["Title"] = utilities.Title("SelectTableNumber", TitleType.Section, sectionID);
            ViewData["Header"] = utilities.Header(HeaderType.Section, sectionID);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
            return View(selectTableNumberModel);   
        }

        public ActionResult OKButtonClick(int sectionID, int tableNumber, bool confirm)
        {
            // Register table in database
            database.RegisterTable(sectionID, tableNumber);

            TableStatus tableStatus = appData.GetTableStatus(sectionID, tableNumber);  // Return value cannot be null as we've just set it
            database.GetRoundData(tableStatus);

            if (database.GetSection(sectionID).DevicesPerTable == 1)
            {
                // Check if tablet device is already registered for this location. One tablet device per table, so Direction defaults to North
                bool deviceStatusExists = appData.DeviceStatusExists(sectionID, tableNumber);
                if (deviceStatusExists && confirm)
                {
                    // Ok to change to this tablet, so set cookie
                    SetCookie(sectionID, tableNumber);
                }
                else if (deviceStatusExists)
                {
                    // Check if table number cookie has not been set - if so go back to confirm
                    if (!CheckCookie(sectionID, tableNumber))
                    {
                        return RedirectToAction("Index", "SelectTableNumber", new { sectionID, tableNumber, confirm = true });
                    }
                    // else = Cookie is Ok, so this is a re-registration and nothing more to do
                }
                else 
                {
                    // Not on list, so need to add it
                    appData.AddDeviceStatus(sectionID, tableNumber, tableStatus.RoundData.NumberNorth, tableStatus.RoundNumber);
                    SetCookie(sectionID, tableNumber);
                }
                DeviceStatus deviceStatus = appData.GetDeviceStatus(sectionID, tableNumber);

                // deviceNumber is the key for identifying this particular tablet device and is used throughout the rest of the application
                int deviceNumber = appData.GetDeviceNumber(deviceStatus);

                if (tableStatus.ReadyForNextRoundNorth)
                {
                    return RedirectToAction("Index", "ShowMove", new { deviceNumber, newRoundNumber = tableStatus.RoundNumber + 1 });
                }
                else if (deviceStatus.RoundNumber == 1 || settings.NumberEntryEachRound)
                {
                    return RedirectToAction("Index", "ShowPlayerIDs", new { deviceNumber });
                }
                else
                {
                    return RedirectToAction("Index", "ShowRoundInfo", new { deviceNumber });
                } 
            }
            else   // More than one tablet device per table, so need to know direction for this tablet device
            {
                return RedirectToAction("Index", "SelectDirection", new { sectionID, tableNumber });
            }
        }

        // Set a cookie for this device
        private void SetCookie(int sectionID, int tableNumber)
        {
            HttpContext? httpContext = httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Response.Cookies.Append("sectionID", sectionID.ToString());
                httpContext.Response.Cookies.Append("tableNumber", tableNumber.ToString());
            }
        }

        // Check if matching cookie set
        private bool CheckCookie(int sectionID, int tableNumber)
        {
            HttpContext? httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null) return false;
            IRequestCookieCollection iRequestCookieCollection = httpContext.Request.Cookies;
            bool cookieSectionIDExists = iRequestCookieCollection.TryGetValue("sectionID", out string? cookieSectionIDString);
            bool cookieTableNumberExists = iRequestCookieCollection.TryGetValue("tableNumber", out string? cookieTableNumberString);
            if (cookieSectionIDExists && cookieTableNumberExists && Convert.ToInt32(cookieSectionIDString) == sectionID && Convert.ToInt32(cookieTableNumberString) == tableNumber)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}