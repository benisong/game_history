# 汉灵帝政务奏折与官阶跃迁反噬系统实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 实现五大类政务奏折的处理管线，以及包含跨级提拔反噬预警的官秩层级（Title Tiers）系统。

**Architecture:** 
1. 新建 `ImperialEdict.cs` 定义枚举与奏折、选项数据模型。
2. 在 `GameState.cs` 中升级 `ActiveEdicts` 类型，并追加 `TitleTier` 到 NPC 模型。
3. 在 `GameEngine.cs` 中实现 `ResolveEdictAction` 处理物理结算与跨级提拔皇权暴跌。
4. 在旬更流逝时，对 `ActiveEdicts` 进行 `ExpiryXun`（生命周期）扣减，过期直接流产并结算惩罚。

**Tech Stack:** C# (.NET 8.0), xUnit

---

## 拟创建/修改的文件映射 (File Structure)

- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\ImperialEdict.cs` (创建)：定义 EdictType 枚举与折子模型。
- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameState.cs` (修改)：追加官阶，升级奏折队列。
- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameEngine.cs` (修改)：增加过期扫描与批复结算方法。
- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Tests\EngineTests.cs` (修改/追加)：编写对应的测试，保障逻辑 Regression 为 0。

---

## 具体实施任务 (Bite-Sized Tasks)

### Task 1: 创建奏折模型与升级 NPC 官秩阶梯

**Files:**
- Create: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\ImperialEdict.cs`
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameState.cs`

- [ ] **Step 1: 创建 `ImperialEdict.cs`**

```csharp
using System;
using System.Collections.Generic;

namespace DonghanEngine.Core;

public enum EdictType
{
    Proposal,      // 建议类 (开仓、修防)
    Remonstrance,  // 劝诫类 (直言犯颜)
    Impeachment,   // 弹劾类 (派系党争)
    Merit,         // 邀功类 (请赏银或官职)
    UrgentCrisis   // 急报类 (天灾、暴动)
}

public class EdictOption
{
    public string Description { get; set; } = string.Empty;
    public string ConsequencePreview { get; set; } = string.Empty;
    
    public int ImperialPowerDelta { get; set; } = 0;
    public int TreasuryDelta { get; set; } = 0;
    public int PrivateTreasuryDelta { get; set; } = 0;
    public int PopularSupportDelta { get; set; } = 0;
    public int TargetNpcPowerDelta { get; set; } = 0;
    public int TargetNpcFavorabilityDelta { get; set; } = 0;
    
    public int GrantedTitleTierDelta { get; set; } = 0; 
}

public class ImperialEdict
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public EdictType Type { get; set; }
    public string SubmittingNpcId { get; set; } = string.Empty; 
    public string TargetNpcId { get; set; } = string.Empty; // 受益/受罚的主要对象
    public string NarrativeContent { get; set; } = string.Empty;
    public int ExpiryXun { get; set; } = 3; // 剩余保质期
    public List<EdictOption> Options { get; set; } = new();
}
```

- [ ] **Step 2: 修改 `GameState.cs`**

1. 将 `List<string> ActiveEdicts` 替换为 `List<ImperialEdict> ActiveEdicts`。
2. 在 `NpcState` 中追加字段 `public int TitleTier { get; set; } = 0; // 0-4级官阶`。
3. 在 `GameState()` 构造函数中，为何进设置 `TitleTier = 4`，张让 `TitleTier = 3`，曹操 `TitleTier = 1`，蹇硕 `TitleTier = 2`。

---

### Task 2: 升级 `GameEngine.cs` 实现奏折审批与过期流产反噬

