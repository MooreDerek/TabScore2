# Refactoring - BaseController TDD Approach

## Overview

Using Test Driven Development to safely refactor controllers to use a BaseController, ensuring no behavior changes are introduced.

## Step 1: Write Tests for Current Behavior (Before Refactoring)

```csharp
public class EnterContractControllerTests
{
    private Mock<IDatabase> mockDatabase;
    private Mock<IAppData> mockAppData;
    private Mock<IUtilities> mockUtilities;
    private EnterContractController controller;

    [SetUp]
    public void Setup()
    {
        mockDatabase = new Mock<IDatabase>();
        mockAppData = new Mock<IAppData>();
        mockUtilities = new Mock<IUtilities>();
        controller = new EnterContractController(mockDatabase.Object, mockAppData.Object, mockUtilities.Object);
        
        // Setup session with valid device number
        var httpContext = new DefaultHttpContext();
        httpContext.Session.SetInt32("DeviceNumber", 1);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Test]
    public void Index_NoDeviceNumber_ReturnsErrorScreen()
    {
        // Arrange - no device number in session
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var result = controller.Index(boardNumber: 5) as RedirectToRouteResult;

        // Assert
        Assert.That(result.RouteName, Is.EqualTo("Default"));
        Assert.That(result.RouteValues["Controller"], Is.EqualTo("ErrorScreen"));
    }

    [Test]
    public void Index_ValidDevice_ReturnsViewWithModel()
    {
        // Arrange
        var deviceStatus = new DeviceStatus(1, "A", 1, 10, 1, Direction.North);
        var tableStatus = new TableStatus(1, 1, 1);
        var result = new Result { BoardNumber = 5 };
        
        mockAppData.Setup(x => x.GetDeviceStatus(1)).Returns(deviceStatus);
        mockAppData.Setup(x => x.GetTableStatus(1, 1)).Returns(tableStatus);
        mockUtilities.Setup(x => x.CreateEnterContractModel(It.IsAny<Result>())).Returns(new EnterContractModel());
        mockUtilities.Setup(x => x.Title("EnterContract", deviceStatus)).Returns("Title");
        mockUtilities.Setup(x => x.Header(HeaderType.FullColoured, deviceStatus)).Returns("Header");
        mockAppData.Setup(x => x.GetTimerSeconds(deviceStatus)).Returns(60);

        // Act
        var result = controller.Index(5) as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ViewData.Model, Is.InstanceOf<EnterContractModel>());
    }

    [Test]
    public void OKButtonContract_ValidInput_RedirectsToEnterLead()
    {
        // Arrange
        var deviceStatus = new DeviceStatus(1, "A", 1, 10, 1, Direction.North);
        var tableStatus = new TableStatus(1, 1, 1);
        tableStatus.ResultData = new Result { BoardNumber = 5 };
        
        mockAppData.Setup(x => x.GetDeviceStatus(1)).Returns(deviceStatus);
        mockAppData.Setup(x => x.GetTableStatus(1, 1)).Returns(tableStatus);

        // Act
        var result = controller.OKButtonContract(1, "S", "X", "N") as RedirectToRouteResult;

        // Assert
        Assert.That(result.RouteValues["Controller"], Is.EqualTo("EnterLead"));
        Assert.That(tableStatus.ResultData.ContractLevel, Is.EqualTo(1));
        Assert.That(tableStatus.ResultData.ContractSuit, Is.EqualTo("S"));
    }
}
```

## Step 2: Run Tests (Should Pass - Green)

```bash
dotnet test --filter "FullyQualifiedName~EnterContractControllerTests"
```

## Step 3: Refactor to BaseController

```csharp
// Create BaseController.cs
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
        if (deviceNumber == -1) return null;
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

## Step 4: Refactor EnterContractController

Before:
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
public class EnterContractController(IDatabase iDatabase, IAppData iAppData, IUtilities iUtilities) 
    : BaseController(iAppData, iUtilities)
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

    public ActionResult OKButtonPass()
    {
        DeviceStatus? deviceStatus = GetDeviceStatus();
        if (deviceStatus == null) return RedirectToErrorScreen();

        Result result = GetResultData(deviceStatus);
        result.ContractLevel = 0;
        result.ContractSuit = string.Empty;
        result.ContractX = string.Empty;
        result.DeclarerNSEW = string.Empty;
        result.LeadCard = string.Empty;
        result.TricksTaken = -1;
        result.Remarks = string.Empty;
        return RedirectToAction("Index", "ConfirmResult");
    }
    
    public ActionResult OKButtonSkip()
    {
        DeviceStatus? deviceStatus = GetDeviceStatus();
        if (deviceStatus == null) return RedirectToErrorScreen();

        Result result = GetResultData(deviceStatus);
        result.ContractLevel = -1;
        result.ContractSuit = string.Empty;
        result.ContractX = string.Empty;
        result.DeclarerNSEW = string.Empty;
        result.LeadCard = string.Empty;
        result.TricksTaken = -1;
        result.Remarks = "Not played";
        database.SetResult(result);
        return RedirectToAction("Index", "ShowBoards");
    }
}
```

## Step 5: Run Tests Again (Should Still Pass - Green)

```bash
dotnet test --filter "FullyQualifiedName~EnterContractControllerTests"
```

## TDD Cycle Summary

1. **Red**: Write tests for current behavior
2. **Green**: Tests pass with original code
3. **Refactor**: Extract common code to BaseController
4. **Green**: Tests still pass, confirming behavior unchanged

This approach ensures the refactoring doesn't introduce regressions. The tests act as a safety net, verifying that the BaseController methods behave identically to the original inline code.
