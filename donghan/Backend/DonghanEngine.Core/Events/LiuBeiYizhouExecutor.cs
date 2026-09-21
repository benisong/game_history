using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 214 年 5 月刘备入蜀因果结算（单一职责）
/// </summary>
public sealed class LiuBeiYizhouExecutor : ILiuBeiYizhouExecutor
{
    public TurnResult Execute(GameState state, LiuBeiYizhouResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库收入与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 刘备权势与好感结算
        if (state.Npcs.TryGetValue("liu_bei", out var liuBei))
        {
            liuBei.AdjustPower(result.LiuBeiPowerDelta);
            liuBei.AdjustFavorability(result.LiuBeiLoyaltyDelta);
            liuBei.InitialLocation = "益州成都";
        }

        // 3. 曹操权势结算
        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
        {
            caoCao.AdjustPower(result.CaoCaoPowerDelta);
        }

        // 4. 益州太守与守军更新
        if (state.Provinces.TryGetValue("yizhou", out var yizhou))
        {
            yizhou.GovernorId = result.YizhouGovernorId;
            yizhou.AdjustGarrison(4000);
            yizhou.AdjustWealth(2000); // 天府之国富庶充实
        }

        // 5. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
