# NPC 惰性按需上场与动态部署系统实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 实现 `INpcLifecycleManager` 下的 `DeployNpcToCourt(string npcId, GameState state)` 核心登台部署方法。让 NPC 数据具有物理按需隔离，无需开局即载入大量不活跃 NPC，只在 AI 调度员（或特定大势）主动发出部署指令时才上场实例化并推入 GameState，最大化大局沙盒加载性能与沉浸度。

**Architecture:** 
1. 在 `INpcLifecycleManager.cs` 中增加并实现 `DeployNpcToCourt(...)`。
2. 在 `EngineTests.cs` 中追加对按需登台部署的物理断言测试。

**Tech Stack:** C# (.NET 8.0), xUnit

---

## 拟创建/修改的文件映射 (File Structure)

- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\INpcLifecycleManager.cs` (修改)：增加并实现部署方法。
- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Tests\EngineTests.cs` (修改/追加)：编写对应的测试，保障登场逻辑 Regression 为 0。

---

## 具体实施任务 (Bite-Sized Tasks)

### Task 1: 升级 `INpcLifecycleManager.cs` 实现按需上场部署接口

**Files:**
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\INpcLifecycleManager.cs`

- [ ] **Step 1: 在 `INpcLifecycleManager` 接口中追加 `DeployNpcToCourt` 签名**

```csharp
public interface INpcLifecycleManager
{
    // 根据 NPC 唯一 ID（如 "dong_zhuo"），从预置池中搜索并部署（登庸）到朝局 Npcs 字典中上场
    void DeployNpcToCourt(string npcId, GameState state);

    // 供 AI 调度员(Scheduler)按需主动调度。进行百官的自然衰老、寿终、疾病生老病死退场结算与新臣登录
    Task ProcessLifecycleStepAsync(GameState state, bool useHighTokenAi);

    // 提供给可视化界面的本地冷备 NPC 预置名单查询
    List<NpcState> GetPresetNpcsFallback();
}
```

- [ ] **Step 2: 在 `NpcLifecycleManager` 类中实现 `DeployNpcToCourt` 方法**

```csharp
    public void DeployNpcToCourt(string npcId, GameState state)
    {
        if (state.Npcs.ContainsKey(npcId))
        {
            return; // 已经部署在朝堂上，无需重复加载
        }

        // 1. 从冷备 A轨/B轨 列表中寻找对应的静态资料模板
        var presets = GetPresetNpcsFallback();
        var template = presets.Find(n => n.Id == npcId);

        if (template != null)
        {
            // 2. 存在模板，反序列化实例化新 NPC 登堂
            var newCourtNpc = new NpcState
            {
                Id = template.Id,
                Name = template.Name,
                Title = template.Title,
                BirthYear = template.BirthYear,
                BaseLongevity = template.BaseLongevity,
                Traits = new List<string>(template.Traits),
                Personality = template.Personality,
                Style = template.Style,
                Faction = template.Faction,
                
                // 赋以初始动态属性
                Health = 100,
                Favorability = 50,
                Power = 15,
                Corruption = template.Id == "dong_zhuo" ? 70 : 20, // 董卓作为割据军阀给予符合历史的高初始贪腐，其余 20
                StashedWealth = template.Id == "dong_zhuo" ? 500 : 50, // 初始资本
                IsActive = true
            };

            // 3. 调用 Registry 统一注册通道使其进入朝堂
            _registry.RegisterNpc(newCourtNpc, state);
            state.AddToChronicle($"【部署】并州刺史【{template.Name}】受到朝局大势感召，奉天子诏命正式踏上宣政殿！");
        }
    }
```

---

### Task 2: 升级 `EngineTests.cs` 断言测试并验证全编译

**Files:**
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Tests\EngineTests.cs`

- [ ] **Step 1: 在 `Test_NPC_Ecosystem_LifecycleAndDescriptiveTraits` 单元测试中追加物理断言**

在 `Test_NPC_Ecosystem_LifecycleAndDescriptiveTraits` 单元测试的尾部追加对按需部署董卓、以及董卓成功上台后的状态断言：
```csharp
        // 7. 验证按需惰性登场部署（DeployNpcToCourt）
        // 游戏初始状态下，Npcs 字典中不包含尚未上场的董卓 (dong_zhuo)
        Assert.False(state.Npcs.ContainsKey("dong_zhuo"));

        // 模拟 AI 调度师主动发送指令部署董卓上场
        manager.DeployNpcToCourt("dong_zhuo", state);

        // 董卓此时应成功从 A/B 轨冷备库中脱水实例化上台
        Assert.True(state.Npcs.ContainsKey("dong_zhuo"));
        var dongZhuo = state.Npcs["dong_zhuo"];
        Assert.Equal("董卓", dongZhuo.Name);
        Assert.Equal("割据军阀", dongZhuo.Faction);
        Assert.Contains("孔武有力", dongZhuo.Traits);
        Assert.Equal(100, dongZhuo.Health); // 初始健康的满额 100 状态
```

- [ ] **Step 2: 运行 `dotnet test` 确保 13/13 测试全数完美绿色通过**

Run: `dotnet test`
Expected: 13 tests passed.
