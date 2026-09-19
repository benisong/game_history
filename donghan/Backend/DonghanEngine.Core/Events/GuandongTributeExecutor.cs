using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 190 年 1 月关东诸侯纳贡因果结算（单一职责）
/// </summary>
public sealed class GuandongTributeExecutor : IGuandongTributeExecutor
{
    public TurnResult Execute(GameState state, GuandongTributeResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库收入与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 将领/诸侯好感度与忠诚度结算
        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
        {
            caoCao.AdjustFavorability(result.CaoCaoLoyaltyDelta);
        }

        if (state.Npcs.TryGetValue("yuan_shao", out var yuanShao))
        {
            yuanShao.AdjustFavorability(result.YuanShaoLoyaltyDelta);
        }

        // 3. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
