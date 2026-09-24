# 《东汉末年汉灵帝：大朝会、西园与多智能体政治博弈策略游戏》后端与前端控制核心架构总纲

---

## 1. 项目概述

《东汉末年汉灵帝》是一款以东汉末年汉灵帝为主角的历史推演与改写文字策略游戏。游戏采用 **C# (.NET 8.0) 模块化后端类库** 作为核心逻辑大脑，结合 **Godot 4.6.3 (.NET/C#)** 游戏引擎搭建原生第一人称前端交互。

本项目规避了高算力损耗的网页套壳模式，完全采用面向对象（C# OOP）的高性能策略引擎，并预留了面向多智能体（Multi-Agent）大语言模型（如 DeepSeek/Gemini）的异步调度中间件与防御性数据缓冲槽，实现真实的朝党斗争、西园理财、天灾赈灾及帝王心术博弈。

**项目规模**：总计 **388 个自动化测试（后端 336 + 前端 V2/V3 52）100% 全绿通过**。后端 `DonghanEngine.Core` 已彻底落地 DDD 分包、不可变/只读集合防篡改封装、充血领域模型（OOP）与细粒度单方法服务接口隔离（ISP）；已实装**全新 V3 沉浸式天子御案与天下沙盘交互界面（`Frontend/V3/MainSceneV3.cs`：左侧 55% 交互式十三州地缘与产权沙盘、触控唤起快捷决策轮盘；右侧 45% 实体化御案工作台、折匣/名册/西园万金堂/起居注 Tab 联动；支持一键无缝双向切换 V2 列表主题与 V3 沙盘主题）**、尚书台岁终大考课与封疆大吏内调博弈系统（`DonghanEngine.Core.Politics.GovernorAppraisalService`）、大汉十三州全景沙盘与地缘土地产权可视化视图（Godot V2 UI `ShowGeopoliticsMap` / `ShowGovernorAppraisal`）、全周期跨年份多朝代历史大事件动态因果网（二袁官渡合流/白门楼特赦吕布克潼关/保全孙策亲征赤壁/关羽存活夷陵消弭）、土地所有权核心玩法与战乱焦土/世家赎田系统（`DonghanEngine.Core.Economy.LandOwnershipService`）、国家级度田令与水利官修子系统（`DonghanEngine.Core.Economy.CadastralAndIrrigationService`）、东汉文武双轨九品官阶晋升与西园鬻官通天子系统（`DonghanEngine.Core.Politics.OfficialRankService`）、宏观经济土地承载力与流民部曲子包（`DonghanEngine.Core.Economy`）、帝王权术财政博弈与抄家关联惩罚/军饷哗变子包（`DonghanEngine.Core.Politics`）、世家察举荐辟系统（`TalentNominationService`）、天子威望非线性力学评估子包（`ImperialPrestigeEvaluator`：46~60 紧凑黄金平衡区）、184年至226年灵帝延寿70岁全周期43项历史大事件全景因果推演管线；主引擎 `GameEngine` 与前端适配层及 Godot UI 已完成全业务交互大闭环；打通与 `GameEngine` 每旬推进与尚书台御批（`ResolveGeopoliticalMemorial`）的双向闭环；前端已建立 V2/V3 契约架构与单一职责适配层。

### 已实现核心系统与交互矩阵
- [x] **V3 沉浸式天子御案与沙盘工作台（V3 Immersive Imperial Desk & Map）**：
  - 左侧 55%：天下十三州交互沙盘，地块点击弹出【度田/水利/赎田/平叛】快捷决策轮盘；
  - 右侧 45%：实体御案折匣（朱批）、尚书台群僚卡片（抄家断案）、西园万金堂（直售一品三公/三军犒赏）；
  - 默认开机即进入 V3 沙盘界面，顶栏常驻【🔄 切换至 V2 典雅列表界面】与【✨ 切换至 V3 沉浸式天子御案】双向一键切换。
- [x] **尚书台年终大考课与刺史内调系统（Governor Appraisal & Recall System）**：
  - 岁终考核各州治安民心、岁输税款与户口增殖，评定【上考/中考/下考】；
  - 忠诚能臣顺服内调拜九卿（执金吾/太常/少府），地方兵印收归中央，皇权提升；
  - 割据军阀（野心 $\ge 80$ 或天子威望低）称病推诿抗命，朝野侧目削弱皇权。
