using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 214 年 5 月刘备入蜀与刘璋献益州（天子诏封皇叔领巴蜀，收纳蜀锦巨贡）因果（单一职责）
/// </summary>
public sealed class LiuBeiYizhouEvaluator : ILiuBeiYizhouEvaluator
{
    public LiuBeiYizhouResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool moderateCourt = state.ImperialPower >= 35;

        // 1. 天子皇权强盛 (>=50)：天子明诏敕封皇叔刘备领益州牧、兼领巴蜀节制，刘备感激涕零，进纳蜀锦与天府贡金三千万
        if (strongCourt)
        {
            return new LiuBeiYizhouResult(
                LiuBeiYizhouOutcome.ImperialRatifiesYizhouAndCollectsTribute,
                "【宗室屏藩 · 天府归汉】刘璋开城出降！天子明诏敕封刘皇叔领益州牧！",
                "【益州】建安十九年夏五月，皇叔刘备引大军围成都，益州牧刘璋开城出降。刘备尽收天府之国户口甲兵，遣简雍奉表入洛阳太庙告捷。天子明察宗盟大义，下明诏敕封刘备为镇西大将军、领益州牧，允其按岁进奉蜀锦、良盐与天府金帛三千万钱。刘备率西川文武遥拜洛阳谢恩，蜀道通达，皇权赫奕！",
                ImperialPowerDelta: 10,
                TreasuryGoldDelta: 3000,
                LiuBeiPowerDelta: 35,
                LiuBeiLoyaltyDelta: 30,
                CaoCaoPowerDelta: 10,
                YizhouGovernorId: "liu_bei");
        }

        // 2. 朝廷中平 (35-49)：曹操警惕进兵汉中扼蜀道，朝廷受贡一千五百万
        if (moderateCourt)
        {
            return new LiuBeiYizhouResult(
                LiuBeiYizhouOutcome.CaoCaoInterferesHanzhong,
                "【巴蜀易主 · 曹公窥蜀】刘备兼并益州！曹操引军西征汉中扼险！",
                "【争夺】刘备得蜀，北方曹操深感威胁，引大军西征汉中张鲁以锁蜀口。刘备遣使入洛阳献贡一千五百万表奏受封，西南与西北大战前夕一触即发！",
                ImperialPowerDelta: 5,
                TreasuryGoldDelta: 1500,
                LiuBeiPowerDelta: 30,
                LiuBeiLoyaltyDelta: 15,
                CaoCaoPowerDelta: 20,
                YizhouGovernorId: "liu_bei");
        }

        // 3. 皇权微弱 (<35)：刘备据剑阁天险自雄
        return new LiuBeiYizhouResult(
            LiuBeiYizhouOutcome.LiuBeiAutonomousDefiance,
            "【剑门关险 · 蜀中自立】刘备据天府险阻！西南割据称雄！",
            "【割据】刘备尽并益州，据剑阁、白帝之险自雄，不复按期输贡。朝廷诏命难越秦岭，西南疆土渐成独立王国！",
            ImperialPowerDelta: -10,
            TreasuryGoldDelta: 0,
            LiuBeiPowerDelta: 40,
            LiuBeiLoyaltyDelta: -15,
            CaoCaoPowerDelta: 10,
            YizhouGovernorId: "liu_bei");
    }
}
