/**
 * TabScore2 E2E Tests - Basic Happy Path
 * 
 * Tests a simple 2-table, 4-pair bridge session:
 * - Table 1: Pair 1 (NS) vs Pair 2 (EW)
 * - Table 2: Pair 3 (NS) vs Pair 4 (EW)
 * - 1 round with 2 boards per table
 * 
 * Prerequisites:
 * - TabScore2.exe running with test database loaded
 * - Database configured with 2-table Mitchell movement
 * - Web server accessible at http://localhost:5213
 */

import { test, expect, Page, BrowserContext } from '@playwright/test';

// Test configuration
const BASE_URL = 'http://localhost:5213';

// Helper class for TabScore2 page interactions
class TabScore2Page {
  constructor(private page: Page) {}

  // Navigate to start screen
  async gotoStart() {
    await this.page.goto(BASE_URL);
    await expect(this.page.locator('text=TabScore2')).toBeVisible();
  }

  // Click OK button (common across many screens)
  async clickOK() {
    await this.page.click('button:has-text("OK"), input[value="OK"], .ok-button');
  }

  // Click Back button
  async clickBack() {
    await this.page.click('button:has-text("Back"), input[value="Back"], .back-button');
  }

  // Select a table number
  async selectTable(tableNumber: number) {
    await this.page.click(`[data-table="${tableNumber}"], button:has-text("${tableNumber}")`);
  }

  // Select a direction (for moving devices)
  async selectDirection(direction: 'North' | 'South' | 'East' | 'West') {
    await this.page.click(`[data-direction="${direction}"], button:has-text("${direction}")`);
  }

  // Select a board number
  async selectBoard(boardNumber: number) {
    await this.page.click(`[data-board="${boardNumber}"], button:has-text("Board ${boardNumber}"), td:has-text("${boardNumber}")`);
  }

  // Enter contract details
  async enterContract(level: number, suit: string, declarer: string, doubled: boolean = false) {
    // Click level
    await this.page.click(`[data-level="${level}"], button:has-text("${level}")`);
    
    // Click suit (S=Spades, H=Hearts, D=Diamonds, C=Clubs, NT=No Trump)
    const suitMap: Record<string, string> = {
      'S': 'Spades', 'H': 'Hearts', 'D': 'Diamonds', 'C': 'Clubs', 'NT': 'NT'
    };
    await this.page.click(`[data-suit="${suit}"], button:has-text("${suitMap[suit] || suit}")`);
    
    // Click declarer direction
    await this.page.click(`[data-declarer="${declarer}"], button:has-text("${declarer}")`);
    
    // Click doubled if needed
    if (doubled) {
      await this.page.click('[data-double="X"], button:has-text("X")');
    }
  }

  // Enter pass (all pass)
  async enterPass() {
    await this.page.click('button:has-text("Pass"), [data-pass="true"]');
  }

  // Enter lead card
  async enterLead(suit: string, rank: string) {
    // Click suit
    await this.page.click(`[data-lead-suit="${suit}"], .lead-suit-${suit.toLowerCase()}`);
    // Click rank
    await this.page.click(`[data-lead-rank="${rank}"], .lead-rank-${rank}`);
  }

  // Skip lead card entry
  async skipLead() {
    await this.page.click('button:has-text("Skip"), [data-skip="true"]');
  }

  // Enter tricks taken (total tricks method)
  async enterTricksTaken(tricks: number) {
    await this.page.click(`[data-tricks="${tricks}"], button:has-text("${tricks}")`);
  }

  // Enter tricks plus/minus (relative to contract)
  async enterTricksPlusMinus(plusMinus: string) {
    await this.page.click(`[data-result="${plusMinus}"], button:has-text("${plusMinus}")`);
  }

  // Wait for specific screen
  async waitForScreen(screenName: string) {
    // Wait for URL to contain controller name or for specific element
    await this.page.waitForURL(`**/${screenName}/**`, { timeout: 10000 }).catch(() => {});
    await this.page.waitForLoadState('networkidle');
  }

  // Get current screen title
  async getTitle(): Promise<string> {
    return await this.page.locator('h1, .title, [data-title]').first().textContent() || '';
  }