- [x] **大汉十三州全景沙盘/地缘态势可视化沙盘（Map / Geopolitics View）**：
  - 十三州势力归属与颜色标记（朝廷直辖、曹操、袁绍、孙坚、刘虞等）；
  - 国家官田 vs 世家私田比例与地税征收呈现；
  - 土地人口承载压力红/黄/绿状态与焦土休耕倒计时可视化；
  - 州郡卡片一键下达度田清查、兴修官渠、准世家赎田或遣将平叛政令。

---

## 2. 核心数据模型与封装约束 (`DonghanEngine.Core.Models`)

### 2.1 五大天子物理属性与防御性保护 (`GameState`)

- **皇权 (ImperialPower)** `[0-100]`：代表天子号令天下的权威度。初始值极弱 **`25`**（政令不出宫门）。属性设置器自带 `Math.Clamp(0, 100)` 保护。过低将导致抄家无官出列响应，甚至引发党羽联合弹劾与逼宫兵变。
- **国库 (Treasury)** `[单位: 万钱]`：大汉朝廷公款，初始窘迫 **`8000`** 万钱，设置器自带非负保护，用于大朝会赈济。
- **私库 (PrivateTreasury)** `[单位: 万钱]`：西园天子内库，初始告急 **`1200`** 万钱，用于犒军、阅兵。
- **民心 (PopularSupport)** `[0-100]`：天下百姓对汉廷的拥戴值。初始濒危 **`28`**（低于活命线，自带 `Math.Clamp(0, 100)`）。低于 30 将直接触发特大地方饥荒或黄巾暴乱预警。
- **健康 (Health)** `[0-100]`：天子龙体状态，因临幸后宫、饮酒纵乐而增损。初始危重 **`35`**（自带 `Math.Clamp(0, 100)`），归零则汉灵帝崩殂。
- **集合完全只读封装（防外部篡改）**：`Npcs` 暴露为 `IReadOnlyDictionary<string, NpcState>`，`Provinces` 暴露为 `IReadOnlyDictionary<string, Province>`，`ActiveEdicts`、`NpcRelations`、`IntelReports`、`Chronicle` 全部暴露为 `IReadOnlyList<T>`。禁止外部直接通过索引器赋新值或裸 `Add()`，所有增删均由 `RegisterNpc`、`RemoveNpc`、`RegisterProvince`、`AddActiveEdict`、`RemoveActiveEdict`、`AddIntelReport`、`SetNpcRelations` 等领域方法安全受控执行。

### 2.2 纪元时间戳与年号系统

`GameState.Year=184`（中平元年/光和七年）、`Month=4`、`Xun=1`（上旬）。三旬为月、十二月为年。自带 `RefreshReignEra()` 动态推导灵帝年号（光和 178-184.11 / 中平 184.12-189）。

### 2.3 十三州拓扑与州郡实体 (`Province` + `ProvinceCatalog`)

> 实装完整大汉十三州：司隶 / 冀州 / 并州 / 兖州 / 豫州 / 荆州 / 青州 / 徐州 / 扬州 / 幽州 / 凉州 / 益州 / 交州。

所有州郡集中定义于 `DonghanEngine.Core.Constants.ProvinceCatalog`，并构建了双向地理邻接网（Neighbors），每郡独立维护 `LocalSupport`（0-100）、`Garrison`、`Wealth`、`GovernorId`、`IsRebelling`、`RebelFaction`、`RebellionMonths`、`LowSupportStreakMonths`，`Neighbors` 暴露为 `IReadOnlyList<string>`。包含完整的充血方法：
- `AdjustLocalSupport(delta)`、`AdjustWealth(delta)`、`AdjustGarrison(delta)`、`AdjustDefenseLevel(delta)`
- `StartRebellion(faction, initialSupport, garrisonMult)`
- `SuppressRebellion(supportRecovery, newGarrison)`
- `PacifyRebellion(supportRecovery)`
- `AppointGovernor(governorId, bonus)` / `RecallGovernor()`

### 2.4 NPC 五维与充血实体 (`NpcState`)

