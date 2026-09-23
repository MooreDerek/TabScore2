// Howell Table 3 Deadlock — Deterministic Reproduction
//
// This test simulates the in-memory state transitions of AppData,
// ShowMoveController.Index (flag-setting), and OKButtonClick (readiness
// gate + UpdateDeviceStatus + UpdateTableStatus) to reproduce the
// permanent deadlock at the switch table in a 7-table Howell movement.
//
// No ASP.NET, no database, no threads — just the state machine.

using Xunit;
using Xunit.Abstractions;

namespace HowellDeadlockSimulation;

// ── Minimal domain types ────────────────────────────────────────────

public enum Direction { North, South, East, West, Sitout }

public record RoundAssignment(int TableNumber, int NS, int EW, int LowBoard, int HighBoard);

public class RoundData
{
    public int NumberNorth { get; set; }
    public int NumberSouth { get; set; }
    public int NumberEast  { get; set; }
    public int NumberWest  { get; set; }
    public int LowBoard    { get; set; }
    public int HighBoard   { get; set; }
}

public class TableStatus
{
    public int       TableNumber             { get; set; }
    public int       RoundNumber             { get; set; }
    public RoundData RoundData               { get; set; } = new();
    public bool      ReadyForNextRoundNorth   { get; set; }
    public bool      ReadyForNextRoundEast    { get; set; }
}

public class DeviceStatus
{
    public int       DeviceNumber { get; set; }
    public int       TableNumber  { get; set; }
    public int       PairNumber   { get; set; }
    public int       RoundNumber  { get; set; }
    public Direction Direction    { get; set; }
}

public record Move(int NewTableNumber, Direction NewDirection);

// ── Simulated AppData ───────────────────────────────────────────────

public class SimulatedAppData
{
    private readonly Dictionary<int, TableStatus> _tables = new();
    private readonly List<DeviceStatus> _devices = [];

    // Movement database — keyed by (round, table) → (NS pair, EW pair, lo board, hi board)
    private readonly Dictionary<(int round, int table), RoundAssignment> _movement = new();

    public void LoadMovement(IEnumerable<RoundAssignment> assignments, int round)
    {
        foreach (var a in assignments)
            _movement[(round, a.TableNumber)] = a;
    }

    public RoundData GetRound(int tableNumber, int roundNumber)
    {
        var a = _movement[(roundNumber, tableNumber)];
        return new RoundData
        {
            NumberNorth = a.NS, NumberSouth = a.NS,
            NumberEast  = a.EW, NumberWest  = a.EW,
            LowBoard = a.LowBoard, HighBoard = a.HighBoard
        };
    }

    // ── Table status ────────────────────────────────────────────────

    public TableStatus GetTableStatus(int tableNumber)
    {
        if (!_tables.ContainsKey(tableNumber))
            _tables[tableNumber] = new TableStatus { TableNumber = tableNumber, RoundNumber = 1 };
        return _tables[tableNumber];
    }

    /// <summary>Production code — the UNFIXED version.</summary>
    public void UpdateTableStatus_Original(int tableNumber, int roundNumber)
    {
        var ts = GetTableStatus(tableNumber);
        ts.RoundNumber = roundNumber;
        ts.RoundData   = GetRound(tableNumber, roundNumber);
        ts.ReadyForNextRoundNorth = false;
        ts.ReadyForNextRoundEast  = false;
    }

    /// <summary>Fixed version — skips the reset if table is already at this round.</summary>
    public void UpdateTableStatus_Fixed(int tableNumber, int roundNumber)
    {
        var ts = GetTableStatus(tableNumber);
        if (ts.RoundNumber >= roundNumber) return;   // ← THE FIX
        ts.RoundNumber = roundNumber;
        ts.RoundData   = GetRound(tableNumber, roundNumber);
        ts.ReadyForNextRoundNorth = false;
        ts.ReadyForNextRoundEast  = false;
    }

    // ── Device status ───────────────────────────────────────────────

    public int AddDevice(int tableNumber, int pairNumber, int roundNumber, Direction direction)
    {
        var ds = new DeviceStatus
        {
            DeviceNumber = _devices.Count,
            TableNumber  = tableNumber,
            PairNumber   = pairNumber,
            RoundNumber  = roundNumber,
            Direction    = direction
        };
        _devices.Add(ds);
        return ds.DeviceNumber;
    }

