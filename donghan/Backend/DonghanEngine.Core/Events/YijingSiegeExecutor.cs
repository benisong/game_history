using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 199 年 3 月公孙瓒易京覆灭与袁绍一统河北因果结算（单一职责）
/// </summary>
public sealed class YijingSiegeExecutor : IYijingSiegeExecutor
{
    public TurnResult Execute(GameState state, YijingSiegeResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库收入与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 西园禁军骑兵扩充
        if (state.WestGardenArmy != null && result.WestGardenTroopBonus > 0)
        {
            state.WestGardenArmy.AdjustSize(result.WestGardenTroopBonus);
            state.WestGardenArmy.AdjustMorale(15);
        }

        // 3. 袁绍与曹操权势与好感结算
        if (state.Npcs.TryGetValue("yuan_shao", out var yuanShao))
        {
            yuanShao.Title = "大将军";
            yuanShao.AdjustPower(result.YuanShaoPowerDelta);
            yuanShao.AdjustFavorability(result.YuanShaoLoyaltyDelta);
        }

        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
        {
            caoCao.Title = "司空";
            caoCao.AdjustPower(result.CaoCaoPowerDelta);
            caoCao.AdjustFavorability(result.CaoCaoLoyaltyDelta);
        }

        // 4. 幽州太守移交与守军更新
        if (state.Provinces.TryGetValue("youzhou", out var youzhou))
        {
            youzhou.GovernorId = result.YouzhouGovernorId;
            youzhou.AdjustGarrison(4000);
        }

        // 5. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
