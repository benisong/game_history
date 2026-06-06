# Godot 4.3 皇帝御案与地方平叛安抚面板实装计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 重构 Godot 4.3 前端 `MainScene.cs`，实现御案四大物理器物交互、尚书台奏折卷宗弹窗、大汉舆图军政弹窗（支持任命太守、军事平叛、遣使招抚四策叠加）、宣政殿大朝会弹窗以及右侧朝臣动态滚动展示。

**Architecture:** 采用 Godot 4.3 原生的 C# 代码动态创建、排版和对齐 UI 控制节点。采用 `WindowManager` 统一管理浮窗退栈，避免 `.tscn` 多人协作和位置缩放的维护性深坑。

**Tech Stack:** C# (NET 8.0), Godot 4.3 Mono / .NET.

---

### Task 1: 升级右侧朝臣列表为动态滚动列表 (Dynamic NPC Scroll List)

**Files:**
- Modify: `donghan/Frontend/MainScene.cs`
- Modify: `donghan/Frontend/MainScene.tscn`

- [ ] **Step 1: 修改 `MainScene.tscn` 场景节点**
  在 `MainScene.tscn` 中，原本 `RightPanel/Ministers` 下方有硬编码的 `HeJinButton`、`ZhangRangButton`、`DisasterReliefButton`、`CaoCaoButton`、`JianShuoButton` 等，我们要清理或者直接使它们在运行时隐藏，改为纵向滚动的 `ScrollContainer`。
  打开 `MainScene.cs`，在 `_Ready` 中隐藏硬编码的那些按钮，并动态向 `RightPanel/Ministers` 中注入一个 `ScrollContainer`，内部含有 `VBoxContainer`（命名为 `_npcListVBox`）。

- [ ] **Step 2: 实装 C# 动态文武百官渲染与更新逻辑**
  在 `MainScene.cs` 中实现 `UpdateNpcList()` 方法：
  ```csharp
  private ScrollContainer? _npcScrollContainer;
  private VBoxContainer? _npcListVBox;

  private void InitializeDynamicNpcList()
  {
      var ministersVBox = GetNodeOrNull<VBoxContainer>("RightPanel/Ministers");
      if (ministersVBox == null) return;

      // 隐藏旧的硬编码按钮，防止并立冲突
      if (_heJinButton != null) _heJinButton.Hide();
      if (_zhangRangButton != null) _zhangRangButton.Hide();
      if (_caoCaoButton != null) _caoCaoButton.Hide();
      if (_jianShuoButton != null) _jianShuoButton.Hide();

      _npcScrollContainer = new ScrollContainer();
      _npcScrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
      _npcScrollContainer.CustomMinimumSize = new Vector2(0, 300);

      _npcListVBox = new VBoxContainer();
      _npcListVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
      _npcScrollContainer.AddChild(_npcListVBox);

      // 将动态滚动列表插入到 ministersVBox 中，位于 SceneTitle 和 InteractiveLabel 之后
      ministersVBox.AddChild(_npcScrollContainer);
  }
  ```
  在 `UpdateUI()` 结束前，调用 `UpdateNpcList()`：
  ```csharp
  private void UpdateNpcList()
  {
      if (_npcListVBox == null || _gameState == null) return;

      // 清理旧节点
      foreach (Node child in _npcListVBox.GetChildren())
      {
          child.QueueFree();
      }

      // 动态载入当前在朝的所有活跃大臣
      foreach (var npc in _gameState.Npcs.Values)
      {
          if (!npc.IsActive) continue;

          string locationTag = npc.GovernedProvinceId != null ? $"【任{_gameState.Provinces[npc.GovernedProvinceId].Name}】" : "【在京】";
          var btn = new Button();
          btn.Text = $"[{npc.Faction}] {npc.Name} {locationTag}";
          btn.Alignment = HorizontalAlignment.Left;
          btn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
          
          string npcId = npc.Id;
          btn.Pressed += () => ShowMinisterDetails(npcId);
          _npcListVBox.AddChild(btn);
      }
  }
  ```

