using Microsoft.AspNetCore.Mvc;
using TabScore2.Globals;
using TabScore2.Tests.Support;

namespace TabScore2.Tests;

public sealed class PersonalHowellEarlyArrivalTests
{
    [Fact]
    public void IncomingPairWaitsWhileTableOneViewerHasNotFinishedNormalWorkflow()
    {
        using PersonalHowellHarness harness = new();
        Assert.IsType<RedirectToActionResult>(harness.EnterPlayerNumber(5, Direction.North, 10001));
        Assert.IsType<RedirectToActionResult>(harness.EnterPlayerNumber(5, Direction.South, 10002));
        Assert.IsType<RedirectToActionResult>(harness.EnterPlayerNumber(12, Direction.East, 10003));
        Assert.IsType<RedirectToActionResult>(harness.EnterPlayerNumber(12, Direction.West, 10004));
        Assert.Equal(new[] { 5, 5, 12, 12 }, harness.Database.PlayerUpdates.Select(update => update.PairNumber));
        Assert.All(harness.Database.PlayerUpdates, update =>
        {
            Assert.Equal(1, update.TableNumber);
            Assert.Equal(1, update.RoundNumber);
        });
        harness.Database.AddCompletedResults(roundNumber: 1, tableNumber: 1);

        Assert.IsType<ViewResult>(harness.ShowMove(pairNumber: 5, newRoundNumber: 2));
        Assert.True(harness.Device(5).ReadyForNextRound);
        Assert.False(harness.Device(12).ReadyForNextRound);

        Assert.IsType<ViewResult>(harness.ShowMove(pairNumber: 6, newRoundNumber: 2));
        RedirectToActionResult blockedMove = Assert.IsType<RedirectToActionResult>(
            harness.AttemptMove(pairNumber: 6, newRoundNumber: 2));

        Assert.Equal("Index", blockedMove.ActionName);
        Assert.Equal("ShowMove", blockedMove.ControllerName);
        Assert.Equal(2, blockedMove.RouteValues!["newRoundNumber"]);
        Assert.Equal(1, blockedMove.RouteValues["tableNotReadyNumber"]);

        Assert.Equal(7, harness.Device(6).TableNumber);
        Assert.Equal(1, harness.Device(6).RoundNumber);
        Assert.Equal(Direction.North, harness.Device(6).Direction);

        Assert.Equal(1, harness.Table(1).RoundNumber);
        Assert.Equal(5, harness.Table(1).RoundData.ContestantNumberNorth);
        Assert.Equal(12, harness.Table(1).RoundData.ContestantNumberEast);
        Assert.Equal(1, harness.Table(1).RoundData.LowBoard);
        Assert.Equal(2, harness.Table(1).RoundData.HighBoard);

        RedirectToActionResult viewerProgress = Assert.IsType<RedirectToActionResult>(
            harness.CompleteViewOnlyBoards(pairNumber: 12));
        Assert.Equal("Index", viewerProgress.ActionName);
        Assert.Equal("ShowRankingList", viewerProgress.ControllerName);
    }
}
