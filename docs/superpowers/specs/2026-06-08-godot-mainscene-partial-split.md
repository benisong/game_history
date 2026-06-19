# 2026-06-08 Godot MainScene partial 拆分记录

> **状态：✅ 已实现 — MainScene 已从单文件拆为多 partial**

## 1. 背景

`donghan/Frontend/MainScene.cs` 曾膨胀到约 1481 行，同时承载：

- 生命周期初始化
- 样式与布局工具
- 主 UI 刷新与场景动作
- 大臣详情与抄家
- 御案、奏折、情报、朝会、开局遮罩
- Mock AI 服务

继续在一个文件里扩展会明显提高维护成本，因此本次进行了**纯结构拆分**，不改玩法逻辑。

---

## 2. 拆分原则

1. **不改业务逻辑**：只移动代码，不重写结算与行为
2. **按职责聚合**：同类字段和方法放到同一 partial
3. **保留主入口清晰度**：`MainScene.cs` 只保留核心字段与生命周期
4. **保持 Godot 编译兼容**：统一 `namespace DonghanFrontend` 与 `public partial class MainScene : Control`
5. **Mock 服务单独隔离**：方便后续替换真实 AI 服务

---

## 3. 拆分后文件

当时拆分后的主要结构：

```text
MainScene.cs
MainScene.CoreActions.cs
MainScene.CourtAndOpening.cs
MainScene.DeskAndAffairs.cs
MainScene.IntelPanel.cs
MainScene.Ministers.cs
MainScene.MockServices.cs
MainScene.Style.cs
```

后续项目继续演进后，实际已在这个基础上继续细拆，例如：

```text
MainScene.CourtPanel.cs
MainScene.CourtTopics.cs
MainScene.CourtRituals.cs
MainScene.CourtDecisions.cs
MainScene.ActionDialogs.cs
MainScene.IntelActions.cs
MainScene.WestGardenPanel.cs
MainScene.ReportsAndFastForward.cs
```

其中 2026-06-19 又完成了一轮纯结构细拆：把原先堆在 `MainScene.CoreActions.cs` 内的“快进 N 旬”与各类奏报弹窗（含朝会/密札/西园/朱批/警示/巡幸奏报）整体迁移到 `MainScene.ReportsAndFastForward.cs`，保持调用入口与玩法行为不变。

> 本文件记录的是“从单文件改为 partial 架构，并继续在功能域内细拆”的思路，不再维护每次演进后的完整逐方法清单。

---

## 4. 各文件职责

### `MainScene.cs`

保留：

- 核心字段
- `_Ready()`
- `_Process(double delta)`
- 初始化编排

定位：**主入口与生命周期编排层**。

### `MainScene.CoreActions.cs`

承载：

- `UpdateUI()`
- 主场景按钮刷新
- 巡幸 / 快捷动作
- 阅兵 / 赈灾
- 玩家输入提交

定位：**基础操作与主 UI 同步层**。

### `MainScene.ReportsAndFastForward.cs`

承载：

- 快进 N 旬弹窗与异步推进
- 各类统一风格的奏报弹窗
- 奏报标题/封签/脚注/正文样式辅助

定位：**阶段结算回奏与快进流程层**。

### `MainScene.Ministers.cs`

承载：

- 大臣详情
- 抄家相关交互
- 动态朝臣列表初始化与刷新

定位：**大臣详情与朝臣列表层**。

### `MainScene.DeskAndAffairs.cs`

承载：

- 御案四入口
- 尚书台奏折面板
- 奏折列表与选项处理

定位：**御案入口与政务处理层**。

### `MainScene.IntelPanel.cs`

承载：

- 黄门密札面板
- 州郡列表与详情
- 任命、召回、平叛、招安 UI 对接

定位：**天下情报与州郡治理层**。

### `MainScene.CourtAndOpening.cs`

承载：

- 大朝会入口与流程
- 起驾相关入口
- 开局遮罩

定位：**朝会流程与开场沉浸层**。

### `MainScene.Style.cs`

承载：

- 全屏与 blocker 工具
- 不透明背景与面板样式
- Label / Layout 辅助函数

定位：**全局样式与布局辅助层**。

### `MainScene.MockServices.cs`

承载：

- `MockScheduler`
- `MockOracle`
- `MockMinisterAgent`
- `MockNarrator`

定位：**前端本地调试依赖层**。

---

## 5. 验证

本次拆分完成后，验证方式为：

### 前端编译

```bash
cd /opt/game_history/donghan/Frontend
dotnet build DonghanFrontend.csproj -v minimal
```

### 后端测试

```bash
cd /opt/game_history/donghan/Backend
dotnet test DonghanEngine.Tests/DonghanEngine.Tests.csproj
```

### 空白检查

```bash
git diff --check
```

---

## 6. 结论

这次拆分的价值是把 `MainScene` 从“一个不断膨胀的大脚本”改成“按功能域分层的 partial 架构”。

它没有解决所有后续膨胀问题，但至少建立了一个可继续细分的结构基础：

- 生命周期集中在主文件
- 功能域各自成块
- Mock 与样式不再混在主流程里

后续如果某个 partial 再次膨胀，原则上应继续在该功能域内部细拆，而不是回退到单文件堆积。