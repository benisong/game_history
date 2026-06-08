# 2026-06-08 Godot MainScene partial 拆分记录

## 1. 背景

`donghan/Frontend/MainScene.cs` 在完成御案四入口、动态朝臣列表、尚书台奏折、黄门密札舆图、大朝会弹窗、开局宣召遮罩、不透明模态弹窗等功能后，文件膨胀到约 1481 行。

该文件同时承担：

- Godot 生命周期初始化
- Mock AI 服务
- 样式与布局工具
- 主 UI 刷新
- 场景动作
- 大臣详情与抄家
- 御案入口
- 奏折面板
- 情报舆图面板
- 太守任免、平叛、招安
- 大朝会
- 开局遮罩

这会导致后续继续扩展四策招安、兵力输入、真实 AI 调度接入时维护成本过高。因此本次执行纯结构拆分，将 `MainScene` 改为多文件 `partial class`。

## 2. 拆分原则

本次拆分遵循以下原则：

1. **不改业务逻辑**：只移动代码块，不重写结算和 UI 行为。
2. **按职责聚合**：相关字段和方法放在同一个 partial 文件中。
3. **保留主入口清晰度**：`MainScene.cs` 只保留核心字段、`_Ready()`、`_Process()` 和初始化主流程。
4. **Godot 自动编译兼容**：所有新文件保持同一 namespace：`DonghanFrontend`，同一声明：`public partial class MainScene : Control`。
5. **Mock 服务单独隔离**：临时调试组件从主场景文件中移出，方便后续替换真实 AI 服务。

## 3. 拆分后的文件结构

拆分后 `donghan/Frontend/` 下新增/调整如下文件：

```text
MainScene.cs                     307 行
MainScene.CoreActions.cs         242 行
MainScene.CourtAndOpening.cs     163 行
MainScene.DeskAndAffairs.cs      209 行
MainScene.IntelPanel.cs          220 行
MainScene.Ministers.cs           146 行
MainScene.MockServices.cs        120 行
MainScene.Style.cs               135 行
```

## 4. 各文件职责

### 4.1 `MainScene.cs`

保留主场景核心生命周期与字段声明：

- `_gameEngine`
- `_gameState`
- 常驻 UI 节点引用
- `_windowManager`
- `_Ready()`
- `_Process(double delta)`

定位：**主入口与初始化编排层**。

### 4.2 `MainScene.CoreActions.cs`

承载主界面刷新与基础玩家动作：

- `UpdateUI()`
- `UpdateSceneButtons()`
- `DoTravel(string location)`
- `DoQuickAction(string actionId)`
- `DoArmyDrillAction(int amount)`
- `DoDisasterReliefAction(int amount, string officerId)`
- `OnSubmitButtonPressed()`
- `OnPlayerInputSubmitted(string text)`

定位：**基础操作与主 UI 状态同步层**。

### 4.3 `MainScene.Ministers.cs`

承载大臣相关 UI 与操作：

- `ShowMinisterDetails(string ministerId)`
- `DoConfiscateAction(string destination)`
- `_npcScrollContainer`
- `_npcListVBox`
- `InitializeDynamicNpcList()`
- `UpdateNpcList()`

定位：**朝臣列表、五维详情、抄家交互层**。

### 4.4 `MainScene.DeskAndAffairs.cs`

承载御案入口和尚书台奏折面板：

- `_deskContainer`
- `InitializeEmperorsDesk()`
- `CreateDeskButton(...)`
- `CreateScrollButtonStyle(...)`
- `_affairsPopup`
- `_edictsItemList`
- `_edictContentLabel`
- `_edictOptionsVBox`
- `InitializeAffairsPanel()`
- `OnAffairsBoxPressed()`
- `UpdateAffairsList()`
- `OnEdictSelected(long index)`

定位：**御案物理入口 + 政务奏折处理层**。

### 4.5 `MainScene.IntelPanel.cs`

承载黄门密札、地方舆图与军政操作：

- `_intelPopup`
- `_provinceItemList`
- `_provinceDetailsLabel`
- `_provinceActionsVBox`
- `_intelGlobalStatsLabel`
- `InitializeIntelPanel()`
- `OnIntelTokenPressed()`
- `UpdateIntelProvinceList()`
- `OnProvinceSelected(long index)`

