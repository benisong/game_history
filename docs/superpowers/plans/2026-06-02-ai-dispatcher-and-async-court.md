# AI 调度员与异步朝堂抉择系统实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task in this session. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 实现以东汉末年汉灵帝为视角的 AI调度员、旬日更迭时间机制、朝堂异步多角色群辩队列与主界面四大核心逻辑（情报、政务、朝会、饮酒）。

**Architecture:** C# 规则引擎与多智能体异步缓冲混合架构。同步通道提供 0 延迟基础数据和打头阵大臣，异步通道利用 `Task.Run` 在后台多线程调度多角色群辩和日常随机阴谋。

**Tech Stack:** C# (.NET 8.0), xUnit

---

## 拟创建/修改的文件映射 (File Structure)

- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameState.cs` (修改)：扩展旬日时间系统（年、月、旬）和朝局异步辩论缓冲。
- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\IAIScheduler.cs` (修改/重构)：重新定义符合 Spec 规范的 AI调度员接口（包含多角色群辩与日常任务旬演进）。
- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameEngine.cs` (修改)：增加旬日流逝 `NextXun()`、初始化政务、情报拉取以及朝会召集的同步通道逻辑。
- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Tests\EngineTests.cs` (修改/追加)：编写对应的测试，保障旬更、异步辩论队列、多通道缓冲没有逻辑 Regression。

---

## 具体实施任务 (Bite-Sized Tasks)

### Task 1: 升级 `GameState.cs` 的旬日时间与系统缓冲

**Files:**
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameState.cs`

- [ ] **Step 1: 在 GameState 类中追加旬日字段、政务、情报、群辩队列定义**

在 `GameState` 中添加以下属性字段：
```csharp
    // 纪元时间系统（旬：1-3，每旬十天，三旬为一月）
    public int Year { get; set; } = 184; // 中平元年
    public int Month { get; set; } = 4;  // 孟春/仲春
    public int Xun { get; set; } = 1;    // 1: 上旬, 2: 中旬, 3: 下旬

    // 主界面四大操作数据结构缓存
    public List<string> IntelReports { get; set; } = new(); // 已收集的百官密录与天下异动
    public List<string> ActiveEdicts { get; set; } = new();  // 待批阅的地方奏折、政务

    // 异步朝局辩论缓冲区
    public System.Collections.Generic.Queue<CourtSpeech> CourtDebateQueue { get; set; } = new();
```

同时，在同一个文件或其顶层，定义 `CourtSpeech` 与 `AIOrchestrationResult` 两个数据承载模型：
```csharp
public class CourtSpeech
{
    public string MinisterId { get; set; } = string.Empty;
    public string MinisterName { get; set; } = string.Empty;
    public string SpeechText { get; set; } = string.Empty;
    public string Stance { get; set; } = "OPPOSE"; // AGREED / OPPOSE / RETALIATE (迎合 / 反对 / 党羽弹劾反噬)
    public int ExpectedFavorabilityChange { get; set; }
    public int ExpectedPowerChange { get; set; }
}

public class AIOrchestrationResult
{
    public string PrimaryIntent { get; set; } = "UNKNOWN"; // POLITICS / POLICY / PERSONAL / SACRIFICE
    public System.Collections.Generic.List<CourtSpeech> Speeches { get; set; } = new();
    public string NarrativeResponse { get; set; } = string.Empty;
}
```

---

### Task 2: 重构并升级 `IAIScheduler.cs` 与 AI调度员通信接口

**Files:**
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\IAIScheduler.cs`

- [ ] **Step 1: 修改并重新定义接口**

将原有的 `IAIScheduler` 接口替换为：
```csharp
using System.Threading.Tasks;

namespace DonghanEngine.Core;

public interface IAIScheduler
{
    // 异步多通道核心编排：分析玩家在朝会上的输入/诏书，并调度多名在场大臣发表群辩演说
    Task<AIOrchestrationResult> OrchestrateGrandCourtAsync(string playerInput, string activeOfficerId, GameState state);

    // 旬更演进调度：每旬流逝时，AI 调度员自主为官员生成阴谋、天下天灾警报任务
    Task OrchestrateXunUpdateAsync(GameState state);
}
```

---

### Task 3: 升级 `GameEngine.cs` 支持“旬日更迭”与“朝会/政务异步双缓冲”

**Files:**
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameEngine.cs`

- [ ] **Step 1: 实现更迭旬日流逝方法 `NextXun()`**

在 `GameEngine` 类中增加旬更方法，每次调用时：
1. `Xun` + 1。
2. 若 `Xun > 3`，`Xun = 1`，且 `Month` + 1。
3. 若 `Month > 12`，`Month = 1`，且 `Year` + 1。
4. 调用 `IAIScheduler.OrchestrateXunUpdateAsync` 触发 AI 异步日常政治任务刷新。

```csharp
    public async Task NextXunAsync()
    {
        _state.Xun++;
        if (_state.Xun > 3)
        {
            _state.Xun = 1;
            _state.Month++;
            if (_state.Month > 12)
            {
                _state.Month = 1;
                _state.Year++;
            }
        }

        _state.AddToChronicle($"【时间更迭】大汉历纪：{_state.Year}年{_state.Month}月 {(_state.Xun == 1 ? "上旬" : _state.Xun == 2 ? "中旬" : "下旬")}。");

        // 异步后台演进官员想法与天灾日常
        await _scheduler.OrchestrateXunUpdateAsync(_state);
    }
