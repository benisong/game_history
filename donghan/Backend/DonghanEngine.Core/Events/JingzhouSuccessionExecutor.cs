using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 208 年 7 月刘表病亡与荆州易主因果结算（单一职责）
/// </summary>
public sealed class JingzhouSuccessionExecutor : IJingzhouSuccessionExecutor
{
    public TurnResult Execute(GameState state, JingzhouSuccessionResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库收入与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 刘表注销（病亡）
        if (state.Npcs.TryGetValue("liu_biao", out var liuBiao))
        {
            liuBiao.IsActive = false;
            liuBiao.DeathReason = "【病卒江陵】建安十三年七月，荆州牧刘表在江陵病逝。";
            liuBiao.Power = 0;
        }

        // 3. 曹操与刘备权势好感结算
        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
        {
            caoCao.AdjustPower(result.CaoCaoPowerDelta);
            caoCao.AdjustFavorability(result.CaoCaoLoyaltyDelta);
        }

        if (state.Npcs.TryGetValue("liu_bei", out var liuBei))
        {
            liuBei.AdjustPower(result.LiuBeiPowerDelta);
            liuBei.AdjustFavorability(result.LiuBeiLoyaltyDelta);
        }

        // 4. 荆州太守与守军更新
        if (state.Provinces.TryGetValue("jingzhou", out var jingzhou))
        {
            jingzhou.GovernorId = result.JingzhouGovernorId;
            jingzhou.AdjustGarrison(3000);
        }

        // 5. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