已包含对后端 API 的 UI 对接：

- `_gameEngine.AssignGovernor(...)`
- `_gameEngine.RecallGovernor(...)`
- `_gameEngine.SuppressRebellion(...)`
- `_gameEngine.PacifyRebellion(...)`

定位：**天下情报、郡县治理、平叛招安层**。

### 4.6 `MainScene.CourtAndOpening.cs`

承载大朝会、起驾入口与开局遮罩：

- `_courtPopup`
- `_courtInput`
- `InitializeCourtPanel()`
- `OnCourtSealPressed()`
- `OnConfirmCourtAssembly()`
- `OnPleasureCenserPressed()`
- `_openingOverlay`
- `ShowOpeningOverlay()`

定位：**大朝会流程 + 开局沉浸遮罩层**。

### 4.7 `MainScene.Style.cs`

承载全局样式与布局工具：

- `ForceExclusiveFullscreen()`
- `ConfigureFullScreenBlocker(...)`
- `EnsureOpaqueSceneBackground()`
- `ApplyOpaquePanelTheme(Node root)`
- `CreateOpaquePanelStyle(string panelName)`
- `SetFullRect(Control control)`
- `ConfigureMinisterPanelLayout()`
- `ConfigureWrappingLabel(...)`

定位：**Godot UI 样式、全屏、不透明、防穿透辅助层**。

### 4.8 `MainScene.MockServices.cs`

承载临时 Mock 调试组件：

- `MockScheduler`
- `MockOracle`
- `MockMinisterAgent`
- `MockNarrator`

定位：**前端本地调试依赖层**。后续接入真实 AI 调度时，可以优先替换此文件内容或改为独立服务注入。

## 5. 验证结果

### 5.1 前端编译

执行：

```bash
cd /opt/game_history/donghan/Frontend
dotnet build DonghanFrontend.csproj -v minimal
```

结果：

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

### 5.2 后端测试

执行：

```bash
cd /opt/game_history/donghan/Backend
dotnet test DonghanEngine.Tests/DonghanEngine.Tests.csproj
```

结果：

```text
Passed: 46
Failed: 0
Skipped: 0
Total: 46
```

### 5.3 空白检查

执行：

```bash
git diff --check
```

结果：无输出，表示没有空白错误。

## 6. 当前 git 变更

```text
M  donghan/Frontend/MainScene.cs
?? donghan/Frontend/MainScene.CoreActions.cs
?? donghan/Frontend/MainScene.CourtAndOpening.cs
?? donghan/Frontend/MainScene.DeskAndAffairs.cs
?? donghan/Frontend/MainScene.IntelPanel.cs
?? donghan/Frontend/MainScene.Ministers.cs
?? donghan/Frontend/MainScene.MockServices.cs
?? donghan/Frontend/MainScene.Style.cs
?? docs/superpowers/specs/2026-06-08-godot-mainscene-partial-split.md
```

## 7. 后续建议

拆分完成后，后续推荐按以下顺序继续优化：

1. **完善情报面板四策招安 UI**
   - 当前 `PacifyRebellion` 调用固定使用 `Persuade | SowDiscord`，`reliefGold = 0`。
   - 后续应补齐四个 CheckBox：离间、说服、赈灾、惩治。
   - 赈灾策略需要数值输入 500-1500 万钱。

2. **完善军事平叛兵力输入**
   - 当前 UI 只选择将领。
   - 设计文档中提到应支持调遣兵力输入和胜率预估。

3. **替换 Mock AI 服务**
   - `MainScene.MockServices.cs` 已独立，后续可以接真实 `IAIScheduler`、`IEventOracle`、`IMinisterAgent`、`INarrator` 实现。

4. **进一步抽取可复用 UI 构建函数**
   - `IntelPanel` 和 `AffairsPanel` 仍存在大量动态 Godot 控件创建逻辑。
   - 可进一步抽出按钮、标题、RichText、HBox/VBox 工厂方法。

5. **补充前端交互测试或最小 smoke test**
   - 当前验证以 build 为主。
   - 若后续引入 Godot 可运行环境，建议加入打开关键弹窗的 smoke 测试流程。