- [ ] **Step 3: 升级 `MinisterOverlayPanel` 大臣属性五维面板**
  修改 `ShowMinisterDetails(string ministerId)`，将弹窗内容升级，显示其武力、统帅、政治、魅力、野心，并挂载太守说明。
  ```csharp
  // 获取新节点
  var scrollContainer = GetNodeOrNull<ScrollContainer>("MinisterOverlayPanel/VBox/ScrollAttributes");
  // 我们可以动态通过代码在 _ministerPanel (MinisterOverlayPanel) 内部动态插入一个 Label 用于五维数值展示
  ```
  修改 `ShowMinisterDetails` 实现：
  ```csharp
  private void ShowMinisterDetails(string ministerId)
  {
      if (_gameState == null || _ministerPanel == null) return;

      if (_gameState.Npcs.TryGetValue(ministerId, out var minister))
      {
          _currentDetailsMinisterId = ministerId;

          if (_ministerTitleLabel != null) _ministerTitleLabel.Text = $"{minister.Name} ({minister.Title})";
          if (_ministerFavorabilityLabel != null) _ministerFavorabilityLabel.Text = $"好感度: {minister.Favorability} / 100";
          if (_ministerPowerLabel != null) _ministerPowerLabel.Text = $"权势: {minister.Power} / 100";

          // 显示贪腐度
          var corruptionLabel = GetNodeOrNull<Label>("MinisterOverlayPanel/VBox/MinisterCorruption");
          if (corruptionLabel != null) corruptionLabel.Text = $"贪腐度: {minister.Corruption} / 100";

          // 显示贪腐存银
          var wealthLabel = GetNodeOrNull<Label>("MinisterOverlayPanel/VBox/MinisterWealth");
          if (wealthLabel != null) wealthLabel.Text = $"私蓄赃款: {minister.StashedWealth} 万钱";

          // 新增或更新五维属性标签
          var fiveAttributesLabel = _ministerPanel.GetNodeOrNull<Label>("VBox/FiveAttributes");
          if (fiveAttributesLabel == null)
          {
              fiveAttributesLabel = new Label();
              fiveAttributesLabel.Name = "FiveAttributes";
              _ministerPanel.GetNode<VBoxContainer>("VBox").AddChild(fiveAttributesLabel);
              // 移动到 CloseButton 之前
              _ministerPanel.GetNode<VBoxContainer>("VBox").MoveChild(fiveAttributesLabel, 5);
          }
          string govText = minister.GovernedProvinceId != null ? $"【外任{_gameState.Provinces[minister.GovernedProvinceId].Name}太守】\n" : "【在京闲置】\n";
          fiveAttributesLabel.Text = govText +
              $"武力: {minister.Martial,-3} | 统帅: {minister.Leadership,-3} | 政治: {minister.Politics,-3}\n" +
              $"魅力: {minister.Charisma,-3} | 野心: {minister.Ambition,-3}";

          _windowManager.PushWindow(_ministerPanel);
      }
  }
  ```

- [ ] **Step 4: 编译并测试右侧朝臣动态列表**
  运行: `dotnet build "C:\Users\beni3\opencode\donghan\Frontend\DonghanFrontend.csproj"`
  双击运行 `run_game.bat` 观察右侧列表和点击大臣头像弹出的五维，确保无崩溃。

---

### Task 2: 御案常驻交互金器 (Emperor's Desk) 实装

**Files:**
- Modify: `donghan/Frontend/MainScene.cs`

