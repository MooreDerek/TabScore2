// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.AspNetCore.Mvc;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.UtilityServices;

namespace TabScore2.Controllers
{
    public class EnterPlayerIdController(IDatabase iDatabase, IExternalNamesDatabase iExternalNamesDatabase, IAppData iAppData, IUtilities iUtilities, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IExternalNamesDatabase externalNamesDatabase = iExternalNamesDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(Direction direction)
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            ViewData["Title"] = utilities.Title("EnterPlayerIds", deviceStatus);
            ViewData["Header"] = utilities.Header(HeaderType.Location, deviceStatus);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
            EnterPlayerIdModel enterPlayerIdModel = utilities.CreateEnterPlayerIdModel(direction);
            return View(enterPlayerIdModel);
        }

        public ActionResult OKButtonClick(Direction direction, string playerId)
        {
            int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
            if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

            string playerName = string.Empty;
            if (playerId == "0")
            {
                playerName = "Unknown";
            }
            else
            {
                switch (settings.NameSource)
                {
                    case 0:
                        playerName = database.GetInternalPlayerName(playerId);
                        break;
                    case 1:
                        playerName = externalNamesDatabase.GetExternalPlayerName(playerId);
                        break;
                    case 2:
                        playerName = string.Empty;
                        break;
                    case 3:
                        playerName = database.GetInternalPlayerName(playerId);
                        if (playerName == string.Empty || playerName.Contains('#') || playerName.Contains("Unknown"))
                        {
                            playerName = externalNamesDatabase.GetExternalPlayerName(playerId);
                        }
                        break;
                }
            }

            TableStatus tableStatus = appData.GetTableStatus(deviceStatus.SectionId, deviceStatus.TableNumber);
            string directionLetter = direction.ToString()[..1];    // Need just N, S, E or W
            int pairNumber = 0;
            switch (direction)
            {
                case Direction.North:
                    tableStatus.RoundData.NameNorth = playerName;
                    pairNumber = tableStatus.RoundData.NumberNorth;
                    break;
                case Direction.South:
                    tableStatus.RoundData.NameSouth = playerName;
                    pairNumber = tableStatus.RoundData.NumberSouth;
                    break;
                case Direction.East:
                    tableStatus.RoundData.NameEast = playerName;
                    pairNumber = tableStatus.RoundData.NumberEast;
                    break;
                case Direction.West:
                    tableStatus.RoundData.NameWest = playerName;
                    pairNumber = tableStatus.RoundData.NumberWest;
                    break;
            }
            database.UpdatePlayer(tableStatus.SectionId, tableStatus.TableNumber, tableStatus.RoundNumber, directionLetter, pairNumber, playerId, playerName);

            return RedirectToAction("Index", "ShowPlayerIds");
        }
    }
}