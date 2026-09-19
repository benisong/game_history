using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 192 年 4 月曹操入主兖州因果结算（单一职责）
/// </summary>
public sealed class CaoCaoYanzhouExecutor : ICaoCaoYanzhouExecutor
{
    public TurnResult Execute(GameState state, CaoCaoYanzhouResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 西园禁军兵员扩充与士气提升
        if (state.WestGardenArmy != null)
        {
            if (result.WestGardenArmyTroopBonus > 0)
            {
                state.WestGardenArmy.AdjustSize(result.WestGardenArmyTroopBonus);
            }
            state.WestGardenArmy.AdjustMorale(result.WestGardenMoraleBonus);
        }

        // 3. 曹操权势与好感结算
        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
        {
            caoCao.AdjustPower(result.CaoCaoPowerDelta);
            caoCao.AdjustFavorability(result.CaoCaoLoyaltyDelta);
        }

        // 4. 兖州领地太守移交与守军充实
        if (state.Provinces.TryGetValue("yanzhou", out var yanzhou))
        {
            yanzhou.GovernorId = "cao_cao";
            yanzhou.AdjustGarrison(5000); // 青州军留守兖州
        }

        // 5. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
