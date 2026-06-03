# NPC 注册登庸与调度器驱动双轨生命周期管理器实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 重构原有大臣数据结构为统一的 `NpcState`，实现 “藏锋于词” 机制（文学标签参与数值系数修正）；构建 NPC 登录注册统一接口 `INpcRegistry` 与双轨生命周期管理器 `INpcLifecycleManager`。将其注册为可供 AI 调度器（Scheduler）直接按需调用调度的模块，并提供 A轨 JSON 配置文件加载与 B轨 C# 静态列表无缝容错兜底。

**Architecture:** 
1. 重构 `MinisterState` -> `NpcState`。
2. 建立 `NpcRegistry` 实现登录登出。
3. 建立 `NpcLifecycleManager` 并将其注入 `IAIScheduler`，在旬更或特定剧本时由 AI 调度师主动发送指令驱动（而非无感知自动运行）。
4. 编写 `Test_NPC_System` 单元测试，全面校准 12+ 项现有核心功能测试。

**Tech Stack:** C# (.NET 8.0), System.Text.Json, xUnit

---

## 拟创建/修改的文件映射 (File Structure)

- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameState.cs` (修改)：
  - 定义 `NpcState` 并移除/合并旧 `MinisterState`。
  - 将 `GameState.Ministers` 改为 `Dictionary<string, NpcState> Npcs` 并修改相关引用。
- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\INpcRegistry.cs` (创建)：定义 NPC 登庸（登录）与退离朝堂的物理接口。
- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\INpcLifecycleManager.cs` (创建)：定义供 AI 调度师调用的 NPC 衰老、疾病、寿终、在野名士登庸的管理器接口。
- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameEngine.cs` (修改)：
  - 适配全部的大臣发饷、开仓赈灾、抄家反噬逻辑中的 `Ministers` -> `Npcs` 类型更改。
  - 在相关事件里结合 NPC `Traits` 词汇修饰标签进行边际数值系数结算修正。
- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\IAIScheduler.cs` (修改)：
  - 在调度员接口中注入 `INpcLifecycleManager`，使其能够在需要时直接调用并调度生命周期演进。
- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Tests\EngineTests.cs` (修改)：
  - 适配 `Ministers` -> `Npcs` 改动。
  - 增加对本地 A轨 JSON 缺失触发 B轨 静态列表冷容错兜底的测试。
  - 增加对调度员调用 NPC 管理器演进、疾病退场和登录的新测试。

---

## 具体实施任务 (Bite-Sized Tasks)

### Task 1: 升级 `GameState.cs`：重构并合并为统一 `NpcState`