    public DeviceStatus GetDevice(int deviceNumber) => _devices[deviceNumber];

    public void UpdateDeviceStatus(int deviceNumber, int tableNumber, int roundNumber, Direction direction)
    {
        var ds = _devices[deviceNumber];
        ds.TableNumber = tableNumber;
        ds.Direction   = direction;
        ds.RoundNumber = roundNumber;
    }
}

// ── Simulated controller actions ────────────────────────────────────

public class SimulatedController
{
    private readonly SimulatedAppData _app;
    private readonly bool _useFix;

    public SimulatedController(SimulatedAppData app, bool useFix)
    {
        _app    = app;
        _useFix = useFix;
    }

    /// <summary>
    /// Equivalent of ShowMoveController.Index — sets the readiness flag
    /// on the device's CURRENT table based on its CURRENT direction.
    /// </summary>
    public void ShowMoveIndex(int deviceNumber, int newRoundNumber)
    {
        var ds = _app.GetDevice(deviceNumber);
        if (ds.TableNumber == 0) return;

        var ts = _app.GetTableStatus(ds.TableNumber);
        if (ts.RoundNumber < newRoundNumber)
        {
            switch (ds.Direction)
            {
                case Direction.North: ts.ReadyForNextRoundNorth = true; break;
                case Direction.East:  ts.ReadyForNextRoundEast  = true; break;
            }
        }
    }

    /// <summary>
    /// Equivalent of ShowMoveController.OKButtonClick.
    /// Returns true if the move succeeded, false if "table not ready".
    /// </summary>
    public bool OKButtonClick(int deviceNumber, Move move, int newRoundNumber)
    {
        var ds  = _app.GetDevice(deviceNumber);
        var nts = _app.GetTableStatus(move.NewTableNumber);

        bool ready;
        if (nts.RoundNumber == newRoundNumber)
            ready = true;
        else if (nts.RoundNumber < newRoundNumber - 1)
            ready = false;
        else
            ready = nts.ReadyForNextRoundNorth && nts.ReadyForNextRoundEast;

        if (ready)
        {
            _app.UpdateDeviceStatus(deviceNumber, move.NewTableNumber, newRoundNumber, move.NewDirection);
            if (_useFix)
                _app.UpdateTableStatus_Fixed(move.NewTableNumber, newRoundNumber);
            else
                _app.UpdateTableStatus_Original(move.NewTableNumber, newRoundNumber);
        }

        return ready;
    }
    /// <summary>
    /// Equivalent of ShowMoveController.OKButtonClick for DevicesPerTable == 1.
    /// The device stays at the table — just advances the round directly.
    /// Only one device per table, so only one UpdateTableStatus call.
    /// </summary>
    public void OKButtonClick_NonMoving(int deviceNumber, int newRoundNumber)
    {
        var ds = _app.GetDevice(deviceNumber);
        ds.RoundNumber = newRoundNumber;
        if (_useFix)
            _app.UpdateTableStatus_Fixed(ds.TableNumber, newRoundNumber);
        else
            _app.UpdateTableStatus_Original(ds.TableNumber, newRoundNumber);
    }
}

// ── The Howell deadlock test ────────────────────────────────────────

public class HowellDeadlockTest
{
    private readonly ITestOutputHelper _out;
    public HowellDeadlockTest(ITestOutputHelper output) => _out = output;

    /// <summary>
    /// Load the 7-table Howell movement data for rounds 2, 3, and 4.
    /// </summary>
    private static void LoadMovement(SimulatedAppData app)
    {
        // Round 2: T1(6,13,2) T2(3,5,3) T3(10,11,4) T4(14,2,5) T5(9,1,6) T6(8,12,7) T7(7,4,8)
        app.LoadMovement([
            new(1,6,13,3,4),  new(2,3,5,5,6),   new(3,10,11,7,8),
            new(4,14,2,9,10), new(5,9,1,11,12),  new(6,8,12,13,14),
            new(7,7,4,15,16)
        ], round: 2);

        // Round 3: T1(7,1,3) T2(4,6,4) T3(11,12,5) T4(14,3,6) T5(10,2,7) T6(9,13,8) T7(8,5,9)
        app.LoadMovement([
            new(1,7,1,5,6),   new(2,4,6,7,8),    new(3,11,12,9,10),
            new(4,14,3,11,12),new(5,10,2,13,14),  new(6,9,13,15,16),
            new(7,8,5,17,18)
        ], round: 3);

        // Round 4: T1(8,2,4) T2(5,7,5) T3(12,13,6) T4(14,4,7) T5(11,3,8) T6(10,1,9) T7(9,6,10)
        app.LoadMovement([
            new(1,8,2,7,8),   new(2,5,7,9,10),   new(3,12,13,11,12),
            new(4,14,4,13,14),new(5,11,3,15,16),  new(6,10,1,17,18),
            new(7,9,6,19,20)
        ], round: 4);
    }

