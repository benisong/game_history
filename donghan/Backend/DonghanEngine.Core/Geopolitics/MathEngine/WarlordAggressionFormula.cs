using System;
using DonghanEngine.Core.Geopolitics.Models;

namespace DonghanEngine.Core.Geopolitics.MathEngine;

/// <summary>
/// 纯数学工具类：计算诸侯进攻欲望与多因子加权概率（Stateless Pure Math）
/// </summary>
public static class WarlordAggressionFormula
{
    public static int CalculateAttackProbability(
        WarlordFaction attacker,
        int targetGarrison,
        int hostilityWithTarget,
        int emperorImperialPower,
        int westGardenArmySize,
        bool hasTruce)
    {
        if (hasTruce) return 0; // 存在朝廷敕令休战，严禁出兵

        int baseChance = attacker.Posture switch
        {
            WarlordPosture.AmbitiousWarlord => 35,
            WarlordPosture.CautiousAutonomist => 5,
            WarlordPosture.LoyalistBanner => 0,
            _ => 0
        };

        if (baseChance == 0) return 0;

        // 1. 兵力对比优势修正
        double ratio = (double)attacker.TotalTroops / Math.Max(1, targetGarrison);
        int strengthBonus = 0;
        if (ratio >= 2.0) strengthBonus = 30;
        else if (ratio >= 1.4) strengthBonus = 20;
        else if (ratio < 0.8) strengthBonus = -30; // 兵力不及，极度顾忌

        // 2. 野心与敌对度修正
        int ambitionBonus = (int)((attacker.ExpansionDesire - 50) * 0.4);
        int hostilityBonus = (int)(hostilityWithTarget * 0.2);

        // 3. 天子皇权与西园禁军威慑修正
        int imperialDeterrence = 0;
        if (emperorImperialPower >= 60 && westGardenArmySize >= 8000)
        {
            imperialDeterrence = 25; // 朝廷威望高且禁军强，极大压制诸侯私战
        }
        else if (emperorImperialPower < 30)
        {
            imperialDeterrence = -15; // 中央暗弱，胆气大增
        }

        int finalProb = baseChance + strengthBonus + ambitionBonus + hostilityBonus - imperialDeterrence;
        return Math.Clamp(finalProb, 0, 95);
    }
}
