using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 208 年 7 月刘表病亡与荆州易主（天子调解分陕 vs 曹操南下刘琮降曹）因果（单一职责）
/// </summary>
public sealed class JingzhouSuccessionEvaluator : IJingzhouSuccessionEvaluator
{
    public JingzhouSuccessionResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool caoCaoLoyal = state.Npcs.TryGetValue("cao_cao", out var cc) && cc.IsActive && cc.Favorability >= 60;

        // 1. 天子皇权强盛 (>=50)：天子敕命宗室长子刘琦领荆州刺史、刘备移驻江夏为藩屏，曹操奉诏止步襄阳纳贡两千五百万，朝廷保持超然调解
        if (strongCourt)
        {
            return new JingzhouSuccessionResult(
                JingzhouSuccessionOutcome.ImperialMediationAndPartition,
                "【荆襄易主 · 帝策分陕】荆州牧刘表病逝！天子明诏调解定江汉！",
                "【江汉】建安十三年秋七月，荆州牧刘表病卒。蔡瑁等拥立少子刘琮，长子刘琦奔江夏。司空曹操大军南下宛洛。天子闻奏，念及刘氏宗室亲睦，降明诏敕命刘琦继领荆州刺史、皇叔刘备屯兵江夏以为藩屏，并下旨申诫曹操不得擅启同室兵端。曹操奉诏陈兵襄阳，刘琦、刘备与曹操皆遣使入洛阳输贡两千五百万钱，荆襄局势暂得平息，皇权赫奕！",
                ImperialPowerDelta: 10,
                TreasuryGoldDelta: 2500,
                CaoCaoPowerDelta: 20,
                CaoCaoLoyaltyDelta: 15,
                LiuBeiPowerDelta: 25,
                LiuBeiLoyaltyDelta: 25,
                JingzhouGovernorId: "liu_bei");
        }

        // 2. 曹操好感较高或自力南下（朝廷中平）：刘琮降曹，曹操尽并荆襄
        if (state.ImperialPower >= 35)
        {
            return new JingzhouSuccessionResult(
                JingzhouSuccessionOutcome.CaoCaoSubduesJingzhouDirect,
                "【刘琮降曹 · 席卷荆襄】刘表病故刘琮举州归附！曹操大军进驻江陵！",
                "【降服】刘表病故，刘琮在蔡瑁唆使下举九郡之众望风降曹。曹操顺流长驱直入占领江陵与襄阳，刘备退保夏口。曹操遣使向洛阳献荆州降表与贡赋一千五百万，曹操兵锋直逼江东孙权！",
                ImperialPowerDelta: 5,
                TreasuryGoldDelta: 1500,
                CaoCaoPowerDelta: 35,
                CaoCaoLoyaltyDelta: 10,
                LiuBeiPowerDelta: -10,
                LiuBeiLoyaltyDelta: 10,
                JingzhouGovernorId: "cao_cao");
        }

        // 3. 皇权微弱 (<35)：刘备据荆襄自雄
        return new JingzhouSuccessionResult(
            JingzhouSuccessionOutcome.LiuBeiConsolidatesJingxiang,
            "【玄德抚民 · 江汉分立】刘表托孤！刘备引江夏军民据江南自雄！",
            "【自雄】荆州动荡，刘备深得荆襄十万士民归附，收江南四郡自立。朝廷诏命难达南荒，江汉割据成型！",
            ImperialPowerDelta: -5,
            TreasuryGoldDelta: 0,
            CaoCaoPowerDelta: 10,
            CaoCaoLoyaltyDelta: -5,
            LiuBeiPowerDelta: 30,
            LiuBeiLoyaltyDelta: 0,
            JingzhouGovernorId: "liu_bei");
    }
}