```

- [ ] **Step 2: 实现同步首发朝会方法 `StartGrandCourtSync()`**

实现大朝会开启的物理同步第一通道：
1. 确定打头阵首发大臣（例如：民心低于50，由大将军何进打头阵上奏要求开仓；反之，由十常侍张让上奏开启卖官）。
2. 清空朝会群辩缓冲区 `CourtDebateQueue`。
3. 向外抛出同步首发故事：
```csharp
    public string StartGrandCourtSync()
    {
        if (_state.CurrentLocation != "宣政殿")
            throw new InvalidOperationException("未起驾宣政殿，不可开启大朝会！");

        _state.CourtDebateQueue.Clear();

        string primaryIssueText;
        if (_state.PopularSupport < 50)
        {
            primaryIssueText = "【大将军何进上奏】：陛下！今大汉十三州民心凋敝、旱灾肆虐，流民嗷嗷待哺。臣请天子速发国库 3000 万钱赈济灾民，以防黄巾贼党作乱！";
        }
        else
        {
            primaryIssueText = "【常侍张让谄言】：陛下，西园新军扩建在即，内库空虚。奴才建言，可效仿桓帝旧制，在西园公开悬牌卖官，以充实陛下私库，岂不美哉？";
        }

        return primaryIssueText;
    }
```

- [ ] **Step 3: 实现异步群辩触发与填充方法 `TriggerCourtDebateAsync()`**

天子在听到首发大臣上奏后，自由打字输入诏令或偏护，异步开启多智能体群辩，并源源不断填充缓冲区：
```csharp
    public async Task TriggerCourtDebateAsync(string playerInput, string activeOfficerId)
    {
        // 1. 发起 AI 后台异步调度
        var orchestratorResult = await _scheduler.OrchestrateGrandCourtAsync(playerInput, activeOfficerId, _state);

        // 2. 将 AI 智能体群辩结果塞进缓冲区队列
        foreach (var speech in orchestratorResult.Speeches)
        {
            _state.CourtDebateQueue.Enqueue(speech);
        }
    }
```

---

### Task 4: 在 `EngineTests.cs` 中增加全面覆盖测试并编译测试 100% 通过

**Files:**
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Tests\EngineTests.cs`

- [ ] **Step 1: 升级测试文件中的 Mock 调度器以满足新接口规范**

修改测试类中的 `MockScheduler` 类：
```csharp
public class MockScheduler : IAIScheduler
{
    public Task<AIOrchestrationResult> OrchestrateGrandCourtAsync(string playerInput, string activeOfficerId, GameState state)
    {
        var result = new AIOrchestrationResult
        {
            PrimaryIntent = "POLITICS",
            NarrativeResponse = "天子震怒，群臣辩驳。"
        };

        // 模拟生成张让和何进针对玩家输入的群辩对话
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
        // 模拟旬更新，添加一条百官阴谋情报和一条政务待办
        state.IntelReports.Add("【群臣密录】：大将军何进正暗中调兵，意图夺取洛阳西园防权。");
        state.ActiveEdicts.Add("【冀州急折】：冀州干旱，求赐赈米。");
        return Task.CompletedTask;
    }
}
```

- [ ] **Step 2: 追加 `Test_Xun_TimeSystem_Update()` 验证旬日时间推演**

测试并验证十天一旬、三旬一月、十二月一年的时间溢出算法：
```csharp
    [Fact]
    public async Task Test_Xun_TimeSystem_Update()
    {
        // Arrange
        var state = new GameState { Year = 184, Month = 12, Xun = 3 }; // 下旬
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act & Assert
        // 跨越旬
        await engine.NextXunAsync();

        // 此时应该是：185年 1月 上旬 (Xun = 1)
        Assert.Equal(185, state.Year);
        Assert.Equal(1, state.Month);
        Assert.Equal(1, state.Xun);
        // 验证 AI 调度员在旬更时，是否成功派发了日常情报与政务
        Assert.Contains("【群臣密录】：大将军何进正暗中调兵", state.IntelReports[0]);
        Assert.Contains("【冀州急折】：冀州干旱", state.ActiveEdicts[0]);
    }
```

- [ ] **Step 3: 追加 `Test_GrandCourt_AsyncDebatePipeline()` 验证异步朝辩双通道机制**

验证同步打头阵大臣获取，以及天子下诏后异步群辩缓冲队列排队机制：
```csharp
    [Fact]
    public async Task Test_GrandCourt_AsyncDebatePipeline()
    {
        // Arrange
        var state = new GameState();
        state.CurrentLocation = "宣政殿";
        state.PopularSupport = 30; // 民心低，应触发何进赈灾首发上奏
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act
        // 1. 同步首发奏折上堂
        string firstIssue = engine.StartGrandCourtSync();

        // Assert 第一通道
        Assert.Contains("何进", firstIssue);
        Assert.Contains("赈济灾民", firstIssue);
        Assert.Empty(state.CourtDebateQueue); // 队列目前应该为空

        // Act 第二通道
        // 2. 玩家在阅读或操作间隙，后台启动异步群辩编排
        await engine.TriggerCourtDebateAsync("准奏！命曹操督办。", "he_jin");

        // Assert 第二通道
        Assert.Equal(2, state.CourtDebateQueue.Count); // 缓冲队列中应成功进栈 2 名大臣发言

        // 3. 模拟前端顺次出栈弹出对话
        var speech1 = state.CourtDebateQueue.Dequeue();
        Assert.Equal("zhang_rang", speech1.MinisterId);
        Assert.Equal("OPPOSE", speech1.Stance);

        var speech2 = state.CourtDebateQueue.Dequeue();
        Assert.Equal("cao_cao", speech2.MinisterId);
        Assert.Equal("AGREED", speech2.Stance);

        Assert.Empty(state.CourtDebateQueue);
    }
```
