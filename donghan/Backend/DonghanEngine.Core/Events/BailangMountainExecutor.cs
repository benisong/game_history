using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 207 年 8 月白狼山之战因果结算（单一职责）
/// </summary>
public sealed class BailangMountainExecutor : IBailangMountainExecutor
{
    public TurnResult Execute(GameState state, BailangMountainResult result)
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

        // 3. 曹操权势与好感结算
        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
        {
            caoCao.AdjustPower(result.CaoCaoPowerDelta);
            caoCao.AdjustFavorability(result.CaoCaoLoyaltyDelta);
        }

        // 4. 并州与幽州治安稳固提升
        if (state.Provinces.TryGetValue("bingzhou", out var bingzhou))
            bingzhou.AdjustDefenseLevel(10);

        if (state.Provinces.TryGetValue("youzhou", out var youzhou))
            youzhou.AdjustDefenseLevel(10);

        // 5. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
