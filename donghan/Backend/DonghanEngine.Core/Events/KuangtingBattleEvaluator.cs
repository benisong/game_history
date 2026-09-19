using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 193 年 3 月匡亭之战（曹操破袁术，天子借力打力借刀杀人）因果（单一职责）
/// </summary>
public sealed class KuangtingBattleEvaluator : IKuangtingBattleEvaluator
{
    public KuangtingBattleResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool caoCaoLoyal = state.Npcs.TryGetValue("cao_cao", out var cc) && cc.IsActive && cc.Favorability >= 50;

        // 1. 天子皇权强盛 (>=50) 且 曹操忠诚 (>=50)：天子下明诏敕令曹操代天巡抚讨逆，曹操匡亭大捷，袁术败走淮南
        if (strongCourt && caoCaoLoyal)
        {
            return new KuangtingBattleResult(
                KuangtingBattleOutcome.ImperialAuthorizesCaoCao,
                "【匡亭大捷 · 奉诏讨逆】曹操奉天子明诏大破袁术于匡亭！袁术溃逃淮南！",
                "【征伐】初平四年春，后将军袁术引大军北犯陈留，擅僭号令。天子降诏敕命兖州牧曹操为征讨主帅，两军战于匡亭，曹操勒兵大破之，连拔襄邑、太寿诸城。袁术单骑奔九江，曹操上表献捷，国库收缴战利金一千五百万，天子皇权大振！",
                ImperialPowerDelta: 8,
                TreasuryGoldDelta: 1500,
                CaoCaoPowerDelta: 25,
                CaoCaoLoyaltyDelta: 20,
                YuanShuPowerDelta: -30,
                YuanShuLoyaltyDelta: -30);
        }

        // 2. 曹操自力破贼（朝廷中平）：曹操私战破袁术，声威大震
        if (caoCaoLoyal)
        {
            return new KuangtingBattleResult(
                KuangtingBattleOutcome.CaoCaoIndependentVictory,
                "【匡亭鏖战 · 曹操破贼】曹操自击袁术大捷！中原贼寇稍定！",
                "【战报】兖州牧曹操与袁术战于匡亭，曹操运奇谋断其粮道，袁术大溃走寿春。曹操威震中原，遣使向洛阳报捷进贡！",
                ImperialPowerDelta: 2,
                TreasuryGoldDelta: 1000,
                CaoCaoPowerDelta: 20,
                CaoCaoLoyaltyDelta: 10,
                YuanShuPowerDelta: -20,
                YuanShuLoyaltyDelta: -15);
        }

        // 3. 皇权微弱且将帅离心：袁术在中原肆虐
        return new KuangtingBattleResult(
            KuangtingBattleOutcome.YuanShuDominatesZhongyuan,
            "【中原兵燹 · 袁术僭越】袁术进犯陈留杀掠无度，朝廷无力遏制！",
            "【丧乱】袁术引黑山贼、匈奴单于於夫罗之众横行豫兖二州，公然抗拒朝命，中原郡县残破，天子降诏责问群雄！",
            ImperialPowerDelta: -10,
            TreasuryGoldDelta: 0,
            CaoCaoPowerDelta: -10,
            CaoCaoLoyaltyDelta: -5,
            YuanShuPowerDelta: 25,
            YuanShuLoyaltyDelta: -40);
    }
}
