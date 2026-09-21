using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 208 年 11 月赤壁之战因果结算（单一职责）
/// </summary>
public sealed class ChibiBattleExecutor : IChibiBattleExecutor
{
    public TurnResult Execute(GameState state, ChibiBattleResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库收入与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 孙权登庸与好感结算
        if (!state.Npcs.ContainsKey("sun_quan"))
        {
            state.RegisterNpc(new NpcState
            {
                Id = "sun_quan",
                Name = "孙权",
                Title = "吴侯",
                TitleTier = 2,
                Favorability = 60,
                Power = 70,
                Corruption = 10,
                StashedWealth = 500,
                BirthYear = 182,
                BaseLongevity = 71,
                Personality = "沉稳",
                Style = "任贤使能",
                Faction = "军阀派",
                Martial = 70,
                Leadership = 88,
                Politics = 90,
                Charisma = 90,
                Ambition = 85,
                InitialLocation = "扬州建业",
                HistoricalRole = "江东之主，孙权字仲谋，坐断东南"
            });
        }

        if (state.Npcs.TryGetValue("sun_quan", out var sunQuan))
            sunQuan.AdjustFavorability(result.SunQuanLoyaltyDelta);

        // 3. 曹操与刘备权势好感结算
        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
        {
            caoCao.AdjustPower(result.CaoCaoPowerDelta);
            caoCao.AdjustFavorability(result.CaoCaoLoyaltyDelta);
        }

        if (state.Npcs.TryGetValue("liu_bei", out var liuBei))
        {
            liuBei.AdjustPower(result.LiuBeiPowerDelta);
            liuBei.AdjustFavorability(result.LiuBeiLoyaltyDelta);
        }

        // 4. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
