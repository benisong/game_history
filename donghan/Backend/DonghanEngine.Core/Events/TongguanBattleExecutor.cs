using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 211 年 3 月潼关之战因果结算（单一职责）
/// </summary>
public sealed class TongguanBattleExecutor : ITongguanBattleExecutor
{
    public TurnResult Execute(GameState state, TongguanBattleResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库收入与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 马超 NPC 登庸与好感结算
        if (!state.Npcs.ContainsKey("ma_chao"))
        {
            state.RegisterNpc(new NpcState
            {
                Id = "ma_chao",
                Name = "马超",
                Title = "偏将军",
                TitleTier = 3,
                Favorability = 50,
                Power = 75,
                Corruption = 20,
                StashedWealth = 300,
                BirthYear = 176,
                BaseLongevity = 47,
                Personality = "勇武",
                Style = "羌胡归心",
                Faction = "军阀派",
                Martial = 97,
                Leadership = 88,
                Politics = 40,
                Charisma = 82,
                Ambition = 85,
                InitialLocation = "凉州武威",
                HistoricalRole = "锦马超，神勇冠绝西州，羌胡畏服"
            });
        }

        if (state.Npcs.TryGetValue("ma_chao", out var maChao))
            maChao.AdjustFavorability(result.MaChaoLoyaltyDelta);

        // 3. 曹操权势与好感结算
        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
        {
            caoCao.AdjustPower(result.CaoCaoPowerDelta);
            caoCao.AdjustFavorability(result.CaoCaoLoyaltyDelta);
        }

        // 4. 凉州太守与守军更新
        if (state.Provinces.TryGetValue("liangzhou", out var liangzhou))
        {
            liangzhou.GovernorId = result.LiangzhouGovernorId;
            liangzhou.AdjustGarrison(3000);
        }

        // 5. 司隶三辅防御加固
        if (state.Provinces.TryGetValue("sili", out var sili))
            sili.AdjustDefenseLevel(10);

        // 6. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
