using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 196 年 8 月国家级屯田制倡议与洛阳太仓丰稔因果（单一职责）
/// </summary>
public sealed class TuntianSystemEvaluator : ITuntianSystemEvaluator
{
    public TuntianSystemResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool hasInitialTreasury = state.Treasury >= 1000;

        // 1. 天子皇权稳固 (>=50) 且国库有启动资金 (>=1000)：全面推行国家级屯田制（官屯+民屯），太仓丰稔
        if (strongCourt && hasInitialTreasury)
        {
            return new TuntianSystemResult(
                TuntianSystemOutcome.NationalTuntianProsperity,
                "【太仓丰稔 · 屯田大成】天子下诏设立典农中郎将！国家级屯田制大获成功！",
                "【农桑】建安元年秋，天子采纳枣祗、韩浩之策，于洛阳京畿、三辅沃野推行国家级屯田制，设典农中郎将招募流民，官给牛种，计口分田。是岁洛阳大稔，太仓粟积盈亿，得谷百万斛！西园军粮充足，百姓安居乐业，民心皇权大振！",
                ImperialPowerDelta: 10,
                PopularSupportDelta: 20,
                TreasuryGoldDelta: 4000,
                WestGardenArmyMoraleBonus: 20,
                CaoCaoLoyaltyDelta: 15);
        }

        // 2. 朝廷中平或资金不足：准许曹操等地方诸侯推行屯田，朝廷坐收漕粮岁贡
        if (state.ImperialPower >= 35)
        {
            return new TuntianSystemResult(
                TuntianSystemOutcome.LocalTuntianAppeasement,
                "【中原垦荒 · 诸侯劝农】朝廷诏准关东推行屯田！中原流民渐得安顿！",
                "【劝农】朝廷颁布《劝农诏》，准许兖州曹操等因地制宜兴修水利、招抚流民开荒屯田。中原农业渐复生机，曹操上表进奉屯田新粮，国库增收，民心稍安。",
                ImperialPowerDelta: 5,
                PopularSupportDelta: 10,
                TreasuryGoldDelta: 2000,
                WestGardenArmyMoraleBonus: 10,
                CaoCaoLoyaltyDelta: 20);
        }

        // 3. 皇权微弱且守旧阻挠：屯田倡议搁置
        return new TuntianSystemResult(
            TuntianSystemOutcome.TuntianStagnation,
            "【朝议不决 · 农政滞碍】守旧公卿阻挠农政！屯田制未能推行！",
            "【滞碍】朝廷公卿以祖宗成法为由争论不休，屯田之议留中不发，荒田未辟，军民仍苦于粮饷匮乏。",
            ImperialPowerDelta: -5,
            PopularSupportDelta: -5,
            TreasuryGoldDelta: 0,
            WestGardenArmyMoraleBonus: 0,
            CaoCaoLoyaltyDelta: -5);
    }
}
