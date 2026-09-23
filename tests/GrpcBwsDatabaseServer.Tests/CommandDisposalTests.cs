// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace GrpcBwsDatabaseServer.Tests
{
    // ---------------------------------------------------------------------------------------------
    // BwsDatabaseService.Initialize and SetResult create one OdbcCommand per SQL statement and, in the
    // pre-fix code, reassigned the same local variable to a new OdbcCommand many times per call without
    // disposing the previous instance. OdbcCommand is a thin wrapper over an unmanaged ODBC statement
    // handle; each undisposed instance leaks that handle until the GC finalizer eventually runs.
    //
    // The real defect lived in GrpcBwsDatabaseServer.GrpcServices.BwsDatabaseService, which cannot run
    // here: it talks to a live Microsoft Access (.bws) file through the Windows-only ODBC driver. These
    // tests instead reproduce the exact shape of the defect - "create a disposable resource, execute it,
    // maybe swallow a benign exception, then create the next one" - against a lightweight test double,
    // so the disposal contract can be verified on any platform, in any CI pipeline.
    //
    // FakeCommand plays the role OdbcCommand plays in production: Execute() either succeeds, or throws
    // the "benign" exception standing in for the Access driver's "column already exists" error.
    // ---------------------------------------------------------------------------------------------

    public sealed class FakeCommand(string sql, bool throwsBenignError = false) : IDisposable
    {
        public string Sql { get; } = sql;
        public bool IsDisposed { get; private set; }

        public void Execute()
        {
            if (throwsBenignError) throw new InvalidOperationException("benign: already exists");
        }

        public void Dispose() => IsDisposed = true;
    }

    public class CommandDisposalTests
    {
        // ---------------------------------------------------------------------------------------
        // 1. REPRODUCE: the pre-fix pattern used throughout Initialize() - reassigning the same
        //    variable to a new command for each statement, disposing only the final one - leaks
        //    every command created before the last.
        // ---------------------------------------------------------------------------------------
        [Fact]
        public void LegacyReassignmentPattern_LeaksAllButTheLastCommand()
        {
            List<FakeCommand> created = [];

            // Mirrors GrpcBwsDatabaseServer.GrpcServices.BwsDatabaseService.Initialize() before the fix:
            // "cmd = new(...); cmd.Execute();" repeated, then a single cmd.Dispose() at the very end.
            FakeCommand cmd = new("ALTER TABLE Section ADD Winners SHORT");
            created.Add(cmd);
            cmd.Execute();

            cmd = new FakeCommand("ALTER TABLE Section ADD MissingPair SHORT");
            created.Add(cmd);
            cmd.Execute();

            cmd = new FakeCommand("ALTER TABLE PlayerNumbers ADD [Name] VARCHAR(30)");
            created.Add(cmd);
            cmd.Execute();

            cmd.Dispose();   // the only Dispose() call the legacy method made

            int leaked = created.Count(c => !c.IsDisposed);
            Assert.Equal(2, leaked);                 // the first two commands are never disposed
            Assert.True(created[0].IsDisposed is false && created[1].IsDisposed is false);
            Assert.True(created[^1].IsDisposed);      // only the last one was
        }

        // ---------------------------------------------------------------------------------------
        // 2. RECTIFY: the fixed helper (ExecuteDdlIgnoringBenignError, mirrored here as
        //    ExecuteIgnoringBenignError) disposes the command it creates on every path, including
        //    when the benign exception is thrown and swallowed.
        // ---------------------------------------------------------------------------------------
        [Fact]
        public void FixedHelper_DisposesCommand_OnSuccessPath()
        {
            FakeCommand? captured = null;
            ExecuteIgnoringBenignError(sql => captured = new FakeCommand(sql));

            Assert.NotNull(captured);
            Assert.True(captured!.IsDisposed);
        }

        [Fact]
        public void FixedHelper_DisposesCommand_WhenBenignErrorIsSwallowed()
        {
            FakeCommand? captured = null;
            ExecuteIgnoringBenignError(sql => captured = new FakeCommand(sql, throwsBenignError: true));

            Assert.NotNull(captured);
            Assert.True(captured!.IsDisposed);   // disposed even though Execute() threw
        }

        [Fact]
        public void FixedHelper_DisposesBothCommands_WhenUpdateOnSuccessRuns()
        {
            List<FakeCommand> created = [];
            FakeCommand ddl = new("ALTER TABLE PlayerNumbers ADD Processed YESNO");
            FakeCommand update = new("UPDATE PlayerNumbers SET Processed=False");
            created.Add(ddl);
            created.Add(update);

            using (ddl)
            {
                ddl.Execute();
                using (update)
                {
                    update.Execute();
                }
            }

            Assert.All(created, c => Assert.True(c.IsDisposed));
        }

        // Minimal stand-in for BwsDatabaseService.ExecuteDdlIgnoringBenignError: creates one command via
        // the supplied factory, disposes it via `using` regardless of outcome, and swallows the "benign"
        // exception the way the production helper swallows the matching OdbcException/SQLState.
        private static void ExecuteIgnoringBenignError(Func<string, FakeCommand> factory)
        {
            using FakeCommand cmd = factory("ALTER TABLE Section ADD Winners SHORT");
            try
            {
                cmd.Execute();
            }
            catch (InvalidOperationException)
            {
                // benign: column/table already exists - ignored, matching the SQLState check in production
            }
        }
    }
}
