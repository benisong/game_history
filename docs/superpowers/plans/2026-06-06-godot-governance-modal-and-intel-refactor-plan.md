# Godot 4.3 皇帝开屏宣召、模态防穿透与情报大收纳实装计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 实现全屏古风开篇不透明宣召、升级 `WindowManager` 支持全模态 100% 不透明与 MouseFilter 防物理穿透拦截，以及从左面板剔除天下民心/西园军势并整编塞入情报密札弹窗。

**Architecture:** 
- 在游戏启动时，生成全屏不透明遮蔽 `_openingOverlay` Panel。
- 在 `WindowManager.cs` 中：对每个压栈的弹窗，在下方动态插入一个全屏阻断器 `ColorRect` (设置其 MouseFilter 为 Stop，并涂覆 85% 暗度半透明)。将弹窗本身的 `panel` StyleBox 重载为不透明、暗金色复古边框，阻断一切底层操作。
- 移除 `MainScene.cs` 左侧军势标签，将其全数重绘在 `InitializeIntelPanel()` 情报舆图的顶部信息区，并做到数据的旬更同步。

---

### Task 1: 实装不透明防穿透模态弹窗系统 (Opaque Modal Popups)

**Files:**
- Modify: `donghan/Frontend/WindowManager.cs`

- [ ] **Step 1: 升级 `WindowManager` 的 `PushWindow` 与 `PopWindow`**
  ```csharp
  using Godot;
  using System.Collections.Generic;

  namespace DonghanFrontend;

  public partial class WindowManager : Node
  {
      private Stack<Control> _windowStack = new();
      private Stack<ColorRect> _blockerStack = new();

      // 打开一个多层全不透明模态防穿透浮窗
      public void PushWindow(Control window)
      {
          if (window == null) return;

          // 1. 创建全屏模态防穿透点击拦截器
          var blocker = new ColorRect();
          blocker.Name = $"{window.Name}_Blocker";
          blocker.Color = new Color(0.1f, 0.1f, 0.1f, 0.85f); // 85% 暗化不透明，盖死底层
          blocker.SetAnchorsPreset(Control.LayoutPreset.FullRect);
          blocker.MouseFilter = Control.MouseFilterEnum.Stop; // 拦截所有事件，彻底不漏点

          // 2. 将遮蔽阻断器动态加入场景，正好垫在弹窗下方
          window.GetParent().AddChild(blocker);
          window.GetParent().MoveChild(blocker, window.GetIndex());

          // 3. 将弹窗自身 Style 重写为 100% 物理不透明，以防半透明漏影
          if (window is Panel panel)
          {
              var opaqueStyle = new StyleBoxFlat();
              opaqueStyle.BgColor = new Color(0.12f, 0.12f, 0.12f, 1.0f); // 100% 绝对不透明深黑灰
              opaqueStyle.SetBorderWidthAll(2);
              opaqueStyle.BorderColor = new Color(0.84f, 0.67f, 0.12f, 1.0f); // 暗金框
              panel.AddThemeStyleboxOverride("panel", opaqueStyle);
          }

          _windowStack.Push(window);
          _blockerStack.Push(blocker);

          blocker.Show();
          window.Show();
          
          GD.Print($"[WindowManager]: 已呼出防穿透弹窗 {window.Name}，阻断器已生效！");
      }

      // 关闭最上层的浮窗，释放其下的阻断器
      public void PopWindow()
      {
          if (_windowStack.Count > 0)
          {
              var topWindow = _windowStack.Pop();
              topWindow.Hide();

              if (_blockerStack.Count > 0)
              {
                  var topBlocker = _blockerStack.Pop();
                  topBlocker.QueueFree(); // 销毁该层遮挡，底层重新可点
              }
              GD.Print($"[WindowManager]: 已关闭弹窗 {topWindow.Name} 并解冻一层遮挡。");
          }
      }

      // 监听全局输入 ESC 键，逐层退栈关闭窗口
      public override void _UnhandledInput(InputEvent @event)
      {
          if (@event.IsActionPressed("ui_cancel")) // 默认对应 ESC
          {
              if (_windowStack.Count > 0)
              {
                  PopWindow();
                  GetViewport().SetInputAsHandled();
              }
          }
      }
  }
  ```

