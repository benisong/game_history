using System;
using System.Collections.Generic;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：依据评估结果执行黄巾爆发的州郡状态修改与起居注记录（单一职责）
/// </summary>
public sealed class YellowTurbanOutbreakExecutor : IYellowTurbanOutbreakExecutor
{
    public TurnResult ExecuteOutbreak(GameState state, YellowTurbanScopeResult scope)
    {
        var result = new TurnResult();

        // 1. 修改波及州郡的充血状态
        foreach (var pid in scope.AffectedProvinceIds)
        {
            if (!state.Provinces.TryGetValue(pid, out var p) || p == null) continue;

            // 若太守在任且该州失陷，太守撤职还朝（或失陷下野）
            if (p.GovernorId != null && state.Npcs.TryGetValue(p.GovernorId, out var gov))
            {
                gov.RevokeGovernor();
            }
            p.RecallGovernor();

            // 根据规模决定初始守军倍率
            int garrisonMultiplier = scope.Intensity switch
            {
                YellowTurbanIntensity.Catastrophic => 3,
                YellowTurbanIntensity.Historical => 2,
                _ => 2
            };

            // 调用 Province 充血方法
            p.StartRebellion("黄巾军", initialLocalSupport: 5, garrisonMultiplier: garrisonMultiplier);
        }

        // 2. 扣除天子民心
        state.PopularSupport = Math.Clamp(state.PopularSupport - scope.PopularSupportDrop, 0, 100);

        // 3. 记录编年史
        state.AddToChronicle(scope.ChronicleText);

        result.StoryText = $"{scope.NarrativeTitle}\n\n{scope.ChronicleText}\n波及州郡：{string.Join("、", scope.AffectedProvinceIds)}。";
        return result;
    }
}
