using System;
using DonghanEngine.Core;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

public sealed class GameEngineSpecialActionService : ISpecialActionService
{
    private readonly GameEngine _engine;

    public GameEngineSpecialActionService(GameEngine engine) => _engine = engine;

    public ActionResult Execute(SpecialActionCommand command)
    {
        try
        {
            var state = _engine.GetState();
            TurnResult result = command.ActionId switch
            {
                "sell_office" => _engine.ExecuteQuickAction("sell_office"),
                "harem_rest" => _engine.ExecuteQuickAction("harem_rest"),
                "disaster_relief" => _engine.ExecuteDisasterReliefAction(command.Amount, command.OfficerId),
                "confiscation" => _engine.ExecuteConfiscationAction(command.TargetNpcId, command.Destination),
                _ => throw new ArgumentOutOfRangeException(nameof(command.ActionId), command.ActionId, "未知特殊行动")
            };

            if (command.ActionId == "disaster_relief" && command.Amount > state.Treasury)
                return ActionResult.Failure("开仓赈灾", result.StoryText, ReportKind.Warning, "InsufficientTreasury");

            if (command.ActionId == "confiscation"
                && (result.StoryText.Contains("抄家流产", StringComparison.Ordinal)
                    || result.StoryText.Contains("抄家受阻", StringComparison.Ordinal)))
                return ActionResult.Failure("抄家回奏", result.StoryText, ReportKind.Warning, "ConfiscationFailed");

            return new ActionResult(true, ResolveTitle(command.ActionId), result.StoryText, ReportKind.Information, Array.Empty<StateChange>());
        }
        catch (Exception ex)
        {
            return ActionResult.Failure(ResolveTitle(command.ActionId), ex.Message, ReportKind.Warning, ex.GetType().Name);
        }
    }

    private static string ResolveTitle(string actionId) => actionId switch
    {
        "sell_office" => "西园鬻官回奏",
        "harem_rest" => "后宫起居回奏",
        "disaster_relief" => "大朝赈灾回奏",
        "confiscation" => "抄家回奏",
        _ => "特殊行动回奏"
    };
}
