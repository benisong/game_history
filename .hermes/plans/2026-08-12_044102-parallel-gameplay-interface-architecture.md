# game_history 平行玩法链路与 C# 接口化设计

> 设计阶段：本文件只冻结架构与迁移边界，不修改现有运行代码、不切换默认场景、不接入新链路。

## 目标

在不破坏当前稳定 `MainScene` 链路的前提下，建立一条完全平行的新玩法链路：以 C# 小型接口、命令对象、结果对象和适配器为边界，逐模块迁移东汉玩法；未来能够增加新的项目、规则集和玩法模块，而不需要继续扩张 `MainScene` 或制造大量隐式 `partial` 依赖。

## 当前基线

- 稳定代码基线：`4e315ac`。
- 当前最新修复提交：`8ade5a5`。
- 旧链路入口：`donghan/Frontend/MainScene.tscn` → `MainScene.cs`。
- 旧链路继续运行、继续编译、继续作为回退基线；迁移期间不重构其业务实现。
- 旧链路的 `MainScene.*.cs` partial 文件视为 Legacy，不作为新架构的共享基类。
- 后端当前已有 `GameEngine`、`GameState`、行动结算和测试，第一阶段通过适配器复用，不改规则结果。

## 总体架构

```text
Legacy 链路（冻结）
MainScene.tscn → MainScene partials → GameEngine

Parallel V2 链路（新增）
MainSceneV2.tscn
  → MainSceneV2 / V2 UI modules
  → GameplayRuntime
  → domain contracts（纯 C#，不依赖 Godot）
  → adapters
  → 现有 GameEngine / GameState
```

迁移顺序：

```text
契约模型
→ 旧引擎适配器
→ V2 空壳场景
→ 状态只读纵向切片
→ 西园完整纵向切片
→ 起驾/情报/朝会/快进
→ 新旧双轨对照
→ 明确批准后切默认入口
```

## 设计原则

1. **旧链路冻结**：新功能不得通过修改旧 `MainScene` 进入生产流程。
2. **新类型隔离**：V2 使用 `MainSceneV2`，不使用 `partial class MainScene` 承载新实现。
3. **接口优先**：UI 依赖能力接口，不依赖 `GameEngine`、可变 `GameState` 或 Godot 控件之外泄漏的具体实现。
4. **小接口**：按能力拆分，不创建包含几十个方法的 `IGameEngine` 巨型接口。
5. **纯领域契约**：接口、命令、结果、快照不引用 `Godot.*`。
6. **适配器复用旧规则**：第一阶段只包住现有 `GameEngine`，不重写已验证规则。
7. **明确失败**：服务返回结构化失败结果；UI 不静默吞错，也不伪造成功报告。
8. **可测试替换**：每个 V2 服务均可用 Fake 替换，以测试 UI 编排和错误路径。
9. **状态单向流动**：UI 发命令 → 服务执行 → 返回结果/状态变化 → Presenter 展示。
10. **每个纵向切片独立 checkpoint**：构建、后端测试、Godot smoke 验证通过后才提交/推送。

## 新目录

```text
donghan/Frontend/
├── MainScene.cs                         # Legacy，冻结
├── MainScene.*.cs                       # Legacy，冻结
├── MainScene.tscn                       # Legacy，冻结
│
├── V2/
│   ├── MainSceneV2.cs                   # 新场景生命周期和依赖组装
│   ├── MainSceneV2.Runtime.cs           # Runtime 初始化、状态刷新
│   ├── MainSceneV2.Navigation.cs        # 页面切换、返回、窗口边界
│   ├── MainSceneV2.Reports.cs           # V2 报告 UI
│   ├── MainSceneV2.WestGarden.cs        # 西园模块
│   ├── MainSceneV2.Travel.cs            # 起驾模块
│   ├── MainSceneV2.Intel.cs             # 情报模块
│   ├── MainSceneV2.Court.cs             # 朝会模块
│   ├── MainSceneV2.FastForward.cs       # 快进模块
│   └── MainSceneV2.tscn                 # V2 独立场景
│
├── V2.Contracts/
│   ├── IGameStateReader.cs
│   ├── ITravelService.cs
│   ├── IWestGardenService.cs
│   ├── IIntelService.cs
│   ├── ICourtService.cs
│   ├── ITurnService.cs
│   ├── IReportPresenter.cs
│   ├── ISceneNavigator.cs
│   └── GameplayContracts.cs
│
├── V2.Adapters/
│   ├── GameEngineStateReader.cs
│   ├── GameEngineTravelService.cs
│   ├── GameEngineWestGardenService.cs
│   ├── GameEngineIntelService.cs
│   ├── GameEngineCourtService.cs
│   ├── GameEngineTurnService.cs
│   └── V2RuntimeFactory.cs
│
└── V2.Fakes/
    ├── FakeStateReader.cs
    ├── FakeTravelService.cs
    ├── FakeWestGardenService.cs
    └── FakeTurnService.cs
```

