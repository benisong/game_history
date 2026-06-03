# 2026-06-02 东汉末年汉灵帝：AI调度员与异步朝局抉择系统设计规范

## 1. 核心目标
建立以东汉末年汉灵帝（玩家）为视角的 **AI调度员（AI Dispatcher）**，驱动多角色 AI（外戚、宦官、清流）在朝堂、情报、政务场景下的拟真演变与博弈。
通过引入 **“旬更更新”** 时间系统、**“四大核心界面”** 以及 **“异步双段延迟加载缓存（Async Deferral Queue）”**，完美保障前端丝滑交互并抹平 AI 模型通信的网络延迟。

---

## 2. 核心界面与模块架构

### 2.1 旬日日期更迭系统 (`Ten-Day Step`)
* 游戏时间放弃通用的“回合”字样，改用东汉历史写实纪元：以 **“旬（每旬十天）”** 为基础更迭单位。
* 三旬为一月，孟春、仲春、季春等四季更替，对应天灾、民生岁收等气候模型。

### 2.2 主界面四大核心操作分区
1. **情报 (Intelligence)**：
   * 呈现大汉十三州民心、天灾预警、黄巾暗流。
   * **群臣密录**：呈现已被近臣/锦衣卫侦察出的官员属性（好感、权势、隐藏政治阴谋、私蓄赃款）。*主界面不再直接悬挂官员状态以增强情报探索感*。
2. **政务处理 (Imperial Edicts)**：
   * 处理 AI 调度员派发的地方奏折、突发民生、灾民起义、以及天子私生活（西园卖官）等常态化决策。
3. **朝会 (Grand Court)**：
   * 召集群臣，进行大政博弈。
   * 呈现**“当前参与朝会百官”**的名册、权势、好感及党派。
   * 触发 **“打头阵与多角力群辩”** 核心博弈。
4. **饮酒娱乐 (Pleasure & Feasts)**：
   * 起驾西园/后宫。与张让、宠妃奢靡饮酒，宠信外戚等。
   * 用于恢复天子精力、健康值，或开启西园内库捞钱。

---

## 3. AI 调度员与异步双段缓冲架构 (Async Deferral Protocol)

为了确保天子在发布诏令（自由对话输入）和召集朝会时没有网络卡顿感，设计 **双通道延迟加载机制**。

### 3.1 架构数据流图
```
  [天子/玩家操作 (例如: 点击朝会 / 输入自由诏令)]
           │
           ├───► [同步通道] (0 延迟)
           │         - 底层 C# 规则引擎计算物理数值 (国库、民心变化)
           │         - 锁定下一个打头阵大臣、初始上奏意图
           │         - 前端立刻播放召集动画/转场遮罩
           │
           └───► [异步通道] (后台 Task 启动)
                     - AI 调度员 (AI Dispatcher) 接收当前 GameState 
                     - 并行分发给 1-3 个大臣智能体 (Agentic Roles)
                     - 大臣 AI 在后台计算“弹劾、迎合或密谋”的叙事文本与数值
                     - 写入 Deferral Buffer 队列
```

### 3.2 异步缓冲触发序列（以召集【朝会】为例）
1. **第一阶段：动画转场与首发上奏（同步）**
   * 玩家点击【朝会】。
   * 引擎立刻根据最急迫的民生状态（例如“冀州大旱低于活命线”），锁定大将军何进打头阵上奏。
   * 前端播放动画（“群臣上殿，趋步趋趋...”）并展示何进首发奏折：*“大将军何进上奏：冀州旱灾爆发，流民嗷嗷待哺，请速发国库 3000 万钱赈济！”*。
2. **第二阶段：点击过渡与多角色群辩生成（异步）**
   * 当玩家在阅读何进上奏，或在点击“准奏”、“驳回”、“交由群臣商议”的间隙。
   * **AI 调度员**已于后台开始运行。它检测到何进的赈灾主张，识别张让、曹操、袁绍等其他在场大臣的性格：
     * **张让（宦官派）立场**：何进必中饱私囊，极力反对。
     * **曹操（清流/强臣）立场**：准奏，但须指派廉洁官员，并自请监督。
   * AI 调度员将这些角色反馈打包生成，压入 `CourtDebateQueue`。
   * 当玩家点击“听取群臣意见”时，多名大臣的党争弹劾与即时对话便可以**毫无卡顿、完全异步地动态刷出**。

---

## 4. 数据结构实体与接口设计

### 4.1 核心状态实体扩展 (`GameState.cs`)
```csharp
public class GameState
{
    // 时间系统
    public int Year { get; set; } = 184; // 中平元年
    public int Month { get; set; } = 1;
    public int Xun { get; set; } = 1; // 1-3 旬 (上旬、中旬、下旬)

    // 前端四大系统缓存
    public List<PoliticalEdict> ActiveEdicts { get; set; } = new(); // 待处理政务
    public List<IntelReport> IntelReports { get; set; } = new();   // 探索出来的情报

    // 异步朝局辩论缓冲区
    public Queue<CourtSpeech> CourtDebateQueue { get; set; } = new();
}
```

### 4.2 调度器决策与任务实体 (`SchedulerDecision.cs`)
```csharp
public class CourtSpeech
{
    public string MinisterId { get; set; } = string.Empty;
    public string MinisterName { get; set; } = string.Empty;
    public string SpeechText { get; set; } = string.Empty;
    public string Stance { get; set; } = "OPPOSE"; // AGREED / OPPOSE / RETALIATE (弹劾/迎合/反噬)
    public int ExpectedFavorabilityChange { get; set; }
    public int ExpectedPowerChange { get; set; }
}

public class AIOrchestrationResult
{
    public string PrimaryIntent { get; set; } = string.Empty; // POLITICS / POLICY / PERSONAL / SACRIFICE
    public List<CourtSpeech> Speeches { get; set; } = new();   // 多角色插播群辩队列
    public string NarrativeResponse { get; set; } = string.Empty; // 天子诏书下达后的总回执叙事
}
```

### 4.3 调度器核心接口 (`IAIScheduler.cs`)
```csharp
public interface IAIScheduler
{
    // 异步分析玩家输入意图并触发多智能体群辩
    Task<AIOrchestrationResult> OrchestrateGrandCourtAsync(string playerInput, string activeOfficerId, GameState state);

    // 旬更演进：AI 调度员后台调度，为大臣规划隐藏阴谋和派化日常随机任务、天灾预警
    Task演进周更日常任务Async(GameState state);
}
```

---

## 5. 自我审查 (Self-Review)
* **占位符检查**：无 TBD 或待定。所有机制（旬制、双通道、多角色群辩、双段异步加载）均有明确的数据实体和交互步骤支撑。
* **一致性**：逻辑与 Godot 前端的移驾面板深度契合，官员状态隐藏到朝会与情报，完全消除主界面杂音。
* **可测试性**：底层 C# 接口提供完备的 `Task` 返回值，便于通过 Mock 模型在 xUnit 单元测试中完美验证多通道缓冲和队列出栈逻辑。
