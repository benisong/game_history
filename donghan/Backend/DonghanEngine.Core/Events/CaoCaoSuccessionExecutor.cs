using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 220 年 1 月曹操病逝与曹丕嗣位因果结算（单一职责）
/// </summary>
public sealed class CaoCaoSuccessionExecutor : ICaoCaoSuccessionExecutor
{
    public TurnResult Execute(GameState state, CaoCaoSuccessionResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库收入与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 曹操注销（病逝）
        if (state.Npcs.TryGetValue("cao_cao", out var caoCao) && result.CaoCaoDied)
        {
            caoCao.IsActive = false;
            caoCao.DeathReason = "【病逝洛阳】建安二十五年正月，魏王曹操在洛阳官邸病殁，寿六十六。";
            caoCao.Power = 0;
        }

        // 3. 曹丕登庸与好感/权势结算
        if (!state.Npcs.ContainsKey("cao_pi"))
        {
            state.RegisterNpc(new NpcState
            {
                Id = "cao_pi",
                Name = "曹丕",
                Title = "魏王",
                TitleTier = 1,
                Favorability = 60,
                Power = 75,
                Corruption = 10,
                StashedWealth = 1000,
                BirthYear = 187,
                BaseLongevity = 40,
                Personality = "深沉",
                Style = "文武兼备",
                Faction = "军阀派",
                Martial = 72,
                Leadership = 86,
                Politics = 92,
                Charisma = 88,
                Ambition = 95,
                InitialLocation = "冀州邺城",
                HistoricalRole = "曹操次子/世子，嗣位魏王，文武兼资"
            });
        }

        if (state.Npcs.TryGetValue("cao_pi", out var caoPi))
        {
            caoPi.AdjustPower(result.CaoPiPowerDelta);
            caoPi.AdjustFavorability(result.CaoPiLoyaltyDelta);
        }

        // 4. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
