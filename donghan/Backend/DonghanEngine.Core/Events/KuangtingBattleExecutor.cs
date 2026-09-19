using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 193 年 3 月匡亭之战因果结算（单一职责）
/// </summary>
public sealed class KuangtingBattleExecutor : IKuangtingBattleExecutor
{
    public TurnResult Execute(GameState state, KuangtingBattleResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 曹操权势与好感结算
        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
        {
            caoCao.AdjustPower(result.CaoCaoPowerDelta);
            caoCao.AdjustFavorability(result.CaoCaoLoyaltyDelta);
        }

        // 3. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
