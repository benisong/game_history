# 东汉模拟器核心架构与领域封装规范设计文档

**文档版本**：v2.1  
**生效日期**：2026-09-18  
**目标工程**：`game_history/donghan` (`DonghanEngine.Core` & `DonghanFrontend.V2`)

---

## 1. 架构目标与重构背景

为解决原项目中存在的对象内部变量裸露、外部随意修改属性破坏数值不变量、以及一个类承担过多异构业务方法（违反单一职责 SRP 与接口隔离 ISP）的问题，系统完成了全面的面向对象深度封装与分包封包重构。

### 核心架构原则：
1. **单一方法与接口隔离（ISP）**：业务层严禁一个类实现多种无关业务方法。每个适配器与领域服务仅面向细粒度契约。
2. **不可变 / 受保护属性实体（Rich Domain Model / 充血模型）**：内部变量与集合私有化，外部禁止直接赋值或修改集合。
3. **数值边界绝对安全（Defensive Clamping）**：所有属性变更必须由实体内部方法或领域服务执行，且强制执行范围约束。
4. **双轨架构无缝衔接**：前端建立独立的 V2 契约体系（`Frontend/V2/Contracts`），与领域服务完全解耦。

---

## 2. 后端核心 DDD 分包架构 (`DonghanEngine.Core`)

```
Backend/DonghanEngine.Core/
├── Models/                        # 领域实体与只读受保护值对象（单类单文件）
│   ├── GameState.cs               # 天子大局主状态（集合只读私有化）
│   ├── NpcState.cs                # 大臣/人物实体（五维防御与充血方法）
│   ├── Province.cs                # 州郡实体（民心、守军与平叛充血方法）
│   ├── ArmyState.cs               # 禁军实体（士气、忠诚与战损充血方法）
│   ├── ImperialEdict.cs           # 奏折与御批选项
│   ├── NpcRelation.cs             # 史实与动态人物关系网
│   └── CourtSpeech.cs             # 朝会发言与阶段模型
├── Constants/                     # 领域常量、文学 Trait 与派系枚举
│   ├── TraitNames.cs              # 31+ 项「藏锋于词」文学词汇常量
│   ├── FactionCatalog.cs          # 六大派系常量
│   └── FactionStance.cs          # 派系朝堂博弈矩阵
├── AI/                            # AI 调度与大臣交互
│   ├── IAIScheduler.cs            # AI 调度器主契约
│   ├── AIOrchestrationResult.cs   # 编排结果模型
│   ├── IMinisterAgent.cs          # 大臣智能体契约
│   ├── IEventOracle.cs            # 天灾与随机事件先知契约
│   ├── IntentClassifier.cs        # 玩家意图分类
│   └── FactionSpeechBank.cs       # 派系专属台词库
├── Npc/                           # NPC 注册、生命周期与特征评估
│   ├── INpcRegistry.cs            # 统一登庸/下野注册中心
│   ├── INpcLifecycleManager.cs    # A/B轨生命周期与发病机制
│   ├── HistoricalNpcPresets.cs    # 71 位史实人物冷备库
│   ├── HistoricalNpcRelations.cs  # 37 条史实羁绊关系
│   └── NpcTraitEvaluator.cs       # 纯函数能力评估器
├── Events/                        # 历史大事件与叙事
│   ├── EventNarratives.cs         # 黄巾、何进、董卓大事件
│   └── INarrator.cs               # 叙事输出器接口
└── Services/                      # 细粒度领域服务接口与引擎主干
    ├── IGameEngine.cs             # ISP 单一职责领域接口契约组合
    ├── GameEngine.cs              # 引擎主干编排
    ├── GameEngine.Province.cs     # 州郡治理与平叛
    ├── GameEngine.ActionSettlements.cs # 动作数值结算与反噬
    └── GameEngine.Narrative.cs    # 叙事与实录渲染
```

---

## 3. 领域模型充血行为与集合封装契约