**Files:**
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameEngine.cs`

- [ ] **Step 1: 在 `GameEngine` 中实现 `ResolveEdictAction`**

处理天子批阅奏折的物理落账：
```csharp
    public TurnResult ResolveEdictAction(string edictId, int optionIndex)
    {
        var result = new TurnResult();
        var edict = _state.ActiveEdicts.Find(e => e.Id == edictId);
        if (edict == null) throw new ArgumentException("无此奏折！");
        if (optionIndex < 0 || optionIndex >= edict.Options.Count) throw new ArgumentException("无效的御批选项！");

        var option = edict.Options[optionIndex];

        // 基础数值结算
        _state.ApplyNumericalDelta(option.ImperialPowerDelta, option.TreasuryDelta, 0);
        _state.PrivateTreasury = Math.Clamp(_state.PrivateTreasury + option.PrivateTreasuryDelta, 0, 999999);
        _state.PopularSupport = Math.Clamp(_state.PopularSupport + option.PopularSupportDelta, 0, 100);

        string promoBacklashText = "";

        if (!string.IsNullOrEmpty(edict.TargetNpcId) && _state.Npcs.TryGetValue(edict.TargetNpcId, out var targetNpc))
        {
            targetNpc.Power = Math.Clamp(targetNpc.Power + option.TargetNpcPowerDelta, 0, 100);
            targetNpc.Favorability = Math.Clamp(targetNpc.Favorability + option.TargetNpcFavorabilityDelta, 0, 100);

            // 处理跨级提拔反噬
            if (option.GrantedTitleTierDelta > 0)
            {
                targetNpc.TitleTier = Math.Clamp(targetNpc.TitleTier + option.GrantedTitleTierDelta, 0, 4);
                
                if (option.GrantedTitleTierDelta >= 2)
                {
                    int backlash = 5 * (option.GrantedTitleTierDelta - 1);
                    _state.ImperialPower = Math.Clamp(_state.ImperialPower - backlash, 0, 100);
                    promoBacklashText = $"\n[color=red]● 跨级拔擢反噬：朝野非议，皇权暴跌 -{backlash}点！[/color]";
                }
            }
        }

        _state.ActiveEdicts.Remove(edict);
        _state.AddToChronicle($"【御批】天子批阅《{edict.Title}》，决断：{option.Description}");
        
        result.StoryText = $"【政务决断】\n\n陛下朱批已下。\n{promoBacklashText}";
        return result;
    }
```

- [ ] **Step 2: 在 `NextXunAsync` 中追加过期衰减机制**

在 `NextXunAsync` 底部添加：
```csharp
        // 奏折过期与流产判定
        var expiredEdicts = new List<ImperialEdict>();
        foreach (var edict in _state.ActiveEdicts)
        {
            edict.ExpiryXun--;
            if (edict.ExpiryXun <= 0)
            {
                expiredEdicts.Add(edict);
            }
        }

        foreach (var expired in expiredEdicts)
        {
            _state.ActiveEdicts.Remove(expired);
            // 流产惩罚：如果是急报，民心暴跌
            if (expired.Type == EdictType.UrgentCrisis)
            {
                _state.PopularSupport = Math.Clamp(_state.PopularSupport - 15, 0, 100);
                _state.AddToChronicle($"【国难】《{expired.Title}》留中不发，导致灾情/兵变恶化，民心大跌！");
            }
            else
            {
                _state.ImperialPower = Math.Clamp(_state.ImperialPower - 2, 0, 100);
                _state.AddToChronicle($"【怠政】《{expired.Title}》过期未批，朝堂议论天子怠政。");
            }
        }
```

---

### Task 3: 更新测试并验证全编译通过

**Files:**
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Tests\EngineTests.cs`
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\IAIScheduler.cs` (修复 Mock 里的 ActiveEdicts 类型)

- [ ] **Step 1: 适配 `MockScheduler` 的字符串转模型**

由于 `ActiveEdicts` 类型变了，修改 `IAIScheduler` 在各处 mock 或真实代码中追加假数据的类型。在 `MockScheduler` 里：
```csharp
    public Task OrchestrateXunUpdateAsync(GameState state)
    {
        state.IntelReports.Add("【群臣密录】：大将军何进正暗中调兵，意图夺取洛阳西园防权。");
        // 改为插入模型
        state.ActiveEdicts.Add(new ImperialEdict {
            Title = "冀州急折", Type = EdictType.UrgentCrisis, NarrativeContent = "冀州干旱，求赐赈米。"
        });
        return Task.CompletedTask;
    }
```

- [ ] **Step 2: 增加测试用例 `Test_Edicts_ResolutionAndPromoBacklash` 及 `Test_Edicts_ExpiryCrisis`**

编写断言测试：
1. 测试跨两级提拔的皇权暴跌。
2. 测试 3 旬不批阅 `UrgentCrisis` 导致民心 -15 暴跌。

- [ ] **Step 3: 运行 `dotnet test` 确保全通过**