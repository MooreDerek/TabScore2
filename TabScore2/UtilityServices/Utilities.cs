using Microsoft.Extensions.Localization;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Models;

namespace TabScore2.UtilityServices
{
    public class Utilities(IStringLocalizer iLocalizer, IDatabase iDatabase, IAppData iAppData, ISettings iSettings) : IUtilities
    {
        private readonly IStringLocalizer localizer = iLocalizer;
        private readonly IDatabase database = iDatabase;
        private readonly IAppData appData = iAppData;
        private readonly ISettings settings = iSettings;
        private static readonly char[] arbitralScoreSeparators = ['%', '-'];

        // PUBLIC CLASSES TO CREATE VIEW MODELS
        public ShowPlayerIDs CreateShowPlayerIDsModel(int tabletDeviceNumber, bool showWarning)
        {
            ShowPlayerIDs showPlayerIDs = new(tabletDeviceNumber, showWarning);
            TabletDeviceStatus tabletDeviceStatus = appData.GetTabletDeviceStatus(tabletDeviceNumber);
            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;
            Round round = tableStatus.RoundData;
            Section section = database.GetSection(tabletDeviceStatus.SectionID);

            if (section.TabletDevicesPerTable == 1)
            {
                if (round.NumberNorth != 0 && round.NumberNorth != section.MissingPair)
                {
                    showPlayerIDs.Add(CreatePlayerEntry(round, Direction.North));
                    showPlayerIDs.Add(CreatePlayerEntry(round, Direction.South));
                }
                if (round.NumberEast != 0 && round.NumberEast != section.MissingPair)
                {
                    showPlayerIDs.Add(CreatePlayerEntry(round, Direction.East));
                    showPlayerIDs.Add(CreatePlayerEntry(round, Direction.West));
                }
            }
            else if (section.TabletDevicesPerTable == 2)
            {
                if (tabletDeviceStatus.Direction == Direction.North)
                {
                    showPlayerIDs.Add(CreatePlayerEntry(round, Direction.North));
                    showPlayerIDs.Add(CreatePlayerEntry(round, Direction.South));
                }
                else
                {
                    showPlayerIDs.Add(CreatePlayerEntry(round, Direction.East));
                    showPlayerIDs.Add(CreatePlayerEntry(round, Direction.West));
                }
            }
            else  // tabletDevicesPerTable == 4
            {
                showPlayerIDs.Add(CreatePlayerEntry(round, tabletDeviceStatus.Direction));
            }

            showPlayerIDs.NumberOfBlankEntries = showPlayerIDs.FindAll(x => x.Name == "").Count;
            showPlayerIDs.ShowMessage = section.TabletDevicesPerTable == 4 || (section.TabletDevicesPerTable == 2 && showPlayerIDs.Count == 4);
            return showPlayerIDs;
        }

        public ShowBoards CreateShowBoardsModel(int tabletDeviceNumber)
        {
            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;
            List<Result> resultsList = database.GetResultsList(tableStatus.SectionID, tableStatus.RoundData.LowBoard, tableStatus.RoundData.HighBoard, tableStatus.TableNumber, tableStatus.RoundNumber);

            ShowBoards showBoards = new(tabletDeviceNumber, settings.ShowTraveller);

            // Check to see if any boards don't have a result, and add dummies to the list
            for (int iBoard = tableStatus.RoundData.LowBoard; iBoard <= tableStatus.RoundData.HighBoard; iBoard++)
            {
                if (resultsList.Find(x => x.BoardNumber == iBoard) == null)
                {
                    Result result = new()
                    {
                        BoardNumber = iBoard,
                        ContractLevel = -999,
                        ContractSuit = "",
                        ContractX = "",
                        DeclarerNSEW = "",
                        TricksTaken = -1,
                    };
                    resultsList.Add(result);
                    showBoards.GotAllResults = false;
                }
            }
            resultsList.Sort((x, y) => x.BoardNumber.CompareTo(y.BoardNumber));
            showBoards.AddRange(resultsList);
            return showBoards;
        }