- [ ] **Step 1: 在 `CenterPanel` 顶部动态插入御案常驻容器**
  在 `MainScene.cs` 的 `_Ready()` 中，隐藏可能缺失的旧 `UI_Layer/Desk` 按钮，直接在 `CenterPanel` 的顶部通过代码注入一个水平排版的 `HBoxContainer`。
  ```csharp
  private void InitializeEmperorsDesk()
  {
      var centerPanel = GetNodeOrNull<Panel>("CenterPanel");
      if (centerPanel == null) return;

      var deskContainer = new HBoxContainer();
      deskContainer.Name = "EmperorsDesk";
      deskContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
      deskContainer.Alignment = BoxContainer.AlignmentMode.Center;
      deskContainer.CustomMinimumSize = new Vector2(0, 45);
      
      // 绝对定位在 CenterPanel 顶端，故事文本区上移
      centerPanel.AddChild(deskContainer);
      centerPanel.MoveChild(deskContainer, 0);

      // 调整 StoryOutput，预留顶部 50 像素
      if (_storyOutput != null)
      {
          _storyOutput.OffsetTop = 55;
      }

      // 创建四大按钮
      _btnAffairsBox = CreateDeskButton("📜 雕龙漆木匣 (政务)", OnAffairsBoxPressed);
      _btnIntelToken = CreateDeskButton("🗺️ 漆木密札 (军政)", OnIntelTokenPressed);
      _btnCourtSeal = CreateDeskButton("👑 传国玉玺 (朝会)", OnCourtSealPressed);
      _btnPleasureCenser = CreateDeskButton("💨 铜制博山炉 (巡幸)", OnPleasureCenserPressed);

      deskContainer.AddChild(_btnAffairsBox);
      deskContainer.AddChild(_btnIntelToken);
      deskContainer.AddChild(_btnCourtSeal);
      deskContainer.AddChild(_btnPleasureCenser);
  }

  private Button CreateDeskButton(string text, Action pressedCallback)
  {
      var btn = new Button();
      btn.Text = text;
      btn.CustomMinimumSize = new Vector2(140, 35);
      btn.Pressed += pressedCallback;
      return btn;
  }
  ```

- [ ] **Step 2: 绑定四大器物的打开事件空套挂载**
  在 `_Ready` 中替换对旧 `UI_Layer` 寻找的逻辑，直接调用 `InitializeEmperorsDesk()`。

- [ ] **Step 3: 编译并测试御案按钮**
  运行: `dotnet build "C:\Users\beni3\opencode\donghan\Frontend\DonghanFrontend.csproj"`

---

### Task 3: 📜 实装“尚书台奏折卷宗”面板 (Affairs Panel)

**Files:**
- Modify: `donghan/Frontend/MainScene.cs`

- [ ] **Step 1: 用 C# 代码动态定义并构建 `_affairsPopup` 面板**
  尚书台奏折卷宗面板将动态生成。支持折子多行加载、选定、审批。
  ```csharp
  private Panel? _affairsPopup;
  private ItemList? _edictsItemList;
  private RichTextLabel? _edictContentLabel;
  private VBoxContainer? _edictOptionsVBox;

  private void InitializeAffairsPanel()
  {
      _affairsPopup = new Panel();
      _affairsPopup.Name = "AffairsPopup";
      _affairsPopup.Visible = false;
      _affairsPopup.CustomMinimumSize = new Vector2(640, 420);
      _affairsPopup.SetAnchorsPreset(Control.LayoutPreset.Center);

      var hBox = new HBoxContainer();
      hBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
      hBox.OffsetLeft = 15; hBox.OffsetTop = 15; hBox.OffsetRight = -15; hBox.OffsetBottom = -15;
      _affairsPopup.AddChild(hBox);

      // 左半边：奏折列表
      var leftVBox = new VBoxContainer();
      leftVBox.CustomMinimumSize = new Vector2(220, 0);
      hBox.AddChild(leftVBox);

      var listTitle = new Label();
      listTitle.Text = "尚书台待批折子";
      leftVBox.AddChild(listTitle);

      _edictsItemList = new ItemList();
      _edictsItemList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
      _edictsItemList.ItemSelected += OnEdictSelected;
      leftVBox.AddChild(_edictsItemList);

      var btnClose = new Button();
      btnCancelRelief.Text = "合上卷宗";
      btnClose.Pressed += _windowManager.PopWindow;
      leftVBox.AddChild(btnClose);

      // 右半边：奏折详情及选项
      var rightVBox = new VBoxContainer();
      rightVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
      hBox.AddChild(rightVBox);

      _edictContentLabel = new RichTextLabel();
      _edictContentLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
      _edictContentLabel.BbcodeEnabled = true;
      rightVBox.AddChild(_edictContentLabel);

      _edictOptionsVBox = new VBoxContainer();
      _edictOptionsVBox.CustomMinimumSize = new Vector2(0, 150);
      rightVBox.AddChild(_edictOptionsVBox);

      AddChild(_affairsPopup);
  }
  ```

