# TabScore2 Project Understanding

**Status:** ANALYSIS IN PROGRESS  
**Last Updated:** 2026-02-26  
**Project Type:** Hybrid Windows Forms + ASP.NET Core MVC Application  
**Target Framework:** .NET 8.0 Windows (x64)

---

## Quick Reference

| Aspect | Details |
|--------|---------|
| **Output Type** | Windows Forms Application (WinExe) |
| **Architecture** | Hybrid: Desktop UI + Web Backend + gRPC Services |
| **Main Port** | 5213 (Kestrel web server) |
| **gRPC Port** | 5119 (Database server) |
| **Language Support** | English, German, Spanish, Dutch |
| **Key Dependencies** | gRPC, protobuf-net, Bootstrap, WebOptimizer |

---

## Architecture Overview

### Multi-Process Architecture
TabScore2 runs **three separate processes**:

1. **SplashScreen.exe** - Optional startup splash screen (configurable)
2. **GrpcBwsDatabaseServer.exe** - gRPC server for database operations (port 5119)
3. **TabScore2.exe** - Main application (hybrid desktop + web)

### Application Layers

#### Desktop Layer (Windows Forms)
- **Entry Point:** [`TabScore2/Program.cs`](TabScore2/Program.cs:18) - `Main()` method
- **Main Form:** [`TabScore2/Forms/MainForm.cs`](TabScore2/Forms/MainForm.cs)
- **Additional Forms:**
  - `SettingsForm.cs` - Application configuration
  - `EditResultForm.cs` - Result editing with multi-language support
  - `ViewResultsForm.cs` - Results viewing interface

#### Web Layer (ASP.NET Core MVC)
- **Controllers:** 19 controllers handling different screens
  - `StartScreenController` - Application entry point
  - `SelectSectionController` - Section selection
  - `SelectTableNumberController` - Table number selection
  - `SelectDirectionController` - Direction selection (Individual/Pair)
  - `EnterPlayerIDController` - Player ID entry
  - `EnterContractController` - Contract entry
  - `EnterHandRecordController` - Hand record entry
  - `EnterLeadController` - Lead card entry
  - `EnterTricksTakenController` - Tricks entry (Total/Plus-Minus modes)
  - `ShowBoardsController` - Board display (Scoring/ViewOnly)
  - `ShowHandRecordController` - Hand record display
  - `ShowMoveController` - Move display
  - `ShowPlayerIDsController` - Player IDs display
  - `ShowRankingListController` - Rankings display
  - `ShowRoundInfoController` - Round information
  - `ShowTravellerController` - Traveller display
  - `ConfirmResultController` - Result confirmation
  - `ErrorScreenController` - Error handling
  - `EndScreenController` - Session end screen

- **Views:** Razor templates in `TabScore2/Views/` with Bootstrap styling
- **Models:** Data transfer objects in `TabScore2/Models/`

#### gRPC Service Layer
- **Server:** `GrpcBwsDatabaseServer/GrpcServices/BwsDatabaseService.cs` (84KB - large service)
- **Contracts:** `GrpcSharedContracts/` - Shared interfaces and message classes
- **Services:**
  - `IBwsDatabaseService` - Main database operations
  - `IExternalNamesDatabaseService` - External names database

---

## Dependency Injection & Services

### Registered Services (from [`Program.cs`](TabScore2/Program.cs:84-99))

**Web Application Services:**
```
- IUtilities → Utilities (Singleton)
- IDatabase → BwsDatabase (Singleton)
- IExternalNamesDatabase → ExternalNamesDatabase (Singleton)
- ISettings → Settings (Singleton)
- IAppData → AppData (Singleton)
- IBwsDatabaseService → gRPC client (port 5119)
- IExternalNamesDatabaseService → gRPC client (port 5119)
```