    /// <summary>
    /// Run the full R2→R3→R4 sequence at Table 3. The staying pair
    /// finishes a round before the arriving pair calls UpdateTableStatus.
    /// Returns whether pair 12 can leave Table 3 for Round 5.
    /// </summary>
    private bool RunScenario(bool useFix)
    {
        var app = new SimulatedAppData();
        LoadMovement(app);
        var ctrl = new SimulatedController(app, useFix);

        // ── Bootstrap: start of Round 2 at Table 3 ─────────────────
        // Pair 10 = NS (direction North), Pair 11 = EW (direction East)
        var ts3 = app.GetTableStatus(3);
        ts3.RoundNumber = 2;
        ts3.RoundData   = app.GetRound(3, 2);

        int dev10 = app.AddDevice(3, pairNumber: 10, roundNumber: 2, Direction.North);
        int dev11 = app.AddDevice(3, pairNumber: 11, roundNumber: 2, Direction.East);

        // Pair 12 starts Round 2 at Table 6 as East
        var ts6 = app.GetTableStatus(6);
        ts6.RoundNumber = 2;
        ts6.RoundData   = app.GetRound(6, 2);
        int dev12 = app.AddDevice(6, pairNumber: 12, roundNumber: 2, Direction.East);

        // Also bootstrap Table 5 for pair 10's R3 destination
        var ts5 = app.GetTableStatus(5);
        ts5.RoundNumber = 2;
        ts5.RoundData   = app.GetRound(5, 2);

        _out.WriteLine("═══ ROUND 2 → ROUND 3 TRANSITION (Table 3) ═══");
        _out.WriteLine($"  Table 3 R2: NS=10, EW=11 → R3: NS=11, EW=12");
        _out.WriteLine($"  Pair 11 stays (East→North), Pair 12 arrives from T6");
        _out.WriteLine("");

        // ── R2→R3: both occupants view ShowMove ─────────────────────
        ctrl.ShowMoveIndex(dev10, newRoundNumber: 3);
        ctrl.ShowMoveIndex(dev11, newRoundNumber: 3);
        _out.WriteLine($"  Flags on T3 after ShowMove: N={ts3.ReadyForNextRoundNorth}, E={ts3.ReadyForNextRoundEast}");

        // Pair 11 (staying, fast) clicks OK first → Table 3 advances to R3
        var move11_r3 = new Move(3, Direction.North);  // stays at T3, switches to North
        bool ok11 = ctrl.OKButtonClick(dev11, move11_r3, newRoundNumber: 3);
        _out.WriteLine($"  Pair 11 OKButtonClick → {(ok11 ? "SUCCESS" : "BLOCKED")}");
        _out.WriteLine($"  T3 state: Round={ts3.RoundNumber}, flags N={ts3.ReadyForNextRoundNorth} E={ts3.ReadyForNextRoundEast}");

        // Pair 10 clicks OK → moves to Table 5
        var move10_r3 = new Move(5, Direction.North);
        // First ensure Table 5 has its flags set (its R2 occupants viewed ShowMove)
        ts5.ReadyForNextRoundNorth = true;
        ts5.ReadyForNextRoundEast  = true;
        bool ok10 = ctrl.OKButtonClick(dev10, move10_r3, newRoundNumber: 3);
        _out.WriteLine($"  Pair 10 OKButtonClick → {(ok10 ? "SUCCESS → Table 5" : "BLOCKED")}");
        _out.WriteLine("");

        // ── KEY TIMING: pair 11 plays entire Round 3, then views ShowMove for R4 ──
        _out.WriteLine("  *** Pair 11 (staying, fast) plays entire Round 3 ***");
        _out.WriteLine("  *** Pair 11 views ShowMove.Index for Round 4 ***");
        ctrl.ShowMoveIndex(dev11, newRoundNumber: 4);
        _out.WriteLine($"  T3 flags after pair 11 sets R4 flag: N={ts3.ReadyForNextRoundNorth}, E={ts3.ReadyForNextRoundEast}");
        _out.WriteLine("");

        // ── LATE ARRIVAL: pair 12 (elderly, slow walk) finally clicks OK ──
        _out.WriteLine("  *** Pair 12 (slow, elderly) finally arrives from Table 6 ***");
        ctrl.ShowMoveIndex(dev12, newRoundNumber: 3); // sets flag on T6, not T3
        var move12_r3 = new Move(3, Direction.East);
        bool ok12_r3 = ctrl.OKButtonClick(dev12, move12_r3, newRoundNumber: 3);
        _out.WriteLine($"  Pair 12 OKButtonClick → {(ok12_r3 ? "SUCCESS → Table 3" : "BLOCKED")}");
        _out.WriteLine($"  T3 state AFTER pair 12 arrives: Round={ts3.RoundNumber}, flags N={ts3.ReadyForNextRoundNorth} E={ts3.ReadyForNextRoundEast}");
        _out.WriteLine("");

        // ── Now pair 12 plays Round 3 and views ShowMove for R4 ────
        _out.WriteLine("  *** Pair 12 plays Round 3 at Table 3 ***");
        ctrl.ShowMoveIndex(dev12, newRoundNumber: 4);
        _out.WriteLine($"  T3 flags after pair 12 sets R4 flag: N={ts3.ReadyForNextRoundNorth}, E={ts3.ReadyForNextRoundEast}");
        _out.WriteLine("");

        // ── R3→R4 transition: pair 11 tries to leave ───────────────
        _out.WriteLine("═══ ROUND 3 → ROUND 4 TRANSITION (Table 3) ═══");
        _out.WriteLine($"  Table 3 R3: NS=11, EW=12 → R4: NS=12, EW=13");
        _out.WriteLine($"  Pair 12 stays (East→North), Pair 11 leaves for T5");
        _out.WriteLine("");

        var move11_r4 = new Move(5, Direction.North);  // pair 11 → Table 5
        // Table 5 is ready (other pairs have set flags)
        ts5.ReadyForNextRoundNorth = true;
        ts5.ReadyForNextRoundEast  = true;

        bool ok11_r4 = ctrl.OKButtonClick(dev11, move11_r4, newRoundNumber: 4);
        _out.WriteLine($"  Pair 11 tries to leave T3 for T5 → {(ok11_r4 ? "SUCCESS" : "BLOCKED")}");

        // Pair 12 tries to advance (staying, direction switch)
        var move12_r4 = new Move(3, Direction.North);  // stays at T3, switches to North
        bool ok12_r4 = ctrl.OKButtonClick(dev12, move12_r4, newRoundNumber: 4);
        _out.WriteLine($"  Pair 12 tries to stay at T3 (switch to North) → {(ok12_r4 ? "SUCCESS" : "BLOCKED")}");
        _out.WriteLine("");
        _out.WriteLine($"  T3 final state: Round={ts3.RoundNumber}, flags N={ts3.ReadyForNextRoundNorth} E={ts3.ReadyForNextRoundEast}");

        // ── Retry loop: can pair 12 ever get unstuck? ───────────────
        _out.WriteLine("");
        _out.WriteLine("  *** Retry loop (5 attempts) ***");
        for (int retry = 1; retry <= 5; retry++)
        {
            // ShowMove.Index re-sets the flag
            ctrl.ShowMoveIndex(dev12, newRoundNumber: 4);
            ok12_r4 = ctrl.OKButtonClick(dev12, move12_r4, newRoundNumber: 4);
            _out.WriteLine($"    Retry {retry}: T3 flags N={ts3.ReadyForNextRoundNorth} E={ts3.ReadyForNextRoundEast} → {(ok12_r4 ? "SUCCESS" : "BLOCKED")}");
            if (ok12_r4) break;
        }

        // Also check: can pair 11 retry if it was blocked?
        if (!ok11_r4)
        {
            _out.WriteLine("");
            ctrl.ShowMoveIndex(dev11, newRoundNumber: 4);
            ok11_r4 = ctrl.OKButtonClick(dev11, move11_r4, newRoundNumber: 4);
            _out.WriteLine($"  Pair 11 retry → {(ok11_r4 ? "SUCCESS" : "STILL BLOCKED")}");
        }

        _out.WriteLine("");
        _out.WriteLine($"  RESULT: Pair 12 at R4 → {(ok12_r4 ? "CAN SCORE ✓" : "PERMANENT DEADLOCK ✗")}");
        return ok12_r4;
    }