        public ShowMove CreateShowMoveModel(int tabletDeviceNumber, int newRoundNumber, int tableNotReadyNumber)
        {
            TabletDeviceStatus tabletDeviceStatus = appData.GetTabletDeviceStatus(tabletDeviceNumber);
            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;    // tableStatus cannot be null, but compiler objects
            Section section = database.GetSection(tabletDeviceStatus.SectionID);

            ShowMove showMove = [];
            showMove.TabletDeviceNumber = tabletDeviceNumber;
            showMove.Direction = tabletDeviceStatus.Direction;
            showMove.NewRoundNumber = newRoundNumber;
            showMove.TableNotReadyNumber = tableNotReadyNumber;
            showMove.TabletDevicesPerTable = section.TabletDevicesPerTable;
            int missingPair = section.MissingPair;

            List<Round> roundsList = database.GetRoundsList(tabletDeviceStatus.SectionID, newRoundNumber);
            if (section.TabletDevicesPerTable == 1)
            {
                if (database.IsIndividual)
                {
                    if (tableStatus.RoundData.NumberNorth != 0)
                    {
                        showMove.Add(GetMove(roundsList, tableStatus.TableNumber, tableStatus.RoundData.NumberNorth, Direction.North));
                    }
                    if (tableStatus.RoundData.NumberSouth != 0)
                    {
                        showMove.Add(GetMove(roundsList, tableStatus.TableNumber, tableStatus.RoundData.NumberSouth, Direction.South));
                    }
                    if (tableStatus.RoundData.NumberEast != 0)
                    {
                        showMove.Add(GetMove(roundsList, tableStatus.TableNumber, tableStatus.RoundData.NumberEast, Direction.East));
                    }
                    if (tableStatus.RoundData.NumberWest != 0)
                    {
                        showMove.Add(GetMove(roundsList, tableStatus.TableNumber, tableStatus.RoundData.NumberWest, Direction.West));
                    }
                }
                else  // Not individual
                {
                    if (tableStatus.RoundData.NumberNorth != 0 && tableStatus.RoundData.NumberNorth != missingPair)
                    {
                        showMove.Add(GetMove(roundsList, tableStatus.TableNumber, tableStatus.RoundData.NumberNorth, Direction.North));
                    }
                    if (tableStatus.RoundData.NumberEast != 0 && tableStatus.RoundData.NumberEast != missingPair)
                    {
                        showMove.Add(GetMove(roundsList, tableStatus.TableNumber, tableStatus.RoundData.NumberEast, Direction.East));
                    }
                }
            }
            else  // TabletDevicesPerTable > 1, so only need move for single player/pair.  tableStatus could be null (at phantom table), so use tabletDeviceStatus
            {
                showMove.Add(GetMove(roundsList, tableStatus.TableNumber, tabletDeviceStatus.PairNumber, tabletDeviceStatus.Direction));
            }

            showMove.BoardsNewTable = -999;
            if (tableStatus != null)  // tableStatus==null => phantom table, so no boards to worry about
            {
                // Show boards move only to North (or North/South) unless missing, in which case only show to East (or East/West)
                showMove.LowBoard = tableStatus.RoundData.LowBoard;
                showMove.HighBoard = tableStatus.RoundData.HighBoard;
                if (showMove.Direction == Direction.North || ((tableStatus.RoundData.NumberNorth == 0 || tableStatus.RoundData.NumberNorth == missingPair) && showMove.Direction == Direction.East))
                {
                    showMove.BoardsNewTable = GetBoardsNewTableNumber(roundsList, tableStatus.TableNumber, tableStatus.RoundData.LowBoard);
                }
            }
            return showMove;
        }

