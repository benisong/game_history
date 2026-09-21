using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 219 年 5 月汉中之战与刘备进位汉中王（天子明诏册封皇叔汉中王，收纳四千万贡金）因果（单一职责）
/// </summary>
public sealed class HanzhongBattleEvaluator : IHanzhongBattleEvaluator
{
    public HanzhongBattleResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool moderateCourt = state.ImperialPower >= 35;

        // 1. 天子皇权强盛 (>=50)：天子降九锡金策册封皇叔刘备为“汉中王”，赐节钺旌旗，刘备进纳四千万贡金，皇权与宗室屏藩大振
        if (strongCourt)
        {
            return new HanzhongBattleResult(
                HanzhongBattleOutcome.ImperialRatifiesKingOfHanzhong,
                "【定军斩将 · 皇叔进王】老将黄忠阵斩夏侯渊！天子明诏册封刘备为汉中王！",
                "【封王】建安二十四年夏五月，皇叔刘备与曹操鏖战汉中，老将黄忠定军山阵斩征西将军夏侯渊，曹操引残兵北退关中，刘备全据汉中九郡。群臣于勉阳设坛表奏洛阳太庙。天子明察宗室屏藩大义，降明诏册封刘备为“汉中王”，赐冕服玄衮、金印紫绶以遥制北方。刘备感激涕零，誓死保卫汉室社稷，进奉汉中战利与西蜀金帛四千万钱入洛阳国库，皇权赫奕！",
                ImperialPowerDelta: 15,
                TreasuryGoldDelta: 4000,
                LiuBeiPowerDelta: 35,
                LiuBeiLoyaltyDelta: 30,
                CaoCaoPowerDelta: -15,
                CaoCaoLoyaltyDelta: 10,
                LiuBeiTitle: "汉中王");
        }

        // 2. 朝廷中平 (35-49)：曹操退守关中，刘备进位汉中王上表称臣，进贡两千万
        if (moderateCourt)
        {
            return new HanzhongBattleResult(
                HanzhongBattleOutcome.CaoCaoHoldsHanzhongFrontier,
                "【汉中易主 · 鼎足鼎立】刘备全据汉中！曹操退守关陇！",
                "【抗衡】曹操汉中失利退归长安，刘备自立汉中王，遣使入洛阳进贡两千万钱表奏谢罪。天子下旨追认抚慰，西南抗衡北方之势完全确立。",
                ImperialPowerDelta: 5,
                TreasuryGoldDelta: 2000,
                LiuBeiPowerDelta: 30,
                LiuBeiLoyaltyDelta: 15,
                CaoCaoPowerDelta: -10,
                CaoCaoLoyaltyDelta: 5,
                LiuBeiTitle: "汉中王");
        }

        // 3. 皇权微弱 (<35)：刘备军势大振直逼关中
        return new HanzhongBattleResult(
            HanzhongBattleOutcome.LiuBeiBreaksIntoGuanzhong,
            "【秦川震动 · 蜀军出峡】刘备大军克汉中进逼三辅！朝廷震恐！",
            "【危局】刘备全胜汉中，兵锋直逼陈仓，关中震动。朝廷诏命无力遏制诸侯大战，西北战火连天！",
            ImperialPowerDelta: -10,
            TreasuryGoldDelta: 0,
            LiuBeiPowerDelta: 40,
            LiuBeiLoyaltyDelta: -15,
            CaoCaoPowerDelta: -20,
            CaoCaoLoyaltyDelta: -10,
            LiuBeiTitle: "汉中王");
    }
}