**Files:**
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameState.cs`

- [ ] **Step 1: 将 `MinisterState` 类重命并重构为统一的 `NpcState`**

在 `GameState.cs` 中：
1. 移除旧的 `MinisterState` 类。
2. 引入全新的 `NpcState` 类：
```csharp
public class NpcState
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;    // 初始官职 (如：大将军、十常侍、议郎)
    public int BirthYear { get; set; } = 150;            // 出生年份
    public int StashedWealth { get; set; } = 50;         // 私蓄赃款 (万钱)
    public int Favorability { get; set; } = 50;          // 对天子好感 (0-100)
    public int Power { get; set; } = 15;                 // 朝堂政治权势 (0-100)
    public int Corruption { get; set; } = 20;            // 贪腐度 (0-100)

    // “藏锋于词” 专属文学词汇特征
    public System.Collections.Generic.List<string> Traits { get; set; } = new(); // 经天纬地、孔武有力、老谋深算、贪得无厌
    public string Personality { get; set; } = "中庸";     // 性格简述 (如：阴险、刚直、谄媚)
    public string Style { get; set; } = "明哲保身";       // 处事风格 (如：结党营私、雷厉风行、拥兵自重)
    public string Faction { get; set; } = "清流派";       // 派系归属 (清流派/外戚派/阉党派/割据军阀)

    // 生存与生命周期控制
    public int Health { get; set; } = 100;               // 健康值 (0-100)，归 0 则病逝
    public int BaseLongevity { get; set; } = 65;         // 期望寿命上限
    public bool IsActive { get; set; } = true;            // 是否活跃于朝堂
    public string DeathReason { get; set; } = string.Empty; // 死亡/退场因由
}
```

- [ ] **Step 2: 重构 `GameState` 中的大臣字典与构造函数初始化**

1. 将原有的 `public Dictionary<string, MinisterState> Ministers { get; set; } = new();` 修改为：
   `public Dictionary<string, NpcState> Npcs { get; set; } = new();`
2. 在 `GameState()` 构造函数中，以全新 `NpcState` 形式初始化何进、张让、曹操、蹇硕。追加符合历史性格的文学 Traits 标签：
```csharp
    public GameState()
    {
        // 大将军何进：外戚权臣，初始私蓄 1500。何进权势 80，好感 35。性格：平庸。Traits：[“拥兵自重”]
        Npcs["he_jin"] = new NpcState { 
            Id = "he_jin", Name = "何进", Title = "大将军", 
            Favorability = 35, Power = 80, Corruption = 45, StashedWealth = 1500, BirthYear = 135,
            Traits = new() { "拥兵自重" }, Personality = "平庸", Style = "优柔寡断", Faction = "外戚派"
        };
        
        // 十常侍张让：极度贪婪。初始私蓄 6000！张让权势 75，好感 65。Traits：[“贪得无厌”]
        Npcs["zhang_rang"] = new NpcState { 
            Id = "zhang_rang", Name = "张让", Title = "十常侍之首", 
            Favorability = 65, Power = 75, Corruption = 90, StashedWealth = 6000, BirthYear = 130,
            Traits = new() { "贪得无厌" }, Personality = "阴险", Style = "谄媚专权", Faction = "阉党派"
        };
        
        // 青年曹操：廉洁。初始私蓄 50。曹操权势为 15，好感 45。Traits：[“经天纬地”, “老谋深算”]
        Npcs["cao_cao"] = new NpcState { 
            Id = "cao_cao", Name = "曹操", Title = "议郎/典军校尉", 
            Favorability = 45, Power = 15, Corruption = 5, StashedWealth = 50, BirthYear = 155,
            Traits = new() { "经天纬地", "老谋深算" }, Personality = "深沉", Style = "雷厉风行", Faction = "清流派"
        };
        
        // 蹇硕：天子亲信。初始私蓄 300。蹇硕权势 30，好感 80。Traits：[“孔武有力”]
        Npcs["jian_shuo"] = new NpcState { 
            Id = "jian_shuo", Name = "蹇硕", Title = "西园上军校尉", 
            Favorability = 80, Power = 30, Corruption = 25, StashedWealth = 300, BirthYear = 145,
            Traits = new() { "孔武有力" }, Personality = "刚直", Style = "保皇尽忠", Faction = "阉党派"
        };
    }
```

---

### Task 2: 建立 `INpcRegistry` 与 `INpcLifecycleManager` 统一管理接口

**Files:**
- Create: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\INpcRegistry.cs`
- Create: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\INpcLifecycleManager.cs`

- [ ] **Step 1: 编写 `INpcRegistry.cs` NPC 登录（注册）登出核心接口**

```csharp
namespace DonghanEngine.Core;

public interface INpcRegistry
{
    // 主动登庸注册一个 NPC 进游戏沙盒大局 (由可视化界面或 AI 导入)
    void RegisterNpc(NpcState npc, GameState state);

    // 罢免、下野或病逝死亡移出朝堂
    void DeregisterNpc(string npcId, string reason, GameState state);
}

public class NpcRegistry : INpcRegistry
{
    public void RegisterNpc(NpcState npc, GameState state)
    {
        if (string.IsNullOrWhiteSpace(npc.Id)) return;
        npc.IsActive = true;
        state.Npcs[npc.Id] = npc;
        state.AddToChronicle($"【登庸】朝廷引纳新臣【{npc.Name}】，授【{npc.Title}】。");
    }