        public ShowTraveller CreateShowTravellerModel(int tabletDeviceNumber)
        {
            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;
            int currentBoardNumber = tableStatus.ResultData!.BoardNumber; 
            ShowTraveller showTraveller = new(tabletDeviceNumber, currentBoardNumber);
            List<Result> resultsList = database.GetResultsList(tableStatus.SectionID, currentBoardNumber);

            // Set maximum match points based on total number of results (including Not Played) for Neuberg formula
            int resultsForThisBoard = resultsList.Count;
            int matchPointsMax = 2 * resultsForThisBoard - 2;

            // Get list of all scorable results for matchpoint calculation
            List<Result> resultsWithContractList = resultsList.FindAll(x => x.ContractLevel >= 0);
            int scoresForThisBoard = resultsWithContractList.Count;

            foreach (Result result in resultsList)
            {
                if (result.Remarks != "" && result.Remarks != "Wrong direction")
                {
                    // No scorable contract, so look for arbitral percentages
                    string[] temp = result.Remarks.Split(arbitralScoreSeparators, StringSplitOptions.RemoveEmptyEntries);
                    if (temp.Length == 2 && uint.TryParse(temp[0], out uint tempInt0) && uint.TryParse(temp[1], out uint tempInt1))
                    {
                        result.ScoreNS = $"<span style=\"color:red\">{temp[0]}%</span>";
                        result.ScoreEW = $"<span style=\"color:red\">{temp[1]}%</span>";
                        result.SortPercentage = Convert.ToDouble(temp[0]);
                        if (result.NumberNorth == tableStatus.RoundData.NumberNorth)
                        {
                            result.PercentageNS = temp[0] + "%";
                            result.PercentageEW = temp[1] + "%";
                        }
                    }
                    else  // Can't work out percentages
                    {
                        result.ScoreNS = result.ScoreEW = "<span style=\"color:red\">???</span>";
                        result.SortPercentage = 0.0;
                        if (result.NumberNorth == tableStatus.RoundData.NumberNorth)
                        {
                            result.PercentageNS = result.PercentageEW = "???";
                            result.Highlight = true;
                        }
                    }
                }
                else  // Genuine result
                {
                    result.CalculateScore();
                    if (result.Score > 0)
                    {
                        result.ScoreNS = result.Score.ToString();
                        result.ScoreEW = "";
                    }
                    else if (result.Score < 0)
                    {
                        result.ScoreEW = (-result.Score).ToString();
                        result.ScoreNS = "";
                    }

                    if (matchPointsMax == 0)
                    {
                        result.SortPercentage = 50.0;
                    }
                    else
                    {
                        // Apply Neuberg formula here
                        int matchpoints = 2 * resultsWithContractList.FindAll(x => x.Score < result.Score).Count + resultsWithContractList.FindAll(x => x.Score == result.Score).Count - 1;
                        double neuberg = ((matchpoints + 1) * resultsForThisBoard / (double)scoresForThisBoard) - 1.0;
                        result.SortPercentage = neuberg / matchPointsMax * 100.0;
                    }
                    if (result.NumberNorth == tableStatus.RoundData.NumberNorth)
                    {
                        int intPercentageNS = Convert.ToInt32(result.SortPercentage);
                        result.PercentageNS = Convert.ToString(intPercentageNS) + "%";
                        result.PercentageEW = Convert.ToString(100 - intPercentageNS) + "%";
                        result.Highlight = true;
                    }

                    if (result.Remarks == "Wrong direction")
                    {
                        (result.NumberEast, result.NumberNorth) = (result.NumberNorth, result.NumberEast);
                        (result.NumberWest, result.NumberSouth) = (result.NumberSouth, result.NumberWest);
                    }
                }
            }

            resultsList.Sort((x, y) => y.SortPercentage.CompareTo(x.SortPercentage));  // Sort traveller into descending percentage order
            showTraveller.AddRange(resultsList);
            if (!settings.ShowPercentage) showTraveller.PercentageNS = "";   // Don't show percentage

            // Determine if there is a hand record to view
            showTraveller.HandRecord = false;
            if (settings.ShowHandRecord && database.HandsCount > 0)
            {
                Hand? hand = database.GetHand(tableStatus.SectionID, currentBoardNumber);
                if (hand != null)
                {
                    showTraveller.HandRecord = true;
                }
                else    // Can't find matching hand record, so try default SectionID = 1
                {
                    hand = database.GetHand(1, currentBoardNumber);
                    if (hand != null)
                    {
                        showTraveller.HandRecord = true;
                    }
                }
            }
            return showTraveller;
        }

