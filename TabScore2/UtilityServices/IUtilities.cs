using TabScore2.Classes;
using TabScore2.Globals;
using TabScore2.Models;

namespace TabScore2.UtilityServices
{
    public interface IUtilities
    {
        ShowPlayerIDs CreateShowPlayerIDsModel(int deviceNumber, bool showWarning);
        EnterPlayerID CreateEnterPlayerIDModel(int deviceNumber, Direction direction);
        ShowRoundInfo CreateShowRoundInfoModel(int deviceNumber);
        ShowBoards CreateShowBoardsModel(int deviceNumber);
        ShowMove CreateShowMoveModel(int deviceNumber, int newRoundNumber, int tableNotReadyNumber);
        EnterContract CreateEnterContractModel(int deviceNumber, Result result, LeadValidationOptions leadValidation = LeadValidationOptions.NoWarning);
        ShowTraveller CreateShowTravellerModel(int deviceNumber);
        ShowHandRecord? CreateShowHandRecordModel(int deviceNumber, int boardNumber);
        ShowRankingList CreateRankingListModel(int deviceNumber);
        
        Move GetMove(List<Round> roundsList, int tableNumber, int pairNumber, Direction direction);
        int GetBoardsFromTableNumber(TableStatus tableStatus);
        List<Ranking> GetRankings(int sectionID);

        string Header(HeaderType headerType, int parameter1 = 0, int parameter2 = 0);
        string Title(string titleString, TitleType titleType = TitleType.Plain, int parameter1 = 0, int parameter2 = 0);
        bool ValidateLead(TableStatus tableStatus, string card);
    }
}
