using System;
using System.Threading.Tasks;
using DonghanEngine.Core;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

public sealed class GameEngineCourtService : ICourtService
{
    private readonly GameEngine _engine;

    public GameEngineCourtService(GameEngine engine) => _engine = engine;

    public Task<string> StartSessionAsync()
    {
        return Task.FromResult(StartSession());
    }

    public string StartSession()
    {
        try
        {
            return _engine.StartGrandCourtSync();
        }
        catch (Exception ex)
        {
            return $"【朝会未开】{ex.Message}";
        }
    }

    public async Task<ActionResult> ExecuteDecisionAsync(CourtDecisionCommand command)
    {
        try
        {
            string input = ResolveDecisionInput(command);
            if (!string.IsNullOrWhiteSpace(command.ActiveOfficerId))
                _engine.ActiveOfficerId = command.ActiveOfficerId;

            var result = await _engine.ProcessPlayerTurnAsync(input);
            return new ActionResult(
                true,
                "朝议圣裁回奏",
                result.StoryText,
                ReportKind.Court,
                Array.Empty<StateChange>());
        }
        catch (Exception ex)
        {
            return ActionResult.Failure("朝议未成", ex.Message, ReportKind.Warning, ex.GetType().Name);
        }
    }

    public async Task<ActionResult> ExecuteFreeEdictAsync(FreeEdictCommand command)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(command.ActiveOfficerId))
                _engine.ActiveOfficerId = command.ActiveOfficerId;
            var result = await _engine.ProcessPlayerTurnAsync(command.PlayerInput);
            return new ActionResult(true, "亲拟圣旨回奏", result.StoryText, ReportKind.Court, Array.Empty<StateChange>());
        }
        catch (Exception ex)
        {
            return ActionResult.Failure("亲拟圣旨未成", ex.Message, ReportKind.Warning, ex.GetType().Name);
        }
    }

    private static string ResolveDecisionInput(CourtDecisionCommand command)
    {
        return command.DecisionId switch
        {
            "military_garden" => "命曹操整西园军",
            "military_north" => "准何进整北军",
            "military_funds" => "命张让核查军费",
            "treasury_eunuch" => "命张让筹措国帑",
            "eunuch_reprimand" => "训诫张让",
            "eunuch_reassure" => "重赏张让",
            "talent_cao" => "召见曹操",
            "talent_jian" => "召见蹇硕",
            _ => throw new ArgumentException($"未知朝会决策：{command.DecisionId}", nameof(command))
        };
    }
}
