using TabScore2.Globals;
using TabScore2.Tests.Support;

namespace TabScore2.Tests;

public sealed class PersonalHowellHarnessTests
{
    [Fact]
    public void RegistersRoundOneDevicesByTableAndDirection()
    {
        using PersonalHowellHarness harness = new();

        List<int> registeredPairs = Enumerable.Range(1, 14)
            .Select(pairNumber => harness.Device(pairNumber).ContestantNumber)
            .Order()
            .ToList();

        Assert.Equal(Enumerable.Range(1, 14), registeredPairs);
        Assert.Equal(1, harness.Device(5).TableNumber);
        Assert.Equal(Direction.North, harness.Device(5).Direction);
        Assert.True(harness.Device(5).Scoring);
        Assert.Equal(1, harness.Device(12).TableNumber);
        Assert.Equal(Direction.East, harness.Device(12).Direction);
        Assert.False(harness.Device(12).Scoring);
    }
}