- [ ] **Step 2: 实现奏折选中、决策与物理落账绑定**
  ```csharp
  private void OnAffairsBoxPressed()
  {
      if (_gameState == null) return;
      if (_gameState.CurrentLocation != "宣政殿")
      {
          if (_storyOutput != null)
          {
              _storyOutput.Text = "【太监急奏】\n\n“陛下，漆木折匣重器存放在宣政殿案上，请移驾宣政殿再行批阅批示！”";
          }
          return;
      }

      UpdateAffairsList();
      _windowManager.PushWindow(_affairsPopup!);
  }

  private void UpdateAffairsList()
  {
      if (_edictsItemList == null || _gameState == null) return;
      _edictsItemList.Clear();
      _edictContentLabel!.Text = "请在左侧选择一封奏折进行批阅批示...";
      foreach (Node opt in _edictOptionsVBox!.GetChildren()) opt.QueueFree();

      foreach (var edict in _gameState.ActiveEdicts)
      {
          string typeTag = edict.Type switch
          {
              EdictType.UrgentCrisis => "[急报]",
              EdictType.Impeachment => "[弹劾]",
              EdictType.Merit => "[邀功]",
              _ => "[奏折]"
          };
          _edictsItemList.AddItem($"{typeTag} {edict.Title} (剩{edict.ExpiryXun}旬)");
      }
  }

  private void OnEdictSelected(long index)
  {
      if (_gameState == null || index < 0 || index >= _gameState.ActiveEdicts.Count) return;
      var edict = _gameState.ActiveEdicts[(int)index];

      _edictContentLabel!.Text = $"[b]【{edict.Title}】[/b]\n\n{edict.NarrativeContent}";

      // 清空选项
      foreach (Node opt in _edictOptionsVBox!.GetChildren()) opt.QueueFree();

      // 动态生成选项
      for (int i = 0; i < edict.Options.Count; i++)
      {
          var option = edict.Options[i];
          var btn = new Button();
          btn.Text = $"朱批：{option.Description}";
          btn.Alignment = HorizontalAlignment.Left;
          
          int optIndex = i;
          btn.Pressed += async () =>
          {
              _windowManager.PopWindow(); // 关闭尚书台弹窗
              
              // 直接复用主引擎的批折子命令
              // 在这里，我们可以直接调用 ProcessPlayerTurnAsync 并传入朱批指令
              string pInput = $"批阅奏折 {edict.Title} 选项 {optIndex + 1}";
              
              if (_storyOutput != null) _storyOutput.Text = "正在起草朱批，下达圣旨...";
              var result = await _gameEngine!.ProcessPlayerTurnAsync(pInput);
              if (_storyOutput != null) _storyOutput.Text = result.StoryText;
              UpdateUI();
          };
          _edictOptionsVBox.AddChild(btn);
      }
  }
  ```

---

### Task 4: 🗺️ 实装“大汉州郡舆图情报与平叛招抚”面板 (Intel Panel)

**Files:**
- Modify: `donghan/Frontend/MainScene.cs`

- [ ] **Step 1: 构建 `_intelPopup` 天下舆图与军政决策面板**
  ```csharp
  private Panel? _intelPopup;
  private ItemList? _provinceItemList;
  private RichTextLabel? _provinceDetailsLabel;
  private VBoxContainer? _provinceActionsVBox;

  private void InitializeIntelPanel()
  {
      _intelPopup = new Panel();
      _intelPopup.Name = "IntelPopup";
      _intelPopup.Visible = false;
      _intelPopup.CustomMinimumSize = new Vector2(720, 450);
      _intelPopup.SetAnchorsPreset(Control.LayoutPreset.Center);

      var hBox = new HBoxContainer();
      hBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
      hBox.OffsetLeft = 15; hBox.OffsetTop = 15; hBox.OffsetRight = -15; hBox.OffsetBottom = -15;
      _intelPopup.AddChild(hBox);

      // 左半边：大汉 6 州郡总览
      var leftVBox = new VBoxContainer();
      leftVBox.CustomMinimumSize = new Vector2(250, 0);
      hBox.AddChild(leftVBox);

      var listTitle = new Label();
      listTitle.Text = "🗺️ 大汉十三州舆图情报";
      leftVBox.AddChild(listTitle);

      _provinceItemList = new ItemList();
      _provinceItemList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
      _provinceItemList.ItemSelected += OnProvinceSelected;
      leftVBox.AddChild(_provinceItemList);

      var btnClose = new Button();
      btnClose.Text = "收起舆图";
      btnClose.Pressed += _windowManager.PopWindow;
      leftVBox.AddChild(btnClose);

      // 右半边：太守任命、平叛、招抚详情与执行台
      var rightVBox = new VBoxContainer();
      rightVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
      hBox.AddChild(rightVBox);

      _provinceDetailsLabel = new RichTextLabel();
      _provinceDetailsLabel.CustomMinimumSize = new Vector2(0, 100);
      _provinceDetailsLabel.BbcodeEnabled = true;
      rightVBox.AddChild(_provinceDetailsLabel);

      _provinceActionsVBox = new VBoxContainer();
      _provinceActionsVBox.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
      rightVBox.AddChild(_provinceActionsVBox);

      AddChild(_intelPopup);
  }
  ```

