using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 223 年 4 月白帝托孤因果结算（单一职责）
/// </summary>
public sealed class BaidiEntrustmentExecutor : IBaidiEntrustmentExecutor
{
    public TurnResult Execute(GameState state, BaidiEntrustmentResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库收入与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 刘备注销（病逝）
        if (state.Npcs.TryGetValue("liu_bei", out var liuBei) && result.LiuBeiDied)
        {
            liuBei.IsActive = false;
            liuBei.DeathReason = "【病逝白帝城】建安二十八年四月，汉中王刘备在永安宫病殁，寿六十三。";
            liuBei.Power = 0;
        }

        // 3. 诸葛亮登庸与好感/权势结算
        if (!state.Npcs.ContainsKey("zhuge_liang"))
        {
            state.RegisterNpc(new NpcState
            {
                Id = "zhuge_liang",
                Name = "诸葛亮",
                Title = "武乡侯·领益州丞相",
                TitleTier = 2,
                Favorability = 70,
                Power = 70,
                Corruption = 0,
                StashedWealth = 200,
                BirthYear = 181,
                BaseLongevity = 54,
                Personality = "谨慎",
                Style = "鞠躬尽瘁",
                Faction = "宗室派",
                Martial = 60,
                Leadership = 98,
                Politics = 100,
                Charisma = 98,
                Ambition = 60,
                InitialLocation = "益州成都",
                HistoricalRole = "诸葛亮字孔明，号卧龙，蜀汉丞相，千古良相"
            });
        }

        if (state.Npcs.TryGetValue("zhuge_liang", out var zhugeLiang))
        {
            zhugeLiang.AdjustPower(result.ZhugeLiangPowerDelta);
            zhugeLiang.AdjustFavorability(result.ZhugeLiangLoyaltyDelta);
        }

        // 4. 益州太守更新为诸葛亮
        if (state.Provinces.TryGetValue("yizhou", out var yizhou))
        {
            yizhou.GovernorId = "zhuge_liang";
            yizhou.AdjustWealth(3000); // 诸葛亮治蜀，益州民富兵强
            yizhou.AdjustDefenseLevel(15);
        }

        // 5. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