        public ShowHandRecord? CreateShowHandRecordModel(int tabletDeviceNumber, int boardNumber)
        {
            TabletDeviceStatus tabletDeviceStatus = appData.GetTabletDeviceStatus(tabletDeviceNumber);
            Hand? hand = database.GetHand(tabletDeviceStatus.SectionID, boardNumber);
            hand ??= database.GetHand(1, boardNumber);
            if (hand == null) return null;

            string dealer = ((boardNumber - 1) % 4) switch
            {
                0 => (string)localizer["N"],
                1 => (string)localizer["E"],
                2 => (string)localizer["S"],
                3 => (string)localizer["W"],
                _ => "#",
            };
            ShowHandRecord showHandRecord = new(tabletDeviceNumber, dealer);
            // Set dealer based on board number
            string A = localizer["A"];
            string K = localizer["K"];
            string Q = localizer["Q"];
            string J = localizer["J"];
            string T = localizer["TenShorthand"];
            showHandRecord.NorthSpades = hand.NorthSpades.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", T);
            showHandRecord.NorthHearts = hand.NorthHearts.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", T);
            showHandRecord.NorthDiamonds = hand.NorthDiamonds.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", T);
            showHandRecord.NorthClubs = hand.NorthClubs.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", T);
            showHandRecord.EastSpades = hand.EastSpades.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", T);
            showHandRecord.EastHearts = hand.EastHearts.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", T);
            showHandRecord.EastDiamonds = hand.EastDiamonds.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", T);
            showHandRecord.EastClubs = hand.EastClubs.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", T);
            showHandRecord.SouthSpades = hand.SouthSpades.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", T);
            showHandRecord.SouthHearts = hand.SouthHearts.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", T);
            showHandRecord.SouthDiamonds = hand.SouthDiamonds.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", T);
            showHandRecord.SouthClubs = hand.SouthClubs.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", T);
            showHandRecord.WestSpades = hand.WestSpades.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", T);
            showHandRecord.WestHearts = hand.WestHearts.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", T);
            showHandRecord.WestDiamonds = hand.WestDiamonds.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", T);
            showHandRecord.WestClubs = hand.WestClubs.Replace("A", A).Replace("K", K).Replace("Q", Q).Replace("J", J).Replace("T", T);

            HandEvaluation? handEvaluation = appData.GetHandEvaluation(tabletDeviceStatus.SectionID, boardNumber);
            if (handEvaluation == null) return showHandRecord;

            showHandRecord.EvalNorthNT = handEvaluation.NorthNotrump;
            showHandRecord.EvalNorthSpades = handEvaluation.NorthSpades;
            showHandRecord.EvalNorthHearts = handEvaluation.NorthHearts;
            showHandRecord.EvalNorthDiamonds = handEvaluation.NorthDiamonds;
            showHandRecord.EvalNorthClubs = handEvaluation.NorthClubs;
            showHandRecord.EvalEastNT = handEvaluation.EastNotrump;
            showHandRecord.EvalEastSpades = handEvaluation.EastSpades;
            showHandRecord.EvalEastHearts = handEvaluation.EastHearts;
            showHandRecord.EvalEastDiamonds = handEvaluation.EastDiamonds;
            showHandRecord.EvalEastClubs = handEvaluation.EastClubs;
            showHandRecord.EvalSouthNT = handEvaluation.SouthNotrump;
            showHandRecord.EvalSouthSpades = handEvaluation.SouthSpades;
            showHandRecord.EvalSouthHearts = handEvaluation.SouthHearts;
            showHandRecord.EvalSouthDiamonds = handEvaluation.SouthDiamonds;
            showHandRecord.EvalSouthClubs = handEvaluation.SouthClubs;
            showHandRecord.EvalWestNT = handEvaluation.WestNotrump;
            showHandRecord.EvalWestSpades = handEvaluation.WestSpades;
            showHandRecord.EvalWestHearts = handEvaluation.WestHearts;
            showHandRecord.EvalWestDiamonds = handEvaluation.WestDiamonds;
            showHandRecord.EvalWestClubs = handEvaluation.WestClubs;

            showHandRecord.HCPNorth = handEvaluation.NorthHcp;
            showHandRecord.HCPSouth = handEvaluation.SouthHcp;
            showHandRecord.HCPEast = handEvaluation.EastHcp;
            showHandRecord.HCPWest = handEvaluation.WestHcp;

            return showHandRecord;
        }

