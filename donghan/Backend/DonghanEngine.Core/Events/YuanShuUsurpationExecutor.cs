using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 197 年 1 月袁术僭号因果结算（单一职责）
/// </summary>
public sealed class YuanShuUsurpationExecutor : IYuanShuUsurpationExecutor
{
    public TurnResult Execute(GameState state, YuanShuUsurpationResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库收入与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 诸侯好感/忠诚度普调（天威浩荡）
        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
            caoCao.AdjustFavorability(result.CaoCaoLoyaltyDelta);

        if (state.Npcs.TryGetValue("liu_bei", out var liuBei))
            liuBei.AdjustFavorability(result.LiuBeiLoyaltyDelta);

        if (state.Npcs.TryGetValue("sun_ce", out var sunCe))
            sunCe.AdjustFavorability(result.SunCeLoyaltyDelta);

        if (state.Npcs.TryGetValue("lv_bu", out var lvBu))
            lvBu.AdjustFavorability(result.LvBuLoyaltyDelta);

        if (state.Npcs.TryGetValue("yuan_shu", out var yuanShu))
        {
            yuanShu.AdjustPower(result.YuanShuPowerDelta);
            if (result.Outcome != YuanShuUsurpationOutcome.YuanShuHoldsHuainan)
            {
                yuanShu.IsActive = false;
                yuanShu.DeathReason = "【僭号败亡】建安二年，袁术僭帝号，为四海诸侯奉天子诏合围诛灭。";
                yuanShu.Power = 0;
                state.IsYuanShuEliminatedEarly = true;
            }
            else
            {
                state.IsYuanShuEliminatedEarly = false;
            }
        }

        // 3. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
