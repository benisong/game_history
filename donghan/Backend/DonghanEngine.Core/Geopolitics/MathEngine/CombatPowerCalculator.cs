using System;
using DonghanEngine.Core.Geopolitics.Models;

namespace DonghanEngine.Core.Geopolitics.MathEngine;

/// <summary>
/// 纯数学工具类：计算野战/攻城综合战斗力与攻防伤亡（Stateless Pure Math）
/// </summary>
public static class CombatPowerCalculator
{
    public static int CalculateAttackerPower(int troops, int generalLeadership, int generalMartial)
    {
        double multiplier = 1.0 + (generalLeadership * 0.008) + (generalMartial * 0.004);
        return (int)(troops * multiplier);
    }

    public static int CalculateDefenderPower(int garrison, int defenseLevel, int generalLeadership)
    {
        int effectiveTroops = garrison + (defenseLevel * 80);
        double multiplier = 1.2 + (generalLeadership * 0.008); // 城防天然拥有 1.2x 地形优势
        return (int)(effectiveTroops * multiplier);
    }

    public static (int AttackerLoss, int DefenderLoss, bool AttackerTriumph) ResolveClash(
        int attackerPower,
        int defenderPower,
        int attackerTroops,
        int defenderGarrison)
    {
        double ratio = (double)attackerPower / Math.Max(1, defenderPower);

        if (ratio >= 1.3)
        {
            // 攻方大捷
            int attLoss = (int)(attackerTroops * 0.15);
            int defLoss = (int)(defenderGarrison * 0.70);
            return (attLoss, defLoss, true);
        }
        else if (ratio <= 0.8)
        {
            // 守方大捷，攻方惨败溃退
            int attLoss = (int)(attackerTroops * 0.40);
            int defLoss = (int)(defenderGarrison * 0.10);
            return (attLoss, defLoss, false);
        }
        else
        {
            // 陷入胶着攻防
            int attLoss = (int)(attackerTroops * 0.20);
            int defLoss = (int)(defenderGarrison * 0.25);
            return (attLoss, defLoss, false);
        }
    }
}