- **五维属性 (0-100)**：武力 Martial、统帅 Leadership、政治 Politics、魅力 Charisma、野心 Ambition（全部自带 `Math.Clamp(0, 100)` 保护）。
- **特质集合只读化**：`Traits` 暴露为 `IReadOnlyList<string>`，提供 `AddTrait`、`RemoveTrait`、`ClearTraits` 受控操作。
- **充血业务方法**：`AdjustFavorability(delta)`、`AdjustPower(delta)`、`AdjustCorruption(delta)`、`AdjustHealth(delta)`、`AdjustStashedWealth(delta)`、`AssignGovernor(provinceId)`、`RevokeGovernor()`、`MarkDeceased(reason)`。
- **派系与官阶**：清流派 / 外戚派 / 阉党派 / 西园亲军 / 割据军阀 / 反叛势力；官阶 0-4（白身布衣 → 三公/大将军）。
- **历史元数据**：`InitialLocation`、`EntryCondition`、`IsHostile`（敌对首领不进入任官候选）、`HistoricalDeathYear`、`SourceNote`。

### 2.5 派系关系网 (`NpcRelation` + `HistoricalNpcRelations`)

`HistoricalNpcRelations.cs` 维护 **37 条历史关系**，类型枚举：
- Kinship（血缘）、Patronage（提携）、FactionAlly（同派系）、TeacherStudent（师生）、SwornBond（结义）、Rivalry（路线之争）、Hostility（死敌）、Command（统属）、RegionalTie（地域）

### 2.6 西园新军 (`ArmyState`)

`WestGardenArmy` 八校尉直属亲军：`Size`（默认 8000）、`BasePayPerTurn`（默认 120 万钱/旬）、`Morale`（0-100）、`Loyalty`（0-100）。具备 `AdjustMorale`、`AdjustLoyalty`、`AdjustSize`、`TakeCasualties` 充血方法。

### 2.7 奏折与选项实体 (`ImperialEdict`)

`ActiveEdicts` 中的奏折对象，`Options` 暴露为 `IReadOnlyList<EdictOption>`，提供 `AddOption`、`DecrementExpiry` 与 `IsExpired` 充血行为。

### 2.8 历史人物冷备 (`HistoricalNpcPresets`)

冷备名单 **71 位** 真实历史人物，涵盖黄巾军、十常侍、清流名臣、军政大将、军阀、谋士与边将。

### 2.9 Trait 字符串常量 (`TraitNames`)

集中定义在 `DonghanEngine.Core.Constants.TraitNames` 常量类，杜绝硬编码拼写错误。详见 §5。

---

## 3. 历史向"旬日"时间系统与年号更迭

游戏采用东汉纪实 **"旬（每旬十天）"** 作为时间更迭单位：
- 一月包含三旬（上旬、中旬、下旬），十二月为一年。
- 每旬流逝时（通过 `NextXunAsync()`），系统自动结算奏折寿命并触发流产惩罚，并调用 AI 调度器生成百官密录（`IntelReports`）与地方紧急奏折（`ActiveEdicts`）。

---

## 4. NPC 生命周期与派系关系

为了建立具有生老病死、历史演进的人才生态，设计了统一的 NPC 物理管理器。其不自主运行，**完全将调度支配权公开给 AI 调度员（Orchestrator）进行显式指令调度**：

### 4.1 统一的登录（注册）接口 (`INpcRegistry`)

- **登录注册 (`RegisterNpc`)**：由可视化界面输入、本地配置文件读取或 AI 调度员自创登庸，将新 NPC 注入游戏大局。
- **致仕下野 (`DeregisterNpc`)**：大臣由于生老病死、贬庶流放或被天子强抄家产，物理退场。其政治生平、死亡或去职原由将作为历史大案永久记入《大汉起居注》（Chronicle）。

### 4.2 双轨制生命周期管理器 (`INpcLifecycleManager`)

- **A轨本地预置（Scheme A）**：优先搜寻本地运行目录下的 `donghan_preset_npcs.json`。
- **B轨嵌入冷备（Scheme B Fallback）**：若本地 JSON 缺失或格式损毁，系统**自动静默降级**采用 C# 硬编码冷备静态名单（71 位真实历史人物）。
- **老病死启发式演进与惰性登台**：
  - **惰性加载部署 (`DeployNpcToCourt`)**：开局洛阳核心朝臣上场。备用英贤静态沉睡，由调度员按需部署上台。
  - **老病死演进**：每逢 3 旬（1个月），所有大臣年龄增长 1 岁。若年龄超过其期望寿命（`BaseLongevity`），每旬会有 15% 的概率寿终寝于邸舍；同时每旬有千分之三的概率染上"洛阳伤寒温疫"，健康值暴跌 30 点。