**Desktop Application Services:**
```
- MainForm (Singleton)
- IDatabase → BwsDatabase (Singleton)
- ISettings → Settings (Singleton)
- IAppData → AppData (Singleton)
- IBwsDatabaseService → gRPC client (port 5119)
- IExternalNamesDatabaseService → gRPC client (port 5119)
- SettingsForm (Transient with Point parameter)
- ViewResultsForm (Transient with Point parameter)
- EditResultForm (Transient with Result and Point parameters)
```

---

## Configuration & Settings

### App.config Settings (from [`App.config`](TabScore2/App.config))

| Setting | Default | Purpose |
|---------|---------|---------|
| `TabletsMove` | False | Tablet movement mode |
| `ShowTimer` | True | Display round timer |
| `SecondsPerBoard` | 390 | Time per board (6.5 min) |
| `AdditionalSecondsPerRound` | 60 | Extra time per round |
| `ShowHandRecordFromDirection` | South | Default hand record view |
| `DoubleDummy` | True | Enable double dummy analysis |
| `SuppressRankingListForLastXRounds` | 0 | Hide rankings for last N rounds |
| `SuppressRankingListForFirstXRounds` | 2 | Hide rankings for first 2 rounds |
| `DatabaseReady` | False | Database initialization flag |
| `IsIndividual` | False | Individual vs Pair scoring |
| `SessionStarted` | False | Session state flag |
| `ShowSplashScreen` | True | Display splash on startup |
| `DefaultShowTraveller` | True | Show traveller by default |
| `DefaultShowPercentage` | True | Show percentages in rankings |
| `DefaultEnterLeadCard` | True | Require lead card entry |
| `DefaultValidateLeadCard` | True | Validate lead card |
| `DefaultShowRanking` | 1 | Ranking display mode |
| `DefaultEnterResultsMethod` | 1 | Result entry method |
| `DefaultShowHandRecord` | True | Show hand records |
| `DefaultNumberEntryEachRound` | False | Number entry per round |
| `DefaultNameSource` | 0 | Name source selection |
| `DefaultManualHandRecordEntry` | False | Manual hand record entry |

---

## Data Models

### Core Domain Classes (in `TabScore2/Classes/`)

| Class | Purpose |
|-------|---------|
| `DeviceStatus.cs` | Device/tablet status |
| `HandEvaluation.cs` | Hand evaluation data |
| `InternalPlayerRecord.cs` | Internal player tracking |
| `Move.cs` | Player movement tracking |
| `PlayerEntry.cs` | Player entry data |
| `ResultsPerBoard.cs` | Per-board results |
| `RoundTimer.cs` | Round timing |
| `ShowBoardsResult.cs` | Board display result |
| `TableStatus.cs` | Table status tracking |
| `TravellerResult.cs` | Traveller result data |

### Shared Domain Classes (in `GrpcSharedContracts/SharedClasses/`)

| Class | Purpose |
|-------|---------|
| `DatabaseSettings.cs` | Database configuration |
| `Hand.cs` | Bridge hand representation |
| `Names.cs` | Player names data |
| `Ranking.cs` | Player/pair rankings |
| `Result.cs` | Game result data |
| `Round.cs` | Round information |
| `Section.cs` | Section/table grouping |

---

## Localization System

### Multi-Language Support
- **Languages:** English (en), German (de), Spanish (es), Dutch (nl)
- **Resource Files:** `TabScore2/Resources/Strings.*.resx`
- **Form Resources:** Individual `.resx` files per form (de, en, es, nl variants)

### Resource Organization
```
TabScore2/Resources/
├── Strings.resx (default)
├── Strings.de.resx (German)
├── Strings.en.resx (English)
├── Strings.es.resx (Spanish)
├── Strings.nl.resx (Dutch)
└── TabScore2Icon.ico

TabScore2/Forms/
├── MainForm.resx
├── MainForm.de.resx
├── MainForm.en.resx
├── MainForm.es.resx
├── MainForm.nl.resx
├── SettingsForm.resx (+ language variants)
├── EditResultForm.resx (+ language variants)
└── ViewResultsForm.resx (+ language variants)
```

