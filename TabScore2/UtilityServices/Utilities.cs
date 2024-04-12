﻿// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.Extensions.Localization;
using System.Text;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.Resources;
using TabScore2.SharedClasses;
using static System.Formats.Asn1.AsnWriter;

namespace TabScore2.UtilityServices
{
    public class Utilities(IStringLocalizer<Strings> iLocalizer, IDatabase iDatabase, IAppData iAppData, ISettings iSettings) : IUtilities
    {
        private readonly IStringLocalizer<Strings> localizer = iLocalizer;
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly ISettings settings = iSettings;
        private static readonly char[] arbitralScoreSeparators = ['%', '-'];

        // PUBLIC CLASSES TO CREATE VIEW MODELS
        public ShowPlayerIDsModel CreateShowPlayerIDsModel(int deviceNumber, bool showWarning)
        {
            ShowPlayerIDsModel showPlayerIDsModel = new(deviceNumber, showWarning);
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
            Round round = tableStatus.RoundData;
            Section section = database.GetSection(deviceStatus.SectionID);

            if (section.DevicesPerTable == 1)
            {
                if (round.NumberNorth != 0 && round.NumberNorth != section.MissingPair)
                {
                    showPlayerIDsModel.Add(CreatePlayerEntry(round, Direction.North));
                    showPlayerIDsModel.Add(CreatePlayerEntry(round, Direction.South));
                }
                if (round.NumberEast != 0 && round.NumberEast != section.MissingPair)
                {
                    showPlayerIDsModel.Add(CreatePlayerEntry(round, Direction.East));
                    showPlayerIDsModel.Add(CreatePlayerEntry(round, Direction.West));
                }
            }
            else if (section.DevicesPerTable == 2)
            {
                if (deviceStatus.Direction == Direction.North)
                {
                    showPlayerIDsModel.Add(CreatePlayerEntry(round, Direction.North));
                    showPlayerIDsModel.Add(CreatePlayerEntry(round, Direction.South));
                }
                else
                {
                    showPlayerIDsModel.Add(CreatePlayerEntry(round, Direction.East));
                    showPlayerIDsModel.Add(CreatePlayerEntry(round, Direction.West));
                }
            }
            else  // tabletDevicesPerTable == 4
            {
                showPlayerIDsModel.Add(CreatePlayerEntry(round, deviceStatus.Direction));
            }

            showPlayerIDsModel.NumberOfBlankEntries = showPlayerIDsModel.FindAll(x => x.DisplayName == string.Empty).Count;
            showPlayerIDsModel.ShowMessage = section.DevicesPerTable == 4 || (section.DevicesPerTable == 2 && showPlayerIDsModel.Count == 4);
            return showPlayerIDsModel;
        }

        public EnterPlayerIDModel CreateEnterPlayerIDModel(int deviceNumber, Direction direction)
        {
            EnterPlayerIDModel enterPlayerIDModel = new(deviceNumber, direction)
            {
                DisplayDirection = localizer[direction.ToString()]
            };
            return enterPlayerIDModel;
        }

        public ShowRoundInfoModel CreateShowRoundInfoModel(int deviceNumber) 
        {
            TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
            Round round = tableStatus.RoundData;
            string unknown = localizer["Unknown"];
            return new(deviceNumber)
            {
                RoundNumber = tableStatus.RoundNumber,
                NumberNorth = round.NumberNorth,
                NumberEast = round.NumberEast,
                NumberSouth = round.NumberSouth,
                NumberWest = round.NumberWest,
                DisplayNameNorth = round.NameNorth.Replace("Unknown", unknown),
                DisplayNameSouth = round.NameSouth.Replace("Unknown", unknown),
                DisplayNameEast = round.NameEast.Replace("Unknown", unknown),
                DisplayNameWest = round.NameWest.Replace("Unknown", unknown),
                LowBoard = round.LowBoard,
                HighBoard = round.HighBoard
            };
        }

        public ShowBoardsModel CreateShowBoardsModel(int deviceNumber)
        {
            TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
            List<Result> resultsList = database.GetResultsList(tableStatus.SectionID, tableStatus.RoundData.LowBoard, tableStatus.RoundData.HighBoard, tableStatus.TableNumber, tableStatus.RoundNumber);

            ShowBoardsModel showBoardsModel = new(deviceNumber, settings.ShowTraveller);
            foreach (Result result in resultsList) 
            {
                showBoardsModel.Add(new ShowBoardsResult(result.BoardNumber, result.ContractLevel, GetContractDisplay(result, true), result.Remarks));
            }

            // Check to see if any boards don't have a result, and add dummies to the list
            for (int iBoard = tableStatus.RoundData.LowBoard; iBoard <= tableStatus.RoundData.HighBoard; iBoard++)
            {
                if (showBoardsModel.Find(x => x.BoardNumber == iBoard) == null)
                {
                    ShowBoardsResult showBoardsResult = new(iBoard, -999, string.Empty, string.Empty);
                    showBoardsModel.Add(showBoardsResult);
                    showBoardsModel.GotAllResults = false;
                }
            }
            showBoardsModel.Sort((x, y) => x.BoardNumber.CompareTo(y.BoardNumber));
            return showBoardsModel;
        }

        public ShowMoveModel CreateShowMoveModel(int deviceNumber, int newRoundNumber, int tableNotReadyNumber)
        {
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            Section section = database.GetSection(deviceStatus.SectionID);

            ShowMoveModel showMoveModel = [];
            showMoveModel.TabletDeviceNumber = deviceNumber;
            showMoveModel.Direction = deviceStatus.Direction;
            showMoveModel.NewRoundNumber = newRoundNumber;
            showMoveModel.TableNotReadyNumber = tableNotReadyNumber;
            showMoveModel.TabletDevicesPerTable = section.DevicesPerTable;
            int missingPair = section.MissingPair;

            List<Round> roundsList = database.GetRoundsList(deviceStatus.SectionID, newRoundNumber);
            if (section.DevicesPerTable == 1)
            {
                TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
                if (settings.IsIndividual)
                {
                    if (tableStatus.RoundData.NumberNorth != 0)
                    {
                        showMoveModel.Add(GetMove(roundsList, tableStatus.TableNumber, tableStatus.RoundData.NumberNorth, Direction.North));
                    }
                    if (tableStatus.RoundData.NumberSouth != 0)
                    {
                        showMoveModel.Add(GetMove(roundsList, tableStatus.TableNumber, tableStatus.RoundData.NumberSouth, Direction.South));
                    }
                    if (tableStatus.RoundData.NumberEast != 0)
                    {
                        showMoveModel.Add(GetMove(roundsList, tableStatus.TableNumber, tableStatus.RoundData.NumberEast, Direction.East));
                    }
                    if (tableStatus.RoundData.NumberWest != 0)
                    {
                        showMoveModel.Add(GetMove(roundsList, tableStatus.TableNumber, tableStatus.RoundData.NumberWest, Direction.West));
                    }
                }
                else  // Not individual
                {
                    if (tableStatus.RoundData.NumberNorth != 0 && tableStatus.RoundData.NumberNorth != missingPair)
                    {
                        showMoveModel.Add(GetMove(roundsList, tableStatus.TableNumber, tableStatus.RoundData.NumberNorth, Direction.North));
                    }
                    if (tableStatus.RoundData.NumberEast != 0 && tableStatus.RoundData.NumberEast != missingPair)
                    {
                        showMoveModel.Add(GetMove(roundsList, tableStatus.TableNumber, tableStatus.RoundData.NumberEast, Direction.East));
                    }
                }
            }
            else  // TabletDevicesPerTable > 1, so only need move for single player/pair.  Could be at phantom table, so use deviceStatus info
            {
                showMoveModel.Add(GetMove(roundsList, deviceStatus.TableNumber, deviceStatus.PairNumber, deviceStatus.Direction));
            }

            showMoveModel.BoardsNewTable = -999;  // Default is not to show boards move
            if (deviceStatus.TableNumber != 0)    // If at a phantom table, there are no boards to worry about
            {
                // Show boards move only to North (or North/South) unless missing, in which case only show to East (or East/West)
                TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
                showMoveModel.LowBoard = tableStatus.RoundData.LowBoard;
                showMoveModel.HighBoard = tableStatus.RoundData.HighBoard;
                if (showMoveModel.Direction == Direction.North || ((tableStatus.RoundData.NumberNorth == 0 || tableStatus.RoundData.NumberNorth == missingPair) && showMoveModel.Direction == Direction.East))
                {
                    showMoveModel.BoardsNewTable = GetBoardsNewTableNumber(roundsList, tableStatus.TableNumber, tableStatus.RoundData.LowBoard);
                }
            }
            return showMoveModel;
        }