- **时间戳原子防颠簸锁（Timestamp Lock）**：内部实装 `LastNpcProcessedTimestamp` 原子时间戳锁。确保在一个游戏旬日之内生命周期只会安全触发 **1 次**。

### 4.3 派系关系（节选）

详见 §2.5 表格。完整 37 条关系定义在 `HistoricalNpcRelations.cs`。

---

## 5. 「藏锋于词」31 项成语与同义词复合特征估值引擎 (`NpcTraitEvaluator`)

本系统彻底抛弃了 `政治：90`、`武力：85` 这类枯燥的快餐数据，而是采用生动的**成语与通俗特征组合（Traits）**修饰百官特征。在底层物理逻辑中，Traits 会深度参与大政结算加成。所有 31 项字符串集中在 `TraitNames.cs` 常量类。

### 5.1 31 项同义双轨对照 Traits

本系统支持成语与通俗同义词**共存累乘计算**，且遵循**"通俗效果仅为成语一半或更少"**的历史严谨度：

1. **大朝会赈灾提振**（系数类，累乘）：
   * 「经天纬地」(成语) → 1.20x  ｜  「擅长民政」(通俗) → 1.08x
   * 「爱民如子」(成语) → 1.15x  ｜  「亲民温和」(通俗) → 1.05x
   * 「豪奢无度」(成语, 负面) → 0.75x  ｜  「铺张浪费」(通俗, 负面) → 0.90x
   * 「不学无术」(成语, 负面) → 0.80x  ｜  「才疏学浅」(通俗, 负面) → 0.90x
2. **犒赏禁军士气提振**（系数类，累乘）：
   * 「孔武有力」(成语) → 1.30x  ｜  「有些力气」(通俗) → 1.10x
   * 「治军严整」(成语) → 1.25x  ｜  「懂点兵法」(通俗) → 1.10x
   * 「不学无术」(负面) → 0.80x  ｜  「才疏学浅」(负面) → 0.90x
3. **犒赏禁军忠诚提振**（系数类，累乘）：
   * 「爱兵如子」(成语) → 1.20x  ｜  「体恤士卒」(通俗) → 1.08x
4. **借刀抄家钦差反噬折减**（数值损益类，**累加**）：
   * 「刚直不阿」(成语, 钦差) → bypass 豁免所有反噬
   * 「老谋深算」(成语, 钦差) → 反噬折减 30%
   * 「有些心计」(通俗, 钦差) → 反噬折减 10%
   * 「说话直率」(通俗, 钦差) → 反噬折减 40%
5. **抄家目标反噬加重**（数值损益类，**累加**）：
   * 「拥兵自重」(成语, 目标) → +5  ｜  「手下有兵」(通俗, 目标) → +2
   * 「门阀世家」(成语, 目标) → +8  ｜  「出身名门」(通俗, 目标) → +3
6. **资金中饱漂没修正**（系数类，累乘）：
   * 「清正廉洁」(成语) → 漂没归零，被构陷时民心暴跌 -20
   * 「不拿公款」(通俗) → 漂没减半，被构陷时民心跌 -8
   * 「贪得无厌」(成语) → 漂没放大 1.50x  ｜  「有些手脏」(通俗) → 1.20x
7. **天子宿行后宫乐**（数值损益类，**累加**）：
   * 温德殿调养时「谄媚专权」/「会拍马屁」随驾，龙体恢复 +15（从 +10 提），好感/权势暴涨
   * 「医术高明」随驾健康恢复 +8（翻倍），「懂点医理」+3
   * 「喜好清谈」随驾健康 +2，但当旬皇权 -1

### 5.2 复合算法公式

- **系数类（民心、士气、忠诚、漂没）**：采用**累乘（Compounded Multiplicative）**算法：
  $$\text{FinalMultiplier} = \prod (\text{TraitMultipliers})$$
- **数值损益类（反噬扣除、目标加重、后宫加成）**：采用**累加（Additive）**算法。

---

## 6. 日常政务处理与加官进爵系统 (Imperial Edicts & Promotion System)

大朝会决策和地方奏折批复被封装为了强类型政务处理管线。

### 6.1 五大历史奏折管线 (`EdictType`)

