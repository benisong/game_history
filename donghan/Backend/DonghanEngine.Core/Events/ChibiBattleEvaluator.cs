using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 208 年 11 月赤壁之战（天子大义持节调停 vs 火烧连环船三国鼎立）因果（单一职责）
/// </summary>
public sealed class ChibiBattleEvaluator : IChibiBattleEvaluator
{
    public ChibiBattleResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool moderateCourt = state.ImperialPower >= 35;

        // 1. 天子皇权强盛 (>=50)：天子遣使持九锡符节赶赴赤壁，下《罢征南兵御札》，三方罢战各安疆界，曹操还军江陵，孙刘各保封域，三方各输贡金入洛阳太库
        if (strongCourt)
        {
            return new ChibiBattleResult(
                ChibiBattleOutcome.ImperialTruceAndTributeBalance,
                "【赤壁持节 · 天威罢兵】曹操引八十万大军饮马长江！天子降明诏持节调停鼎足之势！",
                "【调解】建安十三年冬十一月，司空曹操陈兵赤壁，欲顺流一举兼并江表；孙权、刘备结盟拒敌。大战一触即发之际，天子遣大鸿胪持节立于江心楼船，宣读《罢江汉兵同尊大汉诏》，敕令曹操退兵江陵、孙权保全江东、刘备领江南四郡。三方慑于朝廷正统大义与京畿重兵，皆受诏罢战！曹、刘、孙三方竞相遣使入洛阳进纳谢恩助国贡金四千万，天下三分而悉听朝命，皇权大振！",
                ImperialPowerDelta: 15,
                TreasuryGoldDelta: 4000,
                CaoCaoPowerDelta: -10,
                CaoCaoLoyaltyDelta: 20,
                LiuBeiPowerDelta: 20,
                LiuBeiLoyaltyDelta: 25,
                SunQuanLoyaltyDelta: 25);
        }

        // 2. 朝廷中平 (35-49)：史实赤壁大捷，周瑜黄盖火攻破曹
        if (moderateCourt)
        {
            return new ChibiBattleResult(
                ChibiBattleOutcome.SunLiuChibiFireTriumph,
                "【火烧赤壁 · 鼎足三分】周瑜黄盖运奇谋火烧连环战船！曹操败走华容道！",
                "【战报】周瑜、黄盖施苦肉计，引火船夜袭曹军水寨，赤壁烈焰蔽江，曹操大军溃败，引残部走华容道北归。刘备乘势收荆州四郡，孙权固守江东。曹刘孙鼎足三分之势初成，天下格局大变！",
                ImperialPowerDelta: 5,
                TreasuryGoldDelta: 1500,
                CaoCaoPowerDelta: -30,
                CaoCaoLoyaltyDelta: 5,
                LiuBeiPowerDelta: 30,
                LiuBeiLoyaltyDelta: 15,
                SunQuanLoyaltyDelta: 15);
        }

        // 3. 皇权微弱 (<35)：曹操横渡长江兼并江东
        return new ChibiBattleResult(
            ChibiBattleOutcome.CaoCaoCrossesYangtzeHegemony,
            "【铁锁横江 · 席卷东南】曹操大军克陷柴桑！江东降服！",
            "【混一】曹操水陆大军顺流直下，破柴桑，孙权刘备溃散。曹操尽并荆扬二州，权势滔天，朝廷社稷岌岌可危！",
            ImperialPowerDelta: -15,
            TreasuryGoldDelta: 500,
            CaoCaoPowerDelta: 45,
            CaoCaoLoyaltyDelta: -20,
            LiuBeiPowerDelta: -20,
            LiuBeiLoyaltyDelta: -10,
            SunQuanLoyaltyDelta: -20);
    }
}
