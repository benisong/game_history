using System;
using DonghanEngine.Core.Geopolitics.Contracts;
using DonghanEngine.Core.Geopolitics.Models;

namespace DonghanEngine.Core.Geopolitics.Strategies;

/// <summary>
/// 自守宗室策略（刘表、刘璋、陶谦）：保境安民，加固城防，不主动惹事
/// </summary>
public sealed class CautiousAutonomistStrategy : IWarlordDecisionEvaluator
{
    public WarlordDecision EvaluateDecision(WarlordFaction faction, GeopoliticalContext context)
    {
        // 若军粮充裕且忠诚较高，亦纳贡保平安
        if (faction.Provisions >= 3000 && faction.ImperialLoyalty >= 50)
        {
            return new WarlordDecision(
                WarlordActionType.Tribute,
                string.Empty,
                string.Empty,
                0,
                300,
                "遵制纳贡，遣使向洛阳进奉方物以表忠忱");
        }

        return new WarlordDecision(WarlordActionType.Idle, string.Empty, string.Empty, 0, 0, "修明内政，保境息民");
    }
}
