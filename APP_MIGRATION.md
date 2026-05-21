# TabScore2 — Linux Migration Plan

**Status:** PLANNING  
**Last Updated:** 2026-04-16  
**Goal:** Port TabScore2 to run on Linux while preserving the `.bws` (Access) file format

---

## Constraints

- The `.bws` database file is in **Microsoft Access format** and **cannot be changed**
- Full **read and write** access to the `.bws` file is required
- The application must remain functionally equivalent to the Windows version

---

## Architecture Summary (Current)

TabScore2 runs as three separate processes:

1. **SplashScreen.exe** — Optional startup splash screen
2. **GrpcBwsDatabaseServer.exe** — gRPC server handling all database operations (port 5119)
3. **TabScore2.exe** — Main application (Windows Forms desktop UI + ASP.NET Core web layer)

The web layer (ASP.NET Core MVC, Kestrel on port 5213) is already cross-platform. All Access database access is isolated within `GrpcBwsDatabaseServer`.

---

## Key Blockers for Linux

| Blocker | Detail |
|---------|--------|
| Windows Forms UI | `MainForm`, `SettingsForm`, `EditResultForm`, `ViewResultsForm` use `System.Windows.Forms` |
| Access database write support | `mdbtools` (Linux) is read-only; no free cross-platform .NET library supports Access writes |
| Native Windows DLL | `dds64.dll` (Double Dummy Solver) is a Windows x64 binary |
| Target framework | Project targets `net8.0-windows` |
| `App.config` | Windows-centric configuration pattern |

---

## Database Strategy — The Critical Decision

Because write support is required, the standard Linux Access tooling (`mdbtools`) is not viable. The following options were evaluated:

### Option A: Run GrpcBwsDatabaseServer under Wine *(Selected for PoC)*

Since all database access is already isolated in `GrpcBwsDatabaseServer`, that process alone can be run under Wine on Linux. It retains full access to the Windows ODBC32/Access driver stack. The rest of the application runs natively on .NET/Linux.

- **Pros:** Minimal code changes; leverages existing process isolation; gRPC boundary already in place
- **Cons:** Wine dependency; Wine configuration overhead; not a fully native Linux deployment

### Option B: Commercial ODBC Driver (e.g. Easysoft)

A commercial Linux ODBC driver for Access (e.g. from Easysoft) used with `unixODBC` and `System.Data.Odbc`. Minimal code changes in `GrpcBwsDatabaseServer`.

- **Pros:** Fully native Linux; clean solution
- **Cons:** Commercial license cost; vendor dependency

### Option C: Round-Trip Format Conversion

Convert `.bws` → SQLite at startup; app works against SQLite; convert back to `.bws` on close.

- **Pros:** App fully native on Linux after conversion
- **Cons:** Conversion back to `.mdb` still requires Windows ODBC or Wine; operational complexity; must be lossless

### Option D: Keep Database Server on Windows

`GrpcBwsDatabaseServer` remains a Windows service; rest of the app runs on Linux.

- **Pros:** Zero database code changes
- **Cons:** Requires a Windows runtime; not a full Linux port

---

## Selected Approach: Option A (Wine) — PoC FAILED

**PoC Result: GATE 1 FAILURE**

The PoC was executed on 2026-04-21 using Docker + Ubuntu 22.04 + Wine 32-bit.

### Failure Details

When attempting to execute `GrpcBwsDatabaseServer.exe` (a self-contained .NET 8 x86 Windows binary) under Wine, the process exited immediately with:

```
wine: Unhandled illegal instruction at address 01051CF0 (thread 0024), starting debugger...
Can't attach process 0020: error 5
```

**Root Cause:** Wine's 32-bit CPU emulation layer cannot execute .NET 8 x86 binaries. This is a fundamental architectural incompatibility — not a configuration issue.

### Why This Happened

- The project is configured for `net8.0-windows` targeting `x86` (32-bit) platform
- .NET 8 x86 binaries require CPU instruction sets (SSE2, SSE4.1, etc.) that Wine's legacy 32-bit thunk layer does not properly emulate
- Wine 32-bit support is deprecated and poorly maintained; .NET 8 was designed for modern x64 platforms

### Decision: Pivot to Option B (Commercial ODBC Driver)

Option A (Wine) is **not viable**. 

**Next steps:**
1. Change the target framework from `net8.0-windows` to `net8.0` in `GrpcBwsDatabaseServer.csproj`
2. Evaluate commercial ODBC drivers for Access on Linux (e.g., Easysoft)
3. Develop a PoC for native .NET 8 Linux with commercial ODBC driver
4. This path is clean, fully supported, and avoids the Wine compatibility layer entirely

---

## Alternative Path Not Taken: Option A (Wine) — ABANDONED

Option A was selected as the candidate strategy because:

- It required the fewest code changes
- The gRPC process boundary already isolates the Windows-dependent code
- Wine can be scoped to a single process, limiting its footprint
- If the PoC failed, Option B (commercial ODBC) was the next fallback

**PoC Outcome: Gate 1 Failure — Wine 32-bit cannot run .NET 8 binaries.**

### PoC Objectives

The PoC must confirm two **prerequisite gates** before anything else is tested:

**Gate 1 — .NET 8 runtime runs under Wine**
The .NET 8 Windows runtime must be installable inside a Wine prefix (via `winetricks`) and capable of executing the server binary. Wine's compatibility with recent .NET versions is inconsistent and this is the highest-risk unknown.

**Gate 2 — Wine ODBC32 write support works against `.mdb`**
Wine's ODBC32 implementation must be able to open an Access `.mdb` file and complete write operations (INSERT, UPDATE, DELETE) with correct data persistence. This is largely untested territory.

