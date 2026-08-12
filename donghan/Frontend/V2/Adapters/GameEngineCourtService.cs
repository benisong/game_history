using System;
using System.Threading.Tasks;
using DonghanEngine.Core;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

public sealed class GameEngineCourtService : ICourtService
{
    private readonly GameEngine _engine;

    public GameEngineCourtService(GameEngine engine) => _engine = engine;

    public Task<ActionResult> ExecuteDecisionAsync(CourtDecisionCommand command)
    {
        try
        {
            // V2 首阶段只提供统一契约边界；具体朝会命令映射在 Court 纵向切片中接入。
            // 不把未知 decisionId 静默映射为普通玩家输入，避免误执行。
            throw new NotSupportedException($"朝会决策尚未接入 V2 适配器：{command.DecisionId}");
        }
        catch (Exception ex)
        {
            return Task.FromResult(ActionResult.Failure("朝议未成", ex.Message, ReportKind.Warning, ex.GetType().Name));
        }
    }
}
