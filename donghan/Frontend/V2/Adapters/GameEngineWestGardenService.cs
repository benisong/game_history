using System;
using DonghanEngine.Core;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

public sealed class GameEngineWestGardenService : IWestGardenService
{
    private readonly GameEngine _engine;

    public GameEngineWestGardenService(GameEngine engine) => _engine = engine;

    public ActionResult PayArmy(ArmyPayCommand command) =>
        Execute("发内帑犒军", ReportKind.WestGarden, () =>
            _engine.ExecuteDrillArmyActionWithOfficer(command.Amount, command.OfficerId));

    public ActionResult DrillArmy(ArmyDrillCommand command) => PayArmy(new ArmyPayCommand(command.Amount, command.OfficerId));

    public ActionResult RecruitArmy(RecruitArmyCommand command) =>
        Execute("西园募兵回报", ReportKind.WestGarden, () =>
            _engine.ExecuteRaiseWestGardenTroopsAction(command.Troops));

    private static ActionResult Execute(string title, ReportKind kind, Func<TurnResult> action)
    {
        try
        {
            var result = action();
            return new ActionResult(true, title, result.StoryText, kind, Array.Empty<StateChange>());
        }
        catch (Exception ex)
        {
            return ActionResult.Failure(title, ex.Message, ReportKind.Warning, ex.GetType().Name);
        }
    }
}
