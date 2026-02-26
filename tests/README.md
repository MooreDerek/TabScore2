# TabScore2 E2E Tests

End-to-end tests for TabScore2 using [Playwright](https://playwright.dev/).

## Prerequisites

1. **Node.js 18+** installed
2. **TabScore2** application running with a test database loaded
3. Web server accessible at `http://localhost:5213`

## Setup

```bash
# Navigate to tests directory
cd tests

# Install dependencies
npm install

# Install Playwright browsers
npx playwright install
```

## Test Database Setup

Before running tests, you need a test database configured with:
- 2 tables (Table 1 and Table 2)
- 4 pairs (Pair 1-4)
- 2-table Mitchell movement
- 2 boards per table per round

See [`../plans/test-database-setup.md`](../plans/test-database-setup.md) for detailed database setup instructions.

## Running Tests

### Start TabScore2

1. Launch `TabScore2.exe`
2. Load the test database via the MainForm
3. Ensure "Database Ready" status shows

### Run Tests

```bash
# Run all tests
npm test

# Run tests with browser visible
npm run test:headed

# Run tests in debug mode
npm run test:debug

# Run tests with Playwright UI
npm run test:ui

# Run smoke test only
npm run test:smoke

# View test report
npm run report
```

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `TABSCORE2_URL` | `http://localhost:5213` | Base URL for TabScore2 web server |
| `CI` | - | Set in CI environments for stricter settings |

## Test Structure

```
tests/
├── e2e/
│   └── basic-happy-path.spec.ts   # Main test file
├── package.json                    # Dependencies
├── playwright.config.ts            # Playwright configuration
├── tsconfig.json                   # TypeScript configuration
└── README.md                       # This file
```

## Test Scenarios

### Basic Happy Path (`basic-happy-path.spec.ts`)

Tests a complete 2-table, 4-pair bridge session:

1. **Table Registration**
   - Table 1 registers at start screen
   - Table 2 registers at start screen

2. **Player ID Display**
   - Both tables view player IDs
   - Proceed to round info

3. **Result Entry**
   - Table 1: Board 1 - 4H by South making (10 tricks)
   - Table 2: Board 3 - 3NT by North down 1 (8 tricks)
   - Table 1: Board 2 - Passed out
   - Table 2: Board 4 - 2S by East making (8 tricks)

4. **Round Completion**
   - View rankings (if enabled)
   - View movement for next round

## Writing New Tests

### Page Object Pattern

The tests use a `TabScore2Page` helper class for common interactions:

```typescript
const tabScore = new TabScore2Page(page);

// Navigate
await tabScore.gotoStart();

// Interact
await tabScore.selectTable(1);
await tabScore.enterContract(4, 'H', 'S');
await tabScore.enterTricksTaken(10);
await tabScore.clickOK();

// Wait for screens
await tabScore.waitForScreen('ShowBoards');
```

### Multi-Table Testing

Tests use separate browser contexts to simulate multiple tablets:

```typescript
const table1Context = await browser.newContext();
const table2Context = await browser.newContext();

const table1Page = new TabScore2Page(await table1Context.newPage());
const table2Page = new TabScore2Page(await table2Context.newPage());
```

## Troubleshooting

### Tests fail to connect

- Ensure TabScore2.exe is running
- Check the database is loaded (DatabaseReady = true)
- Verify the web server is accessible at the configured URL

### Tests timeout

- Increase timeouts in `playwright.config.ts`
- Check for JavaScript errors in the browser console
- Verify the expected elements exist on the page

### Selectors not finding elements

- Use Playwright's inspector: `npm run test:debug`
- Check the actual HTML structure of the page
- Update selectors in `TabScore2Page` class

## CI/CD Integration

For CI environments, set the `CI` environment variable:

```yaml
- name: Run E2E Tests
  env:
    CI: true
    TABSCORE2_URL: http://localhost:5213
  run: |
    cd tests
    npm ci
    npx playwright install --with-deps
    npm test
```

Note: TabScore2 must be started separately as it's a Windows Forms application.
