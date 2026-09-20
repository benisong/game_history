using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 196 年 8 月屯田制因果结算（单一职责）
/// </summary>
public sealed class TuntianSystemExecutor : ITuntianSystemExecutor
{
    public TurnResult Execute(GameState state, TuntianSystemResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库收入与皇权、民心结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);
        state.PopularSupport = Math.Clamp(state.PopularSupport + result.PopularSupportDelta, 0, 100);

        // 2. 西园禁军士气增益
        if (state.WestGardenArmy != null && result.WestGardenArmyMoraleBonus > 0)
        {
            state.WestGardenArmy.AdjustMorale(result.WestGardenArmyMoraleBonus);
        }

        // 3. 曹操好感结算
        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
        {
            caoCao.AdjustFavorability(result.CaoCaoLoyaltyDelta);
        }

        // 4. 司隶京畿富庶与民心繁荣提升
        if (state.Provinces.TryGetValue("sili", out var sili))
        {
            sili.AdjustWealth(1500);
            sili.AdjustLocalSupport(10);
        }

        // 5. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
