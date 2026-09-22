using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 226 年 8 月灵帝延寿七旬大寿与大汉中兴盛世大圆满因果（终极大事件·单一职责）
/// </summary>
public sealed class GrandJubileeEvaluator : IGrandJubileeEvaluator
{
    public GrandJubileeResult Evaluate(GameState state)
    {
        bool superStrongCourt = state.ImperialPower >= 50 && (state.WestGardenArmy?.Size ?? 0) >= 8000;
        bool moderateCourt = state.ImperialPower >= 35;

        // 1. 大汉中兴·七旬大圆满巅峰：皇权强盛 (>=50) 且禁军规模宏大 (>=8000)
        // 魏王曹丕、蜀相诸葛亮、吴侯孙权齐聚洛阳九龙殿朝觐九宾之礼，天下大一统尊奉汉室正朔，皇权达成100巅峰
        if (superStrongCourt)
        {
            return new GrandJubileeResult(
                GrandJubileeOutcome.GreatRestorationGrandJubilee,
                "【大汉中兴 · 七旬天子万年】公元226年灵帝七十古稀大寿！天下诸侯九宾来朝！",
                "【大圆满】延熹延祚，光武再临！建安三十一年秋八月，大汉天子寿登七旬！自中平改元延寿至今四十余载，天子运筹帷幄、平黄巾、慑凉州、整西园、置常平、制群雄、分陕鼎足、安西川、平南中！是日，洛阳南宫九龙殿张九宾之礼，魏王曹丕、蜀相诸葛亮、吴侯孙权亲率天下十三州刺史、二百郡太守及四夷朝贡使节齐聚阙下，顿首奉万岁觞！太仓粟积盈亿，西园禁军万骑肃穆，九州同文，天下四海皆奉汉朔！大汉中兴大业，垂范千秋！",
                ImperialPowerDelta: 30,
                TreasuryGoldDelta: 5000,
                CaoPiLoyaltyDelta: 30,
                ZhugeLiangLoyaltyDelta: 30,
                SunQuanLoyaltyDelta: 30,
                WestGardenMoraleBonus: 30,
                GrandRestorationAchieved: true);
        }

        // 2. 朝廷中平 (>=35)：诸侯奉朔·盛世同庆
        if (moderateCourt)
        {
            return new GrandJubileeResult(
                GrandJubileeOutcome.ProsperousAutonomousTributes,
                "【古稀之庆 · 诸侯归心】天子七旬寿诞大典！天下十三州各献重赋！",
                "【盛典】建安三十一年秋，天子寿登七十。虽天下三分，然群雄敬畏天子威严与正统大义，曹丕、诸葛亮、孙权皆遣使入洛阳贡进万乘珍宝与岁贡两千万钱，天子坐镇中枢，四海承平。",
                ImperialPowerDelta: 15,
                TreasuryGoldDelta: 2000,
                CaoPiLoyaltyDelta: 15,
                ZhugeLiangLoyaltyDelta: 15,
                SunQuanLoyaltyDelta: 15,
                WestGardenMoraleBonus: 15,
                GrandRestorationAchieved: false);
        }

        // 3. 皇权微弱 (<35)：藩镇异心
        return new GrandJubileeResult(
            GrandJubileeOutcome.FactionalFissuresAtJubilee,
            "【古稀暮年 · 藩镇异心】天子七旬大寿！关东诸侯应付差事！",
            "【暮年】天子虽享古稀高寿，然朝廷号令渐弛，关东诸侯各怀异心，仅遣微臣虚应故事，大汉天下暗流涌动。",
            ImperialPowerDelta: 0,
            TreasuryGoldDelta: 0,
            CaoPiLoyaltyDelta: 0,
            ZhugeLiangLoyaltyDelta: 0,
            SunQuanLoyaltyDelta: 0,
            WestGardenMoraleBonus: 0,
            GrandRestorationAchieved: false);
    }
}
