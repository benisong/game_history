using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 225 年 3 月诸葛亮南征因果结算（单一职责）
/// </summary>
public sealed class SouthernExpeditionExecutor : ISouthernExpeditionExecutor
{
    public TurnResult Execute(GameState state, SouthernExpeditionResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库收入与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 西园禁军骑兵扩充与士气提升
        if (state.WestGardenArmy != null)
        {
            if (result.WestGardenTroopBonus > 0)
            {
                state.WestGardenArmy.AdjustSize(result.WestGardenTroopBonus);
            }
            state.WestGardenArmy.AdjustMorale(result.WestGardenMoraleBonus);
        }

        // 3. 诸葛亮权势与好感结算
        if (state.Npcs.TryGetValue("zhuge_liang", out var zhugeLiang))
        {
            zhugeLiang.AdjustPower(result.ZhugeLiangPowerDelta);
            zhugeLiang.AdjustFavorability(result.ZhugeLiangLoyaltyDelta);
        }

        // 4. 益州边防加固
        if (state.Provinces.TryGetValue("yizhou", out var yizhou))
        {
            yizhou.AdjustDefenseLevel(10);
            yizhou.AdjustWealth(2000);
        }

        // 5. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