        public EnterContractModel CreateEnterContractModel(int deviceNumber, Result result, bool showTricks = false, LeadValidationOptions leadValidation = LeadValidationOptions.NoWarning)
        {
            EnterContractModel enterContractModel = new(deviceNumber)
            {
                BoardNumber = result.BoardNumber,
                ContractLevel = result.ContractLevel,
                ContractSuit = result.ContractSuit,
                ContractX = result.ContractX,
                DeclarerNSEW = result.DeclarerNSEW,
                LeadCard = result.LeadCard,
                TricksTaken = result.TricksTaken,
                LeadValidation = leadValidation,
                Score = result.Score,
                DeclarerNSEWDisplay = localizer[result.DeclarerNSEW],
                ContractDisplay = GetContractDisplay(result, showTricks)
            };
            return enterContractModel;
        }

        public ShowTravellerModel CreateShowTravellerModel(int deviceNumber)
        {
            TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
            int currentBoardNumber = tableStatus.ResultData!.BoardNumber; 
            ShowTravellerModel showTravellerModel = new(deviceNumber, currentBoardNumber);
            List<Result> resultsList = database.GetResultsList(tableStatus.SectionID, currentBoardNumber);

            // Set maximum match points based on total number of results (including Not Played) for Neuberg formula
            int resultsForThisBoard = resultsList.Count;
            int matchPointsMax = 2 * resultsForThisBoard - 2;

            // Get list of all scorable results for matchpoint calculation
            List<Result> resultsWithContractList = resultsList.FindAll(x => x.ContractLevel >= 0);
            int scoresForThisBoard = resultsWithContractList.Count;

            foreach (Result result in resultsList)
            {
                TravellerResult travellerResult = new()
                {
                    NumberNorth = result.NumberNorth,
                    NumberEast = result.NumberEast,
                    NumberSouth = result.NumberSouth,
                    NumberWest = result.NumberWest
                };
                if (result.ContractLevel < 0)
                {
                    // No scorable contract, so look for arbitral percentages
                    string[] temp = result.Remarks.Split(arbitralScoreSeparators, StringSplitOptions.RemoveEmptyEntries);
                    if (temp.Length == 2 && uint.TryParse(temp[0], out uint tempInt0) && uint.TryParse(temp[1], out uint tempInt1))
                    {
                        travellerResult.ScoreNS = $"<span style=\"color:red\">{temp[0]}%</span>";
                        travellerResult.ScoreEW = $"<span style=\"color:red\">{temp[1]}%</span>";
                        travellerResult.SortPercentage = Convert.ToDouble(temp[0]);
                    }
                    else  // Can't work out percentage
                    {
                        travellerResult.ScoreNS = travellerResult.ScoreEW = "<span style=\"color:red\">???</span>";
                        travellerResult.SortPercentage = 0.0;
                    }
                }
                else  // Genuine result
                {
                    if (result.ContractLevel == 0)
                    {
                        travellerResult.DisplayContract = $"<span style=\"color:darkgreen\">{localizer["Pass"]}</span>";
                    }
                    else if (result.ContractLevel > 0)
                    {
                        StringBuilder s = new();
                        s.Append(result.ContractLevel);
                        switch (result.ContractSuit)
                        {
                            case "NT":
                                s.Append(localizer["NT"]);
                                break;
                            case "S":
                                s.Append("<span style=\"color:black\">&spades;</span>");
                                break;
                            case "H":
                                s.Append("<span style=\"color:red\">&hearts;</span>");
                                break;
                            case "D":
                                s.Append("<span style=\"color:lightsalmon\">&diams;</span>");
                                break;
                            case "C":
                                s.Append("<span style=\"color:lightslategrey\">&clubs;</span>");
                                break;
                        }
                        s.Append(result.ContractX);
                        s.Append(result.TricksTakenSymbol);
                        travellerResult.DisplayContract = s.ToString();
                        travellerResult.DisplayDeclarerNSEW = localizer[result.DeclarerNSEW];

                        if (result.LeadCard != string.Empty && result.LeadCard != "SKIP")
                        {
                            travellerResult.DisplayLeadCard = result.LeadCard.Replace("S", "<span style=\"color:black\">&spades;</span>")
                                    .Replace("H", "<span style=\"color:red\">&hearts;</span>")
                                    .Replace("D", "<span style=\"color:lightsalmon\">&diams;</span>")
                                    .Replace("C", "<span style=\"color:lightslategrey\">&clubs;</span>")
                                    .Replace("T", localizer["TenShorthand"]);
                        }
                    }
                    CalculateScore(result);
                    if (result.Score > 0)
                    {
                        travellerResult.ScoreNS = result.Score.ToString();
                    }
                    else if (result.Score < 0)
                    {
                        travellerResult.ScoreEW = (-result.Score).ToString();
                    }

                    if (matchPointsMax == 0)
                    {
                        travellerResult.SortPercentage = 50.0;
                    }
                    else
                    {
                        // Apply Neuberg formula here
                        int matchpoints = 2 * resultsWithContractList.FindAll(x => x.Score < result.Score).Count + resultsWithContractList.FindAll(x => x.Score == result.Score).Count - 1;
                        double neuberg = ((matchpoints + 1) * resultsForThisBoard / (double)scoresForThisBoard) - 1.0;
                        travellerResult.SortPercentage = neuberg / matchPointsMax * 100.0;
                    }

                    if (result.Remarks == "Wrong direction")  // Swap pairs
                    {
                        (travellerResult.NumberEast, travellerResult.NumberNorth) = (travellerResult.NumberNorth, travellerResult.NumberEast);
                        (travellerResult.NumberWest, travellerResult.NumberSouth) = (travellerResult.NumberSouth, travellerResult.NumberWest);
                    }
                    if (result.NumberNorth == tableStatus.RoundData.NumberNorth)
                    {
                        int intPercentageNS = Convert.ToInt32(travellerResult.SortPercentage);
                        showTravellerModel.PercentageNS = Convert.ToString(intPercentageNS) + "%";
                        showTravellerModel.PercentageEW = Convert.ToString(100 - intPercentageNS) + "%";
                        travellerResult.Highlight = true;
                    }
                }
                showTravellerModel.Add(travellerResult);
            }

            showTravellerModel.Sort((x, y) => y.SortPercentage.CompareTo(x.SortPercentage));  // Sort traveller into descending percentage order
            if (!settings.ShowPercentage) showTravellerModel.PercentageNS = string.Empty;   // Don't show percentage

            // Determine if there is a hand record to view
            if (settings.ShowHandRecord && database.GetHandsCount() > 0)
            {
                Hand hand = database.GetHand(tableStatus.SectionID, currentBoardNumber);
                if (hand.NorthSpades != "###")
                {
                    showTravellerModel.HandRecord = true;
                }
                else if (tableStatus.SectionID > 1)   // Can't find matching hand record, so try default SectionID = 1
                {
                    hand = database.GetHand(1, currentBoardNumber);
                    if (hand.NorthSpades != "###")
                    {
                        showTravellerModel.HandRecord = true;
                    }
                }
            }
            return showTravellerModel;
        }

