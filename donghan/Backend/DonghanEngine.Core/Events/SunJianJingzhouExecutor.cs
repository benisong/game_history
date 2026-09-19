using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 191 年 10 月孙坚征荆州因果结算（单一职责）
/// </summary>
public sealed class SunJianJingzhouExecutor : ISunJianJingzhouExecutor
{
    public TurnResult Execute(GameState state, SunJianJingzhouResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 孙坚状态与权势结算
        if (state.Npcs.TryGetValue("sun_jian", out var sunJian))
        {
            if (result.SunJianDied)
            {
                sunJian.IsActive = false;
                sunJian.DeathReason = "【岘山身亡】初平二年十月，孙坚跨江征荆州，于岘山中暗箭身亡。";
                sunJian.Power = 0;
            }
            else
            {
                sunJian.AdjustPower(result.SunJianPowerDelta);
                sunJian.AdjustFavorability(result.SunJianLoyaltyDelta);
            }
        }

        // 3. 刘表权势与好感结算
        if (state.Npcs.TryGetValue("liu_biao", out var liuBiao))
        {
            liuBiao.AdjustPower(result.LiuBiaoPowerDelta);
            liuBiao.AdjustFavorability(result.LiuBiaoLoyaltyDelta);
        }

        // 4. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