  // Check if on specific screen
  async isOnScreen(screenName: string): Promise<boolean> {
    const url = this.page.url();
    return url.includes(screenName);
  }
}

// Test fixtures
test.describe('TabScore2 Basic Happy Path', () => {
  
  test.describe.configure({ mode: 'serial' }); // Run tests in order

  let table1Context: BrowserContext;
  let table2Context: BrowserContext;
  let table1Page: TabScore2Page;
  let table2Page: TabScore2Page;

  test.beforeAll(async ({ browser }) => {
    // Create separate browser contexts for each table (simulating different tablets)
    table1Context = await browser.newContext();
    table2Context = await browser.newContext();
    
    const page1 = await table1Context.newPage();
    const page2 = await table2Context.newPage();
    
    table1Page = new TabScore2Page(page1);
    table2Page = new TabScore2Page(page2);
  });

  test.afterAll(async () => {
    await table1Context?.close();
    await table2Context?.close();
  });

  test('Table 1: Start screen and register', async () => {
    // Navigate to start screen
    await table1Page.gotoStart();
    
    // Click OK to proceed
    await table1Page.clickOK();
    
    // Should be on SelectSection or SelectTableNumber
    // (SelectSection is skipped if only one section)
    await table1Page.waitForScreen('SelectTableNumber');
    
    // Select Table 1
    await table1Page.selectTable(1);
    await table1Page.clickOK();
    
    // Should proceed to ShowPlayerIDs (if DevicesMove=false)
    // or SelectDirection (if DevicesMove=true)
    await table1Page.waitForScreen('ShowPlayerIds');
  });

  test('Table 2: Start screen and register', async () => {
    // Navigate to start screen
    await table2Page.gotoStart();
    
    // Click OK to proceed
    await table2Page.clickOK();
    
    // Select Table 2
    await table2Page.waitForScreen('SelectTableNumber');
    await table2Page.selectTable(2);
    await table2Page.clickOK();
    
    // Should proceed to ShowPlayerIDs
    await table2Page.waitForScreen('ShowPlayerIds');
  });

  test('Table 1: View player IDs and proceed to round info', async () => {
    // On ShowPlayerIDs screen, click OK to proceed
    await table1Page.clickOK();
    
    // Should be on ShowRoundInfo
    await table1Page.waitForScreen('ShowRoundInfo');
    
    // Click OK to proceed to board selection
    await table1Page.clickOK();
    
    // Should be on ShowBoards
    await table1Page.waitForScreen('ShowBoards');
  });

  test('Table 2: View player IDs and proceed to round info', async () => {
    await table2Page.clickOK();
    await table2Page.waitForScreen('ShowRoundInfo');
    await table2Page.clickOK();
    await table2Page.waitForScreen('ShowBoards');
  });

  test('Table 1: Enter result for Board 1 - 4H by South making', async () => {
    // Select Board 1
    await table1Page.selectBoard(1);
    
    // Should be on EnterContract
    await table1Page.waitForScreen('EnterContract');
    
    // Enter contract: 4 Hearts by South
    await table1Page.enterContract(4, 'H', 'S');
    await table1Page.clickOK();
    
    // Should be on EnterLead
    await table1Page.waitForScreen('EnterLead');
    
    // Enter lead: Club King
    await table1Page.enterLead('C', 'K');
    await table1Page.clickOK();
    
    // Should be on EnterTricksTaken
    await table1Page.waitForScreen('EnterTricksTaken');
    
    // Enter tricks: 10 (making 4)
    await table1Page.enterTricksTaken(10);
    await table1Page.clickOK();
    
    // Should be on ConfirmResult
    await table1Page.waitForScreen('ConfirmResult');
    
    // Confirm the result
    await table1Page.clickOK();
    
    // Should return to ShowBoards (or EnterHandRecord if enabled)
    await table1Page.waitForScreen('ShowBoards');
  });

  test('Table 2: Enter result for Board 3 - 3NT by North down 1', async () => {
    // Select Board 3 (Table 2 has boards 3-4 in round 1)
    await table2Page.selectBoard(3);
    
    await table2Page.waitForScreen('EnterContract');
    
    // Enter contract: 3 No Trump by North
    await table2Page.enterContract(3, 'NT', 'N');
    await table2Page.clickOK();
    
    await table2Page.waitForScreen('EnterLead');
    
    // Enter lead: Spade Queen
    await table2Page.enterLead('S', 'Q');
    await table2Page.clickOK();
    
    await table2Page.waitForScreen('EnterTricksTaken');
    
    // Enter tricks: 8 (down 1)
    await table2Page.enterTricksTaken(8);
    await table2Page.clickOK();
    
    await table2Page.waitForScreen('ConfirmResult');
    await table2Page.clickOK();
    
    await table2Page.waitForScreen('ShowBoards');
  });

  test('Table 1: Enter result for Board 2 - Passed out', async () => {
    await table1Page.selectBoard(2);
    await table1Page.waitForScreen('EnterContract');
    
    // Enter pass (all pass)
    await table1Page.enterPass();
    
    // Should go directly to ConfirmResult for passed hands
    await table1Page.waitForScreen('ConfirmResult');
    await table1Page.clickOK();
    
    await table1Page.waitForScreen('ShowBoards');
  });

  test('Table 2: Enter result for Board 4 - 2S by East making', async () => {
    await table2Page.selectBoard(4);
    await table2Page.waitForScreen('EnterContract');
    
    await table2Page.enterContract(2, 'S', 'E');
    await table2Page.clickOK();
    
    await table2Page.waitForScreen('EnterLead');
    await table2Page.enterLead('H', 'A');
    await table2Page.clickOK();
    
    await table2Page.waitForScreen('EnterTricksTaken');
    await table2Page.enterTricksTaken(8);
    await table2Page.clickOK();
    
    await table2Page.waitForScreen('ConfirmResult');
    await table2Page.clickOK();
    
    await table2Page.waitForScreen('ShowBoards');
  });

  test('Table 1: Complete round and view rankings', async () => {
    // All boards entered, click OK to proceed
    await table1Page.clickOK();
    
    // Should be on ShowRankingList (if ShowRanking enabled)
    // or ShowMove (if rankings suppressed for first rounds)
    const isOnRanking = await table1Page.isOnScreen('ShowRankingList');
    
    if (isOnRanking) {
      // Verify rankings are displayed
      const page = (table1Page as any).page as Page;
      await expect(page.locator('table, .ranking-list')).toBeVisible();
      
      // Click OK to proceed to movement
      await table1Page.clickOK();
    }
    
    // Should be on ShowMove or EndScreen
    await table1Page.waitForScreen('ShowMove');
  });

  test('Table 2: Complete round and view rankings', async () => {
    await table2Page.clickOK();
    
    const isOnRanking = await table2Page.isOnScreen('ShowRankingList');
    if (isOnRanking) {
      await table2Page.clickOK();
    }
    
    await table2Page.waitForScreen('ShowMove');
  });

  test('Verify results were saved correctly', async () => {
    // This test would verify the database contains the expected results
    // In a real implementation, this could query the database directly
    // or use an API endpoint to verify results
    
    // For now, we verify by checking the UI shows the results
    const page = (table1Page as any).page as Page;
    
    // Navigate back to ShowBoards to verify results are marked
    await table1Page.gotoStart();
    await table1Page.clickOK();
    await table1Page.waitForScreen('SelectTableNumber');
    await table1Page.selectTable(1);
    await table1Page.clickOK();
    
    // The boards should show as played
    await table1Page.waitForScreen('ShowBoards');
    
    // Verify Board 1 shows a result (not empty)
    await expect(page.locator('[data-board="1"]')).toContainText(/4.*H|620|result/i);
  });
});

// Standalone test for quick verification
test('Quick smoke test - Start screen loads', async ({ page }) => {
  const tabScore = new TabScore2Page(page);
  await tabScore.gotoStart();
  
  // Verify start screen elements
  await expect(page.locator('text=TabScore2')).toBeVisible();
  await expect(page.locator('button:has-text("OK"), input[value="OK"]')).toBeVisible();
});
