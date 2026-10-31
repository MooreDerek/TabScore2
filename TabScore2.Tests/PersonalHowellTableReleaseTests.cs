using Microsoft.AspNetCore.Mvc;
using TabScore2.Globals;
using TabScore2.Tests.Support;

namespace TabScore2.Tests;

public sealed class PersonalHowellTableReleaseTests
{
    [Theory]
    [InlineData(6, 13)]
    [InlineData(13, 6)]
    public void TableReleasesAfterBothOutgoingDevicesAreReady(
        int firstIncomingPair,
        int secondIncomingPair)
    {
        using PersonalHowellHarness harness = new();
        harness.Database.AddCompletedResults(roundNumber: 1, tableNumber: 1);

        Assert.IsType<ViewResult>(harness.ShowMove(pairNumber: 5, newRoundNumber: 2));
        Assert.IsType<ViewResult>(harness.ShowMove(firstIncomingPair, newRoundNumber: 2));
        Assert.IsType<RedirectToActionResult>(harness.AttemptMove(firstIncomingPair, newRoundNumber: 2));
        Assert.Equal(1, harness.Table(1).RoundNumber);

        Assert.IsType<RedirectToActionResult>(harness.CompleteViewOnlyBoards(pairNumber: 12));
        Assert.IsType<ViewResult>(harness.ShowMove(pairNumber: 12, newRoundNumber: 2));

        AssertSuccessfulMove(harness.AttemptMove(firstIncomingPair, newRoundNumber: 2));
        Assert.Equal(2, harness.Table(1).RoundNumber);
        Assert.Equal(6, harness.Table(1).RoundData.ContestantNumberNorth);
        Assert.Equal(13, harness.Table(1).RoundData.ContestantNumberEast);
        AssertAtRoundTwoTableOne(harness, firstIncomingPair);

        Assert.IsType<ViewResult>(harness.ShowMove(secondIncomingPair, newRoundNumber: 2));
        AssertSuccessfulMove(harness.AttemptMove(secondIncomingPair, newRoundNumber: 2));
        AssertAtRoundTwoTableOne(harness, secondIncomingPair);
    }

    private static void AssertSuccessfulMove(ActionResult result)
    {
        RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("ShowPlayerIds", redirect.ControllerName);
    }

    private static void AssertAtRoundTwoTableOne(PersonalHowellHarness harness, int pairNumber)
    {
        Direction expectedDirection = pairNumber == 6 ? Direction.North : Direction.East;
        Assert.Equal(1, harness.Device(pairNumber).TableNumber);
        Assert.Equal(2, harness.Device(pairNumber).RoundNumber);
        Assert.Equal(expectedDirection, harness.Device(pairNumber).Direction);
        Assert.Equal(expectedDirection == Direction.North, harness.Device(pairNumber).Scoring);
        Assert.False(harness.Device(pairNumber).ReadyForNextRound);
    }
}
