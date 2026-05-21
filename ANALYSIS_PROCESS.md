# TabScore2 Analysis Process & Documentation System

## Purpose
Prevent duplication of analysis work and provide context continuity across LLM sessions.

---

## Quick Start for LLM Sessions

### When Starting a New Session
1. **Reference the documentation:**
   ```
   "I'm working on TabScore2. See PROJECT_UNDERSTANDING.md for full context."
   ```

2. **Specify your current focus:**
   ```
   "I need to review the EnterContractController logic. 
    See PROJECT_UNDERSTANDING.md - Controllers section for overview."
   ```

3. **Check what's already been analyzed:**
   - Look at "Completed Analysis Checklist" in `PROJECT_UNDERSTANDING.md`
   - Avoid re-analyzing completed sections

---

## Documentation Files

### 1. **PROJECT_UNDERSTANDING.md** (Main Reference)
- **What:** Comprehensive project overview and architecture
- **Contains:**
  - Quick reference table
  - Architecture overview (3-process model)
  - All 16 controllers listed
  - Service registration details
  - Configuration settings
  - Data models
  - Localization system
  - Frontend stack
  - Completed analysis checklist
- **Update:** Add findings as analysis progresses
- **Use:** Point LLM at this file for context

### 2. **ANALYSIS_PROCESS.md** (This File)
- **What:** Instructions for using the documentation system
- **Contains:**
  - Quick start guide
  - Documentation file descriptions
  - Update procedures
  - Session workflow

---

## How to Update Documentation

### When Analysis is Complete
1. **Update the Checklist:**
   ```markdown
   - [x] Detailed controller logic (next phase)
   ```

2. **Add Findings to Relevant Section:**
   - If analyzing controllers → Add to "Controllers" section
   - If analyzing views → Add to "Frontend Stack" section
   - If analyzing data flow → Add new "Data Flow" section

3. **Example Addition:**
   ```markdown
   ### EnterContractController
   - **Purpose:** Handles contract entry for bridge scoring
   - **Key Methods:** 
     - GET Index() - Display contract entry form
     - POST Index() - Process contract submission
   - **Dependencies:** IAppData, IUtilities
   - **Session Usage:** Stores contract in session
   ```

### Session Workflow

```
START SESSION
    ↓
[User provides task]
    ↓
[LLM reads PROJECT_UNDERSTANDING.md]
    ↓
[Check "Completed Analysis Checklist"]
    ↓
[Perform analysis on uncompleted items]
    ↓
[Update checklist and add findings]
    ↓
[Update "Next Steps" section]
    ↓
END SESSION
```

---

## Analysis Phases

### Phase 1: Architecture & Configuration ✓ COMPLETE
- [x] Project structure (`.csproj`)
- [x] Entry points (`Program.cs`)
- [x] Configuration (`App.config`)
- [x] Service registration
- [x] Multi-process architecture

### Phase 2: Controllers & Views (NEXT)
- [ ] Analyze each controller's logic
- [ ] Map controller → view relationships
- [ ] Document request/response flows
- [ ] Identify shared patterns

### Phase 3: Data Flow & Services (DEFERRED)
- [ ] gRPC integration details
- [ ] Database service operations
- [ ] Data transformation pipelines
- [ ] Error handling patterns

### Phase 4: Business Logic (DEFERRED)
- [ ] Domain model logic
- [ ] Scoring algorithms
- [ ] Validation rules
- [ ] State management

---

## Key Information for Future Sessions

### Architecture Summary
- **3 Processes:** SplashScreen, GrpcBwsDatabaseServer, TabScore2 (main)
- **Hybrid App:** Windows Forms UI + ASP.NET Core MVC backend
- **Communication:** gRPC (port 5119) for database operations
- **Web Server:** Kestrel on port 5213

### Critical Files
- `TabScore2/Program.cs` - Application startup (158 lines)
- `GrpcBwsDatabaseServer/GrpcServices/BwsDatabaseService.cs` - Main database service (84KB)
- `TabScore2/Forms/MainForm.cs` - Main desktop form
- `TabScore2/Controllers/` - 16 MVC controllers

### Important Ports
- **5119** - gRPC database server
- **5213** - Kestrel web server

### Languages Supported
- English, German, Spanish, Dutch (via `.resx` files)

---

## Tips for Efficient Analysis

1. **Use the Checklist** - Don't re-analyze completed items
2. **Reference Specific Files** - Use line numbers: `Program.cs:18`
3. **Group Related Items** - Analyze all controllers together, not individually
4. **Document Patterns** - Note recurring patterns to avoid repetition
5. **Link to Code** - Always reference actual file locations
6. **Update Incrementally** - Don't wait for perfect analysis, update as you go

---

## Example Session Log

```
SESSION: 2026-02-26 Review EnterContractController
STATUS: Starting Phase 2 - Controllers & Views

TASK: Review EnterContractController for code quality

CONTEXT: 
- See PROJECT_UNDERSTANDING.md
- EnterContractController listed in Controllers section
- Part of Phase 2 analysis (Controllers & Views)

ANALYSIS:
- Reviewed EnterContractController.cs (4896 chars)
- Found X issues, Y patterns
- Updated PROJECT_UNDERSTANDING.md with findings

NEXT: Continue with EnterHandRecordController
```

---

## Maintenance

- **Review Frequency:** Update after each analysis session
- **Archive Old Sessions:** Keep in git history
- **Consolidate Findings:** Merge similar findings into patterns
- **Validate Accuracy:** Cross-reference with actual code

