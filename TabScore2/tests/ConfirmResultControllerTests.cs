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

/// <summary>
/// A simple in-memory implementation of ISession for testing purposes.
/// </summary>
public class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> _storage = new();
    public string Id => Guid.NewGuid().ToString();
    public bool IsAvailable => true;
    public IEnumerable<string> Keys => _storage.Keys;

    public void Clear() => _storage.Clear();
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Remove(string key) => _storage.Remove(key);
    public void Set(string key, byte[] value) => _storage[key] = value;
    public bool TryGetValue(string key, out byte[] value) => _storage.TryGetValue(key, out value);
}

public class ConfirmResultControllerTests
{
    private readonly Mock<IDatabase> _mockDatabase;
    private readonly Mock<IAppData> _mockAppData;
    private readonly Mock<IUtilities> _mockUtilities;
    private readonly ConfirmResultController _controller;
    private readonly ISession _session;

    public ConfirmResultControllerTests()
    {
        _mockDatabase = new Mock<IDatabase>();
        _mockAppData = new Mock<IAppData>(MockBehavior.Strict);
        _mockUtilities = new Mock<IUtilities>();

        _session = new TestSession();
        var httpContext = new DefaultHttpContext { Session = _session };

        _controller = new ConfirmResultController(_mockDatabase.Object, _mockAppData.Object, _mockUtilities.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            }
        };
    }

    private void SetDeviceNumberInSession(int deviceNumber)
    {
        // The error message proves the test environment is BIG-ENDIAN.
        // We must provide the bytes in big-endian order to get the correct integer value.
        // For the integer 1, this is [0, 0, 0, 1].
        _session.Set("DeviceNumber", new byte[] {
            (byte)(deviceNumber >> 24),
            (byte)(deviceNumber >> 16),
            (byte)(deviceNumber >> 8),
            (byte)deviceNumber
        });
    }

    [Fact]
    public void Index_NoDeviceNumber_RedirectsToErrorScreen()
    {
        // Arrange (no session value set)

        // Act
        var result = _controller.Index();

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ErrorScreen", redirectToActionResult.ControllerName);
        Assert.Equal("Index", redirectToActionResult.ActionName);
    }

    [Fact]
    public void Index_BoardNumberIsZero_RedirectsToShowBoards()
    {
        // Arrange
        SetDeviceNumberInSession(1);
        _mockAppData.Setup(x => x.GetDeviceStatus(1)).Returns(new DeviceStatus(1, "A", 1, 1, 1, Direction.North));
        _mockAppData.Setup(x => x.GetTableStatus(1, 1)).Returns(new TableStatus(1, 1, 1) { ResultData = new Result { BoardNumber = 0 } });

        // Act
        var result = _controller.Index();

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ShowBoards", redirectToActionResult.ControllerName);
        Assert.Equal("Index", redirectToActionResult.ActionName);
    }

    [Fact]
    public void Index_ValidRequest_ReturnsViewWithCorrectData()
    {
        // Arrange
        SetDeviceNumberInSession(1);
        var deviceStatus = new DeviceStatus(1, "A", 1, 1, 1, Direction.North);
        var tableStatus = new TableStatus(1, 1, 1) { ResultData = new Result { BoardNumber = 1 } };

        _mockAppData.Setup(x => x.GetDeviceStatus(1)).Returns(deviceStatus);
        _mockAppData.Setup(x => x.GetTableStatus(1, 1)).Returns(tableStatus);
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
        // Arrange (no session value set)

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
        SetDeviceNumberInSession(1);
        var deviceStatus = new DeviceStatus(1, "A", 1, 1, 1, Direction.North);
        var tableStatus = new TableStatus(1, 1, 1) { ResultData = new Result { BoardNumber = 1 } };

        _mockAppData.Setup(x => x.GetDeviceStatus(1)).Returns(deviceStatus);
        _mockAppData.Setup(x => x.GetTableStatus(1, 1)).Returns(tableStatus);
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
        // Arrange (no session value set)

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
        SetDeviceNumberInSession(1);
        var deviceStatus = new DeviceStatus(1, "A", 1, 1, 1, Direction.North);
        var tableStatus = new TableStatus(1, 1, 1) { ResultData = new Result { BoardNumber = 1, ContractLevel = 0 } };

        _mockAppData.Setup(x => x.GetDeviceStatus(1)).Returns(deviceStatus);
        _mockAppData.Setup(x => x.GetTableStatus(1, 1)).Returns(tableStatus);

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
        SetDeviceNumberInSession(1);
        var deviceStatus = new DeviceStatus(1, "A", 1, 1, 1, Direction.North);
        var tableStatus = new TableStatus(1, 1, 1) { ResultData = new Result { BoardNumber = 1, ContractLevel = 1 } };

        _mockAppData.Setup(x => x.GetDeviceStatus(1)).Returns(deviceStatus).Verifiable();
        _mockAppData.Setup(x => x.GetTableStatus(1, 1)).Returns(tableStatus).Verifiable();

        // Act
        var result = _controller.BackButtonClick();

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("EnterTricksTaken", redirectToActionResult.ControllerName);
        _mockAppData.Verify(); // Verify all verifiable setups were called
    }
}