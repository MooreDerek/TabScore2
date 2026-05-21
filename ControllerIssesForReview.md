# Controller Issues For Review

## Critical Defects

### 1. `ShowRankingListController.cs:77` - Potential IndexOutOfRangeException
```csharp
if (showRankingListModel.Count <= 1 && showRankingListModel[0].ScoreDecimal == 0.0)
```
If `Count == 0`, this will throw. Should be `Count == 0 || showRankingListModel[0].ScoreDecimal == 0.0`.

### 2. `ShowRankingListController.cs:103` - No authentication check
```csharp
public JsonResult PollRanking()
{
    int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? 0;
    int sectionId = appData.GetDeviceStatus(deviceNumber).SectionId;
```
`deviceNumber == 0` will throw in `GetDeviceStatus`. Every other action checks for `-1` and redirects to ErrorScreen.

### 3. `ShowBoardsController.cs:57` - Potential IndexOutOfRangeException
```csharp
if (showBoardsModel.First(x => x.BoardNumber == boardNumber).ContractLevel < 0)
```
If `boardNumber` doesn't exist in the list, `First()` throws. No validation that the board exists.

## Race Conditions (Concurrent Access)

### 4. `ShowTravellerController.cs:36` - Same race condition as ShowBoards
```csharp
if (tableStatus.ResultData.BoardNumber == 0)
{
    tableStatus.ResultData = database.GetResult(...);
}
```
If a viewing device hits this while the scoring device is mid-flow, it can overwrite `ResultData` with stale data.

### 5. `ShowRoundInfoController.cs:107-108` - Shared state mutation
```csharp
deviceStatus.RoundNumber--;
tableStatus.RoundNumber--;
```
The Back button decrements shared `TableStatus.RoundNumber`, which affects all devices at the table.

### 6. `ShowMoveController.cs:42-60` - Check-then-act race
```csharp
if (tableStatus.RoundNumber < newRoundNumber)
{
    // sets ReadyForNextRound flags
}
```
Multiple devices checking/setting these flags concurrently without synchronization.

## Code Smells

### 7. Repeated device number pattern (19 controllers)
```csharp
int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
```
This is duplicated in nearly every action. Should be an action filter or base controller.

### 8. `EnterPlayerIDController.cs:69-89` - Switch on direction with duplicated logic
Could use a dictionary or extension method instead of repetitive switch cases.

### 9. `ShowMoveController.cs:108-119` - Hardcoded device counts
```csharp
if (deviceStatus.DevicesPerTable == 2 && ...)
else if (deviceStatus.DevicesPerTable == 4 && ...)
```
No fallback for unexpected values. Should have a default case.

### 10. `ErrorScreenController.cs:27` - Unhandled exception file naming collision
```csharp
string exceptionFileName = $"TabScore2Exception{DateTime.Now:yyMMddHHmmss}.txt";
```
Two exceptions in the same second will collide. Also, `StreamWriter` not disposed (missing `using`).

### 11. `EnterHandRecordController.cs:52` - Stale board number
```csharp
int boardNumber = appData.GetTableStatus(...).ResultData.BoardNumber;
```
Reads `BoardNumber` from shared state after user submitted the form. If another device changed it between form load and submit, wrong board gets the hand record.

### 12. `ConfirmResultController.cs:59` - Magic number
```csharp
if (result.ContractLevel == 0)  // This was passed out
```
Magic number `0` to detect pass-out. Should be a constant or enum value.

## Summary

| Severity | Count | Files |
|----------|-------|-------|
| Critical | 3 | ShowRankingList, ShowBoards |
| Race conditions | 4 | ShowTraveller, ShowRoundInfo, ShowMove, AppData |
| Code smells | 5 | All controllers |
