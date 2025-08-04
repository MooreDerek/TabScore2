using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TabScore2.Controllers;
using TabScore2.DataServices;
using TabScore2.UtilityServices;
using TabScore2.Classes;
using GrpcSharedContracts.SharedClasses;
using TabScore2.Models;
using TabScore2.Globals;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace TabScore2.Tests;

public class ConfirmResultControllerTests
{
    private readonly Mock<IDatabase> _mockDatabase;
    private readonly Mock<IAppData> _mockAppData;
    private readonly Mock<IUtilities> _mockUtilities;
    private readonly Mock<ISettings> _mockSettings;
    private readonly ConfirmResultController _controller;
    private readonly ISession _session;

    public ConfirmResultControllerTests()
    {
        _mockDatabase = new Mock<IDatabase>();
        _mockAppData = new Mock<IAppData>();
        _mockUtilities = new Mock<IUtilities>();
        _mockSettings = new Mock<ISettings>();

        _session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = _session };

        _controller = new ConfirmResultController(_mockDatabase.Object, _mockAppData.Object, _mockUtilities.Object, _mockSettings.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            }
        };
    }

    /// <summary>
    /// Centralized setup method to keep tests DRY.
    /// </summary>
    private (DeviceStatus, TableStatus) SetupTestScenario(int deviceNumber = 1, int boardNumber = 1, int contractLevel = 1, bool verifiable = false)
    {
        _session.Set("DeviceNumber", new byte[] { 0, 0, 0, (byte)deviceNumber });

        var deviceStatus = new DeviceStatus(deviceNumber, "A", 1, 1, 1, Direction.North);
        var tableStatus = new TableStatus(deviceNumber, 1, 1) { ResultData = new Result { BoardNumber = boardNumber, ContractLevel = contractLevel } };

        var deviceStatusSetup = _mockAppData.Setup(x => x.GetDeviceStatus(deviceNumber));
        var tableStatusSetup = _mockAppData.Setup(x => x.GetTableStatus(deviceStatus.SectionId, deviceStatus.TableNumber));

        if (verifiable)
        {
            deviceStatusSetup.Returns(deviceStatus).Verifiable();
            tableStatusSetup.Returns(tableStatus).Verifiable();
        }
        else
        {
            deviceStatusSetup.Returns(deviceStatus);
            tableStatusSetup.Returns(tableStatus);
        }

        return (deviceStatus, tableStatus);
    }

    [Fact]
    public void Index_NoDeviceNumber_RedirectsToErrorScreen()
    {
        // Act
        var result = _controller.Index();

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ErrorScreen", redirectToActionResult.ControllerName);
    }

    [Fact]
    public void Index_BoardNumberIsZero_RedirectsToShowBoards()
    {
        // Arrange
        SetupTestScenario(boardNumber: 0);

        // Act
        var result = _controller.Index();

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ShowBoards", redirectToActionResult.ControllerName);
    }

    [Fact]
    public void Index_ValidRequest_ReturnsViewWithCorrectData()
    {
        // Arrange
        var (deviceStatus, tableStatus) = SetupTestScenario();
        _mockAppData.Setup(x => x.GetTimerSeconds(deviceStatus)).Returns(10);
        _mockUtilities.Setup(x => x.CreateEnterContractModel(tableStatus.ResultData, true, LeadValidationOptions.NoWarning)).Returns(new EnterContractModel());
        _mockUtilities.Setup(x => x.Title("ConfirmResult", deviceStatus)).Returns("Test Title");
        _mockUtilities.Setup(x => x.Header(HeaderType.FullColoured, deviceStatus)).Returns("Test Header");

        // Act
        var result = _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<EnterContractModel>(viewResult.Model);
        Assert.Equal(10, viewResult.ViewData["TimerSeconds"]);
    }

    [Fact]
    public void OKButtonClick_NoDeviceNumber_RedirectsToErrorScreen()
    {
        // Act
        var result = _controller.OKButtonClick();

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ErrorScreen", redirectToActionResult.ControllerName);
    }

    [Fact]
    public void OKButtonClick_WhenCalled_SetsResultAndRedirects()
    {
        // Arrange
        var (_, tableStatus) = SetupTestScenario();
        _mockDatabase.Setup(x => x.SetResult(tableStatus.ResultData));

        // Act
        var result = _controller.OKButtonClick();

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("EnterHandRecord", redirectToActionResult.ControllerName);
        _mockDatabase.Verify(x => x.SetResult(tableStatus.ResultData), Times.Once);
    }

    [Fact]
    public void BackButtonClick_NoDeviceNumber_RedirectsToErrorScreen()
    {
        // Act
        var result = _controller.BackButtonClick();

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ErrorScreen", redirectToActionResult.ControllerName);
    }

    [Fact]
    public void BackButtonClick_ContractLevelIsZero_RedirectsToEnterContract()
    {
        // Arrange
        SetupTestScenario(contractLevel: 0);

        // Act
        var result = _controller.BackButtonClick();

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("EnterContract", redirectToActionResult.ControllerName);
    }

    [Fact]
    public void BackButtonClick_ContractLevelIsNotZero_RedirectsToEnterTricksTaken()
    {
        // Arrange
        SetupTestScenario(verifiable: true);

        // Act
        var result = _controller.BackButtonClick();

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("EnterTricksTaken", redirectToActionResult.ControllerName);
        _mockAppData.Verify();
    }

     [Fact]
    public void Index_PassesConfirmResultDelayToViewData()
    {
        // Arrange
        SetupTestScenario();
        _mockSettings.Setup(s => s.ConfirmResultDelay).Returns(5);

        // Act
        var result = _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(3, viewResult.ViewData["ConfirmResultDelay"]);
    }

}