- **建议折 (Proposal)**：官员申请地方筑防、兴利、开仓，考验天子理财路线。
- **劝诫折 (Remonstrance)**：直言犯颜，因天子空虚、卖官昏聩触发，不听会引发清流折损或暴跌威信。
- **弹劾折 (Impeachment)**：清除异己、党阀倾轧（十常侍与大将军党争）。
- **邀功折 (Merit)**：立功讨赏（可赏金、也可加官）。
- **急报折 (UrgentCrisis)**：突发灾异、兵变胡人入侵，需要紧急乾纲独断。

每条奏折维护 `ExpiryXun`（默认 3 旬保质期），`EdictOption` 携带 `ImperialPowerDelta` / `TreasuryDelta` / `PrivateTreasuryDelta` / `PopularSupportDelta` / `HealthDelta` / `TargetNpcPowerDelta` / `TargetNpcFavorabilityDelta` / `GrantedTitleTierDelta` 八个增量维度。

### 6.2 阶梯官衔与跨级拔擢惩罚 (`TitleTier`)

- 定义了 **0级（白身布衣）** 到 **4级（三公/大将军）** 5 个官位层级（曹操 1，蹇硕 2，张让 3，何进 4）。
- **平稳逐级晋升**：臣子对天子忠诚爆表，好感度大涨。
- **跨级跃升飞进 ($\Delta \text{Tier} \ge 2$)**：
  - 前端触发"德不配位、朝野非议"高亮黄牌预警。
  - 物理落账：天子皇权（ImperialPower）遭受严重非议，扣除 $5 \times (\Delta \text{Tier} - 1)$ 点，且**异步锁定在接下来的 1-2 旬内强制塞入敌对派系发起的弹劾折**。

### 6.3 留中不发过期惩罚 (`ExpiryXun`)

- 奏折如果连续 3 旬未被批复，视为"流产"。
- 急报类流产（留中不发导致大难无法挽回）：**天下民心大跌 -15**，记入实录；普通奏折流产：皇权大跌 -2。

### 6.4 `GameEngine` partial class 分层

为了让核心引擎便于继续扩展，`GameEngine` 采用 `partial class` 分层承载职责：

- **`GameEngine.cs`**（511 行）：保留玩家输入解析、流程编排、状态推进与外部 API 入口。
- **`GameEngine.ActionSettlements.cs`**（366 行）：集中处理"规则结算"，包括西园犒赏、开仓赈灾、抄家反噬等动作的数值计算、Outcome 判定与状态落地。
- **`GameEngine.Narrative.cs`**（125 行）：集中承载"大段叙事文本"，将 RichText/Bbcode 故事、实录 Chronicle 与结算反馈从流程代码中剥离。
- **`GameEngine.Province.cs`**（624 行）：每旬叛乱检测、黄巾触发、野心叛乱、招安/平叛规则。

拆分原则：规则先算出结构化 Settlement，叙事层再根据 Settlement 生成玩家可读文本，主引擎只负责把二者串起来。

---

## 7. AI 调度员与大朝会异步双通道缓冲 (Async Deferral Queue)

为了抹平大模型生成多角色群辩对话的网络通信时延，结合 Godot 前端设计了**异步双通道加载协议**：

```
                    ┌────────────────────────┐
                    │  玩家点击 【召集朝会】  │
                    └───────────┬────────────┘
                                │
        ┌───────────────────────┴───────────────────────┐
        ▼ (同步通道 - 0 延迟)                            ▼ (异步通道 - Task.Run)
┌─────────────────────────────────┐           ┌──────────────────────────────────┐
│ 1. C# 引擎根据数值锁定打头阵。  │           │ 1. AI 调度员分析当前 GameState。 │
│ 2. 展现首发折子，并开启大朝仪   │           │ 2. 并行调度曹操、张让等多个臣子。│
│    三阶段过渡遮罩。             │           │ 3. 计算他们对于政策的弹劾与群辩。│
└─────────────────────────────────┘           └──────────────────────────────────┘
```

### 7.1 大朝仪三段式转场 (Ritual Transition)

点击朝会玉玺时，展示三步走汉代朝会仪式，以富文本定时渐显，在渲染氛围的同时给后台 AI 预留 5-6 秒净空：

1. **起驾换装**：尚衣监服侍陛下于暖阁换玄衣纁裳、冕冠，龙舆启行。
2. **百官趋步**：宣政殿漆门重门大开，百官低头疾行入殿。
3. **静鞭鸣磬**："圣上驾到"，静鞭三响，百官向空置龙椅深揖整肃，天子御极。

### 7.2 多角色党争群辩队列

