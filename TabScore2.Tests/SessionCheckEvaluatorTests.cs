// Tests for SessionCheckEvaluator.Evaluate, the pure comparison of the round 1 layout with registered devices.
// Requires Windows to run: the test project references TabScore2 (net8.0-windows, x64).

using TabScore2.Classes;
using TabScore2.Globals;
using Xunit;

namespace TabScore2.Tests
{
    public class SessionCheckEvaluatorTests
    {
        private static readonly DateTime Now = new(2026, 9, 27, 19, 30, 0);

        // Per-table (Mitchell) layout helper: pair numbers of 0 mean that side is absent
        private static LayoutTable PerTable(int table, int ns, int ew, string nsNames = "A & B", string ewNames = "C & D")
        {
            string[] n = nsNames.Split(" & ");
            string[] e = ewNames.Split(" & ");
            return new LayoutTable(1, "A", table, DevicesMove: false, IsIndividual: false,
                ns, ew, ns, ew,
                ns == 0 ? "" : n[0], ew == 0 ? "" : e[0], ns == 0 ? "" : n[1], ew == 0 ? "" : e[1]);
        }

        private static DeviceSnapshot Device(int number, int table, Direction direction = Direction.North) =>
            new(number, 1, "A", table, direction, 0, 1, 1, false, $"A{table}");

        private static SessionCheck Check(IEnumerable<LayoutTable> tables, params DeviceSnapshot[] devices) =>
            SessionCheckEvaluator.Evaluate(new SessionLayout(Now, tables.ToList(), null), devices, Now);

        [Fact]
        public void AllTablesRegistered_NamesComplete_IsOk()
        {
            SessionCheck check = Check([PerTable(1, 1, 1), PerTable(2, 2, 2)], Device(0, 1), Device(1, 2));
            Assert.Equal(0, check.TablesNeedingAttention);
            Assert.Empty(check.UnexpectedDevices);
        }

        [Fact]
        public void TableWithNoDevice_IsFlagged()
        {
            SessionCheck check = Check([PerTable(1, 1, 1), PerTable(2, 2, 2)], Device(0, 1));
            TableCheckRow row = check.Rows.Single(x => x.Table.TableNumber == 2);
            Assert.False(row.AllDevicesRegistered);
            Assert.Equal(new[] { Direction.North }, row.MissingDevices);
            Assert.Equal(1, check.TablesNeedingAttention);
        }

        [Fact]
        public void PerTableSitout_EastWestMustStillRegister()
        {
            // Half table, N/S absent: the E/W pair has no scoring device but must register at the table
            SessionCheck check = Check([PerTable(1, 1, 1), PerTable(7, 0, 7)], Device(0, 1));
            TableCheckRow sitout = check.Rows.Single(x => x.Table.TableNumber == 7);
            Assert.True(sitout.Table.IsSitoutTable);
            Assert.Equal(new[] { Direction.North }, sitout.Table.ExpectedDevices);  // Per-table registration key is always North
            Assert.False(sitout.AllDevicesRegistered);
            Assert.Equal(NamesState.NotApplicable, sitout.NorthSouthNames);
        }

        [Fact]
        public void PerTableSitout_RegisteredButNoIds_IsStillFlagged()
        {
            LayoutTable sitoutTable = PerTable(7, 0, 7) with { NameEast = "", NameWest = "" };
            SessionCheck check = Check([sitoutTable], Device(0, 7));
            TableCheckRow row = check.Rows.Single();
            Assert.True(row.AllDevicesRegistered);
            Assert.Equal(NamesState.Missing, row.EastWestNames);
            Assert.False(row.IsOk);
        }

        [Fact]
        public void PerTableSitout_RegisteredWithIds_IsOk()
        {
            SessionCheck check = Check([PerTable(7, 0, 7)], Device(0, 7));
            Assert.True(check.Rows.Single().IsOk);
        }

        [Fact]
        public void OneNameMissing_IsFlagged()
        {
            LayoutTable table = PerTable(3, 3, 3) with { NameSouth = "" };
            SessionCheck check = Check([table], Device(0, 3));
            Assert.Equal(NamesState.Missing, check.Rows.Single().NorthSouthNames);
        }

        [Fact]
        public void IdWithoutName_CountsAsEntered()
        {
            // GetNamesForRound returns '#number' when an ID was entered but has no name
            LayoutTable table = PerTable(3, 3, 3) with { NameNorth = "#1234" };
            SessionCheck check = Check([table], Device(0, 3));
            Assert.Equal(NamesState.Complete, check.Rows.Single().NorthSouthNames);
        }

        [Fact]
        public void MovingPairs_SitoutPairMustRegister_AbsentSideIsNotExpected()
        {
            LayoutTable table = PerTable(5, 0, 9) with { DevicesMove = true };
            Assert.Equal(new[] { Direction.East }, table.ExpectedDevices);

            SessionCheck check = Check([table]);
            Assert.Equal(new[] { Direction.East }, check.Rows.Single().MissingDevices);
        }

        [Fact]
        public void MovingPairs_RegisteringAsAbsentSide_IsReportedAsUnexpected()
        {
            LayoutTable table = PerTable(5, 0, 9) with { DevicesMove = true };
            SessionCheck check = Check([table], Device(0, 5, Direction.North));
            Assert.Single(check.UnexpectedDevices);
            Assert.Equal(new[] { Direction.East }, check.Rows.Single().MissingDevices);
        }

        [Fact]
        public void DeviceAtTableZero_IsNotUnexpected()
        {
            SessionCheck check = Check([PerTable(1, 1, 1)], Device(0, 1), Device(1, 0, Direction.Sitout));
            Assert.Empty(check.UnexpectedDevices);
        }

        [Fact]
        public void LayoutError_IsPassedThrough_WithNoRows()
        {
            SessionCheck check = SessionCheckEvaluator.Evaluate(new SessionLayout(Now, [], "MonitorDatabaseError"), [Device(0, 1)], Now);
            Assert.Equal("MonitorDatabaseError", check.ErrorMessage);
            Assert.Empty(check.Rows);
            Assert.Empty(check.UnexpectedDevices);
        }
    }
}
