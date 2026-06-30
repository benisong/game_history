using System;
using System.Collections.Generic;

namespace DonghanEngine.Core;

/// <summary>
/// 无状态静态特征评估器。所有方法均为纯函数。
///
/// 【藏锋于词 · 数值驱动重写】
/// 加成不再读取 Traits 字符串，而是由对应能力维度的【真实数值区间】决定品阶系数
/// (红1.5 / 金1.2 / 紫1.0 / 蓝0.8 / 灰0.5)。词组只是该数值落档的显示名，已并入颜色系数，
/// 不再单独产生修正。强者(高数值/红档)能扭转大局，庸者(低数值/灰档)会把事搞砸。
///
/// 数值对玩家隐藏(后台黑盒)，玩家只见 TraitDeriver 派生的词组去"猜"。
///
/// 维度 → 事件映射:
///   赈灾民心 → 政治    阅兵士气 → 武力    阅兵忠诚 → 统帅
///   漂没贪墨 → 廉洁    抄家心机折减 → 政治    战力 → 武力+统帅    政治外交 → 政治+魅力
/// </summary>
public static class NpcTraitEvaluator
{
    // 1. 开仓赈灾民心提振系数：看政治品阶
    public static double GetDisasterReliefSupportMultiplier(NpcState officer)
        => TraitVocabulary.MultiplierOf(TraitVocabulary.TierOf(officer.Politics));

    // 2. 阅兵发饷将领的禁军士气提振系数：看武力品阶
    public static double GetDrillMoraleMultiplier(NpcState officer)
        => TraitVocabulary.MultiplierOf(TraitVocabulary.TierOf(officer.Martial));

    // 3. 阅兵发饷将领的禁军天子忠诚提振系数：看统帅品阶
    public static double GetDrillLoyaltyMultiplier(NpcState officer)
        => TraitVocabulary.MultiplierOf(TraitVocabulary.TierOf(officer.Leadership));

    // 4. 经办官员中饱漂没修正：看廉洁(Integrity)品阶。
    //    入参 siphonBase 为"最大可漂没基数"(上限)，返回实际漂没 = 基数 × 廉洁档比例。
    //    廉洁越高漂没越少:红→0(两袖清风) 金→0.25 紫→0.5 蓝→0.75 灰→1.0(贪墨吃满)。
    public static int ApplyEmbezzlementSiphon(NpcState officer, int siphonBase)
    {
        double factor = TraitVocabulary.TierOf(officer.Integrity) switch
        {
            TraitTier.Red => 0.0,
            TraitTier.Gold => 0.25,
            TraitTier.Purple => 0.5,
            TraitTier.Blue => 0.75,
            TraitTier.Gray => 1.0,
            _ => 0.5
        };
        return (int)(siphonBase * factor);
    }

    // 5. 强行抄家时由钦差心机决定的政治反噬折减：看钦差政治品阶。
    //    政治越高越能化解党羽反噬。基础损失 15:
    //    红→0(权谋通天,豁免) 金→5 紫→10 蓝→13 灰→15(全额反噬)
    //    目标的权势/门第仍按其真实数值加重反噬(高权势/高魅力者党羽更多)。
    public static int GetConfiscationImperialPowerLoss(NpcState framer, NpcState target)
    {
        int finalLoss = TraitVocabulary.TierOf(framer.Politics) switch
        {
            TraitTier.Red => 0,
            TraitTier.Gold => 5,
            TraitTier.Purple => 10,
            TraitTier.Blue => 13,
            TraitTier.Gray => 15,
            _ => 15
        };

        // 目标反噬加重:权势越高(党羽多)、魅力越高(门生故吏广)，抄家越招怨。
        if (target.Power >= 70) finalLoss += 5;
        else if (target.Power >= 40) finalLoss += 2;
        if (target.Charisma >= 75) finalLoss += 8;
        else if (target.Charisma >= 55) finalLoss += 3;

        return finalLoss;
    }

    // 6. 将领作战力(武力×0.4 + 统帅×0.6)。复合两维，各维品阶系数加权后作用于基础值。
    public static double GetCombatPower(NpcState npc)
    {
        double baseVal = npc.Martial * 0.4 + npc.Leadership * 0.6;
        double mult = TraitVocabulary.MultiplierOf(TraitVocabulary.TierOf(npc.Martial)) * 0.4
                    + TraitVocabulary.MultiplierOf(TraitVocabulary.TierOf(npc.Leadership)) * 0.6;
        double corruptionPenalty = (npc.Corruption / 100.0) * 20;
        return baseVal * mult - corruptionPenalty;
    }

    // 7. 政治外交力(政治×0.6 + 魅力×0.4)。复合两维加权品阶系数。
    public static double GetPoliticalSkill(NpcState npc)
    {
        double baseVal = npc.Politics * 0.6 + npc.Charisma * 0.4;
        double mult = TraitVocabulary.MultiplierOf(TraitVocabulary.TierOf(npc.Politics)) * 0.6
                    + TraitVocabulary.MultiplierOf(TraitVocabulary.TierOf(npc.Charisma)) * 0.4;
        return baseVal * mult;
    }

    // 8. 特使阵亡概率(0-100)：纯数值(武力)，保持不变。
    public static int GetEnvoyDeathRisk(NpcState envoy, int rebellionMonths, int distance, bool usedPunishment, int imperialPower)
    {
        int risk = 40; // base
        if (envoy.Martial >= 70) risk -= 20;
        else if (envoy.Martial >= 50) risk -= 10;
        else if (envoy.Martial < 30) risk += 15;
        risk += rebellionMonths * 5;
        risk += distance * 3;
        if (usedPunishment) risk += 10;
        if (imperialPower < 20) risk += 15;
        return Math.Clamp(risk, 10, 90);
    }
}