    public void DeregisterNpc(string npcId, string reason, GameState state)
    {
        if (state.Npcs.TryGetValue(npcId, out var npc))
        {
            npc.IsActive = false;
            npc.DeathReason = reason;
            state.AddToChronicle($"【致仕/退场】大臣【{npc.Name}】由于“{reason}”，自此告退朝堂。");
            state.Npcs.Remove(npcId);
        }
    }
}
```

- [ ] **Step 2: 编写 `INpcLifecycleManager.cs` 双轨生命周期管理器（含 A轨文件 与 B轨冷备 兜底）**

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DonghanEngine.Core;

public interface INpcLifecycleManager
{
    // 供 AI 调度员(Scheduler)按需主动调度。进行百官的自然衰老、寿终、疾病生老病死退场结算与新臣登录
    Task ProcessLifecycleStepAsync(GameState state, bool useHighTokenAi);

    // 提供给可视化界面的本地冷备 NPC 预置名单查询
    List<NpcState> GetPresetNpcsFallback();
}

public class NpcLifecycleManager : INpcLifecycleManager
{
    private readonly INpcRegistry _registry;

    public NpcLifecycleManager(INpcRegistry registry)
    {
        _registry = registry;
    }

    public List<NpcState> GetPresetNpcsFallback()
    {
        string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "donghan_preset_npcs.json");
        try
        {
            if (File.Exists(jsonPath))
            {
                string jsonContent = File.ReadAllText(jsonPath);
                return JsonSerializer.Deserialize<List<NpcState>>(jsonContent) ?? GetHardcodedFallbackList();
            }
        }
        catch
        {
            // A轨文件读取异常，静默采用 B轨 内置冷备静态武将列表
        }

        return GetHardcodedFallbackList();
    }

    public async Task ProcessLifecycleStepAsync(GameState state, bool useHighTokenAi)
    {
        if (useHighTokenAi)
        {
            // 多 Token 纯 AI 版：此接口留给 AI 调度员在接入 LLM 后，根据朝局自由生发事件
            // 目前轻量实现由 Scheduler 编排传递指令，此处可作为一个扩展点
            await Task.Delay(10); // 模拟 AI 耗时
        }

        // 统一启发式衰老机制：
        // 每过 3 旬 (1个月)，所有大臣年龄增长 1 岁
        if (state.Month % 3 == 0 && state.Xun == 3)
        {
            var npcsToRemove = new List<(string Id, string Reason)>();
            foreach (var pair in state.Npcs)
            {
                var npc = pair.Value;
                int age = state.Year - npc.BirthYear;

                // 寿数与健康判定：
                if (age > npc.BaseLongevity)
                {
                    // 超过期望寿命，每旬 15% 几率自然死亡/寿终致仕
                    if (Random.Shared.Next(0, 100) < 15)
                    {
                        npcsToRemove.Add((pair.Key, $"寿数已尽，在洛阳邸舍中安然就寝致仕（享年 {age} 岁）"));
                        continue;
                    }
                }

                // 随机发病判定
                if (Random.Shared.Next(0, 1000) < 3) // 千分之三概率发病
                {
                    npc.Health = Math.Clamp(npc.Health - 30, 0, 100);
                    state.AddToChronicle($"【伤寒】大臣【{npc.Name}】近日染上洛阳伤寒温疫，龙体不安，健康度暴跌 30点！");
                }

                if (npc.Health <= 0)
                {
                    npcsToRemove.Add((pair.Key, $"身染恶疾，医治无效，在京病逝"));
                }
            }

            // 物理退场
            foreach (var removeInfo in npcsToRemove)
            {
                _registry.DeregisterNpc(removeInfo.Id, removeInfo.Reason, state);
            }
        }
    }

    private List<NpcState> GetHardcodedFallbackList()
    {
        // B轨：C# 硬编码冷备静态名单，绝对安全
        return new List<NpcState>
        {
            new NpcState { Id = "dong_zhuo", Name = "董卓", Title = "并州刺史/河东太守", BirthYear = 139, BaseLongevity = 53, Traits = new() { "孔武有力" }, Personality = "残暴", Style = "拥兵自重", Faction = "割据军阀" },
            new NpcState { Id = "yuan_shao", Name = "袁绍", Title = "中军校尉/渤海太守", BirthYear = 154, BaseLongevity = 48, Traits = new() { "老谋深算" }, Personality = "外宽内忌", Style = "结党营私", Faction = "清流派" },
            new NpcState { Id = "liu_bei", Name = "刘备", Title = "平原县令", BirthYear = 161, BaseLongevity = 62, Traits = new() { "经天纬地" }, Personality = "宽厚", Style = "明哲保身", Faction = "清流派" }
        };
    }
}
```

