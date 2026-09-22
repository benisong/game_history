using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 223 年 4 月白帝托孤与诸葛亮治蜀（天子明诏拜武乡侯领丞相，表忠朝廷）因果（单一职责）
/// </summary>
public sealed class BaidiEntrustmentEvaluator : IBaidiEntrustmentEvaluator
{
    public BaidiEntrustmentResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool moderateCourt = state.ImperialPower >= 35;

        // 1. 天子皇权强盛 (>=50)：天子明诏优抚西蜀，册封诸葛亮为武乡侯、领益州丞相，建立朝廷直通巴蜀公文，诸葛亮表忠进贡四千万
        if (strongCourt)
        {
            return new BaidiEntrustmentResult(
                BaidiEntrustmentOutcome.ImperialAppointsZhugeLiangLoyal,
                "【白帝托孤 · 诸葛秉政】汉中王刘备病逝永安宫！天子明诏拜诸葛亮领蜀相以辅汉室！",
                "【托孤】建安二十八年夏四月，汉中王刘备在白帝城永安宫病逝，寿六十三。临终托孤于丞相诸葛亮。诸葛亮受遗诏，第一道表文直呈洛阳太庙上表誓忠。天子深感其忠贞，下明诏加封诸葛亮为武乡侯、兼领益州丞相，特赐朝廷直通巴蜀公文印信。诸葛亮伏阙谢恩，遣使进奉西蜀岁赋与蜀锦金帛四千万钱入洛阳国库，君臣相得，皇权大振！",
                ImperialPowerDelta: 15,
                TreasuryGoldDelta: 4000,
                ZhugeLiangLoyaltyDelta: 30,
                ZhugeLiangPowerDelta: 35,
                LiuBeiDied: true);
        }

        // 2. 朝廷中平 (35-49)：诸葛亮受托孤治蜀，上表洛阳请封，进贡两千万
        if (moderateCourt)
        {
            return new BaidiEntrustmentResult(
                BaidiEntrustmentOutcome.ZhugeLiangGovernsAutonomously,
                "【鞠躬尽瘁 · 蜀相秉政】刘备病殁白帝城！诸葛亮受托孤主政西蜀！",
                "【主政】刘备病逝，诸葛亮秉政治蜀，抚百姓、示仪轨。诸葛亮遣使入洛阳进贡两千万钱表奏受封，朝廷追认其位，西蜀政局平稳。",
                ImperialPowerDelta: 5,
                TreasuryGoldDelta: 2000,
                ZhugeLiangLoyaltyDelta: 20,
                ZhugeLiangPowerDelta: 30,
                LiuBeiDied: true);
        }

        // 3. 皇权微弱 (<35)：蜀汉内耗失序
        return new BaidiEntrustmentResult(
            BaidiEntrustmentOutcome.ShuHanFactionalChaos,
            "【白帝星落 · 南中蠢动】刘备病逝蜀中动荡！朝廷号令难入剑阁！",
            "【动荡】刘备病故，南中雍闿等借机作乱，蜀汉内外动荡，朝廷诏命无力加涉，西南政局混沌！",
            ImperialPowerDelta: -10,
            TreasuryGoldDelta: 0,
            ZhugeLiangLoyaltyDelta: 0,
            ZhugeLiangPowerDelta: 15,
            LiuBeiDied: true);
    }
}
