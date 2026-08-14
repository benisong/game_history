using System;
using DonghanEngine.Core;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

public sealed class GameEngineWestGardenService : IWestGardenService
{
    private readonly GameEngine _engine;

    public GameEngineWestGardenService(GameEngine engine) => _engine = engine;

    public ActionResult PayArmy(ArmyPayCommand command)
    {
        var beforePrivateTreasury = _engine.GetState().PrivateTreasury;
        var result = Execute("发内帑犒军", ReportKind.WestGarden, () =>
            _engine.ExecuteDrillArmyActionWithOfficer(command.Amount, command.OfficerId));
        if (result.Success && command.Amount > beforePrivateTreasury)
            return ActionResult.Failure("发内帑犒军", result.StoryText, ReportKind.WestGarden, "InsufficientPrivateTreasury");
        return result;
    }

    public ActionResult DrillArmy(ArmyDrillCommand command) => PayArmy(new ArmyPayCommand(command.Amount, command.OfficerId));

    public ActionResult RecruitArmy(RecruitArmyCommand command)
    {
        var state = _engine.GetState();
        var capacity = 12000 - state.WestGardenArmy.Size;
        var actualTroops = Math.Min(command.Troops, Math.Max(0, capacity));
        var batches = actualTroops / 1000;
        var estimatedCost = batches * 300;
        var result = Execute("西园募兵回报", ReportKind.WestGarden, () =>
            _engine.ExecuteRaiseWestGardenTroopsAction(command.Troops));
        if (result.Success && (capacity <= 0 || estimatedCost > state.Treasury))
            return ActionResult.Failure("西园募兵回报", result.StoryText, ReportKind.WestGarden,
                capacity <= 0 ? "WestGardenArmyFull" : "InsufficientTreasury");
        return result;
    }

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
