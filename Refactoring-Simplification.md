# Refactoring - Simplification

## Common Patterns Identified

### 1. Device Number Lookup and Validation
```csharp
// In every action across all controllers:
int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);
```

### 2. Table Status Lookup
```csharp
// Repeated after getting device status:
TableStatus tableStatus = appData.GetTableStatus(deviceStatus.SectionId, deviceStatus.TableNumber);
```

### 3. ViewData Setup
```csharp
// In every Index action:
ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceStatus);
ViewData["Title"] = utilities.Title("ScreenName", deviceStatus);
ViewData["Header"] = utilities.Header(HeaderType.FullColoured, deviceStatus);
ViewData["ButtonOptions"] = ButtonOptions.OKDisabledAndBack;
```

### 4. Getting ResultData
```csharp
// Repeated in multiple OK button actions:
Result result = appData.GetTableStatus(deviceStatus.SectionId, deviceStatus.TableNumber).ResultData;
```

## Proposed Base Controller

```csharp
public class BaseController : Controller
{
    protected readonly IAppData AppData;
    protected readonly IUtilities Utilities;

    protected BaseController(IAppData appData, IUtilities utilities)
    {
        AppData = appData;
        Utilities = utilities;
    }

    protected DeviceStatus? GetDeviceStatus()
    {
        int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
        if (deviceNumber == -1)
        {
            return null;
        }
        return AppData.GetDeviceStatus(deviceNumber);
    }

    protected TableStatus GetTableStatus(DeviceStatus deviceStatus)
    {
        return AppData.GetTableStatus(deviceStatus.SectionId, deviceStatus.TableNumber);
    }

    protected Result GetResultData(DeviceStatus deviceStatus)
    {
        return GetTableStatus(deviceStatus).ResultData;
    }

    protected void SetViewData(string screenName, HeaderType headerType, ButtonOptions buttonOptions, DeviceStatus deviceStatus)
    {
        ViewData["TimerSeconds"] = AppData.GetTimerSeconds(deviceStatus);
        ViewData["Title"] = Utilities.Title(screenName, deviceStatus);
        ViewData["Header"] = Utilities.Header(headerType, deviceStatus);
        ViewData["ButtonOptions"] = buttonOptions;
    }

    protected ActionResult RedirectToErrorScreen()
    {
        return RedirectToAction("Index", "ErrorScreen");
    }
}
```

## Refactored Controller Example

Before (EnterContractController.cs):
```csharp
public class EnterContractController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities) : Controller
{
    private readonly IDatabase database = iDatabase;
    private readonly IAppData appData = iAppData;
    private readonly IUtilities utilities = iUtilities;

    public ActionResult Index(int boardNumber)
    {
        int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
        if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
        DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

        TableStatus tableStatus = appData.GetTableStatus(deviceStatus.SectionId, deviceStatus.TableNumber);
        Result result = tableStatus.ResultData;
        if (tableStatus.ResultData.BoardNumber != boardNumber)
        {
            result = database.GetResult(tableStatus.SectionId, tableStatus.TableNumber, tableStatus.RoundNumber, boardNumber);
            result.NumberNorth = tableStatus.RoundData.NumberNorth;
            result.NumberEast = tableStatus.RoundData.NumberEast;
            result.NumberSouth = tableStatus.RoundData.NumberSouth;
            result.NumberWest = tableStatus.RoundData.NumberWest;
            tableStatus.ResultData = result;
        }

        EnterContractModel enterContractModel = utilities.CreateEnterContractModel(result);

        ViewData["TimerSeconds"] = appData.GetTimerSeconds(deviceStatus);
        ViewData["Title"] = utilities.Title("EnterContract", deviceStatus);
        ViewData["Header"] = utilities.Header(HeaderType.FullColoured, deviceStatus);
        ViewData["ButtonOptions"] = ButtonOptions.OKDisabledAndBack;
        return View(enterContractModel);
    }

    public ActionResult OKButtonContract(int contractLevel, string contractSuit, string contractX, string declarerNSEW)
    {
        int deviceNumber = HttpContext.Session.GetInt32("DeviceNumber") ?? -1;
        if (deviceNumber == -1) return RedirectToAction("Index", "ErrorScreen");
        DeviceStatus deviceStatus = appData.GetDeviceStatus(deviceNumber);

        contractX ??= string.Empty;
        Result result = appData.GetTableStatus(deviceStatus.SectionId, deviceStatus.TableNumber).ResultData;
        result.ContractLevel = contractLevel;
        result.ContractSuit = contractSuit;
        result.ContractX = contractX;
        result.DeclarerNSEW = declarerNSEW;
        result.Remarks = string.Empty;
        return RedirectToAction("Index", "EnterLead", new { leadValidation = LeadValidationOptions.Validate });
    }
}
```

After:
```csharp
public class EnterContractController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities) : BaseController(iAppData, iUtilities)
{
    private readonly IDatabase database = iDatabase;

    public ActionResult Index(int boardNumber)
    {
        DeviceStatus? deviceStatus = GetDeviceStatus();
        if (deviceStatus == null) return RedirectToErrorScreen();

        TableStatus tableStatus = GetTableStatus(deviceStatus);
        Result result = tableStatus.ResultData;
        if (tableStatus.ResultData.BoardNumber != boardNumber)
        {
            result = database.GetResult(tableStatus.SectionId, tableStatus.TableNumber, tableStatus.RoundNumber, boardNumber);
            result.NumberNorth = tableStatus.RoundData.NumberNorth;
            result.NumberEast = tableStatus.RoundData.NumberEast;
            result.NumberSouth = tableStatus.RoundData.NumberSouth;
            result.NumberWest = tableStatus.RoundData.NumberWest;
            tableStatus.ResultData = result;
        }

        EnterContractModel enterContractModel = Utilities.CreateEnterContractModel(result);

        SetViewData("EnterContract", HeaderType.FullColoured, ButtonOptions.OKDisabledAndBack, deviceStatus);
        return View(enterContractModel);
    }

    public ActionResult OKButtonContract(int contractLevel, string contractSuit, string contractX, string declarerNSEW)
    {
        DeviceStatus? deviceStatus = GetDeviceStatus();
        if (deviceStatus == null) return RedirectToErrorScreen();

        contractX ??= string.Empty;
        Result result = GetResultData(deviceStatus);
        result.ContractLevel = contractLevel;
        result.ContractSuit = contractSuit;
        result.ContractX = contractX;
        result.DeclarerNSEW = declarerNSEW;
        result.Remarks = string.Empty;
        return RedirectToAction("Index", "EnterLead", new { leadValidation = LeadValidationOptions.Validate });
    }
}
```

## Benefits

- Eliminates ~12 lines of repetition per action
- Centralizes ViewData setup pattern
- Provides compile-time type safety
- Makes controller actions more readable by focusing on business logic
- Easier to maintain consistent error handling across all controllers
