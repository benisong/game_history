using System;
using System.Collections.Generic;

namespace DonghanEngine.Core;

public static class NpcTraitEvaluator
{
    // 1. 获取开仓赈灾民心提振系数（支持类乘共存）
    public static double GetDisasterReliefSupportMultiplier(NpcState officer)
    {
        double multiplier = 1.0;
        if (officer.Traits.Contains("经天纬地")) multiplier *= 1.20;
        if (officer.Traits.Contains("擅长民政")) multiplier *= 1.08;
        if (officer.Traits.Contains("爱民如子")) multiplier *= 1.15;
        if (officer.Traits.Contains("亲民温和")) multiplier *= 1.05;
        if (officer.Traits.Contains("豪奢无度")) multiplier *= 0.75;
        if (officer.Traits.Contains("铺张浪费")) multiplier *= 0.90;
        if (officer.Traits.Contains("不学无术")) multiplier *= 0.80;
        if (officer.Traits.Contains("才疏学浅")) multiplier *= 0.90;
        return multiplier;
    }

    // 2. 获取阅兵发饷将领的禁军士气提振系数（支持类乘共存）
    public static double GetDrillMoraleMultiplier(NpcState officer)
    {
        double multiplier = 1.0;
        if (officer.Traits.Contains("孔武有力")) multiplier *= 1.30;
        if (officer.Traits.Contains("有些力气")) multiplier *= 1.10;
        if (officer.Traits.Contains("治军严整")) multiplier *= 1.25;
        if (officer.Traits.Contains("懂点兵法")) multiplier *= 1.10;
        if (officer.Traits.Contains("不学无术")) multiplier *= 0.80;
        if (officer.Traits.Contains("才疏学浅")) multiplier *= 0.90;
        return multiplier;
    }

    // 3. 获取阅兵发饷将领的禁军天子忠诚提振系数
    public static double GetDrillLoyaltyMultiplier(NpcState officer)
    {
        double multiplier = 1.0;
        if (officer.Traits.Contains("爱兵如子")) multiplier *= 1.20;
        if (officer.Traits.Contains("体恤士卒")) multiplier *= 1.08;
        return multiplier;
    }

    // 4. 获取经办官员中饱漂没比例的系数修正
    public static int ApplyEmbezzlementSiphon(NpcState officer, int originalSiphon)
    {
        if (officer.Traits.Contains("清正廉洁"))
        {
            return 0;
        }
        if (officer.Traits.Contains("不拿公款"))
        {
            return (int)(originalSiphon * 0.50);
        }
        if (officer.Traits.Contains("贪得无厌"))
        {
            return (int)(originalSiphon * 1.50);
        }
        if (officer.Traits.Contains("有些手脏"))
        {
            return (int)(originalSiphon * 1.20);
        }
        return originalSiphon;
    }

    // 5. 获取强行抄家时，由近臣诬陷钦差导致的朝堂党羽政治反噬的扣除皇权
    public static int GetConfiscationImperialPowerLoss(NpcState framer, NpcState target)
    {
        if (framer.Traits.Contains("刚直不阿"))
        {
            return 0; // Bypass all backlash!
        }

        int basePowerLoss = 15;
        double mitigationMultiplier = 1.0;

        if (framer.Traits.Contains("老谋深算")) mitigationMultiplier *= 0.70; // 30% reduction (ends at 10)
        if (framer.Traits.Contains("有些心计")) mitigationMultiplier *= 0.90; // 10% reduction (ends at 13)
        if (framer.Traits.Contains("说话直率")) mitigationMultiplier *= 0.60; // 40% reduction (ends at 9)

        int finalLoss = (int)(basePowerLoss * mitigationMultiplier);

        // Target escalations
        if (target.Traits.Contains("拥兵自重")) finalLoss += 5;
        if (target.Traits.Contains("手下有兵")) finalLoss += 2;
        if (target.Traits.Contains("门阀世家")) finalLoss += 8;
        if (target.Traits.Contains("出身名门")) finalLoss += 3;

        return finalLoss;
    }
}
