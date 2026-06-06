using System;
using System.Collections.Generic;

namespace DonghanEngine.Core;

/// <summary>
/// 无状态静态特征评估器。所有方法均为纯函数。
/// 
/// 叠加规则：系数类修正采用累乘（Compounded Multiplicative）。
/// 例如同时拥有「老谋深算」(×0.70) 和「说话直率」(×0.60) = 0.70 × 0.60 = 0.42。
/// 数值损益类（目标反噬加重）采用累加（Additive）。
/// </summary>
public static class NpcTraitEvaluator
{
    // 1. 获取开仓赈灾民心提振系数（支持类乘共存）
    public static double GetDisasterReliefSupportMultiplier(NpcState officer)
    {
        double multiplier = 1.0;
        if (officer.Traits.Contains(TraitNames.JingTianWeiDi)) multiplier *= 1.20;
        if (officer.Traits.Contains(TraitNames.ShanChangMinZheng)) multiplier *= 1.08;
        if (officer.Traits.Contains(TraitNames.AiMinRuZi)) multiplier *= 1.15;
        if (officer.Traits.Contains(TraitNames.QinMinWenHe)) multiplier *= 1.05;
        if (officer.Traits.Contains(TraitNames.HaoSheWuDu)) multiplier *= 0.75;
        if (officer.Traits.Contains(TraitNames.PuZhangLangFei)) multiplier *= 0.90;
        if (officer.Traits.Contains(TraitNames.BuXueWuShu)) multiplier *= 0.80;
        if (officer.Traits.Contains(TraitNames.CaiShuXueQian)) multiplier *= 0.90;
        return multiplier;
    }

    // 2. 获取阅兵发饷将领的禁军士气提振系数（支持类乘共存）
    public static double GetDrillMoraleMultiplier(NpcState officer)
    {
        double multiplier = 1.0;
        if (officer.Traits.Contains(TraitNames.KongWuYouLi)) multiplier *= 1.30;
        if (officer.Traits.Contains(TraitNames.YouXieLiQi)) multiplier *= 1.10;
        if (officer.Traits.Contains(TraitNames.ZhiJunYanZheng)) multiplier *= 1.25;
        if (officer.Traits.Contains(TraitNames.DongDianBingFa)) multiplier *= 1.10;
        if (officer.Traits.Contains(TraitNames.BuXueWuShu)) multiplier *= 0.80;
        if (officer.Traits.Contains(TraitNames.CaiShuXueQian)) multiplier *= 0.90;
        return multiplier;
    }

    // 3. 获取阅兵发饷将领的禁军天子忠诚提振系数
    public static double GetDrillLoyaltyMultiplier(NpcState officer)
    {
        double multiplier = 1.0;
        if (officer.Traits.Contains(TraitNames.AiBingRuZi)) multiplier *= 1.20;
        if (officer.Traits.Contains(TraitNames.TiXuShiZu)) multiplier *= 1.08;
        return multiplier;
    }

    // 4. 获取经办官员中饱漂没比例的系数修正
    public static int ApplyEmbezzlementSiphon(NpcState officer, int originalSiphon)
    {
        if (officer.Traits.Contains(TraitNames.QingZhengLianJie))
        {
            return 0;
        }
        if (officer.Traits.Contains(TraitNames.BuNaGongKuan))
        {
            return (int)(originalSiphon * 0.50);
        }
        if (officer.Traits.Contains(TraitNames.TanDeWuYan))
        {
            return (int)(originalSiphon * 1.50);
        }
        if (officer.Traits.Contains(TraitNames.YouXieShouZang))
        {
            return (int)(originalSiphon * 1.20);
        }
        return originalSiphon;
    }

    // 5. 获取强行抄家时，由近臣诬陷钦差导致的朝堂党羽政治反噬的扣除皇权
    public static int GetConfiscationImperialPowerLoss(NpcState framer, NpcState target)
    {
        if (framer.Traits.Contains(TraitNames.GangZhiBuE))
        {
            return 0; // Bypass all backlash!
        }

        int basePowerLoss = 15;
        double mitigationMultiplier = 1.0;

        if (framer.Traits.Contains(TraitNames.LaoMouShenSuan)) mitigationMultiplier *= 0.70; // 30% reduction (ends at 10)
        if (framer.Traits.Contains(TraitNames.YouXieXinJi)) mitigationMultiplier *= 0.90; // 10% reduction (ends at 13)
        if (framer.Traits.Contains(TraitNames.ShuoHuaZhiLv)) mitigationMultiplier *= 0.60; // 40% reduction (ends at 9)

        int finalLoss = (int)(basePowerLoss * mitigationMultiplier);

        // Target escalations
        if (target.Traits.Contains(TraitNames.YongBingZiZhong)) finalLoss += 5;
        if (target.Traits.Contains(TraitNames.ShouXiaYouBing)) finalLoss += 2;
        if (target.Traits.Contains(TraitNames.MenFaShiJia)) finalLoss += 8;
        if (target.Traits.Contains(TraitNames.ChuShenMingMen)) finalLoss += 3;

        return finalLoss;
    }

    // 6. 计算将领作战力（武力×0.4 + 统帅×0.6，Trait 修正）
    public static double GetCombatPower(NpcState npc)
    {
        double baseVal = npc.Martial * 0.4 + npc.Leadership * 0.6;
        double mult = 1.0;
        if (npc.Traits.Contains(TraitNames.KongWuYouLi)) mult *= 1.30;
        if (npc.Traits.Contains(TraitNames.YouXieLiQi)) mult *= 1.10;
        if (npc.Traits.Contains(TraitNames.ZhiJunYanZheng)) mult *= 1.25;
        if (npc.Traits.Contains(TraitNames.DongDianBingFa)) mult *= 1.10;
        if (npc.Traits.Contains(TraitNames.BuXueWuShu)) mult *= 0.80;
        if (npc.Traits.Contains(TraitNames.CaiShuXueQian)) mult *= 0.90;
        double corruptionPenalty = (npc.Corruption / 100.0) * 20;
        return baseVal * mult - corruptionPenalty;
    }

    // 7. 计算政治外交力（政治×0.6 + 魅力×0.4，Trait 修正）
    public static double GetPoliticalSkill(NpcState npc)
    {
        double baseVal = npc.Politics * 0.6 + npc.Charisma * 0.4;
        double mult = 1.0;
        if (npc.Traits.Contains(TraitNames.JingTianWeiDi)) mult *= 1.20;
        if (npc.Traits.Contains(TraitNames.ShanChangMinZheng)) mult *= 1.08;
        if (npc.Traits.Contains(TraitNames.AiMinRuZi)) mult *= 1.15;
        if (npc.Traits.Contains(TraitNames.LaoMouShenSuan)) mult *= 1.15;
        if (npc.Traits.Contains(TraitNames.GangZhiBuE)) mult *= 1.10;
        if (npc.Traits.Contains(TraitNames.TanDeWuYan)) mult *= 0.75;
        return baseVal * mult;
    }

    // 8. 获取特使阵亡概率（0-100）
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
