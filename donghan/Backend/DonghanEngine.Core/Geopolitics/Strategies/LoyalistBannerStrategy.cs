using System;
using DonghanEngine.Core.Geopolitics.Contracts;
using DonghanEngine.Core.Geopolitics.Models;

namespace DonghanEngine.Core.Geopolitics.Strategies;

/// <summary>
/// 忠贞藩屏策略（皇甫嵩、卢植、早期刘备）：恪守臣节，优先向朝廷进贡，绝不主动挑起内乱
/// </summary>
public sealed class LoyalistBannerStrategy : IWarlordDecisionEvaluator
{
    public WarlordDecision EvaluateDecision(WarlordFaction faction, GeopoliticalContext context)
    {
        // 忠臣首选：上缴朝贡以实天子国库
        if (faction.Provisions >= 2000 && faction.ImperialLoyalty >= 60)
        {
            return new WarlordDecision(
                WarlordActionType.Tribute,
                string.Empty,
                string.Empty,
                0,
                500,
                "恪守臣节，按期向洛阳朝廷进奉岁贡与军粮500万钱");
        }

        return new WarlordDecision(WarlordActionType.Idle, string.Empty, string.Empty, 0, 0, "严训部曲，奉诏戒备");
    }
}