    [Fact]
    public void Original_Code_Deadlocks()
    {
        _out.WriteLine("╔══════════════════════════════════════════════════╗");
        _out.WriteLine("║  ORIGINAL CODE (no guard in UpdateTableStatus)  ║");
        _out.WriteLine("╚══════════════════════════════════════════════════╝");
        _out.WriteLine("");

        bool canScore = RunScenario(useFix: false);

        Assert.False(canScore,
            "Expected permanent deadlock: pair 12 should be stuck at 'Waiting for Table 3' forever");
    }

    [Fact]
    public void Fixed_Code_No_Deadlock()
    {
        _out.WriteLine("╔══════════════════════════════════════════════════╗");
        _out.WriteLine("║  FIXED CODE (guard: if RoundNumber >= round)    ║");
        _out.WriteLine("╚══════════════════════════════════════════════════╝");
        _out.WriteLine("");

        bool canScore = RunScenario(useFix: true);

        Assert.True(canScore,
            "Expected no deadlock: pair 12 should be able to score at Table 3");
    }
}

// ── Mitchell movement: same double-call race, but self-resolving ────

public class MitchellMovementTest
{
    private readonly ITestOutputHelper _out;
    public MitchellMovementTest(ITestOutputHelper output) => _out = output;

    /// <summary>
    /// Mitchell 4-table movement:
    ///   Round 1: T1(NS=1,EW=5) T2(NS=2,EW=6)
    ///   Round 2: T1(NS=1,EW=6) T2(NS=2,EW=7)
    ///   Round 3: T1(NS=1,EW=7) ...
    ///
    /// NS pair 1 stays at Table 1 every round — never leaves.
    /// The same UpdateTableStatus double-call race can happen here:
    /// NS pair 1 finishes R2, sets a flag for R3, then late EW device wipes it.
    /// But NS pair 1 is STILL at the table, so the retry re-sets the flag.
    /// </summary>
    private SimulatedAppData BuildMitchellApp()
    {
        var app = new SimulatedAppData();
        app.LoadMovement([new(1, 1, 5, 1, 2), new(2, 2, 6, 3, 4)], round: 1);
        app.LoadMovement([new(1, 1, 6, 3, 4), new(2, 2, 7, 5, 6)], round: 2);
        app.LoadMovement([new(1, 1, 7, 5, 6), new(2, 2, 5, 7, 8)], round: 3);
        return app;
    }

