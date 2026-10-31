using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TabScore2.Classes;
using TabScore2.Controllers;
using TabScore2.Globals;
using TabScore2.Tests.Support;

namespace TabScore2.Tests;

public sealed class PersonalHowellDiscoveredIssueTests
{
    [Fact]
    [Trait("Category", "DiscoveredIssue")]
    public void CurrentBehaviour_TableDirectionReregistrationCanSelectOutgoingPair()
    {
        using PersonalHowellHarness harness = new();

        // Round 1 table 2 contains pairs 2 NS and 4 EW. In round 2, pair 5 arrives EW.
        Assert.IsType<ViewResult>(harness.ShowMove(pairNumber: 2, newRoundNumber: 2));
        Assert.IsType<ViewResult>(harness.ShowMove(pairNumber: 4, newRoundNumber: 2));
        Assert.IsType<ViewResult>(harness.ShowMove(pairNumber: 5, newRoundNumber: 2));
        AssertSuccessfulMove(harness.AttemptMove(pairNumber: 5, newRoundNumber: 2));

        Assert.Equal(2, harness.Table(2).RoundNumber);
        Assert.Equal(5, harness.Table(2).RoundData.ContestantNumberEast);
        Assert.Equal(2, harness.Device(5).RoundNumber);
        Assert.Equal(1, harness.Device(4).RoundNumber);

        SelectDirectionController controller = new(
            harness.Localizer,
            harness.AppData,
            harness.Settings,
            harness.BusLogic);
        DefaultHttpContext httpContext = new();
        httpContext.Session = new TestSession();
        httpContext.Session.SetInt32("SectionId", 1);
        httpContext.Session.SetInt32("TableNumber", 2);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        RedirectToActionResult result = Assert.IsType<RedirectToActionResult>(
            controller.OKButtonClick(Direction.East, confirm: true));
        int selectedDeviceNumber = Assert.IsType<int>(httpContext.Session.GetInt32("DeviceNumber"));
        DeviceStatus selectedDevice = harness.AppData.GetDeviceStatus(selectedDeviceNumber);

        // This assertion documents the defect: pair 4 is selected even though round 2 table 2 EW is pair 5.
        Assert.Equal(4, selectedDevice.ContestantNumber);
        Assert.Equal(1, selectedDevice.RoundNumber);
        Assert.NotEqual(harness.Table(2).RoundData.ContestantNumberEast, selectedDevice.ContestantNumber);
        Assert.Equal("ShowMove", result.ControllerName);
    }


    [Fact]
    [Trait("Category", "DiscoveredIssue")]
    public void CurrentBehaviour_RestartBeforeFirstResultRestoresPreviousRound()
    {
        using PersonalHowellHarness harness = new();
        harness.Database.AddCompletedResults(roundNumber: 1, tableNumber: 1);

        Assert.IsType<ViewResult>(harness.ShowMove(pairNumber: 5, newRoundNumber: 2));
        Assert.IsType<ViewResult>(harness.ShowMove(pairNumber: 12, newRoundNumber: 2));
        Assert.IsType<ViewResult>(harness.ShowMove(pairNumber: 6, newRoundNumber: 2));
        AssertSuccessfulMove(harness.AttemptMove(pairNumber: 6, newRoundNumber: 2));
        Assert.Equal(2, harness.Table(1).RoundNumber);
        Assert.Equal(6, harness.Table(1).RoundData.ContestantNumberNorth);
        Assert.Empty(harness.Database.GetResultsList(1, 3, 4, 1, 2));

        // Clearing AppData models an application restart: device and table state is in memory only.
        harness.AppData.ClearAppData();

        SelectDirectionController controller = new(
            harness.Localizer,
            harness.AppData,
            harness.Settings,
            harness.BusLogic);
        DefaultHttpContext httpContext = new();
        httpContext.Session = new TestSession();
        httpContext.Session.SetInt32("SectionId", 1);
        httpContext.Session.SetInt32("TableNumber", 1);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        RedirectToActionResult result = Assert.IsType<RedirectToActionResult>(
            controller.OKButtonClick(Direction.North, confirm: false));
        int selectedDeviceNumber = Assert.IsType<int>(httpContext.Session.GetInt32("DeviceNumber"));
        DeviceStatus restoredDevice = harness.AppData.GetDeviceStatus(selectedDeviceNumber);

        // This assertion documents the defect: the last saved result is round 1, so restart
        // reconstructs round 1 even though table 1 had already advanced to round 2.
        Assert.Equal(1, harness.Table(1).RoundNumber);
        Assert.Equal(5, harness.Table(1).RoundData.ContestantNumberNorth);
        Assert.Equal(1, restoredDevice.RoundNumber);
        Assert.Equal(5, restoredDevice.ContestantNumber);
        Assert.Equal("ShowPlayerIds", result.ControllerName);
    }

    [Fact]
    [Trait("Category", "DiscoveredIssue")]
    public void CurrentBehaviour_TableStatusReportsReadyForCurrentRoundInsteadOfNextRound()
    {
        using PersonalHowellHarness harness = new();

        Assert.False(harness.Device(5).ReadyForNextRound);
        Assert.False(harness.Device(12).ReadyForNextRound);
        Assert.False(harness.AppData.IsTableReadyForNextRound(1, 1, 2));

        TableStatusDisplay tableOne = Assert.Single(
            harness.AppData.GetAllTableStatuses(),
            status => status.TableNumber == 1);

        // This assertion documents the reporting defect: GetAllTableStatuses asks whether
        // round 1 is ready for round 1, rather than whether it is ready for round 2.
        Assert.True(tableOne.ReadyForNextRound);
    }
    private static void AssertSuccessfulMove(ActionResult result)
    {
        RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("ShowPlayerIds", redirect.ControllerName);
    }
}