- [ ] **Step 2: 编译测试模态阻断逻辑**
  运行: `dotnet build "C:\Users\beni3\opencode\donghan\Frontend\DonghanFrontend.csproj" -c Debug`
  预期: 编译成功。

---

### Task 2: 实现全屏厚重古风开局宣召 (Opening Scroll Box)

**Files:**
- Modify: `donghan/Frontend/MainScene.cs`

- [ ] **Step 1: 在 `_Ready()` 末尾动态构建开局面板 `_openingOverlay` 并通过 `WindowManager` 单独进行压栈**
  在 `MainScene.cs` 的 `_Ready()` 的最末尾，构建并 Push 一个全屏遮罩：
  ```csharp
  private Panel? _openingOverlay;

  private void ShowOpeningOverlay()
  {
      _openingOverlay = new Panel();
      _openingOverlay.Name = "OpeningOverlay";
      _openingOverlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);

      var opaqueStyle = new StyleBoxFlat();
      opaqueStyle.BgColor = new Color(0.08f, 0.08f, 0.08f, 1.0f); // 厚重墨黑褐色
      opaqueStyle.SetBorderWidthAll(4);
      opaqueStyle.BorderColor = new Color(0.72f, 0.58f, 0.12f, 1.0f); // 暗亮金边
      _openingOverlay.AddThemeStyleboxOverride("panel", opaqueStyle);

      var vBox = new VBoxContainer();
      vBox.SetAnchorsPreset(Control.LayoutPreset.Center);
      vBox.CustomMinimumSize = new Vector2(650, 400);
      vBox.AddThemeConstantOverride("separation", 35);
      _openingOverlay.AddChild(vBox);

      var title = new Label();
      title.Text = "👑  东 汉 末 年 · 汉 灵 帝  👑";
      title.HorizontalAlignment = HorizontalAlignment.Center;
      title.AddThemeFontSizeOverride("font_size", 24);
      vBox.AddChild(title);

      var desc = new RichTextLabel();
      desc.BbcodeEnabled = true;
      desc.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
      desc.Text = "[center][font_size=18]「光和七年，春。」[/font_size]\n\n" +
                  "“苍天已死，黄天当立。岁在甲子，天下大吉！”\n" +
                  "张角妖术惑众，巨鹿黄巾并起。外戚何进拥兵坐镇，十常侍张让秉国朝政。\n" +
                  "汉室倾颓，累卵之危。天下百姓，倒悬之急。\n\n" +
                  "[color=yellow]陛下，大汉的八百载基业与十三州舆图，您将如何执掌重构？[/color][/center]";
      vBox.AddChild(desc);

      var btnConfirm = new Button();
      btnConfirm.Text = "—— 临 朝 理 政 ——";
      btnConfirm.CustomMinimumSize = new Vector2(250, 45);
      btnConfirm.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
      btnConfirm.Pressed += () =>
      {
          _openingOverlay.QueueFree(); // 玩家确认后彻底销毁，显示主场景
      };
      vBox.AddChild(btnConfirm);

      AddChild(_openingOverlay);
  }
  ```

- [ ] **Step 2: 关联在 `_Ready()` 的最末尾启动，并将原本写在 `StoryOutput` 里的冗余开篇文本隐藏**
  在 `MainScene.cs` 的 `_Ready()` 结尾，隐藏原有的 `_storyOutput` 初始化文本，改为调用 `ShowOpeningOverlay()`：
  ```csharp
  // 渲染初始界面状态
  UpdateUI();
  if (_storyOutput != null)
  {
      _storyOutput.Text = "陛下已经驾临宣政殿，请在上方抚摩御案物理器物，或在下方朱批下旨，乾纲独断...";
  }
  ShowOpeningOverlay();
  ```

---

### Task 3: 局势/军势数据大收纳进“漆木密札（情报面板）”

**Files:**
- Modify: `donghan/Frontend/MainScene.cs`

