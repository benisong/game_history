using System;
using DonghanEngine.Core;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

public sealed class GameEngineIntelService : IIntelService
{
    private readonly GameEngine _engine;
    private readonly IGameStateReader _state;

    public GameEngineIntelService(GameEngine engine, IGameStateReader state)
    {
        _engine = engine;
        _state = state;
    }

    public ProvinceIntelResult InspectProvince(InspectProvinceCommand command)
    {
        var province = _state.GetProvince(command.ProvinceId);
        return province == null
            ? new ProvinceIntelResult(false, null, "province_not_found", "无此郡县。")
            : new ProvinceIntelResult(true, province);
    }

    public ActionResult ExecuteProvinceAction(ProvinceActionCommand command)
    {
        try
        {
            TurnResult result = command.Action switch
            {
                ProvinceActionKind.RecallGovernor => _engine.RecallGovernor(command.ProvinceId),
                ProvinceActionKind.AssignGovernor => _engine.AssignGovernor(command.ProvinceId, command.OfficerId ?? string.Empty),
                ProvinceActionKind.SuppressRebellion => _engine.SuppressRebellion(command.ProvinceId, command.OfficerId ?? string.Empty, command.Troops),
                ProvinceActionKind.PacifyRebellion => _engine.PacifyRebellion(
                    command.ProvinceId,
                    command.OfficerId ?? string.Empty,
                    ParseStrategies(command.Strategy),
                    command.ReliefGold),
                _ => throw new ArgumentOutOfRangeException(nameof(command.Action))
            };
            return new ActionResult(true, "州郡处置回奏", result.StoryText, ReportKind.Intel, Array.Empty<StateChange>());
        }
        catch (Exception ex)
        {
            return ActionResult.Failure("州郡处置未成", ex.Message, ReportKind.Warning, ex.GetType().Name);
        }
    }

    private static GameEngine.PacifyStrategy ParseStrategies(string? strategy)
    {
        if (string.IsNullOrWhiteSpace(strategy)) return GameEngine.PacifyStrategy.None;
        GameEngine.PacifyStrategy result = GameEngine.PacifyStrategy.None;
        foreach (var token in strategy.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            result |= token.ToLowerInvariant() switch
            {
                "sowdiscord" or "离间" => GameEngine.PacifyStrategy.SowDiscord,
                "persuade" or "说服" => GameEngine.PacifyStrategy.Persuade,
                "disasterrelief" or "赈灾" => GameEngine.PacifyStrategy.DisasterRelief,
                "punish" or "惩治" => GameEngine.PacifyStrategy.Punish,
                _ => GameEngine.PacifyStrategy.None
            };
        }
        return result;
    }
}
