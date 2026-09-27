using System.Collections.Generic;
using DonghanEngine.Core.Balance;

namespace DonghanEngine.Core.Politics;

/// <summary>
/// 特务密令类型
/// </summary>
public enum SpyMissionType
{
    InvestigateGovernorAmbition, // 密探封疆大吏：探明刺史/州牧真实野心与隐匿账目
    TailCourtOfficial,          // 盯梢京畿朝臣：查明结交党人与私下串联动向
    GatherCorruptionEvidence    // 搜集贪墨罪证：为西园查抄或朝堂弹劾搜集铁证
}

/// <summary>
/// 特务密令执行结果
/// </summary>
public sealed record SpyMissionResult(
    bool Success,
    string MissionId,
    SpyMissionType MissionType,
    string TargetNpcId,
    string TargetName,
    int GoldCost,
    int DiscoveredCorruption,
    int DiscoveredAmbition,
    bool EvidenceSecured,
    string NarrativeReport,
    IReadOnlyList<string> DiscoveredTies);

/// <summary>
/// 默认西园特务配置
/// </summary>
public sealed class SpyBalanceConfig : ISpyBalanceProvider
{
    public int BaseMissionCost { get; set; } = 300; // 基础密探经费 300 万钱
    public int EvidenceSuccessRateBase { get; set; } = 75; // 75% 取得关键罪证
    public int DiscoveryAmbitionAccuracy { get; set; } = 90;
}

/// <summary>
/// 西园特务与内廷密谍网契约（单一职责）
/// </summary>
public interface ISpyService
{
    SpyMissionResult DispatchSpyMission(GameState state, SpyMissionType missionType, string targetNpcId);
    IReadOnlyList<NpcState> GetEligibleSpyTargets(GameState state);
    ISpyBalanceProvider GetConfig();
    void UpdateConfig(ISpyBalanceProvider config);
}
