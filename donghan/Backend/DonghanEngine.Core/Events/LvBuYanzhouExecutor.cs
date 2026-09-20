using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 194 年 5 月吕布袭兖州与中原蝗灾赈济因果结算（单一职责）
/// </summary>
public sealed class LvBuYanzhouExecutor : ILvBuYanzhouExecutor
{
    public TurnResult Execute(GameState state, LvBuYanzhouResult result)
    {
        var turnResult = new TurnResult();

        // 1. 扣除赈济军粮/国库开销，结算民心与皇权
        if (result.TreasuryFoodCost > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury - result.TreasuryFoodCost, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);
        state.PopularSupport = Math.Clamp(state.PopularSupport + result.PopularSupportDelta, 0, 100);

        // 2. 吕布 NPC 登庸与好感/权势结算
        if (!state.Npcs.ContainsKey("lv_bu"))
        {
            state.RegisterNpc(new NpcState
            {
                Id = "lv_bu",
                Name = "吕布",
                Title = "平东将军",
                TitleTier = 3,
                Favorability = 40,
                Power = 70,
                Corruption = 60,
                StashedWealth = 500,
                BirthYear = 156,
                BaseLongevity = 43,
                Personality = "暴躁",
                Style = "唯利是图",
                Faction = "军阀派",
                Martial = 100,
                Leadership = 82,
                Politics = 20,
                Charisma = 50,
                Ambition = 95,
                InitialLocation = "兖州濮阳",
                HistoricalRole = "飞将吕布，弓马冠绝天下，桀骜难驯"
            });
        }

        if (state.Npcs.TryGetValue("lv_bu", out var lvBu))
        {
            lvBu.AdjustPower(result.LvBuPowerDelta);
            lvBu.AdjustFavorability(result.LvBuLoyaltyDelta);
        }

        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
        {
            caoCao.AdjustPower(result.CaoCaoPowerDelta);
            caoCao.AdjustFavorability(result.CaoCaoLoyaltyDelta);
        }

        // 3. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
