using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 200 年 2 月官渡前夕因果结算（单一职责）
/// </summary>
public sealed class GuanduPreludeExecutor : IGuanduPreludeExecutor
{
    public TurnResult Execute(GameState state, GuanduPreludeResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库收入与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 西园禁军战马骑兵扩充与士气提升
        if (state.WestGardenArmy != null)
        {
            if (result.WestGardenTroopBonus > 0)
            {
                state.WestGardenArmy.AdjustSize(result.WestGardenTroopBonus);
            }
            state.WestGardenArmy.AdjustMorale(result.WestGardenMoraleBonus);
        }

        // 3. 曹操与袁绍好感/忠诚结算
        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
            caoCao.AdjustFavorability(result.CaoCaoLoyaltyDelta);

        if (state.Npcs.TryGetValue("yuan_shao", out var yuanShao))
            yuanShao.AdjustFavorability(result.YuanShaoLoyaltyDelta);

        // 4. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