    [Fact]
    public void Original_Code_Causes_Transient_Block_But_Self_Resolves()
    {
        _out.WriteLine("Mitchell — original code (no guard)");
        _out.WriteLine("");

        var app = BuildMitchellApp();
        var ctrl = new SimulatedController(app, useFix: false);

        // Bootstrap Table 1 at Round 1: NS=1 (North), EW=5 (East)
        var ts1 = app.GetTableStatus(1);
        ts1.RoundNumber = 1;
        ts1.RoundData = app.GetRound(1, 1);

        int devNS1 = app.AddDevice(1, pairNumber: 1, roundNumber: 1, Direction.North);
        int devEW5 = app.AddDevice(1, pairNumber: 5, roundNumber: 1, Direction.East);

        // EW pair 6 starts at Table 2
        var ts2 = app.GetTableStatus(2);
        ts2.RoundNumber = 1;
        ts2.RoundData = app.GetRound(2, 1);
        int devEW6 = app.AddDevice(2, pairNumber: 6, roundNumber: 1, Direction.East);

        // ── R1 → R2: normal transition ──────────────────────────────
        ctrl.ShowMoveIndex(devNS1, 2);
        ctrl.ShowMoveIndex(devEW5, 2);
        ctrl.ShowMoveIndex(devEW6, 2);  // pair 6 sets flag on T2

        // NS pair 1 advances Table 1 to Round 2
        bool ok = ctrl.OKButtonClick(devNS1, new Move(1, Direction.North), 2);
        Assert.True(ok, "NS pair 1 should advance T1 to R2");
        _out.WriteLine($"  NS pair 1 advances T1 to R2 → OK");

        // ── NS pair 1 finishes R2, sets flag for R3 ────────────────
        _out.WriteLine("");
        _out.WriteLine("  *** NS pair 1 finishes R2, views ShowMove for R3 ***");
        ctrl.ShowMoveIndex(devNS1, 3);
        _out.WriteLine($"  T1 flags: N={ts1.ReadyForNextRoundNorth} E={ts1.ReadyForNextRoundEast}");
        Assert.True(ts1.ReadyForNextRoundNorth, "North flag should be set for R3 transition");

        // ── Late EW pair 6 arrives — wipes the flag ─────────────────
        _out.WriteLine("");
        _out.WriteLine("  *** EW pair 6 (late) arrives at T1 — UpdateTableStatus wipes flags ***");
        ok = ctrl.OKButtonClick(devEW6, new Move(1, Direction.East), 2);
        Assert.True(ok, "EW pair 6 should arrive at T1");
        _out.WriteLine($"  T1 flags after wipe: N={ts1.ReadyForNextRoundNorth} E={ts1.ReadyForNextRoundEast}");
        Assert.False(ts1.ReadyForNextRoundNorth, "North flag should have been wiped");

        // ── NS pair 1 clicks OK for R3 — BLOCKED on first attempt ───
        _out.WriteLine("");
        _out.WriteLine("  *** EW pair 6 finishes R2, both view ShowMove for R3 ***");
        ctrl.ShowMoveIndex(devEW6, 3);
        _out.WriteLine($"  T1 flags: N={ts1.ReadyForNextRoundNorth} E={ts1.ReadyForNextRoundEast}");

        // NS pair 1 tries to advance — flag was wiped, only East is set
        ok = ctrl.OKButtonClick(devNS1, new Move(1, Direction.North), 3);
        _out.WriteLine($"  NS pair 1 first attempt → {(ok ? "OK" : "BLOCKED")}");
        Assert.False(ok, "NS pair 1 should be blocked — North flag was wiped");

        // ── Retry: ShowMove.Index re-sets the flag, because NS pair 1 is STILL HERE ──
        _out.WriteLine("");
        _out.WriteLine("  *** NS pair 1 retries — ShowMove.Index re-sets North flag ***");
        ctrl.ShowMoveIndex(devNS1, 3);
        _out.WriteLine($"  T1 flags after retry: N={ts1.ReadyForNextRoundNorth} E={ts1.ReadyForNextRoundEast}");

        ok = ctrl.OKButtonClick(devNS1, new Move(1, Direction.North), 3);
        _out.WriteLine($"  NS pair 1 retry → {(ok ? "OK ✓" : "BLOCKED ✗")}");
        Assert.True(ok, "NS pair 1 should succeed on retry — they never left the table");

        _out.WriteLine("");
        _out.WriteLine("  RESULT: Transient block, self-resolves on retry ✓");
    }

