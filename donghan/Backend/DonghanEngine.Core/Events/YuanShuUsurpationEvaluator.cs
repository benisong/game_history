using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 197 年 1 月袁术淮南僭号与天子号召天下诸侯共讨（驱虎吞狼绝杀）因果（单一职责）
/// </summary>
public sealed class YuanShuUsurpationEvaluator : IYuanShuUsurpationEvaluator
{
    public YuanShuUsurpationResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool moderateCourt = state.ImperialPower >= 35;

        // 1. 天子皇权强盛 (>=50)：天子持玉玺下九道诛贼朱谕，四方群雄争先奉旨讨贼以表忠诚，孙策绝交，合围寿春，淮南财帛尽归朝廷
        if (strongCourt)
        {
            return new YuanShuUsurpationResult(
                YuanShuUsurpationOutcome.ImperialCoalitionCrushesYuanShu,
                "【奉旨讨逆 · 寿春灰飞】袁术僭号狂逆！天子明诏下九野，四海诸侯奉旨合围！",
                "【诛逆】建安二年春，后将军袁术悍然于寿春称帝，号“仲氏”。天子执真传国玉玺登宣政殿，下《讨淮南僭乱朱谕》，曹操、刘备、孙策、吕布四路大军奉旨出兵合围淮南！孙策发绝交书斥贼，曹刘猛攻寿春，袁术众叛亲离，粮尽呕血而亡。寿春库府珍宝三千万进献洛阳，天下无不慑伏于大汉天威！",
                ImperialPowerDelta: 15,
                TreasuryGoldDelta: 3000,
                CaoCaoLoyaltyDelta: 20,
                LiuBeiLoyaltyDelta: 25,
                SunCeLoyaltyDelta: 25,
                LvBuLoyaltyDelta: 20,
                YuanShuPowerDelta: -90);
        }

        // 2. 朝廷中平 (35-49)：曹操引军奉旨独破寿春
        if (moderateCourt)
        {
            return new YuanShuUsurpationResult(
                YuanShuUsurpationOutcome.CaoCaoIndependentCrush,
                "【曹操克寿春 · 逆贼伏诛】曹操奉诏扫荡淮南！袁术败亡！",
                "【殄灭】袁术僭逆称帝，天子密诏兖州牧曹操征讨。曹操引大军亲征寿春，斩袁术大将桥蕤、李丰，袁术奔溃而死。曹操上表献俘献贡一千五百万，中原威名更盛。",
                ImperialPowerDelta: 8,
                TreasuryGoldDelta: 1500,
                CaoCaoLoyaltyDelta: 15,
                LiuBeiLoyaltyDelta: 10,
                SunCeLoyaltyDelta: 15,
                LvBuLoyaltyDelta: 5,
                YuanShuPowerDelta: -80);
        }

        // 3. 皇权微弱 (<35)：诸侯观望不前，袁术据守淮南
        return new YuanShuUsurpationResult(
            YuanShuUsurpationOutcome.YuanShuHoldsHuainan,
            "【群雄观望 · 伪帝猖狂】朝廷号令难行！袁术割据淮南自立！",
            "【逆乱】袁术僭号自立，四方诸侯怀二心皆按兵不动。朝廷诏命难达江淮，社稷威严大受折损！",
            ImperialPowerDelta: -15,
            TreasuryGoldDelta: 0,
            CaoCaoLoyaltyDelta: -10,
            LiuBeiLoyaltyDelta: -5,
            SunCeLoyaltyDelta: -10,
            LvBuLoyaltyDelta: -15,
            YuanShuPowerDelta: 20);
    }
}
