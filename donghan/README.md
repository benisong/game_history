# 《东汉末年汉灵帝：大朝会、西园与多智能体政治博弈策略游戏》后端与前端控制核心架构总纲

---

## 1. 项目概述

《东汉末年汉灵帝》是一款以东汉末年汉灵帝为主角的历史推演与改写文字策略游戏。游戏采用 **C# (.NET 8.0) 模块化后端类库** 作为核心逻辑大脑，结合 **Godot 4.6.3 (.NET/C#)** 游戏引擎搭建原生第一人称前端交互。

本项目规避了高算力损耗的网页套壳模式，完全采用面向对象（C# OOP）的高性能策略引擎，并预留了面向多智能体（Multi-Agent）大语言模型（如 DeepSeek/Gemini）的异步调度中间件与防御性数据缓冲槽，实现真实的朝党斗争、西园理财、天灾赈灾及帝王心术博弈。

**项目规模**：C# 源码 **9208 行**（Backend 2665 + Tests 1365 + Frontend 5178），3 个 xUnit 测试文件 60 个测试方法 100% 通过。

---

## 2. 核心数据模型 (`DonghanEngine.Core`)

### 2.1 五大天子物理属性 (`GameState`)

- **皇权 (ImperialPower)** `[0-100]`：代表天子号令天下的权威度。初始值极弱 **`25`**（政令不出宫门）。过低将导致抄家无官出列响应，甚至引发党羽联合弹劾与逼宫兵变。
- **国库 (Treasury)** `[单位: 万钱]`：大汉朝廷公款，初始窘迫 **`8000`** 万钱，用于大朝会赈济。
- **私库 (PrivateTreasury)** `[单位: 万钱]`：西园天子内库，初始告急 **`1200`** 万钱，用于犒军、阅兵。
- **民心 (PopularSupport)** `[0-100]`：天下百姓对汉廷的拥戴值。初始濒危 **`28`**（低于活命线）。低于 30 将直接触发特大地方饥荒或黄巾暴乱预警。
- **健康 (Health)** `[0-100]`：天子龙体状态，因临幸后宫、饮酒纵乐而增损。初始危重 **`35`**，归零则汉灵帝崩殂。

### 2.2 纪元时间戳

`GameState.Year=184`（中平元年，光和七年）、`Month=4`、`Xun=1`（上旬）。三旬为月、十二月为年。

### 2.3 十三州郡 (`Province`)

每郡独立维护 `LocalSupport`（0-100）、`Garrison`、`Wealth`、`GovernorId`、`IsRebelling`、`RebelFaction`、`RebellionMonths`、`LowSupportStreakMonths`。叛乱规则在 `GameEngine.Province.cs` 中实现：

- **黄巾之乱触发**：连续 3 月 `LocalSupport<10` 自动触发。
- **野心叛乱**：已派太守且 `LocalSupport ∈ [10,30]` 时按太守野心权重掷骰。
- **平叛 / 招安**：玩家可"颁授虎符 · 出征"或"持节 · 招安"。

### 2.4 NPC 五维与历史预设 (`NpcState`)

- **五维属性 (0-100)**：武力 Martial、统帅 Leadership、政治 Politics、魅力 Charisma、野心 Ambition。
- **派系 (Faction)**：清流派 / 外戚派 / 阉党派 / 割据军阀。
- **官阶 (TitleTier)** 0-4：白身布衣 → 三公/大将军（曹操 1、蹇硕 2、张让 3、何进 4）。
- **初始位置 (InitialLocation)**：洛阳朝堂 / 地方州郡 / 在野 / 边军 / 敌对势力。
- **登场条件 (EntryCondition)**：开局 / 事件触发 / 年月触发 / 冷备。
- **敌对首领 (IsHostile)**：不进入任官/平叛/招安候选（黄巾军与西凉叛军首领标 `IsHostile=true`）。
- **史实卒年 (HistoricalDeathYear)** + **史料来源备注 (SourceNote)**：仅作参考。