当玩家读折并打字批复下诏时，AI 在后台算好的反对派/迎合派立场的群辩对话已被无感地压入 `CourtDebateQueue`。前端只需进行 `state.CourtDebateQueue.Dequeue()` 顺序出栈，流畅度极高。

### 7.3 大朝会五大常议议题 (`CourtTopics`)

`MainScene.CourtTopics.cs` 提供开局即用的五大议题：**整军备寇**、**国帑筹措**、**整饬宦官**、**举荐将才**、**亲拟圣旨**。每议题携带 `Id` / `Category` / `Title` / `Summary` / `Speeches[]` / `Decisions[]`，决策按钮复用现有朝会输入流程（批阅 → 朱批 → 弹回回奏）。

### 7.4 异步接口面

| 接口 | 职责 |
|---|---|
| `IAIScheduler.OrchestrateGrandCourtAsync` | 朝会群辩编排 |
| `IAIScheduler.OrchestrateXunUpdateAsync` | 旬更阴谋 + 天灾告警 |
| `IEventOracle.CheckRandomEventAsync` | 天灾/后宫/健康随机事件 |
| `IMinisterAgent.TalkToMinistersAsync` | 单大臣对话 |
| `INarrator.RenderStoryAsync` | 玩家输入 → 富文本故事渲染 |

---

## 8. Godot 前端第一人称"赤霄御案"视觉美学与 C# 控制层

### 8.1 主界面 4 张横向入口卡

主界面 `MainScene.cs` 采取第一人称视角。御案上**横向并排 4 张 portrait 卡**（270×405，3:4 竖版），居中放置：

- **大朝会**：点击弹出朝会输入，确认后触发三段式大朝仪遮罩动画，再进入玩家诏令的异步处理流程。
- **黄门密札**：点击进入【情报】独占弹窗，显示六郡预警、地方局势与可执行的治理动作。
- **西园别苑**：点击进入西园独立面板，管理新军、账目、募兵。
- **起驾巡幸**：点击后切换至后宫/西园相关地点，并刷新主界面状态。

### 8.2 6 类 PopupSkin 弹窗皮肤

`MainScene.CoreActions.cs` 定义 6 类独占弹窗（`PopupSkin` 枚举），每类有独立标题前缀、背景色调、标签栏布局：

| PopupSkin | 标题前缀 | 用途 |
|---|---|---|
| `Court` | `尚书台 · 百官回奏` / `御览毕 · 收起回奏` | 大朝会回奏 |
| `Intel` | `黄门密札 · 军情战报` / `黄门密札 · 州郡回传` | 情报详情 |
| `WestGarden` | `西园密署 · 军簿回报` | 西园军情 |
| `Document` | `御案折匣 · 朱批回奏` | 折匣批阅 |
| `Travel` | `黄门导驾 · 龙辇奏报` | 起驾回奏 |
| `Warning` | `黄门短札 · 急奏` / `御史台 · 风闻弹奏` | 警告 |

### 8.3 五类主面板

| 文件 | 行数 | 功能 |
|---|---|---|
| `MainScene.CourtPanel.cs` | 810 | 大朝会五段式转场、择议/群臣奏对、群臣辩论渲染 |
| `MainScene.CourtAndOpening.cs` | 389 | 开场"黄巾乱起 · 天子临朝"、横幅岁月、起驾面板 |
| `MainScene.CourtTopics.cs` | 137 | 五大常议议题数据 |
| `MainScene.IntelPanel.cs` | 297 | 黄门密札 6 郡预警 + 叛乱 + 地方详情 |
| `MainScene.IntelActions.cs` | 411 | 情报决策（召还/外任/颁授虎符/招安） |
| `MainScene.WestGardenPanel.cs` | 474 | 西园犒赏/募兵/军簿/账目 |
| `MainScene.DeskAndAffairs.cs` | 475 | 御案折匣 · 尚书台批阅、起居注 |
| `MainScene.Ministers.cs` | 408 | 群臣档案 + 籍没家产入口 |
| `MainScene.MockServices.cs` | 120 | 玩家圣旨→样例回奏占位 |
| `MainScene.Style.cs` | 597 | 全局统一样式函数库 |

### 8.4 全屏、弹窗独占与不透明界面约束

前端界面统一以 `project.godot` 和 `MainScene.cs` 双重约束维持全屏体验：