    [Fact]
    public void Fixed_Code_Avoids_Transient_Block_Entirely()
    {
        _out.WriteLine("Mitchell — fixed code (with guard)");
        _out.WriteLine("");

        var app = BuildMitchellApp();
        var ctrl = new SimulatedController(app, useFix: true);

        var ts1 = app.GetTableStatus(1);
        ts1.RoundNumber = 1;
        ts1.RoundData = app.GetRound(1, 1);

        int devNS1 = app.AddDevice(1, pairNumber: 1, roundNumber: 1, Direction.North);
        int devEW5 = app.AddDevice(1, pairNumber: 5, roundNumber: 1, Direction.East);

        var ts2 = app.GetTableStatus(2);
        ts2.RoundNumber = 1;
        ts2.RoundData = app.GetRound(2, 1);
        int devEW6 = app.AddDevice(2, pairNumber: 6, roundNumber: 1, Direction.East);

        // ── R1 → R2: normal transition ──────────────────────────────
        ctrl.ShowMoveIndex(devNS1, 2);
        ctrl.ShowMoveIndex(devEW5, 2);
        ctrl.ShowMoveIndex(devEW6, 2);

        bool ok = ctrl.OKButtonClick(devNS1, new Move(1, Direction.North), 2);
        Assert.True(ok);
        _out.WriteLine($"  NS pair 1 advances T1 to R2 → OK");

        // ── NS pair 1 finishes R2, sets flag for R3 ────────────────
        ctrl.ShowMoveIndex(devNS1, 3);
        _out.WriteLine($"  T1 flags after NS sets R3 flag: N={ts1.ReadyForNextRoundNorth} E={ts1.ReadyForNextRoundEast}");
        Assert.True(ts1.ReadyForNextRoundNorth);

        // ── Late EW pair 6 arrives — guard prevents flag wipe ───────
        _out.WriteLine("");
        _out.WriteLine("  *** EW pair 6 (late) arrives — guard skips redundant UpdateTableStatus ***");
        ok = ctrl.OKButtonClick(devEW6, new Move(1, Direction.East), 2);
        Assert.True(ok);
        _out.WriteLine($"  T1 flags preserved: N={ts1.ReadyForNextRoundNorth} E={ts1.ReadyForNextRoundEast}");
        Assert.True(ts1.ReadyForNextRoundNorth, "North flag should be PRESERVED by the guard");

        // ── Both view ShowMove for R3, NS pair 1 succeeds first try ─
        _out.WriteLine("");
        ctrl.ShowMoveIndex(devEW6, 3);
        _out.WriteLine($"  T1 flags: N={ts1.ReadyForNextRoundNorth} E={ts1.ReadyForNextRoundEast}");

        ok = ctrl.OKButtonClick(devNS1, new Move(1, Direction.North), 3);
        _out.WriteLine($"  NS pair 1 first attempt → {(ok ? "OK ✓" : "BLOCKED ✗")}");
        Assert.True(ok, "NS pair 1 should succeed on FIRST attempt — no transient block");

        _out.WriteLine("");
        _out.WriteLine("  RESULT: No block at all ✓");
    }
}

