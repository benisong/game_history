using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 222 年 6 月夷陵之战因果结算（单一职责）
/// </summary>
public sealed class YilingBattleExecutor : IYilingBattleExecutor
{
    public TurnResult Execute(GameState state, YilingBattleResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库收入与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 刘备与孙权权势好感结算
        if (state.Npcs.TryGetValue("liu_bei", out var liuBei))
        {
            liuBei.AdjustPower(result.LiuBeiPowerDelta);
            liuBei.AdjustFavorability(result.LiuBeiLoyaltyDelta);
        }

        if (state.Npcs.TryGetValue("sun_quan", out var sunQuan))
        {
            sunQuan.AdjustPower(result.SunQuanPowerDelta);
            sunQuan.AdjustFavorability(result.SunQuanLoyaltyDelta);
        }

        // 3. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
