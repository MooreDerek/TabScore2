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
    public class EnterPlayerIDController(IDatabase iDatabase, IExternalNamesDatabase iExternalNamesDatabase, IAppData iAppData, IUtilities iUtilities, ISettings iSettings) : Controller
    {
        private readonly IDatabase database = iDatabase;
        private readonly IExternalNamesDatabase externalNamesDatabase = iExternalNamesDatabase;
        private readonly IAppData appData = iAppData;
        private readonly IUtilities utilities = iUtilities;
        private readonly ISettings settings = iSettings;

        public ActionResult Index(int tabletDeviceNumber, Direction direction)
        {
            ViewData["Title"] = utilities.Title(tabletDeviceNumber, "EnterPlayerIDs", TitleType.Location);
            ViewData["Header"] = utilities.Header(tabletDeviceNumber, HeaderType.Location);
            ViewData["ButtonOptions"] = ButtonOptions.OKDisabled;
            EnterPlayerID enterPlayerID = new(tabletDeviceNumber, direction);
            return View(enterPlayerID);
        }

        public ActionResult OKButtonClick(int tabletDeviceNumber, Direction direction, string playerID)
        {
            string playerName = string.Empty;
            if (playerID == "0")
            {
                playerName = "Unknown";
            }
            else
            {
                switch (settings.NameSource)
                {
                    case 0:
                        playerName = database.GetInternalPlayerName(playerID);
                        break;
                    case 1:
                        playerName = externalNamesDatabase.GetExternalPlayerName(playerID);
                        break;
                    case 2:
                        playerName = "";
                        break;
                    case 3:
                        playerName = database.GetInternalPlayerName(playerID);
                        if (playerName == "" || playerName.Contains('#') || playerName.Contains("Unknown"))
                        {
                            playerName = externalNamesDatabase.GetExternalPlayerName(playerID);
                        }
                        break;
                }
            }

            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;
            switch (direction)
            {
                case Direction.North:
                    tableStatus.RoundData.NameNorth = playerName;
                    tableStatus.RoundData.PlayerIDNorth = playerID;
                    break;
                case Direction.South:
                    tableStatus.RoundData.NameSouth = playerName;
                    tableStatus.RoundData.PlayerIDSouth = playerID;
                    break;
                case Direction.East:
                    tableStatus.RoundData.NameEast = playerName;
                    tableStatus.RoundData.PlayerIDEast = playerID;
                    break;
                case Direction.West:
                    tableStatus.RoundData.NameWest = playerName;
                    tableStatus.RoundData.PlayerIDWest = playerID;
                    break;
            }
            database.UpdatePlayer(tableStatus, direction, playerID, playerName);

            return RedirectToAction("Index", "ShowPlayerIDs", new { tabletDeviceNumber });
        }
    }
}