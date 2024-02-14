// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.UtilityServices;

namespace TabScore.Controllers
{
    public class SelectDirectionController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities, IHttpContextAccessor iHttpContextAccessor) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;
        private readonly IHttpContextAccessor httpContextAccessor = iHttpContextAccessor;

        public ActionResult Index(int sectionID, int tableNumber, Direction direction = Direction.Null, bool confirm = false) 
        {
            TableStatus tableStatus = appData.GetTableStatus(sectionID, tableNumber);
            Section section = database.GetSection(sectionID);
            SelectDirectionModel selectDirectionModel = new(tableStatus, section, direction, confirm);

            ViewData["Title"] = utilities.Title("SelectDirection", TitleType.SectionTable, sectionID, tableNumber);
            ViewData["Header"] = utilities.Header(HeaderType.SectionTable, sectionID, tableNumber);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
            if (database.IsIndividual)
            {
                return View("Individual", selectDirectionModel);
            }
            else
            {
                return View("Pair", selectDirectionModel);
            }
        }

        public ActionResult OKButtonClick(int sectionID, int tableNumber, Direction direction, int roundNumber, bool confirm)
        {
            TableStatus tableStatus = appData.GetTableStatus(sectionID, tableNumber);

            // Check if device is already registered for this location
            if (appData.DeviceStatusExists(sectionID, tableNumber, direction) && confirm)
            {
                // Ok to change to this tablet, so set cookie
                SetCookie(sectionID, tableNumber, direction);
            }
            else if (appData.DeviceStatusExists(sectionID, tableNumber, direction))
            {
                // Check if table number cookie has not been set - if so go back to confirm
                if (!CheckCookie(sectionID, tableNumber, direction))
                {
                    return RedirectToAction("Index", "SelectDirection", new { sectionID, tableNumber, roundNumber, direction, confirm = true });
                }
                // else = Cookie is Ok, so this is a re-registration and nothing more to do
            }
            else
            {
                // Not on list of registered devices, so need to add it
                int pairNumber = 0;
                if (direction == Direction.North)
                {
                    pairNumber = tableStatus.RoundData.NumberNorth;
                }
                else if (direction == Direction.East)
                {
                    pairNumber = tableStatus.RoundData.NumberEast;
                }
                else if (direction == Direction.South)
                {
                    pairNumber = tableStatus.RoundData.NumberSouth;
                }
                else if (direction == Direction.West)
                {
                    pairNumber = tableStatus.RoundData.NumberWest;
                }
                appData.AddDeviceStatus(sectionID, tableNumber, pairNumber, roundNumber, direction);
                SetCookie(sectionID, tableNumber, direction);
            }
            DeviceStatus deviceStatus = appData.GetDeviceStatus(sectionID, tableNumber, direction);

            // deviceNumber is the key for identifying this particular device and is used throughout the rest of the application
            int deviceNumber = appData.GetDeviceNumber(deviceStatus);

            if (((direction == Direction.North) && tableStatus.ReadyForNextRoundNorth) || ((direction == Direction.East) && tableStatus.ReadyForNextRoundEast) || (direction == Direction.South && tableStatus.ReadyForNextRoundSouth) || (direction == Direction.West && tableStatus.ReadyForNextRoundWest))
            {
                return RedirectToAction("Index", "ShowMove", new { deviceNumber, newRoundNumber = roundNumber + 1 });
            }
            else
            {
                return RedirectToAction("Index", "ShowPlayerIDs", new { deviceNumber });
            }
        }

        // Set a cookie for this device
        private void SetCookie(int sectionID, int tableNumber, Direction direction)
        {
            HttpContext? httpContext = httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Response.Cookies.Append("sectionID", sectionID.ToString());
                httpContext.Response.Cookies.Append("tableNumber", tableNumber.ToString());
                httpContext.Response.Cookies.Append("direction", direction.ToString());
            }
        }

        // Check if matching cookie set
        private bool CheckCookie(int sectionID, int tableNumber, Direction direction)
        {
            HttpContext? httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null) return false;
            IRequestCookieCollection iRequestCookieCollection = httpContext.Request.Cookies;
            bool cookieSectionIDExists = iRequestCookieCollection.TryGetValue("sectionID", out string? cookieSectionIDString);
            bool cookieTableNumberExists = iRequestCookieCollection.TryGetValue("tableNumber", out string? cookieTableNumberString);
            bool cookieDirectionExists = iRequestCookieCollection.TryGetValue("direction", out string? cookieDirectionString);
            if (cookieSectionIDExists && cookieTableNumberExists && cookieDirectionExists && Convert.ToInt32(cookieSectionIDString) == sectionID && Convert.ToInt32(cookieTableNumberString) == tableNumber && direction.ToString() == cookieDirectionString)
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