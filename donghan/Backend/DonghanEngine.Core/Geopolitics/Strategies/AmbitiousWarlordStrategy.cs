using System;
using System.Linq;
using DonghanEngine.Core.Geopolitics.Contracts;
using DonghanEngine.Core.Geopolitics.MathEngine;
using DonghanEngine.Core.Geopolitics.Models;

namespace DonghanEngine.Core.Geopolitics.Strategies;

/// <summary>
/// 称霸枭雄策略（曹操、袁绍、孙策）：扫描周边邻邦，基于多因子概率矩阵与随机数决定是否出征攻伐
/// </summary>
public sealed class AmbitiousWarlordStrategy : IWarlordDecisionEvaluator
{
    private readonly IRandomProvider _random;

    public AmbitiousWarlordStrategy(IRandomProvider random)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
    }

    public WarlordDecision EvaluateDecision(WarlordFaction faction, GeopoliticalContext context)
    {
        if (faction.TotalTroops < 3000 || faction.Provisions < 1500)
        {
            return new WarlordDecision(WarlordActionType.Idle, string.Empty, string.Empty, 0, 0, "军力/粮秣不足，闭关屯田蓄势");
        }

        // 1. 扫描自身控制州郡的所有邻接州郡
        string bestTargetProvince = string.Empty;
        string targetFactionId = string.Empty;
        int minTargetGarrison = int.MaxValue;

        foreach (var myProvId in faction.ControlledProvinces)
        {
            if (!context.AllProvinces.TryGetValue(myProvId, out var myProv)) continue;

            foreach (var neighborId in myProv.Neighbors)
            {
                if (faction.ControlledProvinces.Contains(neighborId)) continue; // 自己已控制
                if (!context.AllProvinces.TryGetValue(neighborId, out var neighborProv)) continue;

                // 找到该州郡的所属势力（若有）
                var defenderFaction = context.AllFactions.Values.FirstOrDefault(f => f.ControlledProvinces.Contains(neighborId));
                string defFactionId = defenderFaction?.FactionId ?? string.Empty;

                // 若有朝廷休战令，跳过
                if (!string.IsNullOrEmpty(defFactionId) && context.Relations.HasTruce(faction.FactionId, defFactionId))
                {
                    continue;
                }

                if (neighborProv.Garrison < minTargetGarrison)
                {
                    minTargetGarrison = neighborProv.Garrison;
                    bestTargetProvince = neighborId;
                    targetFactionId = defFactionId;
                }
            }
        }

        if (string.IsNullOrEmpty(bestTargetProvince))
        {
            return new WarlordDecision(WarlordActionType.Idle, string.Empty, string.Empty, 0, 0, "四邻无虚隙可乘，休养生息");
        }

        // 2. 基于纯数学公式计算进攻概率
        int hostility = string.IsNullOrEmpty(targetFactionId) ? 0 : context.Relations.GetHostility(faction.FactionId, targetFactionId);
        int attackProbability = WarlordAggressionFormula.CalculateAttackProbability(
            faction,
            minTargetGarrison,
            hostility,
            context.EmperorImperialPower,
            context.WestGardenArmySize,
            hasTruce: false);

        int roll = _random.NextPercentage();

        // 3. 命中出征概率：誓师出兵兼并
        if (roll < attackProbability)
        {
            int troopsToCommit = (int)(faction.TotalTroops * 0.75);
            return new WarlordDecision(
                WarlordActionType.LaunchCampaign,
                targetFactionId,
                bestTargetProvince,
                troopsToCommit,
                0,
                $"探知{bestTargetProvince}守备虚弱，兴兵兼并以扩地盘！");
        }

        return new WarlordDecision(WarlordActionType.Idle, string.Empty, string.Empty, 0, 0, "秣马厉兵，观望中原大势");
    }
}
