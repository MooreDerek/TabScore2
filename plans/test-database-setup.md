# Test Database Setup Guide

This document describes how to create a test BWS database for TabScore2 testing.

## Overview

TabScore2 uses Microsoft Access `.bws` (or `.mdb`) database files for storing bridge session data. The database is accessed via ODBC using the "Microsoft Access Driver (*.mdb)" driver.

## Official Documentation

**The authoritative source for the BWS database schema is the Bridgemate Developer's Guide:**

📄 **[Bridgemate Developer's Guide (PDF)](https://www.bridgemate.com/resources/developer/BMdevguide.pdf)**
Document revision 53, October 22nd, 2025

This guide contains:
- Complete database table definitions (pages 9-23)
- Field descriptions and data types
- Guidelines for filling tables with session data (page 29)
- Information on dealing with missing pairs (page 34)
- Rover movement handling (page 35)
- Maximum values and constraints (page 44)

The schema documented below is a **simplified subset** based on what TabScore2 requires. For complete field definitions, refer to the official guide.

## Database Schema

### Required Tables

#### 1. Section
Defines the sections in the event (typically A, B, C, D).

```sql
CREATE TABLE Section (
    ID INTEGER PRIMARY KEY,      -- Section ID (1-4)
    Letter VARCHAR(1),           -- Section letter (A, B, C, D)
    [Tables] INTEGER,            -- Number of tables in section
    Winners INTEGER,             -- 1 = one winner, 2 = two winners
    MissingPair INTEGER          -- Pair number for phantom/sitout (0 if none)
);
```

#### 2. RoundData
Movement data defining which pairs play at which tables each round.

```sql
CREATE TABLE RoundData (
    Section INTEGER,             -- Section ID
    [Table] INTEGER,             -- Table number
    Round INTEGER,               -- Round number
    NSPair INTEGER,              -- North-South pair number
    EWPair INTEGER,              -- East-West pair number
    LowBoard INTEGER,            -- First board number for this round
    HighBoard INTEGER,           -- Last board number for this round
    South INTEGER,               -- (Individual only) South player number
    West INTEGER                 -- (Individual only) West player number
);
```

#### 3. Tables
Tracks table registration status.

```sql
CREATE TABLE Tables (
    Section INTEGER,             -- Section ID
    [Table] INTEGER,             -- Table number
    LogOnOff INTEGER             -- 0 = not registered, 1 = registered
);
```

#### 4. ReceivedData
Stores entered results.

```sql
CREATE TABLE ReceivedData (
    Section INTEGER,
    [Table] INTEGER,
    Round INTEGER,
    Board INTEGER,
    PairNS INTEGER,
    PairEW INTEGER,
    Declarer INTEGER,
    [NS/EW] VARCHAR(2),          -- Declarer direction (N, S, E, W)
    Contract VARCHAR(10),        -- e.g., "4 H" or "PASS"
    Result VARCHAR(3),           -- e.g., "=", "+1", "-2"
    LeadCard VARCHAR(3),         -- e.g., "CK", "S10"
    Remarks VARCHAR(50),
    DateLog DATE,
    TimeLog DATETIME,
    Processed YESNO,
    Processed1 YESNO,
    Processed2 YESNO,
    Processed3 YESNO,
    Processed4 YESNO,
    Erased YESNO,
    South INTEGER,               -- (Individual only)
    West INTEGER                 -- (Individual only)
);
```

#### 5. PlayerNumbers
Maps player IDs to table positions.

```sql
CREATE TABLE PlayerNumbers (
    Section INTEGER,
    [Table] INTEGER,
    Direction VARCHAR(1),        -- N, S, E, W
    [Number] VARCHAR(18),        -- Player ID
    [Name] VARCHAR(30),          -- Player name
    Round INTEGER,
    Processed YESNO,
    Updated YESNO,
    TimeLog DATETIME,
    TabScorePairNo INTEGER       -- Pair/player number for TabScore
);
```

#### 6. PlayerNames
Master player name lookup table.

```sql
CREATE TABLE PlayerNames (
    ID INTEGER,                  -- Numeric player ID
    [Name] VARCHAR(40),          -- Player name
    strID VARCHAR(18)            -- String player ID
);
```

#### 7. HandRecord
Optional hand records for boards.

```sql
CREATE TABLE HandRecord (
    Section INTEGER,
    Board INTEGER,
    NorthSpades VARCHAR(13),
    NorthHearts VARCHAR(13),
    NorthDiamonds VARCHAR(13),
    NorthClubs VARCHAR(13),
    EastSpades VARCHAR(13),
    EastHearts VARCHAR(13),
    EastDiamonds VARCHAR(13),
    EastClubs VARCHAR(13),
    SouthSpades VARCHAR(13),
    SouthHearts VARCHAR(13),
    SouthDiamonds VARCHAR(13),
    SouthClubs VARCHAR(13),
    WestSpades VARCHAR(13),
    WestHearts VARCHAR(13),
    WestDiamonds VARCHAR(13),
    WestClubs VARCHAR(13)
);
```

#### 8. Settings
Application settings.

```sql
CREATE TABLE Settings (
    ShowResults YESNO,           -- Show traveller
    ShowPercentage YESNO,        -- Show percentage in rankings
    LeadCard YESNO,              -- Require lead card entry
    BM2ValidateLeadCard YESNO,   -- Validate lead card against hand
    BM2Ranking INTEGER,          -- 0=never, 1=after each round, 2=final only
    EnterResultsMethod INTEGER,  -- 0=plus/minus, 1=total tricks
    BM2ViewHandRecord YESNO,     -- Show hand records
    BM2NumberEntryEachRound YESNO, -- Re-enter player numbers each round
    BM2NameSource INTEGER,       -- 0=internal, 1=external
    BM2EnterHandRecord YESNO     -- Allow manual hand record entry
);
```

#### 9. Results
Ranking results (typically populated by scoring program).

```sql
CREATE TABLE Results (
    Section INTEGER,
    Orientation VARCHAR(2),      -- "NS" or "EW"
    Number INTEGER,              -- Pair number
    Score VARCHAR(10),           -- Score value
    Rank VARCHAR(10)             -- Rank position
);
```

## Test Data: 2-Table Mitchell Movement

### Section Setup
```sql
INSERT INTO Section (ID, Letter, [Tables], Winners, MissingPair) 
VALUES (1, 'A', 2, 2, 0);
```

### Movement (2 Rounds)
```sql
-- Round 1
INSERT INTO RoundData (Section, [Table], Round, NSPair, EWPair, LowBoard, HighBoard)
VALUES (1, 1, 1, 1, 2, 1, 2);
INSERT INTO RoundData (Section, [Table], Round, NSPair, EWPair, LowBoard, HighBoard)
VALUES (1, 2, 1, 3, 4, 3, 4);

-- Round 2 (EW pairs move, boards move)
INSERT INTO RoundData (Section, [Table], Round, NSPair, EWPair, LowBoard, HighBoard)
VALUES (1, 1, 2, 1, 4, 3, 4);
INSERT INTO RoundData (Section, [Table], Round, NSPair, EWPair, LowBoard, HighBoard)
VALUES (1, 2, 2, 3, 2, 1, 2);
```

### Tables Registration
```sql
INSERT INTO Tables (Section, [Table], LogOnOff) VALUES (1, 1, 0);
INSERT INTO Tables (Section, [Table], LogOnOff) VALUES (1, 2, 0);
```

### Settings
```sql
INSERT INTO Settings (ShowResults, ShowPercentage, LeadCard, BM2ValidateLeadCard, 
    BM2Ranking, EnterResultsMethod, BM2ViewHandRecord, BM2NumberEntryEachRound, 
    BM2NameSource, BM2EnterHandRecord)
VALUES (YES, YES, YES, YES, 1, 1, YES, NO, 0, NO);
```

### Optional: Player Names
```sql
INSERT INTO PlayerNames (ID, Name, strID) VALUES (1001, 'Alice Smith', '1001');
INSERT INTO PlayerNames (ID, Name, strID) VALUES (1002, 'Bob Jones', '1002');
INSERT INTO PlayerNames (ID, Name, strID) VALUES (1003, 'Carol White', '1003');
INSERT INTO PlayerNames (ID, Name, strID) VALUES (1004, 'David Brown', '1004');
INSERT INTO PlayerNames (ID, Name, strID) VALUES (1005, 'Eve Green', '1005');
INSERT INTO PlayerNames (ID, Name, strID) VALUES (1006, 'Frank Black', '1006');
INSERT INTO PlayerNames (ID, Name, strID) VALUES (1007, 'Grace Red', '1007');
INSERT INTO PlayerNames (ID, Name, strID) VALUES (1008, 'Henry Blue', '1008');
```

### Optional: Sample Hand Records
```sql
-- Board 1: Simple hand
INSERT INTO HandRecord (Section, Board, 
    NorthSpades, NorthHearts, NorthDiamonds, NorthClubs,
    EastSpades, EastHearts, EastDiamonds, EastClubs,
    SouthSpades, SouthHearts, SouthDiamonds, SouthClubs,
    WestSpades, WestHearts, WestDiamonds, WestClubs)
VALUES (1, 1,
    'AKQ2', 'KQJ', 'A32', 'K32',
    'J1098', '1098', 'J109', 'J109',
    '7654', 'A765', 'KQ8', 'A87',
    '3', '432', '7654', 'Q654');
```

## Creating the Database File

### Option 1: Using Microsoft Access
1. Open Microsoft Access
2. Create new blank database, save as `.mdb` file
3. Create tables using Design View or SQL
4. Insert test data

### Option 2: Using Python with pyodbc
```python
import pyodbc

# Create connection string
conn_str = (
    r'DRIVER={Microsoft Access Driver (*.mdb, *.accdb)};'
    r'DBQ=C:\path\to\test.mdb;'
)

# Create tables and insert data
conn = pyodbc.connect(conn_str)
cursor = conn.cursor()

# Execute CREATE TABLE and INSERT statements
# ...

conn.commit()
conn.close()
```

### Option 3: Using Existing Scoring Program
1. Use BridgeMate Control Software or similar
2. Create a new event with 2 tables, 4 pairs
3. Set up Mitchell movement
4. Export/save the `.bws` file

## File Location

Place the test database file in one of these locations:
- `TabScore2/TestData/test-2table.bws` (recommended for version control)
- Any accessible location (select via MainForm file dialog)

## Verification

After creating the database, verify by:
1. Launch TabScore2.exe
2. Select the database file via MainForm
3. Check that "Database Ready" status shows
4. Open browser to `http://localhost:5213`
5. Verify StartScreen loads without errors