        public ShowHandRecordModel? CreateShowHandRecordModel(int deviceNumber, int boardNumber)
        {
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            Hand hand = database.GetHand(deviceStatus.SectionID, boardNumber);
            if (hand.NorthSpades == "###")
            {
                hand = database.GetHand(1, boardNumber);
                if (hand.NorthSpades == "###") return null;
            }

            string dealer = ((boardNumber - 1) % 4) switch
            {
                0 => (string)localizer["N"],
                1 => (string)localizer["E"],
                2 => (string)localizer["S"],
                3 => (string)localizer["W"],
                _ => "#",
            };
            ShowHandRecordModel showHandRecordModel = new(deviceNumber, boardNumber, dealer);
            // Set dealer based on board number
            string A = localizer["A"];
            string K = localizer["K"];
            string Q = localizer["Q"];
            string J = localizer["J"];
            string tenShorthand = localizer["TenShorthand"];
            showHandRecordModel.NorthSpadesDisplay = hand.NorthSpades.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", tenShorthand);
            showHandRecordModel.NorthHeartsDisplay = hand.NorthHearts.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", tenShorthand);
            showHandRecordModel.NorthDiamondsDisplay = hand.NorthDiamonds.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", tenShorthand);
            showHandRecordModel.NorthClubsDisplay = hand.NorthClubs.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", tenShorthand);
            showHandRecordModel.EastSpadesDisplay = hand.EastSpades.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", tenShorthand);
            showHandRecordModel.EastHeartsDisplay = hand.EastHearts.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", tenShorthand);
            showHandRecordModel.EastDiamondsDisplay = hand.EastDiamonds.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", tenShorthand);
            showHandRecordModel.EastClubsDisplay = hand.EastClubs.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", tenShorthand);
            showHandRecordModel.SouthSpadesDisplay = hand.SouthSpades.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", tenShorthand);
            showHandRecordModel.SouthHeartsDisplay = hand.SouthHearts.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", tenShorthand);
            showHandRecordModel.SouthDiamondsDisplay = hand.SouthDiamonds.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", tenShorthand);
            showHandRecordModel.SouthClubsDisplay = hand.SouthClubs.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", tenShorthand);
            showHandRecordModel.WestSpadesDisplay = hand.WestSpades.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", tenShorthand);
            showHandRecordModel.WestHeartsDisplay = hand.WestHearts.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", tenShorthand);
            showHandRecordModel.WestDiamondsDisplay = hand.WestDiamonds.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", tenShorthand);
            showHandRecordModel.WestClubsDisplay = hand.WestClubs.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", tenShorthand);

            HandEvaluation? handEvaluation = appData.GetHandEvaluation(deviceStatus.SectionID, boardNumber);
            if (handEvaluation == null) return showHandRecordModel;

            showHandRecordModel.EvalNorthNT = handEvaluation.NorthNotrump > 6 ? (handEvaluation.NorthNotrump - 6).ToString() : string.Empty;
            showHandRecordModel.EvalNorthSpades = handEvaluation.NorthSpades > 6 ? (handEvaluation.NorthSpades - 6).ToString() : string.Empty;
            showHandRecordModel.EvalNorthHearts = handEvaluation.NorthHearts > 6 ? (handEvaluation.NorthHearts - 6).ToString() : string.Empty;
            showHandRecordModel.EvalNorthDiamonds = handEvaluation.NorthDiamonds > 6 ? (handEvaluation.NorthDiamonds - 6).ToString() : string.Empty;
            showHandRecordModel.EvalNorthClubs = handEvaluation.NorthClubs > 6 ? (handEvaluation.NorthClubs - 6).ToString() : string.Empty;
            showHandRecordModel.EvalEastNT = handEvaluation.EastNotrump > 6 ? (handEvaluation.EastNotrump - 6).ToString() : string.Empty;
            showHandRecordModel.EvalEastSpades = handEvaluation.EastSpades > 6 ? (handEvaluation.EastSpades - 6).ToString() : string.Empty;
            showHandRecordModel.EvalEastHearts = handEvaluation.EastHearts > 6 ? (handEvaluation.EastHearts - 6).ToString() : string.Empty;
            showHandRecordModel.EvalEastDiamonds = handEvaluation.EastDiamonds > 6 ? (handEvaluation.EastDiamonds - 6).ToString() : string.Empty;
            showHandRecordModel.EvalEastClubs = handEvaluation.EastClubs > 6 ? (handEvaluation.EastClubs - 6).ToString() : string.Empty;
            showHandRecordModel.EvalSouthNT = handEvaluation.SouthNotrump > 6 ? (handEvaluation.SouthNotrump - 6).ToString() : string.Empty;
            showHandRecordModel.EvalSouthSpades = handEvaluation.SouthSpades > 6 ? (handEvaluation.SouthSpades - 6).ToString() : string.Empty;
            showHandRecordModel.EvalSouthHearts = handEvaluation.SouthHearts > 6 ? (handEvaluation.SouthHearts - 6).ToString() : string.Empty;
            showHandRecordModel.EvalSouthDiamonds = handEvaluation.SouthDiamonds > 6 ? (handEvaluation.SouthDiamonds - 6).ToString() : string.Empty;
            showHandRecordModel.EvalSouthClubs = handEvaluation.SouthClubs > 6 ? (handEvaluation.SouthClubs - 6).ToString() : string.Empty;
            showHandRecordModel.EvalWestNT = handEvaluation.WestNotrump > 6 ? (handEvaluation.WestNotrump - 6).ToString() : string.Empty;
            showHandRecordModel.EvalWestSpades = handEvaluation.WestSpades > 6 ? (handEvaluation.WestSpades - 6).ToString() : string.Empty;
            showHandRecordModel.EvalWestHearts = handEvaluation.WestHearts > 6 ? (handEvaluation.WestHearts - 6).ToString() : string.Empty;
            showHandRecordModel.EvalWestDiamonds = handEvaluation.WestDiamonds > 6 ? (handEvaluation.WestDiamonds - 6).ToString() : string.Empty;
            showHandRecordModel.EvalWestClubs = handEvaluation.WestClubs > 6 ? (handEvaluation.WestClubs - 6).ToString() : string.Empty;

            showHandRecordModel.HCPNorth = handEvaluation.NorthHcp;
            showHandRecordModel.HCPSouth = handEvaluation.SouthHcp;
            showHandRecordModel.HCPEast = handEvaluation.EastHcp;
            showHandRecordModel.HCPWest = handEvaluation.WestHcp;

            // Set perspective options based on combination of settings and number of devices per table
            Section section = database.GetSection(deviceStatus.SectionID);
            if (section.DevicesPerTable == 4)
            {
                showHandRecordModel.PerspectiveButtonOption = HandRecordPerspectiveButtonOptions.None;
                showHandRecordModel.PerspectiveFromDirection = deviceStatus.Direction.ToString();
            }
            else if (section.DevicesPerTable == 2)
            {
                if (deviceStatus.Direction == Direction.North)
                {
                    showHandRecordModel.PerspectiveButtonOption = HandRecordPerspectiveButtonOptions.NS;
                    if (settings.ShowHandRecordFromDirection == "North")
                    {
                        showHandRecordModel.PerspectiveFromDirection = "North";
                    }
                    else
                    {
                        showHandRecordModel.PerspectiveFromDirection = "South";
                    }
                }
                else
                {
                    showHandRecordModel.PerspectiveButtonOption = HandRecordPerspectiveButtonOptions.EW;
                    if (settings.ShowHandRecordFromDirection == "East")
                    {
                        showHandRecordModel.PerspectiveFromDirection = "East";
                    }
                    else
                    {
                        showHandRecordModel.PerspectiveFromDirection = "West";
                    }
                }
            }
            else  // DevicesPerTable == 1
            {
                showHandRecordModel.PerspectiveButtonOption = HandRecordPerspectiveButtonOptions.NSEW;
                showHandRecordModel.PerspectiveFromDirection = settings.ShowHandRecordFromDirection;
            }
            return showHandRecordModel;
        }