说明：第一阶段可以把契约和适配器放在同一个 `DonghanFrontend` 项目中，避免修改 `.csproj` 引入新程序集；当边界稳定后，再决定是否拆为独立 `DonghanGameplay.Contracts` 项目。不能在第一阶段为了形式拆分而增加程序集复杂度。

## 契约设计

### 状态读取

```csharp
public interface IGameStateReader
{
    GameStateSnapshot GetSnapshot();
    ProvinceSnapshot? GetProvince(string provinceId);
    IReadOnlyList<MinisterSnapshot> GetMinisters();
}
```

`GameStateSnapshot` 只包含 UI 和应用层需要的不可变数据：时间、地点、资源、结局状态、军队摘要等。不得把 `GameState` 原对象返回给 UI。

### 场景移动

```csharp
public interface ITravelService
{
    ActionResult Travel(TravelCommand command);
}

public sealed record TravelCommand(string Destination);
```

### 西园能力

```csharp
public interface IWestGardenService
{
    ActionResult PayArmy(ArmyPayCommand command);
    ActionResult DrillArmy(ArmyDrillCommand command);
    ActionResult RecruitArmy(RecruitArmyCommand command);
}

public sealed record ArmyPayCommand(int Amount, string OfficerId);
public sealed record ArmyDrillCommand(int Amount, string OfficerId);
public sealed record RecruitArmyCommand(int Troops);
```

### 情报能力

```csharp
public interface IIntelService
{
    ProvinceIntelResult InspectProvince(InspectProvinceCommand command);
    ActionResult ExecuteProvinceAction(ProvinceActionCommand command);
}
```

具体行动使用枚举/值对象，不使用散落字符串：

```csharp
public sealed record ProvinceActionCommand(
    string ProvinceId,
    ProvinceActionKind Action,
    string? OfficerId,
    int Troops,
    int ReliefGold,
    string? Strategy);
```

### 朝会能力

```csharp
public interface ICourtService
{
    CourtTopicsResult GetTopics();
    CourtDebateResult SelectTopic(string topicId);
    ActionResult ExecuteDecision(CourtDecisionCommand command);
}
```

### 时间推进

```csharp
public interface ITurnService
{
    TurnAdvanceResult AdvanceXun();
    FastForwardResult FastForward(FastForwardCommand command);
}
```

### 报告展示

领域层只返回报告数据：

```csharp
public sealed record ActionResult(
    bool Success,
    string Title,
    string StoryText,
    ReportKind Kind,
    IReadOnlyList<StateChange> Changes,
    string? ErrorCode = null);
```

Godot 层单独实现：

```csharp
public interface IReportPresenter
{
    void Show(ActionResult result);
}
```

`IReportPresenter` 可以位于 UI 应用层；它的实现允许使用 Godot，但领域契约和服务接口不能返回 `Panel`、`Button`、`Label`。

### 导航

```csharp
public interface ISceneNavigator
{
    void OpenCourt();
    void OpenIntel();
    void OpenWestGarden();
    void OpenTravel();
    void CloseCurrent();
}
```

V2 页面模块只调用导航接口，不直接操纵 `WindowManager`。

## 依赖组装

唯一允许知道具体实现的地方是 `V2RuntimeFactory`：

```csharp
public sealed class V2Runtime
{
    public IGameStateReader State { get; }
    public ITravelService Travel { get; }
    public IWestGardenService WestGarden { get; }
    public IIntelService Intel { get; }
    public ICourtService Court { get; }
    public ITurnService Turns { get; }
}
```

组装关系：

```text
V2RuntimeFactory
  ├── new GameState()
  ├── new MockScheduler / MockOracle / ...
  ├── new GameEngine(...)
  ├── new GameEngineStateReader(engine)
  ├── new GameEngineTravelService(engine)
  ├── new GameEngineWestGardenService(engine)
  └── ...
```

