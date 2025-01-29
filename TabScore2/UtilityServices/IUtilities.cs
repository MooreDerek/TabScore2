// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Classes;
using TabScore2.Globals;
using TabScore2.Models;
using TabScore2.SharedClasses;

namespace TabScore2.UtilityServices
{
    public interface IUtilities
    {
        ShowPlayerIDsModel CreateShowPlayerIDsModel(int deviceNumber, bool showWarning);
        EnterPlayerIDModel CreateEnterPlayerIDModel(Direction direction);
        ShowRoundInfoModel CreateShowRoundInfoModel(int deviceNumber);
        ShowBoardsModel CreateShowBoardsModel(int deviceNumber);
        ShowMoveModel CreateShowMoveModel(int deviceNumber, int newRoundNumber, int tableNotReadyNumber);
        EnterContractModel CreateEnterContractModel(Result result, bool showTricks = false, LeadValidationOptions leadValidation = LeadValidationOptions.NoWarning);
        ShowTravellerModel CreateShowTravellerModel(int deviceNumber);
        ShowHandRecordModel? CreateShowHandRecordModel(int deviceNumber, int boardNumber);
        ShowRankingListModel CreateRankingListModel(int deviceNumber);
        
        Move GetMove(List<Round> roundsList, int tableNumber, int pairNumber, Direction direction);
        int GetBoardsFromTableNumber(TableStatus tableStatus);
        List<Ranking> GetRankings(int sectionID);

        string Header(HeaderType headerType, int parameter1 = 0, int parameter2 = 0);
        string Title(string titleString, TitleType titleType = TitleType.Plain, int parameter1 = 0, int parameter2 = 0);
        bool ValidateLead(TableStatus tableStatus, string card);
        public void CalculateScore(Result result);
    }
}