If either gate fails, Option A is not viable and the PoC should be abandoned in favour of Option B (commercial ODBC driver).

If both gates pass, the full PoC objectives are:

1. `GrpcBwsDatabaseServer.exe` launches and runs stably under Wine on a target Linux distribution
2. The ODBC32 Access driver within Wine can open a `.bws` file
3. Both **read and write** operations complete correctly and data integrity is preserved
4. The gRPC endpoint is reachable from a native Linux .NET process
5. Performance is acceptable under realistic load

---

## Full Migration Plan (Post-PoC)

Execution is contingent on PoC success. Steps are ordered by dependency.

### Phase 1: PoC — Wine Database Server Validation (FAILED)
- [x] Build self-contained `GrpcBwsDatabaseServer.exe` (win-x86) via GitHub Action
- [x] Set up Docker environment with Wine 32-bit + ODBC configuration
- [x] Attempt to run exe under Wine
- [x] **Result: Gate 1 Failure** — Wine's 32-bit CPU emulation cannot execute .NET 8 x86 binaries
  - Error: `wine: Unhandled illegal instruction at address 01051CF0`
  - Root cause: .NET 8 x86 requires modern CPU instructions; Wine 32-bit thunk layer does not support them
  - Conclusion: Option A (Wine) is architecturally incompatible with this project's x86 target

### Phase 2 (NEW): Pivot to Native Linux + Commercial ODBC Driver
- [ ] Change target framework: `net8.0-windows` → `net8.0` in `GrpcBwsDatabaseServer.csproj`
- [ ] Evaluate commercial ODBC drivers:
  - [ ] Easysoft — Access ODBC driver for Linux
  - [ ] DataDirect — Commercial ODBC manager
  - [ ] Other alternatives
- [ ] Obtain trial/evaluation license for chosen driver
- [ ] Build and test `GrpcBwsDatabaseServer` as native Linux x64 binary
- [ ] Verify ODBC connection to `.bws` file with read/write operations

### Phase 3 (PREV Phase 2): Target Framework & Configuration
- [ ] Change target framework from `net8.0-windows` to `net8.0` in `TabScore2.csproj`
- [ ] Remove `<UseWindowsForms>true</UseWindowsForms>`
- [ ] Migrate `App.config` settings to `appsettings.json` using `IConfiguration`/`IOptions<T>`
- [ ] Audit and remove any remaining `Microsoft.Win32.*` or other Windows-only API usage

### Phase 4 (PREV Phase 3): Replace Windows Forms UI
- [ ] Identify all functionality exposed by the WinForms layer (MainForm, SettingsForm, EditResultForm, ViewResultsForm)
- [ ] Implement equivalent functionality as ASP.NET Core web admin pages within the existing web layer
- [ ] Remove all `System.Windows.Forms` references
- [ ] Drop the `SplashScreen` project

### Phase 5 (PREV Phase 4): Port the Native DDS Library
- [ ] Obtain source for the Double Dummy Solver (Bo Haglund's DDS — open source)
- [ ] Compile as a Linux shared library (`libdds.so`)
- [ ] Update P/Invoke declarations to reference `libdds.so` on Linux (use runtime OS detection or conditional compilation)
- [ ] Validate DDS results match the Windows `dds64.dll` output

### Phase 6 (PREV Phase 5): Cross-Platform Audit
- [ ] Replace Windows-style file path separators where hardcoded
- [ ] Review `System.Drawing` usage — replace with `System.Drawing.Common` or ImageSharp if needed
- [ ] Audit process-launch code (e.g. starting `SplashScreen.exe`, `GrpcBwsDatabaseServer.exe`)
- [ ] Verify all NuGet packages support `net8.0` (non-Windows TFM)

### Phase 7 (PREV Phase 6): Integration & Testing
- [ ] End-to-end test on Linux: full session from startup through result entry and ranking display
- [ ] Verify multi-language support (en, de, es, nl)
- [ ] Test timer, hand records, double dummy analysis
- [ ] Performance and stability testing

### Phase 8 (PREV Phase 7): Packaging & Deployment
- [ ] Define deployment model (bare metal, systemd services, Docker)
- [ ] Write docker-compose configuration for multi-process deployment (native gRPC server + native app)
- [ ] Document Linux setup prerequisites (ODBC driver configuration, .bws file placement)

---

## Risk Register

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| .NET 8 Windows runtime does not run correctly under Wine | **CONFIRMED FAILURE** | High | Wine is not viable; proceed with Option B (commercial ODBC driver) |
| Commercial ODBC driver cost and vendor lock-in | Medium | Medium | Evaluate multiple vendors; check trial availability |
| Commercial ODBC driver does not support required Access write operations | Low | High | Contact vendor; verify write support before purchasing |
| DDS library Linux build issues | Low | Medium | Source is open; community builds likely available |
| WinForms feature parity gaps in web admin replacement | Medium | Medium | Thorough feature mapping in Phase 4 planning |
| NuGet package Windows-only dependencies | Low | Low | Audit in Phase 6 catches these early |

---

## Decision Log

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-04-16 | Selected Option A (Wine) as PoC candidate | Lowest code-change path; gRPC isolation limits Wine footprint |
| 2026-04-16 | Option B (commercial ODBC) designated as fallback | Viable if Wine PoC fails; requires budget approval |
| 2026-04-16 | Execute PoC via Docker on macOS (not bare Linux) | Eliminates need for VM/Linux machine; Docker mirrors actual Linux deployment |
| 2026-04-21 | PoC executed: Wine 32-bit + .NET 8 x86 binary | Illegal instruction error; Wine cannot execute .NET 8 CPU instructions |
| 2026-04-21 | **Abandoned Option A; Pivoting to Option B** | Gate 1 failure is definitive; Wine is architecturally incompatible with project's x86 target |
