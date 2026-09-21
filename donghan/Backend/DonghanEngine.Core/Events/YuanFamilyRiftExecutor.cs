using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 202 年 5 月袁绍病亡与袁氏争立因果结算（单一职责）
/// </summary>
public sealed class YuanFamilyRiftExecutor : IYuanFamilyRiftExecutor
{
    public TurnResult Execute(GameState state, YuanFamilyRiftResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库收入与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 袁绍注销（病亡）
        if (state.Npcs.TryGetValue("yuan_shao", out var yuanShao) && result.YuanShaoDied)
        {
            yuanShao.IsActive = false;
            yuanShao.DeathReason = "【忧愤发病亡】建安七年五月，大将军袁绍在邺城病殁。";
            yuanShao.Power = 0;
        }

        // 3. 曹操权势与好感结算
        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
        {
            caoCao.AdjustPower(result.CaoCaoPowerDelta);
            caoCao.AdjustFavorability(result.CaoCaoLoyaltyDelta);
        }

        // 4. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