- [ ] **Step 1: 隐藏左面板 `LeftPanel` 军势标签**
  在 `MainScene.cs` 的 `UpdateUI()` 中，彻底注释或删掉将具体数值刷新给 `supportLabel` 和 `armySizeLabel`、`armyMoraleLabel`、`armyLoyaltyLabel` 的代码。
  这些节点不要在左面板更新，使之保持静默！
  ```csharp
  // 隐藏原有的左侧标签更新
  var supportLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/PopularSupportLabel");
  if (supportLabel != null) supportLabel.Hide();

  var armyTitleLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmyTitleLabel");
  if (armyTitleLabel != null) armyTitleLabel.Hide();

  var armySizeLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmySizeLabel");
  if (armySizeLabel != null) armySizeLabel.Hide();

  var armyMoraleLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmyMoraleLabel");
  if (armyMoraleLabel != null) armyMoraleLabel.Hide();

  var armyLoyaltyLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmyLoyaltyLabel");
  if (armyLoyaltyLabel != null) armyLoyaltyLabel.Hide();
  ```

- [ ] **Step 2: 重新绘制“漆木密札（情报弹窗）”顶端的大局综合局势信息**
  修改 `InitializeIntelPanel()`，在左半边情报大舆图的上方，新增一个 `RichTextLabel`（命名为 `_intelGlobalStatsLabel`），专门以大字号、古典红色或暗金字样，展示天下民心与西园军势：
  ```csharp
  private RichTextLabel? _intelGlobalStatsLabel;

  private void InitializeIntelPanel()
  {
      // ... 现有构建代码不变 ...
      // 在 leftVBox 下、_provinceItemList 之上：
      _intelGlobalStatsLabel = new RichTextLabel();
      _intelGlobalStatsLabel.CustomMinimumSize = new Vector2(0, 65);
      _intelGlobalStatsLabel.BbcodeEnabled = true;
      leftVBox.AddChild(_intelGlobalStatsLabel);
      leftVBox.MoveChild(_intelGlobalStatsLabel, 1); // 移动到 Title 后面，列表之前
  }
  ```

- [ ] **Step 3: 实装大局态势更新函数**
  在每次开启密札更新舆图列表 `UpdateIntelProvinceList()` 时，同步更新顶端的态势数据：
  ```csharp
  private void UpdateIntelProvinceList()
  {
      if (_provinceItemList == null || _gameState == null) return;
      _provinceItemList.Clear();
      _provinceDetailsLabel!.Text = "请在左侧大舆图上选择一个郡县进行治理、平叛或安抚...";
      foreach (Node child in _provinceActionsVBox!.GetChildren()) child.QueueFree();

      // 同步更新情报顶部大汉总国势局势与西园军势信息
      if (_intelGlobalStatsLabel != null)
      {
          _intelGlobalStatsLabel.Text = $"[color=gold][b]● 汉室天下大局态势[/b][/color]\n" +
              $"天下民心: [color=yellow]{_gameState.PopularSupport}[/color]/100\n" +
              $"西园新军: [color=yellow]{_gameState.WestGardenArmy.Size}[/color]兵 | 士气: [color=yellow]{_gameState.WestGardenArmy.Morale}[/color]/100 | 忠诚: [color=yellow]{_gameState.WestGardenArmy.Loyalty}[/color]/100";
      }

      foreach (var p in _gameState.Provinces.Values.OrderBy(p => p.Distance))
      {
          string govName = p.GovernorId != null && _gameState.Npcs.TryGetValue(p.GovernorId, out var g) ? g.Name : "暂无太守";
          string status = p.IsRebelling ? $"⚡【叛乱中】{p.RebelFaction}" : "○ 安定";
          _provinceItemList.AddItem($"{p.Name} (太守: {govName}) {status}");
      }
  }
  ```

---

### Task 4: 一键编译、回归验收测试

- [ ] **Step 1: 验证编译通过，前端 0 错误 0 警告**
  运行: `dotnet build "C:\Users\beni3\opencode\donghan\Frontend\DonghanFrontend.csproj" -c Debug`
  预期: 生成成功。

- [ ] **Step 2: 运行全部 46 个测试用例，确保对后端、核心状态绝无回归影响**
  运行: `dotnet test "C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Tests\DonghanEngine.Tests.csproj"`
  预期: 46/46 Passed.