### 3.1 `GameState`
- **只读集合暴露**：`Npcs` (`IReadOnlyDictionary<string, NpcState>`)、`Provinces` (`IReadOnlyDictionary<string, Province>`)、`ActiveEdicts`、`NpcRelations`、`IntelReports`、`Chronicle` (`IReadOnlyList<T>`)。
- **受控集合操作**：`RegisterNpc(npc)`、`RemoveNpc(npcId)`、`RegisterProvince(p)`、`AddActiveEdict(edict)`、`RemoveActiveEdict(edict)`、`AddIntelReport(report)`、`SetNpcRelations(relations)`。
- **数值与年号保护**：`ApplyNumericalDelta(powerDelta, treasuryDelta, healthDelta)`、`RefreshReignEra()`。

### 3.2 `NpcState`
- **五维与数值 Clamp (0-100)**：`Martial`、`Leadership`、`Politics`、`Charisma`、`Ambition`、`Favorability`、`Power`、`Corruption`、`Health`。
- **特质受控管理**：`Traits` 为 `IReadOnlyList<string>`，提供 `AddTrait`、`RemoveTrait`、`ClearTraits`。
- **充血业务方法**：`AdjustFavorability(delta)`、`AdjustPower(delta)`、`AdjustCorruption(delta)`、`AdjustHealth(delta)`、`AdjustStashedWealth(delta)`、`AssignGovernor(provinceId)`、`RevokeGovernor()`、`MarkDeceased(reason)`。

### 3.3 `Province`
- **数值 Clamp**：`LocalSupport` [0, 100]、`DefenseLevel` [0, 100]、`Garrison` >= 0、`Wealth` >= 0。
- **充血业务方法**：
  - `AdjustLocalSupport(delta)`、`AdjustWealth(delta)`、`AdjustGarrison(delta)`、`AdjustDefenseLevel(delta)`
  - `StartRebellion(faction, initialSupport, garrisonMult)`
  - `SuppressRebellion(supportRecovery, newGarrison)`
  - `PacifyRebellion(supportRecovery)`
  - `AppointGovernor(governorId, bonus)`、`RecallGovernor()`

---

## 4. 细粒度接口拆分（ISP 契约体系）

### 4.1 后端领域服务接口 (`DonghanEngine.Core.Services.IGameEngine`)
- `ITravelDomainService`：`MoveToLocation(location)`
- `IDrillArmyDomainService`：`DrillArmyAction(bonusPay, officerId)`
- `IRecruitArmyDomainService`：`RecruitArmyAction(officerId)`
- `IDisasterReliefDomainService`：`ExecuteDisasterReliefAction(amount, officerId)`
- `IConfiscationDomainService`：`ExecuteConfiscationAction(ministerId, targetTreasury)`
- `IQuickActionDomainService`：`ExecuteQuickAction(actionId)`
- `IResolveEdictDomainService`：`ResolveEdictAction(edictId, optionIndex)`
- `IGrandCourtDomainService`：`ProcessGrandCourtInputAsync(playerInput, officerId)`、`ExecuteFreeEdict(text, ministerId)`
- `IProvinceGovernanceDomainService`：`SuppressRebellion`、`PacifyRebellion`、`AssignGovernor`、`RecallGovernor`
- `ITurnAdvanceDomainService`：`NextXunAsync()`
- `IGameStateProvider`：`GetState()`

### 4.2 前端 V2 契约与适配层 (`DonghanFrontend.V2`)
- 每个服务接口仅包含一个业务方法（`IPayArmyService`、`IDrillArmyService`、`IRecruitArmyService`、`IInspectProvinceService`、`IExecuteProvinceActionService`、`IStartCourtSessionService`、`IExecuteCourtDecisionService`、`IExecuteFreeEdictService`、`IAdvanceXunService`、`IFastForwardService`、`IGetPendingEdictsService`、`IResolveEdictService`）。
- 状态读取完全通过不可变快照（`GameStateSnapshot`、`ProvinceSnapshot`、`MinisterSnapshot`）进行，杜绝 UI 层直接触碰核心引擎对象。

---

## 5. 自动化测试验证标准

- **全量测试集合**：133 个自动化测试（后端 93 + 前端 40）全部通过。
- **测试命令**：
  ```bash
  export PATH=$HOME/.dotnet:$PATH
  dotnet test Backend/DonghanEngine.Tests/DonghanEngine.Tests.csproj
  dotnet test Frontend/V2.Tests/DonghanFrontend.V2.Tests.csproj
  ```
