using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 215 年 11 月合肥之战因果结算（单一职责）
/// </summary>
public sealed class HefeiBattleExecutor : IHefeiBattleExecutor
{
    public TurnResult Execute(GameState state, HefeiBattleResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库收入与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 曹操与孙权权势好感结算
        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
        {
            caoCao.AdjustPower(result.CaoCaoPowerDelta);
            caoCao.AdjustFavorability(result.CaoCaoLoyaltyDelta);
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