- [ ] **Step 2: 绑定情报面板开启与左侧列表更新**
  ```csharp
  private void OnIntelTokenPressed()
  {
      UpdateIntelProvinceList();
      _windowManager.PushWindow(_intelPopup!);
  }

  private void UpdateIntelProvinceList()
  {
      if (_provinceItemList == null || _gameState == null) return;
      _provinceItemList.Clear();
      _provinceDetailsLabel!.Text = "请在左侧大舆图上选择一个郡县进行治理、平叛或安抚...";
      foreach (Node child in _provinceActionsVBox!.GetChildren()) child.QueueFree();

      foreach (var p in _gameState.Provinces.Values.OrderBy(p => p.Distance))
      {
          string govName = p.GovernorId != null && _gameState.Npcs.TryGetValue(p.GovernorId, out var g) ? g.Name : "暂无太守";
          string status = p.IsRebelling ? $"⚡【叛乱中】{p.RebelFaction}" : "○ 安定";
          _provinceItemList.AddItem($"{p.Name} (守卫太守: {govName}) {status}");
      }
  }
  ```

- [ ] **Step 3: 渲染右侧详情与军政决策功能（太守外派、平叛、安抚）**
  ```csharp
  private void OnProvinceSelected(long index)
  {
      if (_gameState == null || index < 0 || index >= _gameState.Provinces.Count) return;
      var provincesList = _gameState.Provinces.Values.OrderBy(p => p.Distance).ToList();
      var p = provincesList[(int)index];

      string govName = p.GovernorId != null && _gameState.Npcs.TryGetValue(p.GovernorId, out var g) ? g.Name : "暂无";
      string rebStatus = p.IsRebelling ? $"[color=red]⚡ 叛乱中 ({p.RebelFaction})，已持续 {p.RebellionMonths} 个月[/color]" : "[color=green]○ 安定无事[/color]";

      _provinceDetailsLabel!.Text = $"[b][font_size=16]【{p.Name}】[/font_size][/b] 距京: [color=yellow]{p.Distance}[/color] 里\n" +
          $"地方民心: {p.LocalSupport} / 100 | 郡中守军: {p.Garrison} 人\n" +
          $"地方太守: {govName} | 当前局势: {rebStatus}";

      // 清空操作区
      foreach (Node child in _provinceActionsVBox!.GetChildren()) child.QueueFree();

      // === 太守召回/任命区 ===
      if (p.GovernorId != null)
      {
          var btnRecall = new Button();
          btnRecall.Text = $"召回太守【{govName}】";
          btnRecall.Pressed += () =>
          {
              _windowManager.PopWindow();
              var res = _gameEngine!.RecallGovernor(p.Id);
              if (_storyOutput != null) _storyOutput.Text = res.StoryText;
              UpdateUI();
          };
          _provinceActionsVBox.AddChild(btnRecall);
      }
      else
      {
          // 任命太守列表
          var hBoxGov = new HBoxContainer();
          _provinceActionsVBox.AddChild(hBoxGov);
          var lblGov = new Label(); lblGov.Text = "外派太守: ";
          hBoxGov.AddChild(lblGov);

          var availableNpcs = _gameState.Npcs.Values.Where(n => n.IsActive && n.GovernedProvinceId == null).ToList();
          if (availableNpcs.Count == 0)
          {
              var lblNone = new Label(); lblGov.Text = "（京中暂无闲置文武）";
              hBoxGov.AddChild(lblNone);
          }
          else
          {
              foreach (var npc in availableNpcs.Take(3)) // 推荐前三位
              {
                  var btnGov = new Button();
                  btnGov.Text = $"{npc.Name} (野心 {npc.Ambition})";
                  string npcId = npc.Id;
                  btnGov.Pressed += () =>
                  {
                      _windowManager.PopWindow();
                      var res = _gameEngine!.AssignGovernor(p.Id, npcId);
                      if (_storyOutput != null) _storyOutput.Text = res.StoryText;
                      UpdateUI();
                  };
                  hBoxGov.AddChild(btnGov);
              }
          }
      }

      // === 平叛与安抚 (仅在叛乱时显示) ===
      if (p.IsRebelling)
      {
          // 1. 军事平叛
          var hBoxSuppress = new HBoxContainer();
          _provinceActionsVBox.AddChild(hBoxSuppress);
          var lblSup = new Label(); lblSuppress.Text = "⚔️ 派兵平叛: ";
          hBoxSuppress.AddChild(lblSup);

          var militaryNpcs = _gameState.Npcs.Values.Where(n => n.IsActive && n.GovernedProvinceId == null).ToList();
          foreach (var mil in militaryNpcs.Take(2))
          {
              var btnSup = new Button();
              double combatPower = NpcTraitEvaluator.GetCombatPower(mil);
              double successRate = Math.Clamp(combatPower - p.Distance * 5, 5, 95);
              btnSup.Text = $"{mil.Name} (胜率{successRate:F0}%)";
              
              string milId = mil.Id;
              btnSup.Pressed += () =>
              {
                  _windowManager.PopWindow();
                  var res = _gameEngine!.SuppressRebellion(p.Id, milId);
                  if (_storyOutput != null) _storyOutput.Text = res.StoryText;
                  UpdateUI();
              };
              hBoxSuppress.AddChild(btnSup);
          }

          // 2. 遣使招抚 (简化版：派遣特使说服)
          var hBoxPacify = new HBoxContainer();
          _provinceActionsVBox.AddChild(hBoxPacify);
          var lblPac = new Label(); lblPac.Text = "🌸 遣使招安: ";
          hBoxPacify.AddChild(lblPac);

          foreach (var envoy in militaryNpcs.Take(2))
          {
              var btnPac = new Button();
              double pSkill = NpcTraitEvaluator.GetPoliticalSkill(envoy);
              btnPac.Text = $"{envoy.Name} (说服+离间)";
              
              string envoyId = envoy.Id;
              btnPac.Pressed += () =>
              {
                  _windowManager.PopWindow();
                  // 默认使用说服与离间叠加策略
                  var strategies = PacifyStrategy.Persuade | PacifyStrategy.SowDiscord;
                  var res = _gameEngine!.PacifyRebellion(p.Id, envoyId, strategies, 0);
                  if (_storyOutput != null) _storyOutput.Text = res.StoryText;
                  UpdateUI();
              };
              hBoxPacify.AddChild(btnPac);
          }
      }
  }
  ```

