using GrpcSharedContracts.SharedClasses;
using TabScore2.Globals;

namespace TabScore2.Tests.Support;

internal sealed class HowellMovement
{
    private readonly Dictionary<(int Round, int Table), Round> rounds;

    private HowellMovement(Dictionary<(int Round, int Table), Round> rounds)
    {
        this.rounds = rounds;
    }

    public int NumberOfRounds => rounds.Keys.Max(key => key.Round);
    public int NumberOfTables => rounds.Keys.Max(key => key.Table);

    public static HowellMovement LoadSevenTableFixture()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "7-table-howell.csv");
        string[] lines = File.ReadAllLines(path);
        Dictionary<(int Round, int Table), Round> rounds = [];

        foreach (string line in lines.Skip(1).Where(line => !string.IsNullOrWhiteSpace(line)))
        {
            string[] fields = line.Split(',');
            int roundNumber = int.Parse(fields[0]);

            for (int tableNumber = 1; tableNumber <= 7; tableNumber++)
            {
                int offset = 1 + (tableNumber - 1) * 3;
                int boardSet = int.Parse(fields[offset + 2]);
                rounds[(roundNumber, tableNumber)] = new Round
                {
                    TableNumber = tableNumber,
                    ContestantNumberNorth = int.Parse(fields[offset]),
                    ContestantNumberEast = int.Parse(fields[offset + 1]),
                    LowBoard = boardSet * 2 - 1,
                    HighBoard = boardSet * 2,
                };
            }
        }

        return new HowellMovement(rounds);
    }

    public Round GetRound(int roundNumber, int tableNumber)
    {
        Round source = rounds[(roundNumber, tableNumber)];
        return new Round
        {
            TableNumber = source.TableNumber,
            ContestantNumberNorth = source.ContestantNumberNorth,
            ContestantNumberEast = source.ContestantNumberEast,
            ContestantNumberSouth = source.ContestantNumberNorth,
            ContestantNumberWest = source.ContestantNumberEast,
            LowBoard = source.LowBoard,
            HighBoard = source.HighBoard,
        };
    }

    public IReadOnlyList<Round> GetRound(int roundNumber) =>
        Enumerable.Range(1, NumberOfTables)
            .Select(tableNumber => GetRound(roundNumber, tableNumber))
            .ToList();

    public (int TableNumber, Direction Direction) FindPair(int roundNumber, int pairNumber)
    {
        foreach (Round round in GetRound(roundNumber))
        {
            if (round.ContestantNumberNorth == pairNumber)
            {
                return (round.TableNumber, Direction.North);
            }

            if (round.ContestantNumberEast == pairNumber)
            {
                return (round.TableNumber, Direction.East);
            }
        }

        throw new InvalidOperationException($"Pair {pairNumber} is absent from round {roundNumber}.");
    }
}
