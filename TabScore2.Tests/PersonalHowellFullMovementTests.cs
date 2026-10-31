using GrpcSharedContracts.SharedClasses;
using Microsoft.AspNetCore.Mvc;
using TabScore2.Globals;
using TabScore2.Tests.Support;

namespace TabScore2.Tests;

public sealed class PersonalHowellFullMovementTests
{
    public static IEnumerable<object[]> TransitionCases()
    {
        for (int roundNumber = 1; roundNumber < 13; roundNumber++)
        {
            for (int tableNumber = 1; tableNumber <= 7; tableNumber++)
            {
                yield return [roundNumber, tableNumber, false];
                yield return [roundNumber, tableNumber, true];
            }
        }
    }

    public static IEnumerable<object[]> CompleteRoundCases()
    {
        for (int roundNumber = 1; roundNumber < 13; roundNumber++)
        {
            yield return [roundNumber, false];
            yield return [roundNumber, true];
        }
    }

    [Fact]
    public void SevenTableFixtureDefinesACompleteFullHowell()
    {
        HowellMovement movement = HowellMovement.LoadSevenTableFixture();

        Assert.Equal(13, movement.NumberOfRounds);
        Assert.Equal(7, movement.NumberOfTables);

        for (int roundNumber = 1; roundNumber <= movement.NumberOfRounds; roundNumber++)
        {
            IReadOnlyList<Round> rounds = movement.GetRound(roundNumber);
            List<int> pairs = rounds
                .SelectMany(round => new[] { round.ContestantNumberNorth, round.ContestantNumberEast })
                .Order()
                .ToList();

            Assert.True(
                Enumerable.Range(1, 14).SequenceEqual(pairs),
                $"Round {roundNumber} does not contain every pair exactly once.");
            Assert.Equal(Enumerable.Range(1, 7), rounds.Select(round => round.TableNumber));

            (int tableNumber, Direction direction) = movement.FindPair(roundNumber, pairNumber: 14);
            Assert.True(
                tableNumber == 4 && direction == Direction.North,
                $"Pair 14 is not stationary NS at table 4 in round {roundNumber}.");
        }
    }

    [Theory]
    [MemberData(nameof(TransitionCases))]
    public void DestinationWaitsForOutgoingDevicesThenAcceptsBothIncomingPairs(
        int roundNumber,
        int tableNumber,
        bool reverseIncomingOrder)
    {
        int newRoundNumber = roundNumber + 1;
        using PersonalHowellHarness harness = new(roundNumber);
        Round outgoing = harness.Movement.GetRound(roundNumber, tableNumber);
        Round incoming = harness.Movement.GetRound(newRoundNumber, tableNumber);
        int[] incomingPairs = reverseIncomingOrder
            ? [incoming.ContestantNumberEast, incoming.ContestantNumberNorth]
            : [incoming.ContestantNumberNorth, incoming.ContestantNumberEast];
        int[] outgoingPairs = [outgoing.ContestantNumberNorth, outgoing.ContestantNumberEast];
        string scenario = $"round {roundNumber}->{newRoundNumber}, table {tableNumber}, first pair {incomingPairs[0]}";

        Assert.IsType<ViewResult>(harness.ShowMove(incomingPairs[0], newRoundNumber));
        int readyOutgoingDevices = outgoingPairs.Count(pairNumber => harness.Device(pairNumber).ReadyForNextRound);
        Assert.True(readyOutgoingDevices < 2, $"Both outgoing devices were unexpectedly ready for {scenario}.");
        AssertBlocked(harness.AttemptMove(incomingPairs[0], newRoundNumber), newRoundNumber, tableNumber, scenario);
        Assert.Equal(roundNumber, harness.Table(tableNumber).RoundNumber);

        if (readyOutgoingDevices == 0)
        {
            Assert.IsType<ViewResult>(harness.ShowMove(outgoingPairs[0], newRoundNumber));
        }

        Assert.Equal(1, outgoingPairs.Count(pairNumber => harness.Device(pairNumber).ReadyForNextRound));
        AssertBlocked(harness.AttemptMove(incomingPairs[0], newRoundNumber), newRoundNumber, tableNumber, scenario);
        Assert.Equal(roundNumber, harness.Table(tableNumber).RoundNumber);

        foreach (int outgoingPair in outgoingPairs.Where(pairNumber => !harness.Device(pairNumber).ReadyForNextRound))
        {
            Assert.IsType<ViewResult>(harness.ShowMove(outgoingPair, newRoundNumber));
        }

        Assert.Equal(2, outgoingPairs.Count(pairNumber => harness.Device(pairNumber).ReadyForNextRound));
        AssertSuccessfulMove(harness.AttemptMove(incomingPairs[0], newRoundNumber), scenario);
        AssertTableAtRound(harness, tableNumber, newRoundNumber, incoming, scenario);
        AssertPairAtExpectedLocation(harness, incomingPairs[0], newRoundNumber, scenario);

        Assert.IsType<ViewResult>(harness.ShowMove(incomingPairs[1], newRoundNumber));
        AssertSuccessfulMove(harness.AttemptMove(incomingPairs[1], newRoundNumber), scenario);
        AssertPairAtExpectedLocation(harness, incomingPairs[1], newRoundNumber, scenario);
    }