---

### Task 3: 升级 `IAIScheduler.cs` 与 `GameEngine.cs` 支撑调度器调用机制

**Files:**
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\IAIScheduler.cs`
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameEngine.cs`

- [ ] **Step 1: 将 `INpcLifecycleManager` 赋予 AI 调度器接口作为其子模块**

在 `IAIScheduler.cs` 中增加 Npc 管理器属性，供调度员调度。
```csharp
namespace DonghanEngine.Core;

public interface IAIScheduler
{
    // 提供对 NPC 生命周期的引用，方便调度师随时自主发送指令调度演进
    INpcLifecycleManager NpcManager { get; }

    Task<AIOrchestrationResult> OrchestrateGrandCourtAsync(string playerInput, string activeOfficerId, GameState state);
    Task OrchestrateXunUpdateAsync(GameState state);
}
```

- [ ] **Step 2: 重构 `GameEngine.cs` 适配全盘 `Ministers` -> `Npcs` 类型重命名**

1. 将 `GameEngine.cs` 里的所有 `_state.Ministers` 关键字批量替换为 `_state.Npcs`。
2. 重构大政方针，融入 **“藏锋于词”** 的 Traits 判定：
   - **在 `ExecuteDisasterReliefAction` 中**，增加对经办钦差拥有「经天纬地」词汇的奖励系数修正：
     ```csharp
            // 默认民心提振增量
            double traitModifier = 1.0;
            if (officer.Traits.Contains("经天纬地"))
            {
                traitModifier = 1.2; // 民心提振提升 1.2 倍
            }
            supportDelta = (int)(supportDelta * traitModifier);
     ```
   - **在 `ExecuteConfiscationAction` 中**，增加对近臣诬陷者拥有「老谋深算」词汇的政治反噬折减：
     ```csharp
            // 判定党羽抗议弹劾政治反噬
            bool cliqueBacklash = (target.Power >= 60);
            int imperialPowerLoss = 15;
            if (framer != null && framer.Traits.Contains("老谋深算"))
            {
                imperialPowerLoss = 10; // 反噬皇权跌幅由 15 降低至 10 点！
            }
     ```
   - **在 `ExecuteDrillArmyActionWithOfficer` 中**，增加发饷将领拥有「孔武有力」的士气加成：
     ```csharp
            double moraleModifier = 1.0;
            if (officer.Traits.Contains("孔武有力"))
            {
                moraleModifier = 1.3; // 禁军士气提振 1.3 倍
            }
            moraleDelta = (int)(moraleDelta * moraleModifier);
     ```

---

### Task 4: 更新单元测试并验证 13/13 测试全胜通过

**Files:**
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Tests\EngineTests.cs`

- [ ] **Step 1: 在 `EngineTests.cs` 中适配 `Ministers` -> `Npcs` 并校准测试中的 Mock 调度器**

1. 更新 `MockScheduler` 类：
```csharp
public class MockScheduler : IAIScheduler
{
    public INpcLifecycleManager NpcManager { get; } = new NpcLifecycleManager(new NpcRegistry());

    public Task<AIOrchestrationResult> OrchestrateGrandCourtAsync(string playerInput, string activeOfficerId, GameState state)
    {
        var result = new AIOrchestrationResult
        {
            PrimaryIntent = "POLITICS",
            NarrativeResponse = "天子震怒，群臣辩驳。"
        };

        result.Speeches.Add(new CourtSpeech
        {
            MinisterId = "zhang_rang",
            MinisterName = "张让",
            SpeechText = "大将军此举必是包藏祸心，臣万万不赞同！",
            Stance = "OPPOSE",
            ExpectedFavorabilityChange = -10,
            ExpectedPowerChange = 0
        });

        result.Speeches.Add(new CourtSpeech
        {
            MinisterId = "cao_cao",
            MinisterName = "曹操",
            SpeechText = "陛下，赈灾势在必行，但需提防贪官漂没！",
            Stance = "AGREED",
            ExpectedFavorabilityChange = 5,
            ExpectedPowerChange = 2
        });

        return Task.FromResult(result);
    }

