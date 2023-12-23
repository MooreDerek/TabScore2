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

        string Header(int deviceNumber, HeaderType headerType, int boardNumber = 0);
        string Title(int deviceNumber, string titleString, TitleType titleType);
        bool ValidateLead(TableStatus tableStatus, string card);
    }
}