---

## Frontend Stack

### CSS & JavaScript
- **Bootstrap 5** - Responsive UI framework (`wwwroot/lib/bootstrap/`)
- **Custom CSS** - `wwwroot/css/icomoon.css` (icon fonts)
- **JavaScript Files:**
  - `EnterContract.js` - Contract entry logic
  - `EnterHandRecord.js` - Hand record entry (24KB - complex)
  - `EnterLead.js` - Lead card entry
  - `EnterPlayerID.js` - Player ID entry
  - `ShowHandRecord.js` - Hand record display
  - `TotalTricks.js` - Total tricks entry
  - `TricksPlusMinus.js` - Plus/minus tricks entry
  - `IndividualRankingList.js` - Individual rankings
  - `OneWinnerRankingList.js` - Single winner rankings
  - `TwoWinnersRankingList.js` - Pair rankings
  - `MainLayout.js` - Layout management

### Static Assets
- `wwwroot/Cards Logo.png` - Card suit logo
- `wwwroot/Chequered Flag Logo.png` - Finish flag logo
- `wwwroot/favicon.ico` - Browser icon

---

## Build & Deployment

### Project Configuration
- **Platform:** x64 only (no x86 support)
- **Unsafe Code:** Enabled (`AllowUnsafeBlocks`)
- **Nullable:** Enabled (strict null checking)
- **Implicit Usings:** Enabled
- **Debug Info:** Full (both Debug and Release)

### External Dependencies
- **NuGet Packages:**
  - `Grpc.Net.Client` v2.63.0
  - `protobuf-net.Grpc` v1.1.1
  - `protobuf-net.Grpc.ClientFactory` v1.1.1
  - `LigerShark.WebOptimizer.Core` v3.0.413
  - `Microsoft.AspNetCore.Session` v2.3.0

- **Native Dependencies:**
  - `dds64.dll` - Double dummy solver (copied to output)

### Project References
- `GrpcSharedContracts` - Shared gRPC contracts

---

## Session Management

### Session Configuration
- **Cookie Name:** `.TabScore2.Session`
- **Idle Timeout:** 6 hours
- **Essential Cookie:** Yes (required for functionality)

---

## Error Handling

- **Global Exception Handler:** Routes to `/ErrorScreen/Index`
- **Error Screen Controller:** [`ErrorScreenController.cs`](TabScore2/Controllers/ErrorScreenController.cs)

---

## Completed Analysis Checklist

- [x] Project configuration and dependencies (`.csproj`)
- [x] Program.cs entry point and DI setup
- [x] App.config settings and defaults
- [x] Multi-process architecture overview
- [x] Service registration and dependencies
- [x] Data models and domain classes
- [x] Localization system
- [x] Frontend stack (Bootstrap, JavaScript)
- [x] Build configuration
- [ ] Detailed controller logic (next phase)
- [ ] Data flow and gRPC integration (next phase)
- [ ] Business logic in DataServices (deferred per user request)
- [ ] UtilityServices implementation (deferred per user request)

---

## Next Steps for Future Analysis

1. **Controller Deep Dive** - Examine each controller's request handling
2. **View Templates** - Analyze Razor templates and data binding
3. **gRPC Integration** - Study BwsDatabaseService communication
4. **Data Flow** - Trace data from UI through services to database
5. **Business Logic** - Review complex algorithms in domain classes

---

## How to Use This Document

1. **Point LLM at this file** when starting new analysis sessions
2. **Update the "Completed Analysis Checklist"** as work progresses
3. **Add findings to relevant sections** as they're discovered
4. **Record current focus** in the "Next Steps" section
5. **Prevent duplication** by checking what's already been analyzed

**Example:** "I'm working on TabScore2. See PROJECT_UNDERSTANDING.md for context. I need to analyze the EnterContractController logic."

