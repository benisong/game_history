using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 222 年 6 月夷陵之战与孙刘和睦因果（单一职责）
/// </summary>
public sealed class YilingBattleEvaluator : IYilingBattleEvaluator
{
    public YilingBattleResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool moderateCourt = state.ImperialPower >= 35;

        // 1. 天子皇权强盛 (>=50)：天子在洛阳遣使持节赶赴夷陵前线，宣读《同气连枝罢战御札》，敕令孙刘休兵修好，孙权送还降俘、刘备退驻白帝，两家各纳贡金入朝
        if (strongCourt)
        {
            return new YilingBattleResult(
                YilingBattleOutcome.ImperialMediationSunLiuAlliance,
                "【夷陵持节 · 孙刘修好】天子降明诏持节调停夷陵！孙刘二次和睦同尊大汉！",
                "【休兵】建安二十七年夏六月，汉中王刘备起倾国之兵东征孙权，两军夹江对峙于夷陵。洛阳天子亲颁御札遣重臣持节急赴峡口，宣谕宗室大义与天下大势，严命两家罢兵修好、屏藩汉室。刘备、孙权深明大义，奉诏罢战，孙权送还降俘质子，刘备还师白帝城。孙刘两家各进纳贡金三千万钱入洛阳国库，孙刘同盟重归于好，皇权大振！",
                ImperialPowerDelta: 15,
                TreasuryGoldDelta: 3000,
                LiuBeiPowerDelta: 10,
                LiuBeiLoyaltyDelta: 25,
                SunQuanPowerDelta: 10,
                SunQuanLoyaltyDelta: 25);
        }

        // 2. 朝廷中平 (35-49)：史实陆逊火烧连营，孙权遣使向朝廷献捷进贡一千五百万
        if (moderateCourt)
        {
            return new YilingBattleResult(
                YilingBattleOutcome.LuXunFireAttackTriumph,
                "【火烧连营 · 猇亭大捷】陆逊运奇谋火烧连营七百里！刘备败退白帝城！",
                "【大捷】陆逊在猇亭以火攻击破蜀军连营，刘备大军溃散，退保白帝城。孙权遣使向洛阳献捷进贡一千五百万钱，两家兵力皆受重创，曹丕乘机南窥。",
                ImperialPowerDelta: 5,
                TreasuryGoldDelta: 1500,
                LiuBeiPowerDelta: -30,
                LiuBeiLoyaltyDelta: -10,
                SunQuanPowerDelta: 25,
                SunQuanLoyaltyDelta: 15);
        }

        // 3. 皇权微弱 (<35)：刘备击破东吴
        return new YilingBattleResult(
            YilingBattleOutcome.LiuBeiCrushesEasternWu,
            "【蜀军破吴 · 席卷荆襄】刘备含怒大破东吴！江南烽烟蔽日！",
            "【溃败】刘备水陆并进击溃吴军，席卷荆扬，孙权退保建业。朝廷诏命难行，南方陷入空前混战！",
            ImperialPowerDelta: -10,
            TreasuryGoldDelta: 0,
            LiuBeiPowerDelta: 35,
            LiuBeiLoyaltyDelta: 0,
            SunQuanPowerDelta: -35,
            SunQuanLoyaltyDelta: -20);
    }
}
