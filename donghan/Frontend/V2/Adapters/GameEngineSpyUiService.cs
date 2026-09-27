using System.Collections.Generic;
using DonghanEngine.Core;
using DonghanEngine.Core.Politics;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

/// <summary>
/// 纯 UI 适配层：将西园特务暗署服务包装给前端调用（单一职责）
/// </summary>
public sealed class GameEngineSpyUiService : ISpyUiService
{
    private readonly IGameEngine _engine;

    public GameEngineSpyUiService(IGameEngine engine)
    {
        _engine = engine;
    }

    public SpyMissionResult DispatchMission(SpyMissionType missionType, string targetNpcId)
    {
        return _engine.DispatchSpyMission(missionType, targetNpcId);
    }

    public IReadOnlyList<NpcState> GetEligibleTargets()
    {
        return _engine.GetEligibleSpyTargets();
    }
}