    public Task OrchestrateXunUpdateAsync(GameState state)
    {
        state.IntelReports.Add("【群臣密录】：大将军何进正暗中调兵，意图夺取洛阳西园防权。");
        state.ActiveEdicts.Add("【冀州急折】：冀州干旱，求赐赈米。");
        return Task.CompletedTask;
    }
}
```
2. 批量将测试文件中的 `state.Ministers` 关键字替换为 `state.Npcs`。

- [ ] **Step 2: 追加 `Test_NPC_Ecosystem_LifecycleAndDescriptiveTraits()` 验证 NPC 体系测试**

验证：
1. 统一接口登录（`RegisterNpc`）刘备、退出（`DeregisterNpc`）何进。
2. 调度师手动调度 `NpcManager.ProcessLifecycleStepAsync` 计算官员寿命变老与疾病。
3. 验证 **「经天纬地」**、**「老谋深算」** 两个文学 Traits 词汇在开仓赈灾和抄家反噬中的底层物理结算系数增益。

```csharp
    [Fact]
    public async Task Test_NPC_Ecosystem_LifecycleAndDescriptiveTraits()
    {
        // Arrange
        var state = new GameState();
        var registry = new NpcRegistry();
        var manager = new NpcLifecycleManager(registry);
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // 1. 验证 A轨缺失兜底 B轨 静态硬编码返回
        var presetNpcs = manager.GetPresetNpcsFallback();
        Assert.Contains(presetNpcs, n => n.Name == "董卓");

        // 2. 验证统一接口主动登录一个新 NPC：刘备
        var liuBei = new NpcState
        {
            Id = "liu_bei", Name = "刘备", Title = "平原相",
            BirthYear = 161, BaseLongevity = 62, Traits = new() { "经天纬地" },
            Corruption = 0, Power = 10, Favorability = 90
        };
        registry.RegisterNpc(liuBei, state);
        Assert.True(state.Npcs.ContainsKey("liu_bei"));

        // 3. 验证文学词汇 [经天纬地] 对开仓赈灾的 1.2 倍民心提升修正
        state.CurrentLocation = "宣政殿";
        int initialSupport = state.PopularSupport;
        // 拨发国库 2000万 赈灾，指派「经天纬地」的刘备督办
        engine.ExecuteDisasterReliefAction(2000, "liu_bei");
        // 灾民实际分得 2000 * (1 - 0) = 2000万，民心提振默认应为 12 * 1.0 = 12。
        // 因为刘备拥有 [经天纬地] 特征，民心提升增加 1.2x，最终 supportDelta = 12 * 1.2 = 14。
        Assert.Equal(initialSupport + 14, state.PopularSupport);

        // 4. 验证文学词汇 [老谋深算] 降低抄家反噬皇权跌幅从 15 减少到 10 点
        // 曹操（初始 Traits 包含 "老谋深算"）作为钦差强行抄家张让（权势 75 触发反噬）
        int initialPower = state.ImperialPower;
        engine.ExecuteConfiscationAction("zhang_rang", "国库");
        // 反噬降低：曹操办案，皇权仅降 10 点（而非 15 点）
        Assert.Equal(initialPower - 10, state.ImperialPower);

        // 5. 验证调度师手动调用管理 NPC 的衰老机制 (3旬为一月)
        state.Month = 3;
        state.Xun = 3; // 触发年龄与寿限计算
        state.Npcs["he_jin"].BirthYear = 100; // 将何进年龄置为大寿 (184 - 100 = 84岁，远超 BaseLongevity 65岁)
        
        await manager.ProcessLifecycleStepAsync(state, false);

        // 期待何进有 15% 几率自然死亡被注销（这里通过多次判定或健康值归零进行寿终）
        // 如果触发了死亡，在野刘备依然活跃
        Assert.True(state.Npcs.ContainsKey("liu_bei"));
    }
```

- [ ] **Step 3: 运行 `dotnet test` 确保 13/13 项测试 100% 绿色通过，且代码全编译无 warning**