        public ShowRankingListModel CreateRankingListModel(int deviceNumber)
        {
            DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
            ShowRankingListModel showRankingListModel = [];
            showRankingListModel.TabletDeviceNumber = deviceNumber;
            showRankingListModel.RoundNumber = deviceStatus.RoundNumber;

            // Set player numbers to highlight appropriate rows of ranking list
            if (database.GetSection(deviceStatus.SectionID).DevicesPerTable == 1)
            {
                TableStatus tableStatus = appData.GetTableStatus(deviceNumber);
                showRankingListModel.NumberNorth = tableStatus.RoundData.NumberNorth;
                showRankingListModel.NumberEast = tableStatus.RoundData.NumberEast;
                showRankingListModel.NumberSouth = tableStatus.RoundData.NumberSouth;
                showRankingListModel.NumberWest = tableStatus.RoundData.NumberWest;
            }
            else  // More than one tablet device per table
            {
                // Only need to highlight one row entry, so use NumberNorth as proxy
                showRankingListModel.NumberNorth = deviceStatus.PairNumber;
            }

            showRankingListModel.AddRange(GetRankings(deviceStatus.SectionID));
            return showRankingListModel;
        }


        // OTHER PUBLIC UTLITY CLASSES
        public Move GetMove(List<Round> roundsList, int tableNumber, int pairNumber, Direction direction)
        {
            Move move = new(pairNumber);
            Round? round;
            if (settings.IsIndividual)
            {
                move.DirectionString = direction.ToString();
                // Try Direction = North
                round = roundsList.Find(x => x.NumberNorth == pairNumber);
                if (round != null)
                {
                    move.NewTableNumber = round.TableNumber;
                    move.NewDirection = Direction.North;
                    move.NewDirectionString = "North";
                    move.Stay = (move.NewTableNumber == tableNumber && direction == Direction.North);
                    if (round.NumberEast == 0) move.NewTableIsSitout = true;
                    return move;
                }

                // Try Direction = South
                round = roundsList.Find(x => x.NumberSouth == pairNumber);
                if (round != null)
                {
                    move.NewTableNumber = round.TableNumber;
                    move.NewDirection = Direction.South;
                    move.NewDirectionString = "South";
                    move.Stay = (move.NewTableNumber == tableNumber && direction == Direction.South);
                    if (round.NumberEast == 0) move.NewTableIsSitout = true;
                    return move;
                }

                // Try Direction = East
                round = roundsList.Find(x => x.NumberEast == pairNumber);
                if (round != null)
                {
                    move.NewTableNumber = round.TableNumber;
                    move.NewDirection = Direction.East;
                    move.NewDirectionString = "East";
                    move.Stay = (move.NewTableNumber == tableNumber && direction == Direction.East);
                    if (round.NumberNorth == 0) move.NewTableIsSitout = true;
                    return move;
                }

                // Try Direction = West
                round = roundsList.Find(x => x.NumberWest == pairNumber);
                if (round != null)
                {
                    move.NewTableNumber = round.TableNumber;
                    move.NewDirection = Direction.West;
                    move.NewDirectionString = "West";
                    move.Stay = (move.NewTableNumber == tableNumber && direction == Direction.West);
                    if (round.NumberNorth == 0) move.NewTableIsSitout = true;
                    return move;
                }

                else   // No move info found - move to phantom table
                {
                    move.NewTableNumber = 0;
                    move.NewDirection = Direction.Sitout;
                    move.NewDirectionString = "Sitout";
                    move.Stay = false;
                    return move;
                }
            }
            else   // Pairs - if there is a sitout pair (at an imaginary sitout table, eg a rover), it works like East
            {
                if (direction == Direction.North)
                {
                    move.DirectionString = move.NewDirectionString = "NorthSouth";
                    round = roundsList.Find(x => x.NumberNorth == pairNumber);
                }
                else
                {
                    move.DirectionString = move.NewDirectionString = "EastWest";
                    round = roundsList.Find(x => x.NumberEast == pairNumber);
                }

                if (round != null)
                {
                    move.NewTableNumber = round.TableNumber;
                    move.NewDirection = direction;
                    if (direction == Direction.North && round.NumberEast == 0) move.NewTableIsSitout = true;
                    if (direction == Direction.East && round.NumberNorth == 0) move.NewTableIsSitout = true;
                }
                else
                {
                    // Pair changes Direction
                    if (direction == Direction.North)
                    {
                        round = roundsList.Find(x => x.NumberEast == pairNumber);
                    }
                    else
                    {
                        round = roundsList.Find(x => x.NumberNorth == pairNumber);
                    }

                    if (round != null)
                    {
                        move.NewTableNumber = round.TableNumber;
                        if (direction == Direction.North)
                        {
                            move.NewDirection = Direction.East;
                            move.NewDirectionString = "EastWest";
                            if (round.NumberNorth == 0) move.NewTableIsSitout = true;
                        }
                        else
                        {
                            move.NewDirection = Direction.North;
                            move.NewDirectionString = "NorthSouth";
                            if (round.NumberEast == 0) move.NewTableIsSitout = true;
                        }
                    }
                    else   // No move info found - move to phantom table
                    {
                        move.NewTableNumber = 0;
                        move.NewDirection = Direction.Sitout;
                        move.NewDirectionString = "Sitout";
                    }
                }
                move.Stay = (move.NewTableNumber == tableNumber && move.NewDirection == direction);
                return move;
            }
        }

        public int GetBoardsFromTableNumber(TableStatus tableStatus)
        {
            // Get a list of all possible tables from which boards could have moved
            List<Round> tableList = database.GetRoundsList(tableStatus.SectionID, tableStatus.RoundNumber).FindAll(x => x.LowBoard == tableStatus.RoundData.LowBoard);
            if (tableList.Count == 0)
            {
                // No table, so boards must have come from relay table
                return 0;
            }
            else if (tableList.Count == 1)
            {
                // Just one table, so use it
                return tableList[0].TableNumber;
            }
            else
            {
                // Find the next table up from which the boards could have moved
                tableList.Sort((x, y) => x.TableNumber.CompareTo(y.TableNumber));
                Round? boardMoveFromTable = tableList.Find(x => x.TableNumber > tableStatus.TableNumber);
                if (boardMoveFromTable != null)
                {
                    return boardMoveFromTable.TableNumber;
                }

                // Next table up must be lowest table number in the list
                return tableList[0].TableNumber;
            }
        }

