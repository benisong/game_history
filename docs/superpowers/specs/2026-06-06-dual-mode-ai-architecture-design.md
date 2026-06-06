# 2026-06-06 双模式 AI 架构设计

> **状态：📋 未实现 — 仅设计文档**
>
> ⚠️ ═══════════════════════════════════════════════════════ ⚠️
> ⚠️  本文档描述的是计划中的架构，尚未编写任何实现代码。          ⚠️
> ⚠️  当前游戏使用 Mock* 类提供固定模板对话，完全可玩但无 AI。    ⚠️
> ⚠️ ═══════════════════════════════════════════════════════ ⚠️

---

## 1. 设计目标

游戏支持三种 AI 模式，用户在设置中自由切换：

| 模式 | 引擎 | 需求 | 体验 |
|---|---|---|---|
| **基础模式**（当前） | 纯规则 + 预制文本 | 无 | 可玩，对话简单 |
| **本地模式** | 本地 LLM（千问 7B / 3B） | 4060+ GPU | 文本润色，离线可用 |
| **云端模式** | DeepSeek / Gemini API | 联网 + API Key | 全生成，体验最佳 |

三种模式共用同一个接口抽象（`IAIScheduler` / `INarrator` / `IEventOracle` / `IMinisterAgent`），`GameEngine` 不需要感知模式差异。

---

## 2. 架构总览

```
                    ┌─── GameEngine ───────────────┐
                    │  所有数值计算在此               │
                    │  好感/皇权/民心 代码计算         │
                    │  NpcTraitEvaluator             │
                    └──────────┬────────────────────┘
                               │ 依赖 4 个抽象接口
         ┌─────────────────────┼─────────────────────────┐
         │                     │                         │
  ┌──────▼──────┐     ┌───────▼────────┐     ┌──────────▼──────────┐
  │ BaseProvider │     │ LocalProvider  │     │  CloudProvider      │
  │ (当前 Mock)  │     │ (LLamaSharp)   │     │  (HTTP API)         │
  │              │     │                │     │                     │
  │ ChoiceEngine │     │ ChoiceEngine   │     │ 完整 GameState →    │
  │ + TextLibrary│     │ + TextLibrary  │     │ LLM 全量生成        │
  │ 无 LLM       │     │ + LLM 润色     │     │ 不可预测 高开放性    │
  └──────────────┘     └────────────────┘     └─────────────────────┘
```

**关键设计原则：**

- 本地模式的 LLM 是**可选项**——加载失败/超时/崩溃 → 自动降级到 ChoiceEngine + TextLibrary（等同于基础模式）
- 云端模式失败 → 自动降级到本地模式（如果已加载）或基础模式
- 三种模式共享同一套 `ChoiceEngine` 和 `TextLibrary`，差异仅在最后一步是否调用 LLM

---

## 3. 三个核心模块

### 3.1 ChoiceEngine（本地函数，无 LLM）

```
输入：GameState + 玩家操作
     ↓
  分析维度：
  · 关键词匹配（"赈灾"/"抄家"/"赏赐"）
  · 数值触发（民心<30 → 危机场景）
  · 派系关系（谁好感>60？谁权势>75？）
     ↓
输出：场景标签 + 参与者列表
     例："大朝会_民心危机_何进提议赈灾"
```

### 3.2 TextLibrary（预制文本库）

约 85 个带占位符的模板，按场景标签索引：

```
场景：大朝会 → 民心危机 → 何进提议赈灾
  ├── 模板A：何进奏请 → 曹操附议 → 张让反对
  ├── 模板B：何进奏请 → 曹操谨慎 → 张让阴阳怪气
  └── 模板C：何进奏请 → 蹇硕力挺 → 张让阻挠

模板示例：
  "陛下圣明！{灾区}大旱三月，赤地千里，
   臣{candidate}愿领旨开仓赈济！"
```

ChoiceEngine 根据 GameState 加权随机选择一个模板，填充 `{灾区}` `{candidate}` 等变量后输出。

### 3.3 LLM Polish（千问 7B / 3B）

