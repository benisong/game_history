# 30+ 成语与通俗特征复合修饰库与计算引擎实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 创建高内聚、低耦合的 `NpcTraitEvaluator.cs` 静态修饰评估器，实装 32 项历史成语与同义通俗词语特征（通俗加成折半）。支持多重 Traits 共存下的类乘与累加计算。在 `GameEngine.cs` 中剔除杂乱判断，干净地对接此 Traits 估值引擎。

**Architecture:** 
1. 新建 `NpcTraitEvaluator.cs` 评估类。
2. 重构并精简 `GameEngine.cs`（阅兵、赈灾、抄家、后宫调养），完美对接特征估值。
3. 在 `EngineTests.cs` 中增加复合特征共存（如同时拥有成语与通俗特征）的类乘叠加断言，进行绿色全验证。

**Tech Stack:** C# (.NET 8.0), xUnit

---

## 拟创建/修改的文件映射 (File Structure)

- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\NpcTraitEvaluator.cs` (创建)：32 项高内聚 Traits 大局评估计算引擎。
- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameEngine.cs` (修改)：清除散落零碎的判断，全盘对接 `NpcTraitEvaluator` 评估方法。
- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Tests\EngineTests.cs` (修改/追加)：校准并增加 Traits 单元测试。

---

## 具体实施任务 (Bite-Sized Tasks)

### Task 1: 创建高内聚 Traits 大政评估器 `NpcTraitEvaluator.cs`

**Files:**
- Create: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\NpcTraitEvaluator.cs`

- [ ] **Step 1: 编写 `NpcTraitEvaluator.cs` 计算 32 项特征的累乘与累加加成**

```csharp
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
```

---

### Task 2: 重构 `GameEngine.cs` 全盘对接 `NpcTraitEvaluator`

**Files:**
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameEngine.cs`

- [ ] **Step 1: 重构犒赏禁军中的资金漂没、士气、忠诚度特征计算**

在 `ExecuteDrillArmyActionWithOfficer` 中：
```csharp
        double corruptionRate = (officer.Corruption / 100.0) * 0.50; 
        int siphonedAmount = (int)(paidAmount * corruptionRate);

        // 统一对接评估器：清正/不拿钱/贪得无厌/脏手
        siphonedAmount = NpcTraitEvaluator.ApplyEmbezzlementSiphon(officer, siphonedAmount);
        if (siphonedAmount > paidAmount) siphonedAmount = paidAmount;

        // ...

        // 对接评估器：孔武有力/有些力气/治军严整/懂兵法/不学无术
        double moraleMultiplier = NpcTraitEvaluator.GetDrillMoraleMultiplier(officer);
        int finalMoraleDelta = (int)(moraleDelta * moraleMultiplier);

        // 对接评估器：爱兵如子/体恤士卒
        double loyaltyMultiplier = NpcTraitEvaluator.GetDrillLoyaltyMultiplier(officer);
        int finalLoyaltyDelta = (int)(loyaltyDelta * loyaltyMultiplier);

        army.Morale = Math.Clamp(army.Morale + finalMoraleDelta, 0, 100);
        army.Loyalty = Math.Clamp(army.Loyalty + finalLoyaltyDelta, 0, 100);
```

- [ ] **Step 2: 重构开仓赈灾中的民心特征计算**

在 `ExecuteDisasterReliefAction` 中：
```csharp
        double corruptionRate = (officer.Corruption / 100.0) * 0.75;
        int siphonedAmount = (int)(reliefAmount * corruptionRate);

        // 对接评估器
        siphonedAmount = NpcTraitEvaluator.ApplyEmbezzlementSiphon(officer, siphonedAmount);
        if (siphonedAmount > reliefAmount) siphonedAmount = reliefAmount;

        // ...

        // 对接评估器：经天纬地/擅长民政/爱民如子/亲民温和/豪奢/铺张
        double supportMultiplier = NpcTraitEvaluator.GetDisasterReliefSupportMultiplier(officer);
        int finalSupportDelta = (int)(supportDelta * supportMultiplier);
```

- [ ] **Step 3: 重构抄家弹劾政治反噬中的皇权损失计算**

在 `ExecuteConfiscationAction` 中：
```csharp
        // 对接评估器
        int finalPowerLoss = NpcTraitEvaluator.GetConfiscationImperialPowerLoss(framer, target);
```

- [ ] **Step 4: 重构后宫生活中的谄媚、医术与清谈计算**

在 `ExecuteQuickAction` 的 `harem_rest` 后宫调养中：
```csharp
        else if (actionId == "harem_rest" && _state.CurrentLocation == "后宫")
        {
            int extraHealth = 0;
            int imperialPowerDelta = 0;

            foreach (var npc in _state.Npcs.Values)
            {
                if (npc.IsActive)
                {
                    if (npc.Traits.Contains("谄媚专权"))
                    {
                        npc.Favorability = Math.Clamp(npc.Favorability + 15, 0, 100);
                        npc.Power = Math.Clamp(npc.Power + 5, 0, 100);
                        extraHealth += 5;
                    }
                    if (npc.Traits.Contains("会拍马屁"))
                    {
                        npc.Favorability = Math.Clamp(npc.Favorability + 6, 0, 100);
                        npc.Power = Math.Clamp(npc.Power + 2, 0, 100);
                        extraHealth += 2;
                    }
                    if (npc.Traits.Contains("医术高明"))
                    {
                        extraHealth += 8;
                    }
                    if (npc.Traits.Contains("懂点医理"))
                    {
                        extraHealth += 3;
                    }
                    if (npc.Traits.Contains("喜好清谈"))
                    {
                        extraHealth += 2;
                        imperialPowerDelta -= 1; // 清谈误国，该旬扣减 1 点皇权
                    }
                }
            }

            _state.Health = Math.Clamp(_state.Health + 10 + extraHealth, 0, 100);
            _state.ImperialPower = Math.Clamp(_state.ImperialPower + imperialPowerDelta, 0, 100);
            
            _state.AddToChronicle("【后宫】天子龙体困乏，宿于温德殿中调养休息。");
            result.StoryText = $"【后宫春深】\n\n红粉深处，金炉香暖。陛下于温德殿中卸下凡尘政务，临幸嫔妃，调养龙体，疲意尽消。\n\n[color=green]● 皇帝健康：+{10 + extraHealth} (龙体充沛)[/color]\n[color=red]● 朝廷皇权：{(imperialPowerDelta != 0 ? imperialPowerDelta.ToString() : "无变动")}[/color]";
        }
```

---

### Task 3: 升级测试并验证全编译 13/13 单元测试

**Files:**
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Tests\EngineTests.cs`

- [ ] **Step 1: 在 `EngineTests.cs` 中增加 Traits 共存及乘积测试**

在 `Test_NPC_Ecosystem_LifecycleAndDescriptiveTraits` 单元测试中，增加对「经天纬地」和「爱民如子」在同一个 NPC 身上累乘共存的数值物理断言：
```csharp
        // 6. 验证文学词汇 [经天纬地] 与 [爱民如子] 累乘共存：1.20 * 1.15 = 1.38x 增益
        liuBei.Traits.Add("爱民如子");
        int secondarySupport = state.PopularSupport;
        // 再次开仓赈灾，拨发 1000 万钱（民心提振基数默认 12点）
        engine.ExecuteDisasterReliefAction(1000, "liu_bei");
        // 复合计算：12 * 1.20 * 1.15 = 16.56 -> 16 点民心提振
        Assert.Equal(secondarySupport + 16, state.PopularSupport);
```

- [ ] **Step 2: 运行 `dotnet test` 确保 13/13 测试全胜通过**