未来替换玩法时只替换工厂注册：

```text
LateHanGameplayFactory
ThreeKingdomsGameplayFactory
FantasyCourtGameplayFactory
```

V2 UI 不变。

## 玩法模块扩展边界

第二阶段以后再增加：

```csharp
public interface IGameplayModule
{
    string ModuleId { get; }
    void Register(GameplayRegistry registry);
}
```

第一阶段不立即引入动态插件、反射扫描或外部程序集加载。先使用显式工厂和显式注册，确保调试和 Godot 导出可靠。模块注册稳定后，再评估是否需要插件化。

## 迁移阶段

### Phase 0：契约冻结

只新增纯 C# 契约、快照和命令对象；不改 Legacy，不新增 Godot UI。

验收：前端 build、后端 88 项测试通过。

### Phase 1：旧引擎适配器

新增 `GameEngine*Service`，将现有动作包装成接口；补充适配器测试，重点验证：成功结果、失败结果、状态变化和报告字段。

验收：旧链路不变；新适配器不改变规则。

### Phase 2：V2 空壳

新增 `MainSceneV2.tscn` 和 `MainSceneV2.cs`，显示状态快照、四个导航入口、报告占位。默认入口仍为 Legacy。

验收：V2 可单独启动，Legacy 仍能启动。

### Phase 3：西园纵向切片

实现：

```text
V2 主界面 → 西园 → 状态读取 → 发饷/募兵 → ActionResult → 报告 → 关闭返回
```

验收：旧链路和 V2 在相同初始状态下结果一致；完成真实 Godot 操作验证。

### Phase 4：起驾、情报、朝会、快进

严格一次迁移一个模块，每个模块独立测试和 checkpoint。

### Phase 5：双轨对照

使用相同初始状态和命令序列比较：资源、地点、兵力、士气、忠诚、Chronicle、事件和报告类型。

### Phase 6：入口切换

只有用户明确要求切换时，才把默认入口改为 V2；Legacy 场景保留为回退入口。

## 测试设计

### 契约/模型测试

- 命令边界和非法值拒绝；
- 快照不暴露可变对象；
- 失败结果包含稳定错误码；
- 状态变化列表与实际状态一致。

### 适配器测试

- 每个接口至少覆盖成功和失败路径；
- 使用现有 `GameEngine` 真实规则，不只测试 Mock；
- 比较适配器结果与直接旧引擎结果。

### V2 UI 测试

- V2 场景可启动；
- 状态文本来自快照；
- 点击动作只通过接口；
- 报告展示和关闭；
- 服务失败时显示错误报告，不静默返回。

### 双轨 E2E

最小流程：

```text
进入西园
→ 发饷
→ 关闭军簿
→ 返回主界面
```

随后扩展：

```text
起驾 → 情报选州 → 朝会选题 → 执行决策 → 快进
```

## 明确不做的事情

- 不直接 cherry-pick `093c13c`；
- 不把 `MainScene` 再拆成更多共享隐式字段的 partial；
- 不修改 Legacy 业务逻辑以配合 V2；
- 不创建 `IGameEngine` 巨型接口；
- 不让接口返回 Godot 控件；
- 不在第一阶段引入反射插件系统；
- 不在 V2 未完成双轨验证前切换默认入口；
- 不把 Fake 测试通过表述成真实 Godot E2E 通过。

## 首个实现 checkpoint

第一阶段完成后必须满足：

```text
[ ] Contracts 编译
[ ] GameEngine 适配器编译
[ ] V2RuntimeFactory 编译
[ ] Legacy 前端 build 通过
[ ] 后端 88/88 通过
[ ] Git diff --check 通过
[ ] 工作区只包含 V2 新链路文件
[ ] commit + push checkpoint
```

## 待用户确认的关键决策

1. V2 第一条纵向切片是否采用“西园”作为首个完整玩法？建议：是。
2. 第一阶段契约和适配器是否继续放在现有 `DonghanFrontend` 项目内？建议：是，先减少程序集耦合。
3. 是否保持 Legacy 为默认入口，V2 通过独立场景启动？建议：是。
4. 是否先使用显式 `V2RuntimeFactory`，暂不引入动态插件扫描？建议：是。

以上四项确认后，再进入 Phase 0 编码；当前不修改旧链路，也不切换默认入口。