```
输入：TextLibrary 输出的填充后文本（~100 字）
     ↓
  Prompt：
    "你是东汉文学侍从。润色以下朝堂对话，保持原意，
     增加细节，使用半文言风格。不要改变任何事实信息。"
     ↓
输出：润色后的文本（~150-200 字）
     ↓
  失败 → 直接使用输入原文，游戏正常运行
```

**模型要求极低：**
- 任务从"生成"降级为"润色"，3B 模型即可胜任
- 输出格式自由（纯文本），不需要结构化解析
- 失败代价为零（原文已可用）

---

## 4. 云端模式（全生成）

与本地模式的关键差异：

| | 本地模式 | 云端模式 |
|---|---|---|
| ChoiceEngine | ✅ 用 | ❌ 不用 |
| TextLibrary | ✅ 用 | 仅作 few-shot 参考 |
| LLM 角色 | 润色 | 从头生成 |
| 输出确定性 | 高（模板约束） | 低（开放性） |
| 大臣行为 | 从预设选项中随机 | LLM 自主决定立场 |

云端模式下，`GameState` 完整序列化后发送给 LLM，由 LLM 直接产出：
- 多位大臣的辩论发言 + 立场
- 新奏折的标题、描述、选项
- 情报密报的内容
- 随机事件的描述

---

## 5. 回退链

```
用户操作
  │
  ├─ 云端模式？
  │   ├─ HTTP 成功 → 展示
  │   └─ 超时/失败 → 尝试本地 LLM
  │                   ├─ 已加载 → 本地润色模式
  │                   └─ 未加载 → 基础模式（模板原文）
  │
  └─ 本地模式？
      ├─ LLM 加载成功 → 润色后展示
      └─ 加载失败 → 基础模式（模板原文）
```

任何路径的终点都保证有文本输出，游戏永不因 AI 故障而中断。

---

## 6. 实现清单（未开始）

### 新增文件

```
DonghanEngine.Core/
  AI/
    ILlmProvider.cs          # 抽象 LLM 调用接口
    ChoiceEngine.cs           # 场景决策引擎
    TextLibrary.cs            # 预制文本库（~85 模板）
    PromptTemplates.cs        # Prompt 模板

  AI/Providers/
    BaseProvider.cs           # 基础模式（当前 Mock 逻辑）
    LocalProvider.cs          # LLamaSharp 本地推理
    CloudProvider.cs          # DeepSeek/Gemini HTTP API

  AI/Models/
    LlmService.cs             # 模型加载/推理/卸载
    ModelManager.cs           # 下载/校验/切换模型

DonghanEngine.Tests/
  ChoiceEngineTests.cs        # 场景决策测试
  TextLibraryTests.cs         # 模板完整性测试
```

### 接口定义（预览，未实现）

```csharp
// 新增的顶层抽象，原有 4 个接口保持不变
public interface ILlmProvider
{
    // 纯文本润色（本地模式用）
    Task<string> PolishAsync(string rawText, GameContext ctx);

    // 全量生成（云端模式用）
    Task<AIOrchestrationResult> GenerateCourtAsync(string input, GameState state);
    Task<string> GenerateIntelAsync(GameState state);
    Task<string> GenerateNarrativeAsync(string input, GameState state);
}
```

---

## 7. 与现有游戏集成

- `GameEngine` 无需任何改动——它只依赖现有的 4 个接口
- `IAIScheduler` / `INarrator` / `IEventOracle` / `IMinisterAgent` 签名不变
- 新 Provider 实现这些接口，内部调用 `ILlmProvider` 和 `ChoiceEngine`
- Godot 前端新增一个「设置 → AI 引擎」下拉菜单

---

## 8. 设计决策记录

| 决策 | 理由 |
|---|---|
| 本地 LLM 只做润色 | 模型稳定性要求降到最低，失败无代价 |
| 预制文本库 85 个模板 | 足够覆盖核心场景，维护成本可控 |
| 3B 模型即可（非必须 7B） | 润色任务比生成任务简单一个数量级 |
| Steam DLC 分发模型 | 用户可选装，不装也能玩（基础模式） |
| 三种模式共用 ChoiceEngine | 基础模式不是"阉割版"，而是正确行为的 reference |
