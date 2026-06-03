# 汉灵帝昏聩开局与初始政局危机数值重置实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 调整汉灵帝初始属性（极低皇权、垂死健康、濒危民心）与四大权臣重置，以高度契合中平年间的大厦将倾历史，带给玩家极致的庙堂博弈危机感。同时修复受初始属性影响而红损的 xUnit 单元测试。

**Architecture:** 修改 C# 后端 `GameState.cs` 的初始构造函数，重置各项历史参数；并在 `EngineTests.cs` 中对受属性变动影响的初始 Assert 值进行校准适配。

**Tech Stack:** C# (.NET 8.0), xUnit

---

## 拟创建/修改的文件映射 (File Structure)

- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameState.cs` (修改)：重置天子（皇权 25、健康 35、民心 28、国库 8000、私库 1200）和四位大臣的历史真实数值。
- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Tests\EngineTests.cs` (修改)：校准单元测试对初始资源（Treasury / PrivateTreasury 等）的 Assert 断言值，保证全部绿色通过。

---

## 具体实施任务 (Bite-Sized Tasks)

### Task 1: 修改 `GameState.cs` 的天子与重臣初始构造参数

**Files:**
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameState.cs`

- [ ] **Step 1: 升级属性和 Ministers 初始值**

在 `GameState` 类的属性定义和 `GameState()` 构造函数中修改以下值：
```csharp
    public int ImperialPower { get; set; } = 25; // 皇权 (0-100) - 初始极弱，政令不出宫门
    public int Treasury { get; set; } = 8000;    // 朝廷国库 (万钱) - 初始开支窘迫
    public int PrivateTreasury { get; set; } = 1200; // 西园天子私库 (万钱) - 内库告急
    public int PopularSupport { get; set; } = 28;  // 天下民心 (0-100) - 跌破活命红线，黄巾蠢动
    public int Health { get; set; } = 35;        // 皇帝健康 (0-100) - 龙体极其虚弱，沉迷享乐濒危
```

修改构造函数中四位大臣的权势、好感：
```csharp
    public GameState()
    {
        // 预设核心大臣势力并配置其符合史实的初始贪腐度与存银(万钱)
        // 大将军何进：外戚权臣，中等贪腐，手握天下重兵。初始私蓄 1500 万钱。何进权势大涨至 80，好感降低至 35
        Ministers["he_jin"] = new MinisterState { Name = "何进", Title = "大将军", Favorability = 35, Power = 80, Corruption = 45, StashedWealth = 1500 };
        
        // 十常侍张让：历史极度贪婪，擅权夺利。初始私蓄 6000 万钱！张让权势调高至 75，好感度 65
        Ministers["zhang_rang"] = new MinisterState { Name = "张让", Title = "十常侍之首", Favorability = 65, Power = 75, Corruption = 90, StashedWealth = 6000 };
        
        // 青年曹操：廉洁自律。初始私蓄 50 万钱。曹操权势为 15，好感 45
        Ministers["cao_cao"] = new MinisterState { Name = "曹操", Title = "议郎/典军校尉", Favorability = 45, Power = 15, Corruption = 5, StashedWealth = 50 };
        
        // 宦官上军校尉蹇硕：天子亲信。初始私蓄 300 万钱。蹇硕权势 30，好感 80
        Ministers["jian_shuo"] = new MinisterState { Name = "蹇硕", Title = "西园上军校尉", Favorability = 80, Power = 30, Corruption = 25, StashedWealth = 300 };
    }
```

---

### Task 2: 校准 `EngineTests.cs` 中受属性改动而影响的 Assert

**Files:**
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Tests\EngineTests.cs`

- [ ] **Step 1: 适配何进与蹇硕在各抄家、阅兵测试中的初始权势和资源值断言**

由于何进的初始 Power 变为了 80（大于等于 60 的反噬线），张让变为了 75，蹇硕变为了 30，国库和私库初始值下调：
1. 校验测试中由于 `PrivateTreasury` 或 `Treasury` 变动导致的断言（如原 `Assert.Equal(initialPrivateTreasury - 1000, state.PrivateTreasury)`，因为是相对差值，通常会自动适配；但一些绝对初始值判断需格外留意）。
2. 在 `Test_Confiscation_WithCliqueBacklash_LootSplittingAndEmbezzlement` 测试中，蹇硕作为清廉经办，他的初始 Power 变为了 30（而不是旧版的 40）。抄家结束后其 Power 成长为 30 + 3 = 33。需要将断言修改为：
```csharp
        // 4. 钦差代表天子执行抄家特权，朝堂权势获得成长 +3（初始 30 + 3 = 33）
        Assert.Equal(30 + 3, state.Ministers["jian_shuo"].Power);
```

- [ ] **Step 2: 运行整个测试套件，并修复任何红字**

在终端运行：`dotnet test`
根据报错定位其他需要校准的初始数值断言（主要是初始数值由于硬编码对不齐导致的报错）。修改测试直到 12/12 单元测试完美通过！