    [Theory]
    [MemberData(nameof(CompleteRoundCases))]
    public void AllPairsReachTheirAssignedLocationsWhenEveryoneIsReady(
        int roundNumber,
        bool reversePairOrder)
    {
        int newRoundNumber = roundNumber + 1;
        using PersonalHowellHarness harness = new(roundNumber);
        // This is the complementary happy path: once every outgoing device is ready,
        // arrival order must not affect any table or device assignment.
        int[] pairOrder = reversePairOrder
            ? Enumerable.Range(1, 14).Reverse().ToArray()
            : Enumerable.Range(1, 14).ToArray();

        foreach (int pairNumber in pairOrder)
        {
            Assert.IsType<ViewResult>(harness.ShowMove(pairNumber, newRoundNumber));
        }

        foreach (int pairNumber in pairOrder)
        {
            string scenario = $"round {roundNumber}->{newRoundNumber}, pair {pairNumber}";
            AssertSuccessfulMove(harness.AttemptMove(pairNumber, newRoundNumber), scenario);
            AssertPairAtExpectedLocation(harness, pairNumber, newRoundNumber, scenario);
        }

        foreach (Round expectedRound in harness.Movement.GetRound(newRoundNumber))
        {
            AssertTableAtRound(
                harness,
                expectedRound.TableNumber,
                newRoundNumber,
                expectedRound,
                $"completed round {roundNumber}->{newRoundNumber}");
        }
    }

    private static void AssertBlocked(
        ActionResult result,
        int newRoundNumber,
        int tableNumber,
        string scenario)
    {
        RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.True(
            redirect.ActionName == "Index" && redirect.ControllerName == "ShowMove",
            $"Expected a ShowMove wait redirect for {scenario}.");
        Assert.Equal(newRoundNumber, redirect.RouteValues!["newRoundNumber"]);
        Assert.Equal(tableNumber, redirect.RouteValues["tableNotReadyNumber"]);
    }

    private static void AssertSuccessfulMove(ActionResult result, string scenario)
    {
        RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.True(
            redirect.ActionName == "Index" && redirect.ControllerName == "ShowPlayerIds",
            $"Expected successful movement to ShowPlayerIds for {scenario}.");
    }

    private static void AssertTableAtRound(
        PersonalHowellHarness harness,
        int tableNumber,
        int roundNumber,
        Round expected,
        string scenario)
    {
        Assert.True(
            harness.Table(tableNumber).RoundNumber == roundNumber
                && harness.Table(tableNumber).RoundData.ContestantNumberNorth == expected.ContestantNumberNorth
                && harness.Table(tableNumber).RoundData.ContestantNumberEast == expected.ContestantNumberEast,
            $"Table {tableNumber} has inconsistent state for {scenario}.");
    }

    private static void AssertPairAtExpectedLocation(
        PersonalHowellHarness harness,
        int pairNumber,
        int roundNumber,
        string scenario)
    {
        (int expectedTable, Direction expectedDirection) = harness.Movement.FindPair(roundNumber, pairNumber);
        Assert.True(
            harness.Device(pairNumber).TableNumber == expectedTable
                && harness.Device(pairNumber).RoundNumber == roundNumber
                && harness.Device(pairNumber).Direction == expectedDirection
                && harness.Device(pairNumber).Scoring == (expectedDirection == Direction.North)
                && !harness.Device(pairNumber).ReadyForNextRound,
            $"Pair {pairNumber} has inconsistent state for {scenario}.");
    }
}