### 2.5 派系关系网 (`NpcRelation` + `HistoricalNpcRelations`)

`HistoricalNpcRelations.cs` 维护 **37 条历史关系**，类型枚举：

- Kinship（血缘）、Patronage（提携）、FactionAlly（同派系）、TeacherStudent（师生）、SwornBond（结义）、Rivalry（路线之争）、Hostility（死敌）、Command（统属）、RegionalTie（地域）

节选：

| 来源 | 目标 | 类型 | 强度 | 标签 |
|---|---|---|---|---|
| 何进 | 张让 | Hostility | 88 | 诛宦死敌 |
| 何进 | 袁绍 | FactionAlly | 68 | 诛宦暂盟 |
| 张让 | 赵忠 | FactionAlly | 92 | 十常侍核心 |
| 袁绍 | 袁术 | Rivalry | 72 | 宗族相竞 |
| 卢植 | 刘备 | TeacherStudent | 86 | 师生 |
| 卢植 | 公孙瓒 | TeacherStudent | 80 | 师生 |
| 董卓 | 李傕 | Command | 86 | 董卓部曲 |
| 丁原 | 吕布 | Command | 82 | 并州上下 |

### 2.6 西园新军 (`ArmyState`)

`WestGardenArmy` 八校尉直属亲军：`Size`（默认 8000）、`BasePayPerTurn`（默认 120 万钱/旬）、`Morale`（0-100）、`Loyalty`（0-100）。

### 2.7 历史人物冷备 (`HistoricalNpcPresets`)

冷备名单 **71 位** 真实历史人物，涵盖黄巾军（张角、张宝、张梁）、十常侍（赵忠、段珪、毕岚、夏恽、郭胜、宋典、韩悝）、清流名臣（王允、卢植、蔡邕、马日磾、袁隗、杨彪）、军政大将（皇甫嵩、朱儁、张温、崔烈）、边军与军阀（董卓、吕布、丁原、公孙瓒、刘表、刘焉、刘虞、韩馥、张扬、马腾）、士人谋士（荀爽、荀彧、荀攸、郭嘉、程煜、贾诩、田丰、桥玄、桥瑁）、武将（张辽、张绣、张济、张郃、于禁、太史慈、张飞、关羽、赵云、孔融、孔宙、蒯越、祢衡）、黄巾外围（张燕、张牛角、边章、北宫伯玉、韩遂）等。`IsHostile=true` 标记 7 人（黄巾与西凉叛军首领 + 张燕），不进入任官候选。

### 2.8 Trait 字符串常量 (`TraitNames`)

所有 **31 项** trait 字符串集中定义在 `TraitNames.cs` 常量类，杜绝硬编码拼写错误。详见 §5。

---

## 3. 历史向"旬日"时间系统

游戏不使用通用的"Turn"，而采用东汉纪实 **"旬（每旬十天）"** 作为时间更迭单位：

- 一月包含三旬（上旬、中旬、下旬），十二月为一年。
- 每旬流逝时（通过 `NextXunAsync()`），系统自动扣除开支，并通过 AI 调度员生成最新的百官阴谋（`IntelReports`）与地方紧急奏折（`ActiveEdicts`）。

---

## 4. NPC 生命周期与派系关系

为了建立具有生老病死、历史演进的人才生态，设计了统一的 NPC 物理管理器。其不自主运行，**完全将调度支配权公开给 AI 调度员（Orchestrator）进行显式指令调度**：

### 4.1 统一的登录（注册）接口 (`INpcRegistry`)

- **登录注册 (`RegisterNpc`)**：由可视化界面输入、本地配置文件读取或 AI 调度员自创登庸，将新 NPC 注入游戏大局。
- **致仕下野 (`DeregisterNpc`)**：大臣由于生老病死、贬庶流放或被天子强抄家产，物理退场。其政治生平、死亡或去职原由将作为历史大案永久记入《大汉起居注》（Chronicle）。

### 4.2 双轨制生命周期管理器 (`INpcLifecycleManager`)

