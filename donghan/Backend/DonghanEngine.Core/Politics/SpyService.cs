using System;
using System.Collections.Generic;
using System.Linq;
using DonghanEngine.Core.Balance;

namespace DonghanEngine.Core.Politics;

/// <summary>
/// 纯领域服务：西园内廷特务暗署与密探刺探引擎（单一职责）
/// 支持 IInitializableBalance<ISpyBalanceProvider> 接口，供超级控制工具动态调参
/// </summary>
public sealed class SpyService : ISpyService, IInitializableBalance<ISpyBalanceProvider>
{
    private ISpyBalanceProvider _config;

    public SpyService(ISpyBalanceProvider? config = null)
    {
        _config = config ?? new SpyBalanceConfig();
    }

    public void InitializeConfig(ISpyBalanceProvider config) => UpdateConfig(config);

    public void UpdateConfig(ISpyBalanceProvider config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public ISpyBalanceProvider GetConfig() => _config;

    public IReadOnlyList<NpcState> GetEligibleSpyTargets(GameState state)
    {
        if (state == null) return Array.Empty<NpcState>();
        // 排除蹇硕自己（特务头子），其余均可刺探
        return state.Npcs.Values
            .Where(n => n.Id != "jian_shuo")
            .OrderByDescending(n => n.Corruption + n.Ambition)
            .ToList()
            .AsReadOnly();
    }

    public SpyMissionResult DispatchSpyMission(GameState state, SpyMissionType missionType, string targetNpcId)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (!state.Npcs.TryGetValue(targetNpcId, out var target))
        {
            return new SpyMissionResult(
                Success: false,
                MissionId: Guid.NewGuid().ToString("N"),
                MissionType: missionType,
                TargetNpcId: targetNpcId,
                TargetName: "未知官员",
                GoldCost: 0,
                DiscoveredCorruption: 0,
                DiscoveredAmbition: 0,
                EvidenceSecured: false,
                NarrativeReport: "【西园密报】目标官员查无此人，密探无功而返。",
                DiscoveredTies: Array.Empty<string>());
        }

        int cost = _config.BaseMissionCost;
        // 扣除西园内帑或国库
        if (state.PrivateTreasury >= cost)
        {
            state.PrivateTreasury -= cost;
        }
        else if (state.Treasury >= cost)
        {
            state.Treasury -= cost;
        }
        else
        {
            return new SpyMissionResult(
                Success: false,
                MissionId: Guid.NewGuid().ToString("N"),
                MissionType: missionType,
                TargetNpcId: targetNpcId,
                TargetName: target.Name,
                GoldCost: cost,
                DiscoveredCorruption: 0,
                DiscoveredAmbition: 0,
                EvidenceSecured: false,
                NarrativeReport: $"【西园密报】内帑与太仓经费不足（需 {cost} 万钱），特务密探无法支应线人盘缠。",
                DiscoveredTies: Array.Empty<string>());
        }

        bool secured = (target.Corruption > 30 || target.Ambition > 40);
        var ties = state.NpcRelations
            .Where(r => r.FromNpcId == targetNpcId || (r.IsMutual && r.ToNpcId == targetNpcId))
            .Select(r => $"{r.Label}(与{(r.FromNpcId == targetNpcId ? r.ToNpcId : r.FromNpcId)})")
            .ToList();

        string narrative = missionType switch
        {
            SpyMissionType.InvestigateGovernorAmbition =>
                $"【西园密报 · 密探封疆】西园死士潜入 {target.Name} 任所，查实其野心约 {target.Ambition}，暗中囤积私兵与粮草，{(target.Ambition >= 70 ? "隐有割据称霸之志，需严加提防！" : "暂守藩臣之节。")}",
            SpyMissionType.TailCourtOfficial =>
                $"【西园密报 · 盯梢京官】中官密探昼夜盯梢 {target.Name} 府邸，查获其暗中结交派系（{target.Faction}），私下往来密信若干，朝局分量评定为 {target.Power}。",
            SpyMissionType.GatherCorruptionEvidence =>
                $"【西园密报 · 搜集罪证】死士已暗中查抄其隐匿私账，估算其私蓄赃银约 {target.StashedWealth} 万钱。{(secured ? "铁证如山，随时可请天子明诏或西园密令查抄！" : "其人行事谨慎，尚未拿获致命铁证。")}",
            _ => $"【西园密报】特务回报 {target.Name} 动向。"
        };

        state.AddToChronicle($"【西园密令】蹇硕麾下特务密探完成对 {target.Name} 的侦查。");

        return new SpyMissionResult(
            Success: true,
            MissionId: Guid.NewGuid().ToString("N"),
            MissionType: missionType,
            TargetNpcId: targetNpcId,
            TargetName: target.Name,
            GoldCost: cost,
            DiscoveredCorruption: target.Corruption,
            DiscoveredAmbition: target.Ambition,
            EvidenceSecured: secured,
            NarrativeReport: narrative,
            DiscoveredTies: ties.AsReadOnly());
    }
}
