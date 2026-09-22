using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 226 年 8 月灵帝七旬大寿与大汉中兴大圆满因果结算（单一职责）
/// </summary>
public sealed class GrandJubileeExecutor : IGrandJubileeExecutor
{
    public TurnResult Execute(GameState state, GrandJubileeResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库收入与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 西园禁军士气飙升至巅峰
        if (state.WestGardenArmy != null)
        {
            state.WestGardenArmy.AdjustMorale(result.WestGardenMoraleBonus);
        }

        // 3. 魏蜀吴三大执政与天下臣工忠诚度提升至巅峰
        if (state.Npcs.TryGetValue("cao_pi", out var caoPi))
            caoPi.AdjustFavorability(result.CaoPiLoyaltyDelta);

        if (state.Npcs.TryGetValue("zhuge_liang", out var zhugeLiang))
            zhugeLiang.AdjustFavorability(result.ZhugeLiangLoyaltyDelta);

        if (state.Npcs.TryGetValue("sun_quan", out var sunQuan))
            sunQuan.AdjustFavorability(result.SunQuanLoyaltyDelta);

        // 4. 起居注铭刻大中兴宏伟终章
        state.AddToChronicle(result.ChronicleText);

        // 5. 若达成大中兴，锁定最终盛世状态
        if (result.GrandRestorationAchieved)
        {
            state.ImperialPower = 100;
        }

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