        public List<Ranking> GetRankings(int sectionID)
        {
            List<Ranking> rankings = database.GetRankingList(sectionID);
            if (rankings.Count == 0)  // Results table either doesn't exist or contains no entries, so try to calculate rankings
            {
                if (settings.IsIndividual)
                {
                    rankings.AddRange(CalculateIndividualRankingFromResults(sectionID));
                }
                else
                {
                    rankings.AddRange(CalculateRankingFromResults(sectionID));
                }
            }

            // Make sure that ranking list is sorted into presentation order
            rankings.Sort((x, y) =>
            {
                int sortValue = y.Orientation.CompareTo(x.Orientation);    // N's first then E's
                if (sortValue == 0) sortValue = y.ScoreDecimal.CompareTo(x.ScoreDecimal);
                if (sortValue == 0) sortValue = x.PairNo.CompareTo(y.PairNo);
                return sortValue;
            });
            return rankings;
        }

        public string Header(HeaderType headerType, int parameter1 = 0, int parameter2 = 0)
        {
            switch (headerType)
            {
                case HeaderType.Location:
                    // parameter1 = deviceNumber
                    return $"{appData.GetDeviceStatus(parameter1).Location}";
                case HeaderType.Round:
                    // parameter1 = deviceNumber
                    DeviceStatus deviceStatus = appData.GetDeviceStatus(parameter1);
                    return $"{deviceStatus.Location}: {localizer["Rd"]} {deviceStatus.RoundNumber}";
                case HeaderType.FullPlain:
                    // parameter1 = deviceNumber
                    deviceStatus = appData.GetDeviceStatus(parameter1);
                    TableStatus tableStatus = appData.GetTableStatus(parameter1);
                    if (settings.IsIndividual)
                    {
                        return $"{deviceStatus.Location}: {localizer["Rd"]} {tableStatus.RoundNumber}: {tableStatus.RoundData.NumberNorth}+{tableStatus.RoundData.NumberSouth} v {tableStatus.RoundData.NumberEast}+{tableStatus.RoundData.NumberWest}";
                    }
                    else
                    {
                        return $"{deviceStatus.Location}: {localizer["Rd"]} {tableStatus.RoundNumber}: {localizer["N"]}{localizer["S"]} {tableStatus.RoundData.NumberNorth} v {localizer["E"]}{localizer["W"]} {tableStatus.RoundData.NumberEast}";
                    }
                case HeaderType.FullColoured:
                    // parameter1 = deviceNumber, parameter2 = boardNumber
                    deviceStatus = appData.GetDeviceStatus(parameter1);
                    tableStatus = appData.GetTableStatus(parameter1);
                    if (settings.IsIndividual)
                    {
                        return $"{deviceStatus.Location}: {localizer["Rd"]} {tableStatus.RoundNumber}: {ColourPairByVulnerability("NS", parameter2, $"{tableStatus.RoundData.NumberNorth}+{tableStatus.RoundData.NumberSouth}")} v {ColourPairByVulnerability("EW", parameter2, $"{tableStatus.RoundData.NumberEast}+{tableStatus.RoundData.NumberWest}")}";
                    }
                    else
                    {
                        return $"{deviceStatus.Location}: {localizer["Rd"]} {tableStatus.RoundNumber}: {ColourPairByVulnerability("NS", parameter2, $"{localizer["N"]}{localizer["S"]} {tableStatus.RoundData.NumberNorth}")} v {ColourPairByVulnerability("EW", parameter2, $"{localizer["E"]}{localizer["W"]} {tableStatus.RoundData.NumberEast}")}";
                    }
                case HeaderType.Section:
                    // parameter1 = sectionID
                    return $"{localizer["Section"]} {database.GetSection(parameter1).Letter}";
                case HeaderType.SectionTable:
                    // parameter1 = sectionID, parameter2 = tableNumber
                    return $"{localizer["Table"]} {database.GetSection(parameter1).Letter}{parameter2}";
                default:
                    return string.Empty;
            }
        }

        public string Title(string titleString, TitleType titleType = TitleType.Plain, int parameter1 = 0, int parameter2 = 0)
        {
            switch (titleType)
            {
                case TitleType.Location:
                    // parameter1 = deviceNumber
                    DeviceStatus deviceStatus = appData.GetDeviceStatus(parameter1);
                    return $"{deviceStatus.Location}: {localizer["Rd"]} {deviceStatus.RoundNumber}: {localizer[titleString]}";
                case TitleType.Plain:
                    return $"{localizer[titleString]}";
                case TitleType.Section:
                    // parameter1 = sectionID
                    return $"{localizer["Section"]} {database.GetSection(parameter1).Letter}: {localizer[titleString]}";
                case TitleType.SectionTable:
                    // parameter1 = sectionID, parameter2 = tableNumber
                    return $"{database.GetSection(parameter1).Letter}{parameter2}: {localizer[titleString]}";
                default:
                    return string.Empty; 
            };
        }

        public bool ValidateLead(TableStatus tableStatus, string card)
        {
            if (database.GetHandsCount() == 0) return true;    // No hand records to validate against
            if (tableStatus.ResultData == null) return true;  // No result (shouldn't be possible at this stage)
            if (card == "SKIP") return true;    // Lead card entry has been skipped, so no validation

            Hand hand = database.GetHand(tableStatus.SectionID, tableStatus.ResultData.BoardNumber);
            if (hand.NorthSpades == "###")     // Can't find matching hand record, so try default SectionID = 1
            {
                hand = database.GetHand(1, tableStatus.ResultData.BoardNumber);
                if (hand.NorthSpades == "###") return true;    // Still no match, so no validation possible
            }

            string cardSuit = card[..1];
            string cardValue = card.Substring(1,1);

            switch (tableStatus.ResultData.DeclarerNSEW)
            {
                case "N":
                    switch (cardSuit)
                    {
                        case "S":
                            if (hand.EastSpades.Contains(cardValue)) return true;
                            break;
                        case "H":
                            if (hand.EastHearts.Contains(cardValue)) return true;
                            break;
                        case "D":
                            if (hand.EastDiamonds.Contains(cardValue)) return true;
                            break;
                        case "C":
                            if (hand.EastClubs.Contains(cardValue)) return true;
                            break;
                    }
                    break;
                case "S":
                    switch (cardSuit)
                    {
                        case "S":
                            if (hand.WestSpades.Contains(cardValue)) return true;
                            break;
                        case "H":
                            if (hand.WestHearts.Contains(cardValue)) return true;
                            break;
                        case "D":
                            if (hand.WestDiamonds.Contains(cardValue)) return true;
                            break;
                        case "C":
                            if (hand.WestClubs.Contains(cardValue)) return true;
                            break;
                    }
                    break;
                case "E":
                    switch (cardSuit)
                    {
                        case "S":
                            if (hand.SouthSpades.Contains(cardValue)) return true;
                            break;
                        case "H":
                            if (hand.SouthHearts.Contains(cardValue)) return true;
                            break;
                        case "D":
                            if (hand.SouthDiamonds.Contains(cardValue)) return true;
                            break;
                        case "C":
                            if (hand.SouthClubs.Contains(cardValue)) return true;
                            break;
                    }
                    break;
                case "W":
                    switch (cardSuit)
                    {
                        case "S":
                            if (hand.NorthSpades.Contains(cardValue)) return true;
                            break;
                        case "H":
                            if (hand.NorthHearts.Contains(cardValue)) return true;
                            break;
                        case "D":
                            if (hand.NorthDiamonds.Contains(cardValue)) return true;
                            break;
                        case "C":
                            if (hand.NorthClubs.Contains(cardValue)) return true;
                            break;
                    }
                    break;
            }
            return false;
        }