- `window/size/mode=4` 强制独占全屏，`_Process()` 会在运行中检测并恢复 `ExclusiveFullscreen`。
- `WindowManager` 采用栈式弹窗管理；弹窗打开时会插入全屏 `ModalBlocker`（RGB 0.04/0.035/0.03，alpha 1.0），阻断原窗口输入，必须关闭当前弹窗后才能继续操作底层界面。
- 开场遮罩、朝会转场、弹窗面板和模态遮罩全部使用 alpha 为 `1.0` 的不透明背景，避免窗口叠加时露出底层内容。
- NPC 信息弹窗扩大到稳定尺寸，并对所有固定标签与动态生成的五维属性标签启用自动换行，避免文本溢出窗口。

### 8.5 视觉美术（运行时合成）

主界面 4 张卡的图样采用 **Pillow 合成管线**（非 AI 出图）：

- 基础底色 `RGB(24, 18, 14)` 暗漆
- 左 58% 嵌入 1024² 方形背景图 + 金边
- 右侧暗渐变
- 底部 86px 黑色 shade + 居中标题/副标题
- 脚本位于 `art/cards/generate_landscape_cards.py`（可生成横版作图库参考但不入主界面）

历史图库参考（`art/portraits/`、`art/landscape/`）由 1024² 独立方图组成。

---

## 9. 目录结构与分包架构

```
game_history/
├── art/                                         # Pillow 合成素材与脚本
├── docs/                                        # 设计文档与技术方案
└── donghan/                                     # 主项目
    ├── README.md                                # 本文件
    ├── Backend/
    │   ├── Backend.sln / Backend.slnx
    │   ├── DonghanEngine.Core/                  # 核心后端逻辑类库（DDD 目录封包架构）
    │   │   ├── Models/                          # 领域实体与不可变/只读集合值对象
    │   │   │   ├── GameState.cs                 # 天子大局状态（只读集合私有化）
    │   │   │   ├── NpcState.cs                  # 大臣/人物实体（五维 Clamp 与充血方法）
    │   │   │   ├── Province.cs                  # 州郡实体（民心守军与平叛招安充血方法）
    │   │   │   ├── ArmyState.cs                 # 禁军实体（士气忠诚与充血方法）
    │   │   │   ├── ImperialEdict.cs             # 奏折与御批选项
    │   │   │   ├── NpcRelation.cs               # 人物羁绊与关系边
    │   │   │   └── CourtSpeech.cs               # 朝会辩论发言与阶段模型
    │   │   ├── Constants/                       # 领域常量、文学 Trait 与派系枚举
    │   │   │   ├── TraitNames.cs                # 31 项成语/通俗特征常量
    │   │   │   ├── FactionCatalog.cs            # 六大派系常量
    │   │   │   └── FactionStance.cs             # 派系朝堂博弈矩阵
    │   │   ├── AI/                              # AI 调度与大臣交互
    │   │   │   ├── IAIScheduler.cs              # AI 调度器契约
    │   │   │   ├── AIOrchestrationResult.cs     # 编排结果模型
    │   │   │   ├── IMinisterAgent.cs            # 大臣智能体契约
    │   │   │   ├── IEventOracle.cs              # 随机天灾先知契约
    │   │   │   ├── IntentClassifier.cs          # 玩家意图分类
    │   │   │   └── FactionSpeechBank.cs         # 派系专属台词库
    │   │   ├── Npc/                             # NPC 注册与生命周期
    │   │   │   ├── INpcRegistry.cs              # 统一登庸/下野注册中心
    │   │   │   ├── INpcLifecycleManager.cs      # A/B轨生命周期与发病机制
    │   │   │   ├── HistoricalNpcPresets.cs      # 71 位史实人物冷备库
    │   │   │   ├── HistoricalNpcRelations.cs    # 37 条史实羁绊关系
    │   │   │   └── NpcTraitEvaluator.cs         # 纯函数能力评估器
    │   │   ├── Events/                          # 历史大事件与叙事
    │   │   │   ├── EventNarratives.cs           # 黄巾、何进、董卓大事件
    │   │   │   └── INarrator.cs                 # 叙事输出器接口
    │   │   ├── Services/                        # 细粒度领域服务接口与引擎主干
    │   │   │   ├── IGameEngine.cs               # ISP 单一职责领域接口契约组合
    │   │   │   ├── GameEngine.cs                # 引擎主干编排
    │   │   │   ├── GameEngine.Province.cs       # 州郡治理与平叛
    │   │   │   ├── GameEngine.ActionSettlements.cs # 动作数值结算与反噬
    │   │   │   └── GameEngine.Narrative.cs      # 叙事与实录渲染
    │   │   └── DonghanEngine.Core.csproj
    │   └── DonghanEngine.Tests/                 # 核心引擎自动化测试套件（93 个测试）
    │       ├── DomainEncapsulationTests.cs      # 领域模型 Clamp 与封装安全性测试
    │       ├── EngineTests.cs                   # 核心引擎流程测试
    │       ├── HistoricalNpcPresetTests.cs      # 冷备人物与关系网测试
    │       ├── ProvinceRebellionTests.cs        # 叛乱/黄巾/平叛/招安测试
    │       └── DonghanEngine.Tests.csproj
    ├── Frontend/                                # Godot 4.6.3 (.NET) 游戏前端
    │   ├── V2/                                  # V2 细粒度接口优先架构
    │   │   ├── Contracts/                       # 单方法服务接口与快照数据契约
    │   │   │   ├── IGameplayServices.cs         # ISP 单一方法业务契约
    │   │   │   ├── GameplayContracts.cs         # 只读快照模型（Minister/Province）
    │   │   │   └── V2Runtime.cs                 # V2 运行时环境容器
    │   │   ├── Adapters/                        # 领域服务适配器（每个类单一职责）
    │   │   │   ├── GameEngineStateReader.cs     # 状态快照读取适配
    │   │   │   ├── GameEngineCourtService.cs    # 大朝会适配
    │   │   │   ├── GameEngineEdictService.cs    # 尚书台奏折批阅适配
    │   │   │   ├── GameEngineIntelService.cs    # 密札与州郡治理适配
    │   │   │   ├── GameEngineSpecialActionService.cs # 籍没/卖官/微服等特殊行动适配
    │   │   │   ├── GameEngineTravelService.cs   # 宫廷移动巡幸适配
    │   │   │   ├── GameEngineTurnService.cs     # 旬日更迭与快进适配
    │   │   │   ├── GameEngineWestGardenService.cs # 西园禁军运维适配
    │   │   │   └── V2RuntimeFactory.cs          # 契约工厂组装
    │   │   └── MainSceneV2.cs                   # V2 纯契约驱动主场景交互控制
    │   ├── V2.Tests/                            # 前端 V2 契约与适配层独立测试（40 个测试）
    │   │   └── *.cs                             # 13 个测试文件覆盖所有 V2 服务契约
    │   ├── MainScene.*.cs                       # 原始/并存 UI 面板与样式支持
    │   ├── WindowManager.cs                     # 栈式弹窗与全屏模态阻断器
    │   ├── MainScene.tscn                       # 主场景树
    │   ├── project.godot                        # Godot 独占全屏与项目配置
    │   └── DonghanFrontend.csproj
    └── Console/                                 # 控制台原型 CLI
        ├── Program.cs
        └── Console.csproj
```