- [ ] **Step 4: 编译并测试大汉舆图与平叛安抚决策**
  运行: `dotnet build "C:\Users\beni3\opencode\donghan\Frontend\DonghanFrontend.csproj"`

---

### Task 5: 👑 实装“天子玉玺与大朝会召见”面板 (Grand Court Panel)

**Files:**
- Modify: `donghan/Frontend/MainScene.cs`

- [ ] **Step 1: 构建 `_courtPopup` 朝会发起弹窗**
  天子玉玺点击后，弹出输入口诏。
  ```csharp
  private Panel? _courtPopup;
  private LineEdit? _courtInput;

  private void InitializeCourtPanel()
  {
      _courtPopup = new Panel();
      _courtPopup.Name = "CourtPopup";
      _courtPopup.Visible = false;
      _courtPopup.CustomMinimumSize = new Vector2(400, 200);
      _courtPopup.SetAnchorsPreset(Control.LayoutPreset.Center);

      var vBox = new VBoxContainer();
      vBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
      vBox.OffsetLeft = 20; vBox.OffsetTop = 20; vBox.OffsetRight = -20; vBox.OffsetBottom = -20;
      vBox.ThemeOverrideConstantsAdd("separation", 15);
      _courtPopup.AddChild(vBox);

      var lbl = new Label();
      lbl.Text = "👑 宣政殿 · 鸣磬起朝会";
      lbl.HorizontalAlignment = HorizontalAlignment.Center;
      vBox.AddChild(lbl);

      _courtInput = new LineEdit();
      _courtInput.PlaceholderText = "输入圣旨口召，如：重赏曹操、弹劾张让";
      vBox.AddChild(_courtInput);

      var hBox = new HBoxContainer();
      hBox.Alignment = BoxContainer.AlignmentMode.Center;
      hBox.ThemeOverrideConstantsAdd("separation", 20);
      vBox.AddChild(hBox);

      var btnConfirm = new Button();
      btnConfirm.Text = "宣旨起大朝仪";
      btnConfirm.Pressed += OnConfirmCourtAssembly;
      hBox.AddChild(btnConfirm);

      var btnCancel = new Button();
      btnCancel.Text = "暂缓朝会";
      btnCancel.Pressed += _windowManager.PopWindow;
      hBox.AddChild(btnCancel);

      AddChild(_courtPopup);
  }
  ```