        public void CalculateScore(Result result)
        {
            if (result.DeclarerNSEW == null) return;
            if (result.ContractLevel <= 0) return;
            if (result.DeclarerNSEW == "N" || result.DeclarerNSEW == "S")
            {
                result.Vulnerable = Global.IsNSVulnerable(result.BoardNumber);
            }
            else
            {
                result.Vulnerable = Global.IsEWVulnerable(result.BoardNumber);
            }

            int score;
            int diff = result.TricksTaken - result.ContractLevel - 6;
            if (diff < 0)      // Contract not made
            {
                if (result.ContractX == string.Empty)
                {
                    if (result.Vulnerable)
                    {
                        score = 100 * diff;
                    }
                    else
                    {
                        score = 50 * diff;
                    }
                }
                else if (result.ContractX == "x")
                {
                    if (result.Vulnerable)
                    {
                        score = 300 * diff + 100;
                    }
                    else
                    {
                        score = 300 * diff + 400;
                        if (diff == -1) score -= 200;
                        if (diff == -2) score -= 100;
                    }
                }
                else  // ContractX = "xx"
                {
                    if (result.Vulnerable)
                    {
                        score = 600 * diff + 200;
                    }
                    else
                    {
                        score = 600 * diff + 800;
                        if (diff == -1) score -= 400;
                        if (diff == -2) score -= 200;
                    }
                }
            }
            else      // Contract made
            {
                // Basic score, game/part-score bonuses and making x/xx contract bonuses
                if (result.ContractSuit == "C" || result.ContractSuit == "D")
                {
                    if (result.ContractX == string.Empty)
                    {
                        score = 20 * (result.TricksTaken - 6);
                        if (result.ContractLevel <= 4)
                        {
                            score += 50;
                        }
                        else
                        {
                            if (result.Vulnerable) score += 500;
                            else score += 300;
                        }
                    }
                    else if (result.ContractX == "x")
                    {
                        score = 40 * result.ContractLevel + 50;
                        if (result.Vulnerable) score += 200 * diff;
                        else score += 100 * diff;
                        if (result.ContractLevel <= 2)
                        {
                            score += 50;
                        }
                        else
                        {
                            if (result.Vulnerable) score += 500;
                            else score += 300;
                        }
                    }
                    else    // ContractX = "xx"
                    {
                        score = 80 * result.ContractLevel + 100;
                        if (result.Vulnerable) score += 400 * diff;
                        else score += 200 * diff;
                        if (result.ContractLevel == 1)
                        {
                            score += 50;
                        }
                        else
                        {
                            if (result.Vulnerable) score += 500;
                            else score += 300;
                        }
                    }
                }
                else   // Major suits and NT
                {
                    if (result.ContractX == string.Empty)
                    {
                        score = 30 * (result.TricksTaken - 6);
                        if (result.ContractSuit == "NT")
                        {
                            score += 10;
                            if (result.ContractLevel <= 2)
                            {
                                score += 50;
                            }
                            else
                            {
                                if (result.Vulnerable) score += 500;
                                else score += 300;
                            }
                        }
                        else    // Major suit
                        {
                            if (result.ContractLevel <= 3)
                            {
                                score += 50;
                            }
                            else
                            {
                                if (result.Vulnerable) score += 500;
                                else score += 300;
                            }
                        }
                    }
                    else if (result.ContractX == "x")
                    {
                        score = 60 * result.ContractLevel + 50;
                        if (result.ContractSuit == "NT") score += 20;
                        if (result.Vulnerable) score += 200 * diff;
                        else score += 100 * diff;
                        if (result.ContractLevel <= 1)
                        {
                            score += 50;
                        }
                        else
                        {
                            if (result.Vulnerable) score += 500;
                            else score += 300;
                        }
                    }
                    else    // ContractX = "xx"
                    {
                        score = 120 * result.ContractLevel + 100;
                        if (result.ContractSuit == "NT") score += 40;
                        if (result.Vulnerable) score += 400 * diff + 500;
                        else score += 200 * diff + 300;
                    }
                }
                // Slam bonuses
                if (result.ContractLevel == 6)
                {
                    if (result.Vulnerable) score += 750;
                    else score += 500;
                }
                else if (result.ContractLevel == 7)
                {
                    if (result.Vulnerable) score += 1500;
                    else score += 1000;
                }
            }
            if (result.DeclarerNSEW == "E" || result.DeclarerNSEW == "W") score = -score;
            result.Score = score;
        }


        // PRIVATE CLASSES
        private PlayerEntry CreatePlayerEntry(Round round, Direction direction)
        {
            int number;
            string name;
            if (direction == Direction.North)
            {
                name = round.NameNorth;
                number = round.NumberNorth;
            }
            else if (direction == Direction.East)
            {
                name = round.NameEast;
                number = round.NumberEast;
            }
            else if (direction == Direction.South)
            {
                name = round.NameSouth;
                number = round.NumberSouth;
            }
            else
            {
                name = round.NameWest;
                number = round.NumberWest;
            }
            return new PlayerEntry(name.Replace("Unknown", localizer["Unknown"]), number, direction);
        }

        private string GetContractDisplay(Result result, bool showTricks)
        {
            if (result.ContractLevel == -1)  // Board not played or arbitral result of some kind
            {
                if (result.Remarks == "Not played")
                {
                    return $"<span style=\"color:red\">{localizer["NotPlayed"]}</span>";
                }
                else if (result.Remarks == "Arbitral score")
                {
                    return $"<span style=\"color:red\">{localizer["ArbitralScore"]}</span>";
                }
                else
                {
                    return $"<span style=\"color:red\">{result.Remarks}</span>";
                }
            }
            else if (result.ContractLevel == 0)
            {
                return $"<span style=\"color:darkgreen\">{localizer["AllPass"]}</span>";
            }
            else if (result.ContractLevel > 0)
            {
                StringBuilder s = new();
                s.Append(result.ContractLevel);
                switch (result.ContractSuit)
                {
                    case "NT":
                        s.Append(localizer["NT"]);
                        break;
                    case "S":
                        s.Append("<span style=\"color:black\">&spades;</span>");
                        break;
                    case "H":
                        s.Append("<span style=\"color:red\">&hearts;</span>");
                        break;
                    case "D":
                        s.Append("<span style=\"color:lightsalmon\">&diams;</span>");
                        break;
                    case "C":
                        s.Append("<span style=\"color:lightslategrey\">&clubs;</span>");
                        break;
                }
                s.Append(result.ContractX);

                if (showTricks)
                {
                    string tricksTakenSymbol;
                    int tricksTakenLevel = result.TricksTaken - result.ContractLevel - 6;
                    if (tricksTakenLevel == 0)
                    {
                        tricksTakenSymbol = "=";
                    }
                    else
                    {
                        tricksTakenSymbol = tricksTakenLevel.ToString("+#;-#;0");
                    }
                    s.Append(tricksTakenSymbol);
                }
                s.Append($" {localizer["by"]} ");
                s.Append(localizer[result.DeclarerNSEW]);
                return s.ToString();
            }
            return string.Empty;
        }