### 9.1 全量自动化测试保障（133/133 全绿通过）

- **后端核心测试 (`DonghanEngine.Tests`)：** **93 个测试** 覆盖领域属性防御 Clamp、不可变/只读集合操作、Traits 复合加成、历史大事件时序、NPC 生老病死、地方叛乱与招安平叛。
- **前端 V2 测试 (`DonghanFrontend.V2.Tests`)：** **40 个测试** 覆盖状态快照提取、百官名册五维与 Traits 展示、大朝会与自由诏书、奏折批阅与流产、西园操作、特殊行动与连续旬日快进。

### 9.2 构建与测试命令

```bash
# 1. 运行核心后端测试 (93/93 Passed)
export PATH=$HOME/.dotnet:$PATH
dotnet test Backend/DonghanEngine.Tests/DonghanEngine.Tests.csproj

# 2. 运行前端 V2 契约架构测试 (40/40 Passed)
dotnet test Frontend/V2.Tests/DonghanFrontend.V2.Tests.csproj

# 3. 前端工程构建验证
dotnet build Frontend/DonghanFrontend.csproj -v minimal
```

### 9.3 Git 卫生

`.gitignore` 已排除：

- `donghan/Frontend/.godot/`（编辑器缓存）
- `donghan/Frontend/godot/`（runtime app_userdata 日志）
- `donghan/Frontend/bin/` `obj/` `*.import`（编译产物）
- `wuxia-inheritance/` `xiuxian-game/` `武侠/`（其他子项目）
