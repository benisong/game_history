using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 219 年 10 月襄樊之战因果结算（单一职责）
/// </summary>
public sealed class XiangfanBattleExecutor : IXiangfanBattleExecutor
{
    public TurnResult Execute(GameState state, XiangfanBattleResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库收入与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 刘备、曹操、孙权权势好感结算
        if (state.Npcs.TryGetValue("liu_bei", out var liuBei))
        {
            liuBei.AdjustPower(result.LiuBeiPowerDelta);
            liuBei.AdjustFavorability(result.LiuBeiLoyaltyDelta);
        }

        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
        {
            caoCao.AdjustPower(result.CaoCaoPowerDelta);
        }

        if (state.Npcs.TryGetValue("sun_quan", out var sunQuan))
        {
            sunQuan.AdjustPower(result.SunQuanPowerDelta);
            sunQuan.AdjustFavorability(result.SunQuanLoyaltyDelta);
        }

        // 3. 荆州归属与防务
        if (state.Provinces.TryGetValue("jingzhou", out var jingzhou))
        {
            if (!result.GuanYuSaved && result.Outcome == XiangfanBattleOutcome.HistoricalLvmengCrossesYangtze)
            {
                jingzhou.GovernorId = "sun_quan"; // 孙权偷袭夺荆州
            }
            else
            {
                jingzhou.GovernorId = "liu_bei"; // 关羽保全江陵
                jingzhou.AdjustDefenseLevel(10);
            }
        }

        // 4. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