        private static int GetBoardsNewTableNumber(List<Round> roundsList, int tableNumber, int lowBoard)
        {
            // Get a list of all possible tables to which boards could move
            List<Round> tableList = roundsList.FindAll(x => x.LowBoard == lowBoard);
            if (tableList.Count == 0)
            {
                // No table, so move to relay table
                return 0;
            }
            else if (tableList.Count == 1)
            {
                // Just one table, so use it
                return tableList[0].TableNumber;
            }
            else
            {
                // Find the next table down to which the boards could move
                tableList.Sort((x, y) => x.TableNumber.CompareTo(y.TableNumber));
                Round? boardsMoveToTable = tableList.FindLast(x => x.TableNumber < tableNumber);
                if (boardsMoveToTable != null)
                {
                    return boardsMoveToTable.TableNumber;
                }

                // Next table down must be highest table number in the list
                return tableList[^1].TableNumber;
            }
        }
        
        private List<Ranking> CalculateRankingFromResults(int sectionID)
        {
            // Uses Neuberg formula for match point pairs to create ranking list based on data in ReceivedData table for this section
            // This might include score adjustments
            // If anything goes wrong, this function returns an empty list and no ranking list is shown

            List<Ranking> rankingList = [];
            int winners = database.GetSection(sectionID).Winners;
            if (winners == 0)
            {
                // Winners not set, so no chance of calculating ranking
                return rankingList;
            }

            List<Result> resultsList = database.GetResultsList(sectionID);

            // Create a list of how many times each board has been played in order to find maximum value for any board
            List<ResultsPerBoard> resultsPerBoardList = [];
            int maxResultsPerBoard = 1;
            foreach (Result result in resultsList)
            {
                CalculateScore(result);
                ResultsPerBoard? resultsPerBoard = resultsPerBoardList.Find(x => x.BoardNumber == result.BoardNumber);
                if (resultsPerBoard == null)
                {
                    resultsPerBoardList.Add(new ResultsPerBoard() { BoardNumber = result.BoardNumber, NumberOfResults = 1 });
                }
                else
                {
                    resultsPerBoard.NumberOfResults++;
                    if (maxResultsPerBoard < resultsPerBoard.NumberOfResults) maxResultsPerBoard = resultsPerBoard.NumberOfResults;
                }
            }
            int matchPointsMax = 2 * maxResultsPerBoard - 2;

            // Calculate MPs 
            foreach (Result result in resultsList)
            {
                if (result.ContractLevel >= 0)  // The current result has a genuine score, so there is at least one genuine score for this board
                {
                    List<Result> currentBoardScoresList = resultsList.FindAll(x => x.ContractLevel >= 0 && x.BoardNumber == result.BoardNumber);
                    int scoresForThisBoard = currentBoardScoresList.Count;
                    int matchpoints = 2 * currentBoardScoresList.FindAll(x => x.Score < result.Score).Count + currentBoardScoresList.FindAll(x => x.Score == result.Score).Count - 1;

                    // Apply Neuberg formula here
                    double neuberg = ((matchpoints + 1) * maxResultsPerBoard / (double)scoresForThisBoard) - 1.0;
                    if (result.Remarks == "Wrong direction")
                    {
                        result.MatchpointsEW = neuberg;
                        result.MatchpointsNS = matchPointsMax - neuberg;
                    }
                    else  // Normal case
                    {
                        result.MatchpointsNS = neuberg;
                        result.MatchpointsEW = matchPointsMax - neuberg;
                    }
                }
                else
                {
                    // No scorable contract, so look for arbitral score
                    try
                    {
                        string[] temp = result.Remarks.Split(arbitralScoreSeparators, StringSplitOptions.RemoveEmptyEntries);
                        int percentageNS = Convert.ToInt32(temp[0]);
                        int percentageEW = Convert.ToInt32(temp[1]);
                        result.MatchpointsNS = matchPointsMax * percentageNS / 100.0;
                        result.MatchpointsEW = matchPointsMax * percentageEW / 100.0;
                    }
                    catch
                    {
                        // Can't work out percentages, so make it 50%/50%
                        result.MatchpointsNS = result.MatchpointsEW = matchPointsMax / 2.0;
                    }
                }
            }

            if (winners == 1)
            {
                // Add up MPs for each pair, creating Ranking List entries as we go
                foreach (Result result in resultsList)
                {
                    Ranking? rankingListFind = rankingList.Find(x => x.PairNo == result.NumberNorth);
                    if (rankingListFind == null)
                    {
                        Ranking ranking = new() {
                            PairNo = result.NumberNorth,
                            Orientation = "0",
                            MP = result.MatchpointsNS,
                            MPMax = matchPointsMax 
                        };
                        rankingList.Add(ranking);
                    }
                    else
                    {
                        rankingListFind.MP += result.MatchpointsNS;
                        rankingListFind.MPMax += matchPointsMax;
                    }
                    rankingListFind = rankingList.Find(x => x.PairNo == result.NumberEast);
                    if (rankingListFind == null)
                    {
                        Ranking ranking = new()
                        {
                            PairNo = result.NumberEast,
                            Orientation = "0",
                            MP = result.MatchpointsEW,
                            MPMax = matchPointsMax
                        };
                        rankingList.Add(ranking);
                    }
                    else
                    {
                        rankingListFind.MP += result.MatchpointsEW;
                        rankingListFind.MPMax += matchPointsMax;
                    }
                }

                // Calculate percentages
                foreach (Ranking ranking in rankingList)
                {
                    if (ranking.MPMax == 0)
                    {
                        ranking.ScoreDecimal = 50.0;
                    }
                    else
                    {
                        ranking.ScoreDecimal = 100.0 * ranking.MP / ranking.MPMax;
                    }
                    ranking.Score = ranking.ScoreDecimal.ToString("0.##");
                }

                // Calculate ranking
                foreach (Ranking ranking in rankingList)
                {
                    double currentScoreDecimal = ranking.ScoreDecimal;
                    int rank = rankingList.FindAll(x => x.ScoreDecimal > currentScoreDecimal).Count + 1;
                    ranking.Rank = rank.ToString();
                    if (rankingList.FindAll(x => x.ScoreDecimal == currentScoreDecimal).Count > 1)
                    {
                        ranking.Rank += "=";
                    }
                }
            }
            else    // Winners = 2
            {
                // Add up MPs for each pair, creating Ranking List entries as we go
                foreach (Result result in resultsList)
                {
                    Ranking? rankingListFind = rankingList.Find(x => x.PairNo == result.NumberNorth && x.Orientation == "N");
                    if (rankingListFind == null)
                    {
                        Ranking ranking = new()
                        {
                            PairNo = result.NumberNorth,
                            Orientation = "N",
                            MP = result.MatchpointsNS,
                            MPMax = matchPointsMax
                        };
                        rankingList.Add(ranking);
                    }
                    else
                    {
                        rankingListFind.MP += result.MatchpointsNS;
                        rankingListFind.MPMax += matchPointsMax;
                    }
                    rankingListFind = rankingList.Find(x => x.PairNo == result.NumberEast && x.Orientation == "E");
                    if (rankingListFind == null)
                    {
                        Ranking ranking = new()
                        {
                            PairNo = result.NumberEast,
                            Orientation = "E",
                            MP = result.MatchpointsEW,
                            MPMax = matchPointsMax
                        };
                        rankingList.Add(ranking);
                    }
                    else
                    {
                        rankingListFind.MP += result.MatchpointsEW;
                        rankingListFind.MPMax += matchPointsMax;
                    }
                }

                // Calculate percentages
                foreach (Ranking ranking in rankingList)
                {
                    if (ranking.MPMax == 0)
                    {
                        ranking.ScoreDecimal = 50.0;
                    }
                    else
                    {
                        ranking.ScoreDecimal = 100.0 * ranking.MP / ranking.MPMax;
                    }
                    ranking.Score = ranking.ScoreDecimal.ToString("0.##");
                }

                // Sort and calculate rank within Orientation subsections - matching ranks are show by an '='
                foreach (Ranking ranking in rankingList)
                {
                    double currentScoreDecimal = ranking.ScoreDecimal;
                    string currentOrientation = ranking.Orientation;
                    int rank = rankingList.FindAll(x => x.Orientation == currentOrientation && x.ScoreDecimal > currentScoreDecimal).Count + 1;
                    ranking.Rank = rank.ToString();
                    if (rankingList.FindAll(x => x.Orientation == currentOrientation && x.ScoreDecimal == currentScoreDecimal).Count > 1)
                    {
                        ranking.Rank += "=";
                    }
                }
            }
            return rankingList;
        }