- [ ] **Step 2: 实装大朝会宣召及过渡逻辑**
  ```csharp
  private void OnCourtSealPressed()
  {
      if (_gameState == null) return;
      if (_gameState.CurrentLocation != "宣政殿")
      {
          if (_storyOutput != null) _storyOutput.Text = "【太监急奏】\n\n“陛下，天子玉玺非大朝会宣政殿不可轻动！”";
          return;
      }
      _courtInput!.Text = "";
      _windowManager.PushWindow(_courtPopup!);
  }

  private async void OnConfirmCourtAssembly()
  {
      string txt = _courtInput!.Text;
      if (string.IsNullOrWhiteSpace(txt)) return;

      _windowManager.PopWindow(); // 关闭发起弹窗

      // 触发大朝会三阶段过渡动画/文字效果
      if (_transitionMask != null)
      {
          _transitionMask.Show();
          
          string[] rituals = new[] {
              "【大朝仪 · 钟磬齐鸣】\n\n宣政殿前，礼部钟磬齐鸣，太监高唱：\n“天子登临——百官跪迎——！”",
              "【大朝仪 · 天子加冕】\n\n陛下御带冕旒，身披龙袍，在宿卫亲军的护送下缓缓步入金銮。龙威赫赫，百官莫敢直视。",
              "【大朝仪 · 宣旨群辩】\n\n“众卿平身——！”\n内侍太监缓缓展开黄绢，陛下口召已下：\n『" + txt + "』"
          };

          for (int i = 0; i < rituals.Length; i++)
          {
              if (_ritualTextLabel != null) _ritualTextLabel.Text = rituals[i];
              await ToSignal(GetTree().CreateTimer(1.5f), "timeout");
          }

          _transitionMask.Hide();
      }

      // 执行 ProcessPlayerTurnAsync
      if (_storyOutput != null) _storyOutput.Text = "百官正在唇枪舌战，商议对策...";
      var result = await _gameEngine!.ProcessPlayerTurnAsync(txt);
      if (_storyOutput != null) _storyOutput.Text = result.StoryText;
      UpdateUI();
  }
  ```

- [ ] **Step 3: 绑定三大面板的初始化**
  在 `_Ready()` 末尾，也就是 `UpdateUI()` 之前，依次调用：
  ```csharp
  InitializeDynamicNpcList();
  InitializeAffairsPanel();
  InitializeIntelPanel();
  InitializeCourtPanel();
  ```

- [ ] **Step 4: 编译并执行完整大朝会起立交互测试**
  运行: `dotnet build "C:\Users\beni3\opencode\donghan\Frontend\DonghanFrontend.csproj"`

---

### Task 6: 御案功能与地方治理总合落账集成与验收测试

- [ ] **Step 1: 后台跑通全部 46 项测试，确保核心库完全不受界面集成影响**
  运行: `dotnet test "C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Tests\DonghanEngine.Tests.csproj"`
  预期: 46 / 46 Passed.

- [ ] **Step 2: 编译 Godot Frontend 确保完全成功 (0 错误 0 警告)**
  运行: `dotnet build "C:\Users\beni3\opencode\donghan\Frontend\DonghanFrontend.csproj" -c Debug`
  预期: Build Succeeded.
