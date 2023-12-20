using TabScore.Models;
using TabScore2.Classes;
using TabScore2.Globals;
using TabScore2.Models;

namespace TabScore2.UtilityServices
{
    public interface IUtilities
    {
        ShowPlayerIDs CreateShowPlayerIDsModel(int tabletDeviceNumber, bool showWarning);
        ShowBoards CreateShowBoardsModel(int tabletDeviceNumber);
        ShowMove CreateShowMoveModel(int tabletDeviceNumber, int newRoundNumber, int tableNotReadyNumber);
        ShowTraveller CreateShowTravellerModel(int tabletDeviceNumber);
        ShowHandRecord? CreateShowHandRecordModel(int tabletDeviceNumber, int boardNumber);
        ShowRankingList CreateRankingListModel(int tabletDeviceNumber);
        
        Move GetMove(List<Round> roundsList, int tableNumber, int pairNumber, Direction direction);
        int GetBoardsFromTableNumber(TableStatus tableStatus);
        List<Ranking> GetRankings(int sectionID);

        string Header(int tabletDeviceNumber, HeaderType headerType, int boardNumber = 0);
        string Title(int tabletDeviceNumber, string titleString, TitleType titleType);
        bool ValidateLead(TableStatus tableStatus, string card);
    }
}