- **A轨本地预置（Scheme A）**：优先搜寻本地运行目录下的 `donghan_preset_npcs.json`。
- **B轨嵌入冷备（Scheme B Fallback）**：若本地 JSON 被玩家手动删除或格式损毁，系统将**自动且静默地无缝降级**采用 C# 硬编码冷备静态名单（71 位真实历史人物），**100% 确保游戏在任何恶劣环境下绝对不崩溃**。
- **老病死启发式演进与惰性登台**：
  - **惰性加载部署 (`DeployNpcToCourt`)**：开局仅 5 人（何进、张让、曹操、蹇硕、袁绍）上场。备用英贤（董卓、卢植、王允等）静态沉睡，由调度员按需部署上台，大幅节省内存。
  - **老病死演进**：每逢 3 旬（1个月），所有大臣年龄增长 1 岁。若年龄超过其期望寿命（`BaseLongevity`），每旬会有 15% 的概率寿终寝于邸舍；同时每旬有千分之三的概率染上"洛阳伤寒温疫"，健康值暴跌 30 点。
- **时间戳原子防颠簸锁（Timestamp Lock）**：内部实装 `LastNpcProcessedTimestamp` 原子时间戳锁。确保在一个游戏旬日之内，无论玩家网络颠簸或调度师多次调用生命 Process，生命周期都只会安全触发 **1 次**，彻底断绝"由于高频调用导致官员瞬间老死"的底层漏洞。

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
- **黄门密札**：点击进入【情报】独占弹窗，显示十三州预警、地方局势与可执行的治理动作。
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
| `MainScene.IntelPanel.cs` | 297 | 黄门密札 13 州预警 + 叛乱 + 地方详情 |
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

## 9. 目录结构与自动化测试验证

```
game_history/
├── art/                          # Pillow 合成素材与脚本
│   ├── cards/                    # 主界面 4 张卡合成脚本
│   ├── landscape/                # 横版参考图
│   ├── portraits/                # 1024² 历史人物方图
│   └── icon/                     # 应用图标
├── docs/                         # 设计文档与协作记录
└── donghan/                      # 主项目
    ├── README.md                 # 本文件
    ├── Backend/
    │   ├── Backend.sln / Backend.slnx
    │   ├── DonghanEngine.Core/                       # 核心后端逻辑类库（2665 行）
    │   │   ├── GameState.cs                          # 五大属性 + 时间戳 + Province/Relation
    │   │   ├── GameEngine.cs                         # 主流程编排 + 玩家输入解析 (511)
    │   │   ├── GameEngine.ActionSettlements.cs       # 规则结算、Outcome 判定 (366)
    │   │   ├── GameEngine.Narrative.cs               # 叙事文本、实录 RichText 反馈 (125)
    │   │   ├── GameEngine.Province.cs                # 叛乱检测、黄巾触发、平叛招安 (624)
    │   │   ├── ImperialEdict.cs                      # 5 大奏折、EdictOption 增量维度
    │   │   ├── NpcState.cs / NpcRelation.cs          # NPC 五维 + 派系关系类型枚举
    │   │   ├── HistoricalNpcPresets.cs               # 71 位真实历史人物冷备名单
    │   │   ├── HistoricalNpcRelations.cs             # 37 条派系/血缘/师生关系网
    │   │   ├── TraitNames.cs                         # 31 项 trait 字符串常量
    │   │   ├── NpcTraitEvaluator.cs                  # 复合共存累乘评估引擎
    │   │   ├── INpcRegistry.cs / NpcRegistry.cs      # 动态登庸与下野
    │   │   ├── INpcLifecycleManager.cs               # A/B 轨冷备、寿命发病、按需部署
    │   │   ├── IAIScheduler.cs / IEventOracle.cs     # AI 编排 + 随机事件
    │   │   ├── IMinisterAgent.cs / INarrator.cs      # 单大臣对话 + 叙事渲染
    │   │   └── DonghanEngine.Core.csproj
    │   └── DonghanEngine.Tests/                      # xUnit 自动化测试（1365 行 / 60 个测试）
    │       ├── EngineTests.cs                        # 28 个核心引擎测试
    │       ├── HistoricalNpcPresetTests.cs           # 6 个冷备名单 + 关系网测试
    │       ├── ProvinceRebellionTests.cs             # 26 个叛乱 / 黄巾 / 招安 / 平叛测试
    │       └── DonghanEngine.Tests.csproj
    ├── Console/                                       # Legacy 控制台原型（仅保留可执行 CLI 入口）
    │   ├── Program.cs
    │   └── Console.csproj
    └── Frontend/                                       # Godot 4.6.3 (.NET) 游戏项目（5178 行）
        ├── MainScene.cs                                # 主入口 + 御案卷轴 + 全屏恢复 (185)
        ├── MainScene.CoreActions.cs                    # 弹窗栈 + PopupSkin 6 类 (777)
        ├── MainScene.CourtAndOpening.cs                # 开场 + 起驾转场 (389)
        ├── MainScene.CourtPanel.cs                     # 大朝会五段式转场 (810)
        ├── MainScene.CourtTopics.cs                    # 五大常议议题 (137)
        ├── MainScene.DeskAndAffairs.cs                 # 御案折匣 · 尚书台 (475)
        ├── MainScene.IntelPanel.cs                     # 黄门密札 13 州 (297)
        ├── MainScene.IntelActions.cs                   # 情报决策 (411)
        ├── MainScene.Ministers.cs                      # 群臣档案 + 籍没 (408)
        ├── MainScene.WestGardenPanel.cs                # 西园犒赏/募兵/账目 (474)
        ├── MainScene.MockServices.cs                   # 圣旨 → 样例回奏 (120)
        ├── MainScene.Style.cs                          # 统一样式函数库 (597)
        ├── WindowManager.cs                            # 栈式弹窗 + ModalBlocker (98)
        ├── MainScene.tscn                              # 场景树
        ├── project.godot                               # Godot 全屏配置
        └── DonghanFrontend.csproj
```