// ── Non-moving Howell: single device stays at table ─────────────────

public class NonMovingHowellTest
{
    private readonly ITestOutputHelper _out;
    public NonMovingHowellTest(ITestOutputHelper output) => _out = output;

    /// <summary>
    /// Howell with DevicesPerTable == 1: the device stays at the table
    /// and pairs rotate around it. Only ONE UpdateTableStatus call per
    /// round transition — no double-call, so the fix must be transparent.
    ///
    /// Uses Table 3 from the same 7-table Howell movement:
    ///   Round 2: T3(NS=10,EW=11)
    ///   Round 3: T3(NS=11,EW=12)
    ///   Round 4: T3(NS=12,EW=13)
    /// </summary>
    [Theory]
    [InlineData(false, "original")]
    [InlineData(true, "fixed")]
    public void NonMoving_Device_Advances_Normally(bool useFix, string label)
    {
        _out.WriteLine($"Non-moving Howell (DevicesPerTable=1) — {label} code");
        _out.WriteLine("");

        var app = new SimulatedAppData();

        app.LoadMovement([new(3, 10, 11, 7, 8)], round: 2);
        app.LoadMovement([new(3, 11, 12, 9, 10)], round: 3);
        app.LoadMovement([new(3, 12, 13, 11, 12)], round: 4);

        var ctrl = new SimulatedController(app, useFix);

        // Single device at Table 3, registered as North, DevicesPerTable = 1
        var ts3 = app.GetTableStatus(3);
        ts3.RoundNumber = 2;
        ts3.RoundData = app.GetRound(3, 2);
        int dev = app.AddDevice(3, pairNumber: 0, roundNumber: 2, Direction.North);

        // ── Advance R2 → R3 (single device, single call) ───────────
        ctrl.OKButtonClick_NonMoving(dev, 3);
        _out.WriteLine($"  After R2→R3: T3 round={ts3.RoundNumber} NS={ts3.RoundData.NumberNorth} EW={ts3.RoundData.NumberEast}");

        Assert.Equal(3, ts3.RoundNumber);
        Assert.Equal(11, ts3.RoundData.NumberNorth);
        Assert.Equal(12, ts3.RoundData.NumberEast);

        // ── Advance R3 → R4 (single device, single call) ───────────
        ctrl.OKButtonClick_NonMoving(dev, 4);
        _out.WriteLine($"  After R3→R4: T3 round={ts3.RoundNumber} NS={ts3.RoundData.NumberNorth} EW={ts3.RoundData.NumberEast}");

        Assert.Equal(4, ts3.RoundNumber);
        Assert.Equal(12, ts3.RoundData.NumberNorth);
        Assert.Equal(13, ts3.RoundData.NumberEast);

        // ── Verify flags are clean for next transition ──────────────
        Assert.False(ts3.ReadyForNextRoundNorth);
        Assert.False(ts3.ReadyForNextRoundEast);

        _out.WriteLine("");
        _out.WriteLine($"  RESULT: Non-moving device advances T3 through 3 rounds correctly ✓");
    }
}
