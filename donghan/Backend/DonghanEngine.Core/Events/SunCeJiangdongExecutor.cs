using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 195 年 2 月孙策平定江东因果结算（单一职责）
/// </summary>
public sealed class SunCeJiangdongExecutor : ISunCeJiangdongExecutor
{
    public TurnResult Execute(GameState state, SunCeJiangdongResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 孙策 NPC 登庸与权势/好感结算
        if (!state.Npcs.ContainsKey("sun_ce"))
        {
            state.RegisterNpc(new NpcState
            {
                Id = "sun_ce",
                Name = "孙策",
                Title = "折冲校尉",
                TitleTier = 3,
                Favorability = 60,
                Power = 75,
                Corruption = 10,
                StashedWealth = 300,
                BirthYear = 175,
                BaseLongevity = 26,
                Personality = "勇烈",
                Style = "豪迈善断",
                Faction = "军阀派",
                Martial = 95,
                Leadership = 92,
                Politics = 70,
                Charisma = 92,
                Ambition = 90,
                InitialLocation = "扬州吴郡",
                HistoricalRole = "小霸王孙策，勇冠三军，开创江东基业"
            });
        }

        if (state.Npcs.TryGetValue("sun_ce", out var sunCe))
        {
            sunCe.AdjustPower(result.SunCePowerDelta);
            sunCe.AdjustFavorability(result.SunCeLoyaltyDelta);
        }

        // 3. 扬州领地太守移交与守军更新
        if (state.Provinces.TryGetValue("yangzhou", out var yangzhou))
        {
            yangzhou.GovernorId = result.YangzhouGovernorId;
            yangzhou.AdjustGarrison(4000);
        }

        // 4. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
