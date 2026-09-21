using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 215 年 11 月合肥之战与逍遥津之役因果（单一职责）
/// </summary>
public sealed class HefeiBattleEvaluator : IHefeiBattleEvaluator
{
    public HefeiBattleResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool moderateCourt = state.ImperialPower >= 35;

        // 1. 天子皇权强盛 (>=50)：张辽大破孙权后，天子遣使持节调停江淮战事，曹孙两家各纳贡金入朝，国库充盈，皇权大振
        if (strongCourt)
        {
            return new HefeiBattleResult(
                HefeiBattleOutcome.ImperialMediationAndHuaiheTruce,
                "【威震逍遥 · 帝策调停】张辽八百破十万！天子遣使持节调停江淮！",
                "【江淮】建安二十年冬十一月，吴侯孙权引十万大军围合肥。守将张辽率八百勇士冲阵，大破吴军于逍遥津，孙权几见擒获。天子闻奏，念及东南生灵，遣九卿持节赶赴江淮前线宣读《申诫罢战诏》，令曹操守合肥、孙权还军江东。曹操、孙权皆奉诏息兵，各遣使入洛阳纳贡金三千万，皇权赫奕！",
                ImperialPowerDelta: 10,
                TreasuryGoldDelta: 3000,
                CaoCaoLoyaltyDelta: 20,
                SunQuanLoyaltyDelta: 20,
                CaoCaoPowerDelta: 20,
                SunQuanPowerDelta: -15);
        }

        // 2. 朝廷中平 (35-49)：张辽威震逍遥津，曹操向朝廷献捷进贡一千五百万
        if (moderateCourt)
        {
            return new HefeiBattleResult(
                HefeiBattleOutcome.ZhangLiaoIndependentGlory,
                "【逍遥津捷 · 威震江表】张辽力挫十万吴军！曹操上表洛阳献捷！",
                "【大捷】张辽合肥鏖战大捷，威名震动江东，小儿夜不敢啼。曹操遣使向洛阳献俘献贡一千五百万，朝廷优诏褒奖张辽忠勇，孙权受挫退兵。",
                ImperialPowerDelta: 5,
                TreasuryGoldDelta: 1500,
                CaoCaoLoyaltyDelta: 15,
                SunQuanLoyaltyDelta: 5,
                CaoCaoPowerDelta: 25,
                SunQuanPowerDelta: -20);
        }

        // 3. 皇权微弱 (<35)：孙权攻破合肥
        return new HefeiBattleResult(
            HefeiBattleOutcome.SunQuanBreaksHefei,
            "【合肥陷落 · 孙氏北进】孙权十万大军拔合肥！淮南战火蔓延！",
            "【兵燹】合肥城陷，孙权大军进逼寿春，江淮门户洞开。朝廷诏命难行，江淮烽火连绵，社稷威严大损！",
            ImperialPowerDelta: -10,
            TreasuryGoldDelta: 0,
            CaoCaoLoyaltyDelta: -10,
            SunQuanLoyaltyDelta: -15,
            CaoCaoPowerDelta: -20,
            SunQuanPowerDelta: 35);
    }
}
