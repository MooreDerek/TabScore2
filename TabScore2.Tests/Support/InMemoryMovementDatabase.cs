using GrpcSharedContracts.SharedClasses;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Globals;

namespace TabScore2.Tests.Support;

internal sealed record PlayerUpdate(
    int SectionId,
    int TableNumber,
    int RoundNumber,
    string Direction,
    int PairNumber,
    int PlayerId,
    string PlayerName);

internal sealed class InMemoryMovementDatabase(HowellMovement movement, int initialRound) : IDatabase
{
    private readonly List<Result> results = [];

    public HowellMovement Movement { get; } = movement;
    public List<PlayerUpdate> PlayerUpdates { get; } = [];
    public int InitialRound { get; set; } = initialRound;
    public Section Section { get; } = new()
    {
        SectionId = 1,
        SectionLetter = "A",
        NumberOfTables = 7,
        Winners = 1,
        NumberOfRounds = 13,
    };

    public string Initialize(string pathToDatabase) => string.Empty;
    public void WebappInitialize() { }
    public bool IsDatabaseConnectionOK() => true;
    public List<Section> GetSectionsList() => [Section];
    public Section GetSection(int sectionId) => Section;
    public void RegisterTable(int sectionId, int tableNumber) { }
    public int GetNumberOfRoundsInSection(int sectionId, bool forceDatabaseRead = false) => Movement.NumberOfRounds;
    public int GetLastRoundWithResultsForTable(int sectionId, int tableNumber) => InitialRound;

    public Location GetLastLocationWithResultsForContestant(int sectionId, int contestantNumber) =>
        new() { TableNumber = 0, RoundNumber = 0 };

    public Location GetStartLocationForContestant(int sectionId, int contestantNumber)
    {
        (int tableNumber, Direction direction) = Movement.FindPair(1, contestantNumber);
        return new Location { TableNumber = tableNumber, RoundNumber = 1, Direction = direction };
    }

    public List<Round> GetRoundsListForSectionRound(int sectionId, int roundNumber) =>
        [.. Movement.GetRound(roundNumber)];

    public List<Round> GetRoundsListForSection(int sectionId) =>
        Enumerable.Range(1, Movement.NumberOfRounds)
            .SelectMany(Movement.GetRound)
            .ToList();

    public Round GetRound(int sectionId, int tableNumber, int roundNumber) =>
        Movement.GetRound(roundNumber, tableNumber);

    public Result GetResult(int sectionId, int tableNumber, int roundNumber, int boardNumber) =>
        results.Find(result => result.SectionId == sectionId
            && result.TableNumber == tableNumber
            && result.RoundNumber == roundNumber
            && result.BoardNumber == boardNumber)
        ?? new Result
        {
            SectionId = sectionId,
            TableNumber = tableNumber,
            RoundNumber = roundNumber,
            BoardNumber = boardNumber,
            ContractLevel = -999,
            TricksTaken = -1,
        };

    public void SetResult(Result result)
    {
        results.RemoveAll(existing => existing.SectionId == result.SectionId
            && existing.TableNumber == result.TableNumber
            && existing.RoundNumber == result.RoundNumber
            && existing.BoardNumber == result.BoardNumber);
        results.Add(result);
    }

    public List<Result> GetResultsList(int sectionId = 0, int lowBoard = 0, int highBoard = 0, int tableNumber = 0, int roundNumber = 0)
    {
        IEnumerable<Result> query = results;
        if (sectionId != 0)
        {
            query = query.Where(result => result.SectionId == sectionId);
        }

        if (lowBoard != 0 && highBoard == 0)
        {
            query = query.Where(result => result.BoardNumber == lowBoard);
        }
        else if (lowBoard != 0)
        {
            query = query.Where(result => result.TableNumber == tableNumber
                && result.RoundNumber == roundNumber
                && result.BoardNumber >= lowBoard
                && result.BoardNumber <= highBoard);
        }

        return query.ToList();
    }

    public void AddCompletedResults(int roundNumber, int tableNumber)
    {
        Round round = Movement.GetRound(roundNumber, tableNumber);
        for (int boardNumber = round.LowBoard; boardNumber <= round.HighBoard; boardNumber++)
        {
            SetResult(new Result
            {
                SectionId = Section.SectionId,
                SectionLetter = Section.SectionLetter,
                TableNumber = tableNumber,
                RoundNumber = roundNumber,
                BoardNumber = boardNumber,
                ContestantNumberNorth = round.ContestantNumberNorth,
                ContestantNumberEast = round.ContestantNumberEast,
                ContractLevel = 0,
                TricksTaken = -1,
            });
        }
    }

    public string GetInternalPlayerName(int playerId) => $"Player {playerId}";

    public void UpdatePlayer(int sectionId, int tableNumber, int roundNumber, string directionLetter, int pairNumber, int playerId, string playerName) =>
        PlayerUpdates.Add(new PlayerUpdate(sectionId, tableNumber, roundNumber, directionLetter, pairNumber, playerId, playerName));

    public NamesForRound GetNamesForTableRound(int sectionId, int roundNumber, int numberNorth, int numberEast, int numberSouth, int numberWest) => new();

    public int GetHandsCount() => 0;
    public List<Hand> GetHandsList() => [];
    public Hand GetHand(int sectionId, int boardNumber) => new() { SectionId = sectionId, BoardNumber = boardNumber, NorthSpades = "###" };
    public void AddHand(Hand hand) { }
    public void AddHands(List<Hand> newHandsList) { }
    public void GetDatabaseSettings(int sectionId = 1, int roundNumber = 0) { }
    public void SetDatabaseSettings() { }
    public List<Ranking> GetRankingList(int sectionId) => [];
}
