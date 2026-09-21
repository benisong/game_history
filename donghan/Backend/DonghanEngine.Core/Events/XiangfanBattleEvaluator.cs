using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 219 年 10 月襄樊之战与关羽水淹七军因果（单一职责）
/// </summary>
public sealed class XiangfanBattleEvaluator : IXiangfanBattleEvaluator
{
    public XiangfanBattleResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool moderateCourt = state.ImperialPower >= 35;

        // 1. 天子皇权强盛 (>=50)：天子在洛阳遣使持九锡节钺赶赴樊城前线，明令各方罢战，关羽奉诏撤军回江陵保全荆州免死，孙权受申诫不敢妄动，各纳贡金入朝
        if (strongCourt)
        {
            return new XiangfanBattleResult(
                XiangfanBattleOutcome.ImperialMediationSavesGuanYu,
                "【威震华夏 · 帝节罢兵】关羽水淹七军斩庞德！天子持节调停保全云长免遭暗算！",
                "【襄樊】建安二十四年冬十月，前将军关羽北伐襄樊，汉水暴溢，水淹于禁七军，生擒于禁、斩庞德，威震华夏。曹操震恐，孙权暗遣吕布袭江陵。洛阳天子亲临宣阳门，遣使持节赶赴襄樊前线颁下《诫群雄罢斗诏》，严旨令关羽还师江陵固守荆土、敕孙权曹操息兵各安封疆。关羽奉诏回军保全荆州免遭暗算，孙权受申诫敛兵，曹刘孙三家各进贡金三千万，皇权大振！",
                ImperialPowerDelta: 15,
                TreasuryGoldDelta: 3000,
                LiuBeiPowerDelta: 20,
                LiuBeiLoyaltyDelta: 30,
                CaoCaoPowerDelta: -10,
                SunQuanPowerDelta: -10,
                SunQuanLoyaltyDelta: 20,
                GuanYuSaved: true);
        }

        // 2. 朝廷中平 (35-49)：史实吕蒙白衣渡江袭荆州，关羽走麦城遇害
        if (moderateCourt)
        {
            return new XiangfanBattleResult(
                XiangfanBattleOutcome.HistoricalLvmengCrossesYangtze,
                "【白衣渡江 · 麦城星陨】吕蒙白衣渡江袭江陵！关羽败走麦城遇害！",
                "【星陨】孙权吕蒙暗中渡江偷袭江陵，关羽后路断绝，走麦城遇害。刘备痛失荆州大部，孙刘同盟决裂，天下格局剧变，曹操遣使向洛阳进献襄阳战报贡金一千五百万。",
                ImperialPowerDelta: 5,
                TreasuryGoldDelta: 1500,
                LiuBeiPowerDelta: -30,
                LiuBeiLoyaltyDelta: -10,
                CaoCaoPowerDelta: 15,
                SunQuanPowerDelta: 25,
                SunQuanLoyaltyDelta: 5,
                GuanYuSaved: false);
        }

        // 3. 皇权微弱 (<35)：关羽强克襄樊进逼洛阳
        return new XiangfanBattleResult(
            XiangfanBattleOutcome.GuanYuBreaksFanCity,
            "【水淹樊城 · 威逼中原】关羽大军克樊城兵指宛洛！朝廷震动！",
            "【震动】关羽水陆并进克樊城，斩将搴旗直指宛洛。朝廷震恐，曹操几欲迁都，关东烽火连天，天子号令难行！",
            ImperialPowerDelta: -15,
            TreasuryGoldDelta: 0,
            LiuBeiPowerDelta: 40,
            LiuBeiLoyaltyDelta: 0,
            CaoCaoPowerDelta: -25,
            SunQuanPowerDelta: -15,
            SunQuanLoyaltyDelta: -10,
            GuanYuSaved: true);
    }
}