        public ShowRankingList CreateRankingListModel(int tabletDeviceNumber)
        {
            TabletDeviceStatus tabletDeviceStatus = appData.GetTabletDeviceStatus(tabletDeviceNumber);
            ShowRankingList showRankingList = [];
            showRankingList.TabletDeviceNumber = tabletDeviceNumber;
            showRankingList.RoundNumber = tabletDeviceStatus.RoundNumber;

            // Set player numbers to highlight appropriate rows of ranking list
            if (database.GetSection(tabletDeviceStatus.SectionID).TabletDevicesPerTable == 1)
            {
                TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;
                showRankingList.NumberNorth = tableStatus.RoundData.NumberNorth;
                showRankingList.NumberEast = tableStatus.RoundData.NumberEast;
                showRankingList.NumberSouth = tableStatus.RoundData.NumberSouth;
                showRankingList.NumberWest = tableStatus.RoundData.NumberWest;
            }
            else  // More than one tablet device per table
            {
                // Only need to highlight one row entry, so use NumberNorth as proxy
                showRankingList.NumberNorth = tabletDeviceStatus.PairNumber;
            }

            showRankingList.AddRange(GetRankings(tabletDeviceStatus.SectionID));
            return showRankingList;
        }


        // OTHER PUBLIC UTLITY CLASSES
        public Move GetMove(List<Round> roundsList, int tableNumber, int pairNumber, Direction direction)
        {
            Move move = new(pairNumber, direction);
            Round? round;
            if (database.IsIndividual)
            {
                // Try Direction = North
                round = roundsList.Find(x => x.NumberNorth == pairNumber);
                if (round != null)
                {
                    move.NewTableNumber = round.TableNumber;
                    move.NewDirection = Direction.North;
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
                    move.Stay = (move.NewTableNumber == tableNumber && direction == Direction.West);
                    if (round.NumberNorth == 0) move.NewTableIsSitout = true;
                    return move;
                }

                else   // No move info found - move to phantom table
                {
                    move.NewTableNumber = 0;
                    move.NewDirection = Direction.Sitout;
                    move.Stay = false;
                    return move;
                }
            }
            else   // Pairs - if there is a sitout pair (at an imaginary sitout table, eg a rover), it works like East
            {
                if (direction == Direction.North)
                {
                    round = roundsList.Find(x => x.NumberNorth == pairNumber);
                }
                else
                {
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
                            if (round.NumberNorth == 0) move.NewTableIsSitout = true;
                        }
                        else
                        {
                            move.NewDirection = Direction.North;
                            if (round.NumberEast == 0) move.NewTableIsSitout = true;
                        }
                    }
                    else   // No move info found - move to phantom table
                    {
                        move.NewTableNumber = 0;
                        move.NewDirection = Direction.Sitout;
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
                if (database.IsIndividual)
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

        public string Header(int tabletDeviceNumber, HeaderType headerType, int boardNumber = 0)
        {
            TabletDeviceStatus tabletDeviceStatus = appData.GetTabletDeviceStatus(tabletDeviceNumber);
            TableStatus tableStatus = appData.GetTableStatus(tabletDeviceNumber)!;
            switch (headerType)
            {
                case HeaderType.Location:
                    return $"{tabletDeviceStatus.Location}";
                case HeaderType.Round:
                    return $"{tabletDeviceStatus.Location}: {localizer["Round"]} {tabletDeviceStatus.RoundNumber}";
                case HeaderType.FullPlain:
                    if (database.IsIndividual)
                    {
                        return $"{tabletDeviceStatus.Location}: {localizer["Round"]} {tableStatus.RoundNumber}: {tableStatus.RoundData.NumberNorth}+{tableStatus.RoundData.NumberSouth} v {tableStatus.RoundData.NumberEast}+{tableStatus.RoundData.NumberWest}";
                    }
                    else
                    {
                        return $"{tabletDeviceStatus.Location}: {localizer["Round"]} {tableStatus.RoundNumber}: {localizer["N"]}{localizer["S"]} {tableStatus.RoundData.NumberNorth} v {localizer["E"]}{localizer["W"]} {tableStatus.RoundData.NumberEast}";
                    }
                case HeaderType.FullColoured:
                    if (database.IsIndividual)
                    {
                        return $"{tabletDeviceStatus.Location}: {localizer["Round"]} {tableStatus.RoundNumber}: {ColourPairByVulnerability("NS", boardNumber, $"{tableStatus.RoundData.NumberNorth}+{tableStatus.RoundData.NumberSouth}")} v {ColourPairByVulnerability("EW", boardNumber, $"{tableStatus.RoundData.NumberEast}+{tableStatus.RoundData.NumberWest}")}";
                    }
                    else
                    {
                        return $"{tabletDeviceStatus.Location}: {localizer["Round"]} {tableStatus.RoundNumber}: {ColourPairByVulnerability("NS", boardNumber, $"{localizer["N"]}{localizer["S"]} {tableStatus.RoundData.NumberNorth}")} v {ColourPairByVulnerability("EW", boardNumber, $"{localizer["E"]}{localizer["W"]} {tableStatus.RoundData.NumberEast}")}";
                    }
                default:
                    return string.Empty;
            }
        }

        public string Title(int tabletDeviceNumber, string titleString, TitleType titleType)
        {
            TabletDeviceStatus tabletDeviceStatus = appData.GetTabletDeviceStatus(tabletDeviceNumber);
            return titleType switch
            {
                TitleType.Plain => $"{localizer[titleString]}",
                TitleType.Location => $"{localizer[titleString]} - {tabletDeviceStatus.Location}",
                _ => string.Empty,
            };
        }

        public bool ValidateLead(TableStatus tableStatus, string card)
        {
            if (database.HandsCount == 0) return true;    // No hand records to validate against
            if (tableStatus.ResultData == null) return true;  // No result (shouldn't be possible at this stage)
            if (card == "SKIP") return true;    // Lead card entry has been skipped, so no validation

            Hand? hand = database.GetHand(tableStatus.SectionID, tableStatus.ResultData.BoardNumber);
            if (hand == null)     // Can't find matching hand record, so try default SectionID = 1
            {
                hand = database.GetHand(1, tableStatus.ResultData.BoardNumber);
                if (hand == null) return true;    // Still no match, so no validation possible
            }

            string cardSuit = card[..1];
            string cardValue = card[1..1];

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


        // PRIVATE CLASSES
        private static PlayerEntry CreatePlayerEntry(Round round, Direction direction)
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
            return new PlayerEntry(name, number, direction);
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
                        Ranking ranking = new(result.NumberNorth, "0", result.MatchpointsNS, matchPointsMax);
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
                        Ranking ranking = new(result.NumberEast, "0", result.MatchpointsEW, matchPointsMax);
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
                        Ranking ranking = new(result.NumberNorth, "N", result.MatchpointsNS, matchPointsMax);
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
                        Ranking ranking = new(result.NumberEast, "E", result.MatchpointsEW, matchPointsMax);
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
                    Ranking ranking = new(result.NumberNorth, "0", result.MatchpointsNS, matchPointsMax);
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
                    Ranking ranking = new(result.NumberEast, "0", result.MatchpointsEW, matchPointsMax);
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
                    Ranking ranking = new(result.NumberSouth, "0", result.MatchpointsNS, matchPointsMax);
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
                    Ranking ranking = new(result.NumberWest, "0", result.MatchpointsEW, matchPointsMax);
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





        /*
                public void InitializeResult(Result result, TableStatus tableStatus, int boardNumber)
                {
                    result.NumberNorth = tableStatus.RoundData.NumberNorth;
                    result.NumberSouth = tableStatus.RoundData.NumberSouth;
                    result.NumberEast = tableStatus.RoundData.NumberEast;
                    result.NumberWest = tableStatus.RoundData.NumberWest;
                    result.BoardNumber = boardNumber;

                    DatabaseResult databaseResult = database.GetDatabaseResult(tableStatus.SectionID, tableStatus.TableNumber, tableStatus.RoundNumber, boardNumber);
                    result.Remarks = databaseResult.Remarks;
                    result.DeclarerNSEW = databaseResult.DeclarerNSEW;
                    result.DeclarerNSEWDisplay = result.DeclarerNSEW switch
                    {
                        "N" => (string)localizer["N"],
                        "S" => (string)localizer["S"],
                        "E" => (string)localizer["E"],
                        "W" => (string)localizer["W"],
                        _ => ""
                    };

                    result.TricksTaken = databaseResult.TricksTaken;
                    if (result.TricksTaken >= 0)
                    {
                        int tricksTakenLevel = result.TricksTaken - result.ContractLevel - 6;
                        if (tricksTakenLevel == 0)
                        {
                            result.TricksTakenSymbol = "=";
                        }
                        else
                        {
                            result.TricksTakenSymbol = tricksTakenLevel.ToString("+#;-#;0");
                        }
                    }

                    result.ContractLevel = databaseResult.ContractLevel;
                    result.ContractSuit = databaseResult.ContractSuit;
                    result.ContractX = databaseResult.ContractX;
                    if (result.ContractLevel == -1)  // Board not played or arbitral result of some kind
                    {
                        result.ContractDisplay = $"<span style=\"color:red\">{localizer[result.Remarks]}</span>";
                    }
                    else if (result.ContractLevel == 0)
                    {
                        result.ContractDisplay = $"<span style=\"color:darkgreen\">{localizer["AllPass"]}</span>";
                        result.ContractTravellerDisplay = $"<span style=\"color:darkgreen\">{localizer["Pass"]}</span>";
                    }
                    else if (result.ContractLevel > 0)
                    {
                        //Normal contract and result
                        StringBuilder sb = new(string.Empty);
                        sb.Append(result.ContractLevel);
                        switch (result.ContractSuit)
                        {
                            case "NT":
                                sb.Append(localizer["NT"]);
                                break;
                            case "S":
                                sb.Append("<span style=\"color:black\">&spades;</span>");
                                break;
                            case "H":
                                sb.Append("<span style=\"color:red\">&hearts;</span>");
                                break;
                            case "D":
                                sb.Append("<span style=\"color:lightsalmon\">&diams;</span>");
                                break;
                            case "C":
                                sb.Append("<span style=\"color:lightslategrey\">&clubs;</span>");
                                break;
                        }
                        sb.Append(result.ContractX);
                        result.ContractTravellerDisplay = sb.ToString();
                        sb.Append($"{result.TricksTakenSymbol} {localizer["by"]} ");
                        sb.Append(result.DeclarerNSEWDisplay);
                        result.ContractDisplay = sb.ToString();
                    }

                    if (databaseResult.LeadCard.Length > 2 && databaseResult.LeadCard.Substring(1, 2) == "10")
                    {
                        LeadCard = string.Concat(databaseResult.LeadCard.AsSpan(0, 1), "T");
                    }
                    else
                    {
                        LeadCard = databaseResult.LeadCard;
                    }
                    if (LeadCard == string.Empty || LeadCard == "SKIP")
                    {
                        LeadCardDisplay = string.Empty;
                    }
                    else
                    {
                        LeadCardDisplay = LeadCard.Replace("S", "<span style=\"color:black\">&spades;</span>")
                                                  .Replace("H", "<span style=\"color:red\">&hearts;</span>")
                                                  .Replace("D", "<span style=\"color:lightsalmon\">&diams;</span>")
                                                  .Replace("C", "<span style=\"color:lightslategrey\">&clubs;</span>")
                                                  .Replace("T", localizer["TenShorthand"]);
                    }
                }

                bool vul;
                    if (DeclarerNSEW == "N" || DeclarerNSEW == "S")
                    {
                        vul = utilities.GetIsNSVulnerable(BoardNumber);
                    }
                    else
                    {
                        vul = utilities.GetIsEWVulnerable(BoardNumber);
                    }



    
                        // Look up player name using table from scoring database
                        public static string GetNameFromPlayerNamesTable(string playerID)
                        {
                            if (PlayerNamesTable.Count == 0)
                            {
                                return "#" + playerID;
                            }
                            PlayerRecord player = PlayerNamesTable.Find(x => x.ID == playerID);
                            if (player == null)
                            {
                                return Strings.Unknown + " #" + playerID;
                            }
                            else
                            {
                                return player.Name;
                            }
                        }

                        // Function to deal with different display format options for blank and unknown names
                        public string FormatName(string name, string number)
                        {
                            if (name == string.Empty || name == localizer["Unknown"])
                            {
                                if (number == string.Empty)
                                {
                                    return string.Empty;
                                }
                                else if (number == "0")
                                {
                                    return localizer["Unknown"];
                                }
                                else
                                {
                                    return localizer["Unknown"] + " #" + number;
                                }
                            }
                            else
                            {
                                return name;
                            }
                        }
                */
    }
}