        private List<Ranking> CalculateIndividualRankingFromResults(int sectionID)
        {
            // Uses Neuberg formula for match point pairs to create ranking list based on data in ReceivedData table
            // This might include score adjustments
            // If anything goes wrong, this function returns an empty list and no ranking list is shown

            List<Result> resultsList = database.GetResultsList(sectionID);

            // Create a list of how many times each board has been played in order to find maximum value for any board
            List<ResultsPerBoard> resultsPerBoardList = [];
            int maxResultsPerBoard = 1;
            foreach (Result result in resultsList)
            {
                CalculateScore(result);
                ResultsPerBoard? resultsPerBoard = resultsPerBoardList.Find(x => x.BoardNumber == result.BoardNumber);
                if (resultsPerBoard == null)
                {
                    resultsPerBoardList.Add(new ResultsPerBoard() { BoardNumber = result.BoardNumber, NumberOfResults = 1 });
                }
                else
                {
                    resultsPerBoard.NumberOfResults++;
                    if (maxResultsPerBoard < resultsPerBoard.NumberOfResults) maxResultsPerBoard = resultsPerBoard.NumberOfResults;
                }
            }
            int matchPointsMax = 2 * maxResultsPerBoard - 2;

            // Calculate MPs 
            foreach (Result result in resultsList)
            {
                if (result.ContractLevel >= 0)  // The current result has a genuine score, so there is at least one genuine score for this board
                {
                    List<Result> currentBoardScoresList = resultsList.FindAll(x => x.ContractLevel >= 0 && x.BoardNumber == result.BoardNumber);
                    int scoresForThisBoard = currentBoardScoresList.Count;
                    int matchpoints = 2 * currentBoardScoresList.FindAll(x => x.Score < result.Score).Count + currentBoardScoresList.FindAll(x => x.Score == result.Score).Count - 1;

                    // Apply Neuberg formula here
                    double neuberg = ((matchpoints + 1) * maxResultsPerBoard / (double)scoresForThisBoard) - 1.0;
                    if (result.Remarks == "Wrong direction")
                    {
                        result.MatchpointsEW = neuberg;
                        result.MatchpointsNS = matchPointsMax - neuberg;
                    }
                    else  // Normal case
                    {
                        result.MatchpointsNS = neuberg;
                        result.MatchpointsEW = matchPointsMax - neuberg;
                    }
                }
                else
                {
                    // No scorable contract, so look for arbitral score
                    try
                    {
                        string[] temp = result.Remarks.Split(arbitralScoreSeparators, StringSplitOptions.RemoveEmptyEntries);
                        int percentageNS = Convert.ToInt32(temp[0]);
                        int percentageEW = Convert.ToInt32(temp[1]);
                        result.MatchpointsNS = matchPointsMax * percentageNS / 100.0;
                        result.MatchpointsEW = matchPointsMax * percentageEW / 100.0;
                    }
                    catch
                    {
                        // Can't work out percentages, so make it 50%/50%
                        result.MatchpointsNS = result.MatchpointsEW = matchPointsMax / 2.0;
                    }
                }
            }

            // Add up MPs for each player, creating Ranking List entries as we go
            List<Ranking> rankingList = [];
            foreach (Result result in resultsList)
            {
                Ranking? rankingListFind = rankingList.Find(x => x.PairNo == result.NumberNorth);
                if (rankingListFind == null)
                {
                    Ranking ranking = new()
                    {
                        PairNo = result.NumberNorth,
                        Orientation = "0",
                        MP = result.MatchpointsNS,
                        MPMax = matchPointsMax
                    };
                    rankingList.Add(ranking);
                }
                else
                {
                    rankingListFind.MP += result.MatchpointsNS;
                    rankingListFind.MPMax += matchPointsMax;
                }
                rankingListFind = rankingList.Find(x => x.PairNo == result.NumberEast);
                if (rankingListFind == null)
                {
                    Ranking ranking = new()
                    {
                        PairNo = result.NumberEast,
                        Orientation = "0",
                        MP = result.MatchpointsEW,
                        MPMax = matchPointsMax
                    };
                    rankingList.Add(ranking);
                }
                else
                {
                    rankingListFind.MP += result.MatchpointsEW;
                    rankingListFind.MPMax += matchPointsMax;
                }
                rankingListFind = rankingList.Find(x => x.PairNo == result.NumberSouth);
                if (rankingListFind == null)
                {
                    Ranking ranking = new()
                    {
                        PairNo = result.NumberSouth,
                        Orientation = "0",
                        MP = result.MatchpointsNS,
                        MPMax = matchPointsMax
                    };
                    rankingList.Add(ranking);
                }
                else
                {
                    rankingListFind.MP += result.MatchpointsNS;
                    rankingListFind.MPMax += matchPointsMax;
                }
                rankingListFind = rankingList.Find(x => x.PairNo == result.NumberWest);
                if (rankingListFind == null)
                {
                    Ranking ranking = new()
                    {
                        PairNo = result.NumberWest,
                        Orientation = "0",
                        MP = result.MatchpointsEW,
                        MPMax = matchPointsMax
                    };
                    rankingList.Add(ranking);
                }
                else
                {
                    rankingListFind.MP += result.MatchpointsEW;
                    rankingListFind.MPMax += matchPointsMax;
                }
            }

            // Calculate percentages
            foreach (Ranking ranking in rankingList)
            {
                if (ranking.MPMax == 0)
                {
                    ranking.ScoreDecimal = 50.0;
                }
                else
                {
                    ranking.ScoreDecimal = 100.0 * ranking.MP / ranking.MPMax;
                }
                ranking.Score = ranking.ScoreDecimal.ToString("0.##");
            }

            // Calculate ranks - matching ranks are show by an '='
            rankingList.Sort((x, y) => y.ScoreDecimal.CompareTo(x.ScoreDecimal));
            foreach (Ranking ranking in rankingList)
            {
                double currentScoreDecimal = ranking.ScoreDecimal;
                int rank = rankingList.FindAll(x => x.ScoreDecimal > currentScoreDecimal).Count + 1;
                ranking.Rank = rank.ToString();
                if (rankingList.FindAll(x => x.ScoreDecimal == currentScoreDecimal).Count > 1)
                {
                    ranking.Rank += "=";
                }
            }

            return rankingList;
        }

        private static string ColourPairByVulnerability(string direction, int boardNumber, string pair)
        {
            if (direction == "NS")
            {
                if (Global.IsNSVulnerable(boardNumber))
                {
                    return $"<span style=\"color:red\">{pair}</span>";
                }
                else
                {
                    return $"<span style=\"color:green\">{pair}</span>";
                }
            }
            else
            {
                if (Global.IsEWVulnerable(boardNumber))
                {
                    return $"<span style=\"color:red\">{pair}</span>";
                }
                else
                {
                    return $"<span style=\"color:green\">{pair}</span>";
                }
            }
        }
    }
}
