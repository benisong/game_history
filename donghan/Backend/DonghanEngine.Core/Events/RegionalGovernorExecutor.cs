using System;
using System.Collections.Generic;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行废史立牧后的州牧就任、地方民心大幅提振与皇权沉降结算（单一职责）
/// </summary>
public sealed class RegionalGovernorExecutor : IRegionalGovernorExecutor
{
    public TurnResult Execute(GameState state, RegionalGovernorResult result)
    {
        var turnResult = new TurnResult();

        // 1. 登庸并指派各州牧
        if (result.Decision == RegionalGovernorDecision.AdoptStatePastorSystem)
        {
            foreach (var (provinceId, npcId) in result.ProvinceGovernorMappings)
            {
                // 若该 NPC 处于冷备池中，先登庸
                if (!state.Npcs.ContainsKey(npcId))
                {
                    if (HistoricalNpcPresets.All.Find(n => n.Id == npcId) is { } preset)
                    {
                        state.RegisterNpc(HistoricalNpcPresets.Clone(preset));
                    }
                }

                // 任命州牧并大幅提升该州民心与防御
                if (state.Provinces.TryGetValue(provinceId, out var p) &&
                    state.Npcs.TryGetValue(npcId, out var governor))
                {
                    // 若此前有太守，先撤任
                    p.RecallGovernor();
                    governor.RevokeGovernor();

                    // 就任州牧
                    p.AppointGovernor(npcId, result.RemoteProvinceSupportBonus);
                    governor.AssignGovernor(provinceId);
                    governor.AdjustPower(20); // 州牧执掌军政，权势大增
                }
            }
        }

        // 2. 皇权与民心调整
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);
        state.PopularSupport = Math.Clamp(state.PopularSupport + result.PopularSupportDelta, 0, 100);

        // 3. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