### 9.1 测试覆盖（60/60 100% 通过）

- **`EngineTests.cs`（28 个 Fact）**：验证 A/B 轨冷备降级（董卓）、刘备登庸与寿命变老、刘备「经天纬地」开仓提振、曹操「老谋深算」强抄张让反噬折减、以及「经天纬地」+「爱民如子」的 **Traits 共存复合累乘 ($1.20 \times 1.15 = 1.38x$)**。
- **`HistoricalNpcPresetTests.cs`（6 个 Fact）**：验证冷备名单完整性、派系关系网引用合法、敌对首领排除。
- **`ProvinceRebellionTests.cs`（26 个 Fact）**：验证黄巾之乱触发条件、野心叛乱、招安/平叛分支、远京距离惩罚。

### 9.2 构建与测试命令

```bash
# 后端测试
cd /opt/game_history/donghan/Backend
dotnet test DonghanEngine.Tests/DonghanEngine.Tests.csproj
# 期望：Passed!  - Failed: 0, Passed: 60, Skipped: 0, Total: 60

# 前端构建
cd /opt/game_history/donghan/Frontend
dotnet build DonghanFrontend.csproj -v minimal
# 期望：Build succeeded. 0 Warning(s) 0 Error(s)

# Godot headless 验证（不打开 GUI）
/opt/godot/Godot_v4.6.3-stable_mono_linux_x86_64/Godot_v4.6.3-stable_mono_linux.x86_64 \
  --headless --path /opt/game_history/donghan/Frontend
```

### 9.3 Git 卫生

`.gitignore` 已排除：

- `donghan/Frontend/.godot/`（编辑器缓存）
- `donghan/Frontend/godot/`（runtime app_userdata 日志）
- `donghan/Frontend/bin/` `obj/` `*.import`（编译产物）
- `wuxia-inheritance/` `xiuxian-game/` `武侠/`（其他子项目）
