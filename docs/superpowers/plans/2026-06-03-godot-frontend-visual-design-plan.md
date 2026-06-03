# Godot 4.3 汉灵帝前端主场景物态化交互与朝仪遮罩系统实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 基于“赤霄御案”视角，重构 `MainScene.cs` 的 UI 交互系统。绑定四大物理器物节点事件（漆木匣、密札、玉玺、博山炉），并实装对接大朝仪三段式过渡文本的 `Tween` 遮罩逻辑，完全隐藏 AI 调度的大政运算延迟。

**Architecture:** 
1. Godot `MainScene.cs` 重构：声明 4 个 `TextureButton` 和对应的辅界面 `Control` 层面板。
2. 遮罩转场系统：在点击“玉玺”进入朝会时，调用引擎 `GetGrandCourtRitualStages()` 并通过 Godot 计时器与 Tween 实装换装、趋步、鸣磬三阶段文本演进。

**Tech Stack:** C# (Godot 4.3 .NET), DonghanEngine.Core

---

## 拟创建/修改的文件映射 (File Structure)

- `C:\Users\beni3\opencode\donghan\Frontend\MainScene.cs` (修改)：重构 UI 树节点绑定，增加四大物件的点击事件，并实现三阶段朝仪大典的异步遮罩表现逻辑。

---

## 具体实施任务 (Bite-Sized Tasks)

### Task 1: 在 `MainScene.cs` 中建立节点映射与事件绑定

**Files:**
- Modify: `C:\Users\beni3\opencode\donghan\Frontend\MainScene.cs`

- [ ] **Step 1: 在 `MainScene` 顶部声明节点引用**

替换旧的按钮定义，引入新的器物按钮和面板：
```csharp
    // 物理器物按钮
    private Button _btnAffairsBox;      // 雕龙漆木匣 (政务)
    private Button _btnIntelToken;      // 漆木密札 (情报)
    private Button _btnCourtSeal;       // 天子玉玺 (朝会)
    private Button _btnPleasureCenser;  // 铜制博山炉 (娱乐)

    // 辅界面层
    private Control _panelAffairs;
    private Control _panelIntel;
    private Control _panelCourt;
    private Control _panelPleasure;

    // 大朝仪转场遮罩
    private ColorRect _transitionMask;
    private RichTextLabel _ritualTextLabel;
```

- [ ] **Step 2: 在 `_Ready` 中安全获取节点并订阅事件**

由于实际的 `.tscn` 场景节点可能尚未在编辑器中改名，我们使用容错的 `GetNodeOrNull`：
```csharp
    public override void _Ready()
    {
        _state = new GameState();
        _scheduler = new MockScheduler(); 
        _oracle = new MockOracle();
        _ministerAgent = new MockMinisterAgent();
        _narrator = new MockNarrator();
        _engine = new GameEngine(_state, _scheduler, _oracle, _ministerAgent, _narrator);

        // 获取并绑定四个物理器物按钮（兼容旧版测试节点）
        _btnAffairsBox = GetNodeOrNull<Button>("UI_Layer/Desk/BtnAffairsBox") ?? new Button();
        _btnIntelToken = GetNodeOrNull<Button>("UI_Layer/Desk/BtnIntelToken") ?? new Button();
        _btnCourtSeal = GetNodeOrNull<Button>("UI_Layer/Desk/BtnCourtSeal") ?? new Button();
        _btnPleasureCenser = GetNodeOrNull<Button>("UI_Layer/Desk/BtnPleasureCenser") ?? new Button();

        _btnAffairsBox.Pressed += OnAffairsBoxPressed;
        _btnIntelToken.Pressed += OnIntelTokenPressed;
        _btnCourtSeal.Pressed += OnCourtSealPressed;
        _btnPleasureCenser.Pressed += OnPleasureCenserPressed;

        // 获取面板与遮罩
        _panelAffairs = GetNodeOrNull<Control>("UI_Layer/PanelAffairs");
        _transitionMask = GetNodeOrNull<ColorRect>("UI_Layer/TransitionMask");
        if (_transitionMask != null)
        {
            _ritualTextLabel = _transitionMask.GetNodeOrNull<RichTextLabel>("RitualTextLabel");
            _transitionMask.Hide();
        }

        RefreshUI();
    }
```

- [ ] **Step 3: 添加按键的空钩子方法**

```csharp
    private void OnAffairsBoxPressed()
    {
        GD.Print("【互动】打开雕龙漆木匣，翻开奏折...");
        if (_panelAffairs != null) _panelAffairs.Show();
    }

    private void OnIntelTokenPressed()
    {
        GD.Print("【互动】拿起漆木密札，黄门暗探呈上竹简...");
    }

    private void OnPleasureCenserPressed()
    {
        GD.Print("【互动】点燃博山炉，紫烟升起，起驾后宫/西园...");
        _engine.TravelToLocation("后宫");
        RefreshUI();
    }
```

---

### Task 2: 实装大朝会按钮的“三阶段大朝仪”遮罩过渡逻辑

**Files:**
- Modify: `C:\Users\beni3\opencode\donghan\Frontend\MainScene.cs`

- [ ] **Step 1: 在 `MainScene` 中编写 `OnCourtSealPressed` 和异步朝仪步进方法**

此方法使用 `async/await` 搭配 `Task.Delay` 或 `ToSignal(GetTree().CreateTimer(2.0), "timeout")`，依次播放换装、入殿、鸣磬文案，并在这 6 秒钟内同步完成后台 LLM 群辩通信的掩盖！
```csharp
    private async void OnCourtSealPressed()
    {
        GD.Print("【互动】重重砸下天子和氏璧玉玺！准备大朝会！");

        // 1. 获取三阶段历史大朝仪剧本文案
        var ritualStages = _engine.GetGrandCourtRitualStages();

        if (_transitionMask != null && _ritualTextLabel != null)
        {
            _transitionMask.Show();
            
            // 并行后台处理大朝会初始化与异步 AI 群辩逻辑（用时无感隐藏）
            _engine.TravelToLocation("宣政殿");
            _engine.StartGrandCourtSync();
            
            // 我们在此模拟一下天子批阅首发折子的空隙，触发群辩（在真实游戏中，这会在天子看完首发奏折后点击选项时发生）
            var aiTask = _engine.TriggerCourtDebateAsync("准奏", "he_jin"); 

            // 2. 步进式播放三大朝仪（前端渲染遮罩过渡）
            foreach (var stage in ritualStages)
            {
                _ritualTextLabel.Text = $"[center][b]{stage.Title}[/b]\n\n{stage.Narrative}[/center]";
                
                // 停留 2 秒展示极具历史压抑感的文字
                await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
            }

            // 等待后台通信兜底（万一 AI 超过 6 秒还没算完，此处 await 确保不会穿帮）
            await aiTask;

            _transitionMask.Hide();
            GD.Print("【系统】大朝仪结束，宫门大开，正式进入朝会博弈界面！");
        }
        else
        {
            // Fallback：如果场景节点还没搭建，直接控制台输出
            _engine.TravelToLocation("宣政殿");
            foreach(var s in ritualStages) GD.Print(s.Title + "\n" + s.Narrative);
        }

        RefreshUI();
    }
```

- [ ] **Step 2: 验证 Godot 前端 C# 代码编译无误**

Run: `dotnet build donghan/Frontend/DonghanFrontend.csproj`
Expected: 编译成功，0 Errors。
