using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 219 年 5 月汉中之战因果结算（单一职责）
/// </summary>
public sealed class HanzhongBattleExecutor : IHanzhongBattleExecutor
{
    public TurnResult Execute(GameState state, HanzhongBattleResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库收入与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 刘备进位汉中王与好感/权势结算
        if (state.Npcs.TryGetValue("liu_bei", out var liuBei))
        {
            liuBei.Title = result.LiuBeiTitle;
            liuBei.TitleTier = 1; // 王爵
            liuBei.AdjustPower(result.LiuBeiPowerDelta);
            liuBei.AdjustFavorability(result.LiuBeiLoyaltyDelta);
        }

        // 3. 曹操权势与好感结算
        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
        {
            caoCao.AdjustPower(result.CaoCaoPowerDelta);
            caoCao.AdjustFavorability(result.CaoCaoLoyaltyDelta);
        }

        // 4. 益州与司隶关隘防御提升
        if (state.Provinces.TryGetValue("yizhou", out var yizhou))
            yizhou.AdjustDefenseLevel(15); // 剑阁汉中固若金汤

        // 5. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
