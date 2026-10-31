using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GrpcSharedContracts.SharedClasses;
using TabScore2.BusinessLogic;
using TabScore2.Classes;
using TabScore2.Controllers;
using TabScore2.DataServices;
using TabScore2.Globals;
using TabScore2.Resources;

namespace TabScore2.Tests.Support;

internal sealed class PersonalHowellHarness : IDisposable
{
    private readonly Dictionary<int, int> deviceNumberByPair = [];

    public PersonalHowellHarness(int initialRound = 1)
    {
        Movement = HowellMovement.LoadSevenTableFixture();
        Database = new InMemoryMovementDatabase(Movement, initialRound);
        Settings = new TestSettings();
        Localizer = new TestStringLocalizer<Strings>();
        ExternalNamesDatabase = new TestExternalNamesDatabase();
        AppData = new AppData(Localizer, Database, Settings);
        AppData.ClearAppData();
        BusLogic = new BusLogic(Localizer, Database, AppData, Settings);

        RegisterRound(initialRound);
    }

    public HowellMovement Movement { get; }
    public InMemoryMovementDatabase Database { get; }
    public TestSettings Settings { get; }
    public TestStringLocalizer<Strings> Localizer { get; }
    public TestExternalNamesDatabase ExternalNamesDatabase { get; }
    public AppData AppData { get; }
    public BusLogic BusLogic { get; }

    public DeviceStatus Device(int pairNumber) =>
        AppData.GetDeviceStatus(deviceNumberByPair[pairNumber]);

    public TableStatus Table(int tableNumber) =>
        AppData.GetTableStatus(Database.Section.SectionId, tableNumber);

    public ActionResult ShowMove(int pairNumber, int newRoundNumber) =>
        CreateShowMoveController(pairNumber).Index(newRoundNumber);

    public ActionResult AttemptMove(int pairNumber, int newRoundNumber) =>
        CreateShowMoveController(pairNumber).OKButtonClick(newRoundNumber);

    public ActionResult EnterPlayerNumber(int pairNumber, Direction playerDirection, int playerNumber) =>
        CreateEnterPlayerIdController(pairNumber).OKButtonClick(playerDirection, playerNumber);

    public ActionResult CompleteViewOnlyBoards(int pairNumber) =>
        CreateShowBoardsController(pairNumber).OKButtonClick();

    public void Dispose() => AppData.ClearAppData();

    private void RegisterRound(int roundNumber)
    {
        foreach (Round round in Movement.GetRound(roundNumber))
        {
            int northDeviceNumber = AppData.AddDeviceStatusForTableDirection(
                Database.Section.SectionId,
                round.TableNumber,
                Direction.North);
            int eastDeviceNumber = AppData.AddDeviceStatusForTableDirection(
                Database.Section.SectionId,
                round.TableNumber,
                Direction.East);

            deviceNumberByPair.Add(round.ContestantNumberNorth, northDeviceNumber);
            deviceNumberByPair.Add(round.ContestantNumberEast, eastDeviceNumber);
        }
    }

    private ShowMoveController CreateShowMoveController(int pairNumber)
    {
        ShowMoveController controller = new(Database, BusLogic, AppData, Settings);
        SetControllerSession(controller, pairNumber);
        return controller;
    }

    private ShowBoardsController CreateShowBoardsController(int pairNumber)
    {
        ShowBoardsController controller = new(AppData, BusLogic);
        SetControllerSession(controller, pairNumber);
        return controller;
    }

    private EnterPlayerIdController CreateEnterPlayerIdController(int pairNumber)
    {
        EnterPlayerIdController controller = new(Database, ExternalNamesDatabase, AppData, BusLogic, Settings);
        SetControllerSession(controller, pairNumber);
        return controller;
    }

    private void SetControllerSession(Controller controller, int pairNumber)
    {
        DefaultHttpContext httpContext = new();
        httpContext.Session = new TestSession();
        httpContext.Session.SetInt32("DeviceNumber", deviceNumberByPair[pairNumber]);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }
}
