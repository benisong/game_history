using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 200 年 10 月官渡决胜因果结算（单一职责）
/// </summary>
public sealed class GuanduVictoryExecutor : IGuanduVictoryExecutor
{
    public TurnResult Execute(GameState state, GuanduVictoryResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库收入与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 曹操与袁绍权势/忠诚结算
        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
        {
            caoCao.AdjustPower(result.CaoCaoPowerDelta);
            caoCao.AdjustFavorability(result.CaoCaoLoyaltyDelta);
        }

        if (state.Npcs.TryGetValue("yuan_shao", out var yuanShao))
        {
            yuanShao.AdjustPower(result.YuanShaoPowerDelta);
            yuanShao.AdjustFavorability(result.YuanShaoLoyaltyDelta);
        }

        // 3. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
