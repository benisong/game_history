using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 193 年 9 月曹操征徐州与刘备入主徐州因果结算（单一职责）
/// </summary>
public sealed class XuzhouSuccessionExecutor : IXuzhouSuccessionExecutor
{
    public TurnResult Execute(GameState state, XuzhouSuccessionResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 刘备 NPC 登庸与好感/权势结算
        if (!state.Npcs.ContainsKey("liu_bei"))
        {
            state.RegisterNpc(new NpcState
            {
                Id = "liu_bei",
                Name = "刘备",
                Title = "徐州牧",
                TitleTier = 3,
                Favorability = 70,
                Power = 50,
                Corruption = 10,
                StashedWealth = 200,
                BirthYear = 161,
                BaseLongevity = 62,
                Personality = "仁德",
                Style = "宽厚爱民",
                Faction = "宗室派",
                Martial = 75,
                Leadership = 80,
                Politics = 78,
                Charisma = 99,
                Ambition = 75,
                InitialLocation = "徐州下邳",
                HistoricalRole = "汉室宗亲，徐州牧，仁德布于四海"
            });
        }

        if (state.Npcs.TryGetValue("liu_bei", out var liuBei))
        {
            liuBei.AdjustPower(result.LiuBeiPowerDelta);
            liuBei.AdjustFavorability(result.LiuBeiLoyaltyDelta);
        }

        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
        {
            caoCao.AdjustPower(result.CaoCaoPowerDelta);
            caoCao.AdjustFavorability(result.CaoCaoLoyaltyDelta);
        }

        // 3. 徐州领地太守移交与守军更新
        if (state.Provinces.TryGetValue("xuzhou", out var xuzhou))
        {
            xuzhou.GovernorId = result.XuzhouGovernorId;
            xuzhou.AdjustGarrison(3000);
        }

        // 4. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
