using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行西园八校尉建军后的军力扩充、将领权势升降与起居注（单一职责）
/// </summary>
public sealed class WestGardenEstablishmentExecutor : IWestGardenEstablishmentExecutor
{
    public TurnResult Execute(GameState state, WestGardenEstablishmentResult result)
    {
        var turnResult = new TurnResult();

        // 1. 扣除私库置军费用
        if (result.PrivateTreasuryCost > 0)
        {
            state.PrivateTreasury = Math.Clamp(state.PrivateTreasury - result.PrivateTreasuryCost, 0, 999999);
        }

        // 2. 禁军扩充与士气忠诚提升
        if (state.WestGardenArmy != null)
        {
            if (result.ArmySizeBonus > 0)
            {
                state.WestGardenArmy.AdjustSize(result.ArmySizeBonus);
            }
            state.WestGardenArmy.AdjustMorale(result.ArmyMoraleBonus);
            state.WestGardenArmy.AdjustLoyalty(result.ArmyLoyaltyBonus);
        }

        // 3. 将领权势调整
        if (state.Npcs.TryGetValue("he_jin", out var heJin))
        {
            heJin.AdjustPower(result.HeJinPowerDelta);
        }

        if (state.Npcs.TryGetValue("jian_shuo", out var jianShuo))
        {
            jianShuo.AdjustPower(result.JianShuoPowerDelta);
        }

        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
        {
            caoCao.AdjustPower(result.CaoCaoPowerDelta);
        }

        if (state.Npcs.TryGetValue("yuan_shao", out var yuanShao))
        {
            yuanShao.AdjustPower(result.YuanShaoPowerDelta);
        }

        // 4. 天子皇权变更
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 5. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
