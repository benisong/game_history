using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DonghanEngine.Core;

namespace DonghanFrontend;

public partial class MainScene : Control
{
    // C# 游戏逻辑引擎与状态声明
    private GameEngine? _gameEngine;
    private GameState? _gameState;

    // UI Control 节点引用
    private Label? _reignLabel;
    private Label? _imperialPowerLabel;
    private Label? _treasuryLabel;
    private Label? _privateTreasuryLabel;
    private Label? _healthLabel;
    private RichTextLabel? _chronicleLog;
    private RichTextLabel? _storyOutput;
    private LineEdit? _playerInputEdit;
    private Button? _submitButton;

    // 大臣/场景交互 Control 节点引用
    private Label? _sceneTitleLabel;
    private Label? _interactiveLabel;
    private Button? _heJinButton;
    private Button? _zhangRangButton;
    private Button? _caoCaoButton;
    private Button? _jianShuoButton;

    // 物理器物按钮
    private Button? _btnIntelToken;      // 漆木密札 (情报)
    private Button? _btnCourtSeal;       // 朝会卷轴
    private Button? _btnTravelCarriage;  // 御驾马车
    private Label? _mainTimeLabel;       // 主界面必要时间显示

    // P1-C2 自动推进：主界面"快进 N 旬"按钮
    private Button? _fastForwardButton;

    // 辅助面板层
    private Control? _panelAffairs;

    // 大朝仪转场遮罩
    private ColorRect? _transitionMask;
    private RichTextLabel? _ritualTextLabel;
    private Control? _courtRitualOverlay;
    private bool _isUnskippableTransitionActive;

    // P1-B2 修复：缓存朝会仪式 overlay 的子节点引用，避免每帧 GetNodeOrNull + ! 强制解引用风险
    private Label? _ritualTitleLabel;
    private Label? _ritualSceneNameLabel;
    private TextureRect? _ritualArtworkRect;
    private RichTextLabel? _ritualImageTextLabel;
    private Label? _ritualCaptionLabel;
    private Label? _ritualPreloadHintLabel;

    // A1 结局面板：游戏结局弹出后只显示一次，避免 _Process 每帧重弹
    private bool _outcomeHandled;

    // P1-C1 Godot 开局新手引导：仅在第一次启动时显示，可点击/Enter 关闭
    private static bool _openingTutorialShown = false;
    private Control? _openingTutorialOverlay;
    private GameOutcome _handledOutcome = GameOutcome.Playing;

    // 场景专属 Action 节点引用
    private Label? _actionLabel;
    private Button? _sellOfficeButton;
    private Button? _drillArmyButton;
    private Button? _recruitArmyButton;
    private Label? _haremActionLabel;
    private Button? _haremRestButton;

    // 弹窗与管理器
    private Button? _travelButton;
    
    private WindowManager _windowManager = new();

    private string _currentDetailsMinisterId = string.Empty;
    private bool _forceFullscreen = true;

    public override void _Ready()
    {
        ForceExclusiveFullscreen();
        EnsureOpaqueSceneBackground();

        // 1. 初始化 C# 面向对象后端游戏实例
        _gameState = new GameState();
        
        var scheduler = new MockScheduler();
        var oracle = new MockOracle();
        var ministerAgent = new MockMinisterAgent();
        var narrator = new MockNarrator();

        _gameEngine = new GameEngine(_gameState, scheduler, oracle, ministerAgent, narrator);

        // 2. 注册窗口管理器节点到场景中
        AddChild(_windowManager);

        _panelAffairs = GetNodeOrNull<Control>("UI_Layer/PanelAffairs");
        _transitionMask = GetNodeOrNull<ColorRect>("UI_Layer/TransitionMask");
        if (_transitionMask != null)
        {
            _ritualTextLabel = _transitionMask.GetNodeOrNull<RichTextLabel>("RitualTextLabel");
            ConfigureFullScreenBlocker(_transitionMask, zIndex: 3_000);
            _transitionMask.Hide();
        }

        // 3. 获取 UI 节点引用
        _reignLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ReignLabel");
        _imperialPowerLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ImperialPowerLabel");
        _treasuryLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/TreasuryLabel");
        _privateTreasuryLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/PrivateTreasuryLabel");
        _healthLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/HealthLabel");
        _chronicleLog = GetNodeOrNull<RichTextLabel>("LeftPanel/VBoxContainer/ChronicleLog");
        
        _storyOutput = GetNodeOrNull<RichTextLabel>("CenterPanel/StoryOutput");
        _playerInputEdit = GetNodeOrNull<LineEdit>("BottomPanel/PlayerInputEdit");
        _submitButton = GetNodeOrNull<Button>("BottomPanel/SubmitButton");
        // P1-C2：主界面"快进 N 旬"按钮
        _fastForwardButton = GetNodeOrNull<Button>("BottomPanel/FastForwardButton");

        // 右侧大臣及场景节点
        _sceneTitleLabel = GetNodeOrNull<Label>("RightPanel/Ministers/SceneTitle");
        _interactiveLabel = GetNodeOrNull<Label>("RightPanel/Ministers/InteractiveLabel");
        _heJinButton = GetNodeOrNull<Button>("RightPanel/Ministers/HeJinButton");
        _zhangRangButton = GetNodeOrNull<Button>("RightPanel/Ministers/ZhangRangButton");
        _caoCaoButton = GetNodeOrNull<Button>("RightPanel/Ministers/CaoCaoButton");
        _jianShuoButton = GetNodeOrNull<Button>("RightPanel/Ministers/JianShuoButton");

        // 专属场景动作
        _actionLabel = GetNodeOrNull<Label>("RightPanel/Ministers/ActionLabel");
        _sellOfficeButton = GetNodeOrNull<Button>("RightPanel/Ministers/SellOfficeButton");
        _drillArmyButton = GetNodeOrNull<Button>("RightPanel/Ministers/DrillArmyButton");
        _recruitArmyButton = GetNodeOrNull<Button>("RightPanel/Ministers/RecruitArmyButton");
        _haremActionLabel = GetNodeOrNull<Label>("RightPanel/Ministers/HaremActionLabel");
        _haremRestButton = GetNodeOrNull<Button>("RightPanel/Ministers/HaremRestButton");

        // 起驾入口；大臣详情弹窗已改为动态创建，不再绑定旧 MinisterOverlayPanel。
        _travelButton = GetNodeOrNull<Button>("RightPanel/Ministers/TravelButton");
        ApplySceneActionButtonStyles();

        InitializeEmperorsDesk();

        // 4. 绑定交互事件
        if (_submitButton != null) _submitButton.Pressed += OnSubmitButtonPressed;
        if (_playerInputEdit != null) _playerInputEdit.TextSubmitted += OnPlayerInputSubmitted;
        // P1-C2：绑定"快进 N 旬"按钮 + F 键
        if (_fastForwardButton != null) _fastForwardButton.Pressed += ShowFastForwardDialog;

        // 大臣头像详情点击
        if (_heJinButton != null) _heJinButton.Pressed += () => ShowMinisterDetails("he_jin");
        if (_zhangRangButton != null) _zhangRangButton.Pressed += () => ShowMinisterDetails("zhang_rang");
        if (_caoCaoButton != null) _caoCaoButton.Pressed += () => ShowMinisterDetails("cao_cao");
        if (_jianShuoButton != null) _jianShuoButton.Pressed += () => ShowMinisterDetails("jian_shuo");

        // 旧 MinisterOverlayPanel 不再作为主流程；大臣详情、籍没确认均由动态 PopupSkin 弹窗接管。
        // 场景切换与行动弹窗均由代码动态创建；旧 tscn 弹窗节点已移除。
        if (_travelButton != null) _travelButton.Pressed += ShowTravelOverlay;
        if (_drillArmyButton != null) _drillArmyButton.Pressed += ShowArmyDrillDialog;
        if (_recruitArmyButton != null) _recruitArmyButton.Pressed += ShowRecruitArmyDialog;

        var disasterBtn = GetNodeOrNull<Button>("RightPanel/Ministers/DisasterReliefButton");
        if (disasterBtn != null) disasterBtn.Pressed += ShowDisasterReliefDialog;

        // 场景专属快捷指令事件绑定
        if (_sellOfficeButton != null) _sellOfficeButton.Pressed += () => DoQuickAction("sell_office");
        if (_haremRestButton != null) _haremRestButton.Pressed += () => DoQuickAction("harem_rest");

        InitializeAffairsPanel();
        InitializeIntelPanel();
        InitializeCourtPanel();
        InitializeWestGardenPanel();
        ApplyOpaquePanelTheme(this);

        // 渲染初始界面状态
        UpdateUI();
        SetAnnualMajorEventBanner();
        // P1-C1 开局新手引导（仅首次启动；DONGHAN_SKIP_TUTORIAL=1 可跳过）
        if (OS.GetEnvironment("DONGHAN_SKIP_TUTORIAL") != "1" && !_openingTutorialShown)
        {
            ShowOpeningTutorial();
        }
        if (OS.GetEnvironment("DONGHAN_SKIP_OPENING") != "1")
        {
            ShowOpeningOverlay();
        }
    }

    public override void _Process(double delta)
    {
        // A1：结局判定 — 引擎 UpdateOutcome() 在 NextXunAsync 末尾设 Outcome，这里每帧检查
        // _outcomeHandled 防止 _Process 重弹；解锁唯一方式是重开（用户可点弹出里的退出）
        CheckAndShowOutcomeIfGameOver();

        if (!_forceFullscreen) return;

        var mode = DisplayServer.WindowGetMode();
        if (mode != DisplayServer.WindowMode.ExclusiveFullscreen)
        {
            ForceExclusiveFullscreen();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!_isUnskippableTransitionActive) return;

        if (@event is InputEventKey or InputEventMouseButton or InputEventScreenTouch or InputEventJoypadButton)
        {
            GetViewport().SetInputAsHandled();
        }
    }

    // P1-C2：F 键全局触发"快进 N 旬"弹窗（避免与底部 LineEdit 文字输入冲突，仅在无文本输入框聚焦时生效）
    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is InputEventKey k && k.Pressed && !k.Echo && k.Keycode == Key.F)
        {
            // 跳过当玩家正在 LineEdit 中输入（避免和"福"等字符冲突）
            var focus = GetViewport().GuiGetFocusOwner();
            if (focus is LineEdit) return;
            if (_gameState == null || _gameEngine == null) return;
            if (_gameState.Outcome != GameOutcome.Playing) return; // 已结局不能再快进
            ShowFastForwardDialog();
            GetViewport().SetInputAsHandled();
        }
    }

    // === P1-C1 Godot 开局新手引导 ===
    // 全屏半透明遮罩 + 中央面板，列出玩法要点。仅首次启动显示。
    // 按 Enter / Space / 鼠标点击 均可关闭。再次启动不会重弹。
    private void ShowOpeningTutorial()
    {
        if (_openingTutorialShown) return;

        var overlay = new ColorRect
        {
            Name = "OpeningTutorialOverlay",
            Color = new Color(0, 0, 0, 0.88f),
            MouseFilter = Control.MouseFilterEnum.Stop,
            ZIndex = 2_900
        };
        SetFullRect(overlay);
        AddChild(overlay);

        // 中央内容面板
        var panel = new Panel { ZIndex = 2_901 };
        panel.SetSize(new Vector2(820, 620));
        panel.Position = new Vector2((Size.X - 820) / 2, (Size.Y - 620) / 2);
        panel.MouseFilter = Control.MouseFilterEnum.Stop;
        ApplyOpaquePanelTheme(panel);
        overlay.AddChild(panel);

        var margin = new MarginContainer();
        margin.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        margin.AddThemeConstantOverride("margin_left", 36);
        margin.AddThemeConstantOverride("margin_right", 36);
        margin.AddThemeConstantOverride("margin_top", 28);
        margin.AddThemeConstantOverride("margin_bottom", 28);
        panel.AddChild(margin);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 14);
        margin.AddChild(vbox);

        // 标题
        var title = new Label
        {
            Text = "新 手 指 引 · 灵 帝 江 山",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.AddThemeColorOverride("font_color", new Color(0.94f, 0.78f, 0.36f, 1.0f));
        title.AddThemeFontSizeOverride("font_size", 32);
        vbox.AddChild(title);

        // 内容（RichTextLabel 支持 BBCode 加粗/换行）
        var body = new RichTextLabel
        {
            BbcodeEnabled = true,
            FitContent = true,
            ScrollActive = false,
            CustomMinimumSize = new Vector2(740, 460)
        };
        body.AddThemeColorOverride("default_color", new Color(0.91f, 0.85f, 0.66f, 1.0f));
        body.AddThemeFontSizeOverride("normal_font_size", 18);
        vbox.AddChild(body);

        body.Text = "[b][color=#f0c85a]▍ 你是谁[/color][/b]\n" +
            "汉灵帝刘宏。光和七年（中平元年）四月，太平道张角聚众谋反，" +
            "外戚何进与十常侍张让明争暗斗，天下摇摇欲坠。你的目标：活过 189 年。\n\n" +

            "[b][color=#f0c85a]▍ 四张御案卡[/color][/b]\n" +
            "[color=#cfa860]· 大朝会[/color] — 召集群臣决议（赈灾/讨伐/招安/卖官）\n" +
            "[color=#cfa860]· 黄门密札[/color] — 情报：六郡局势 + 群臣动向 + 待办决策\n" +
            "[color=#cfa860]· 西园别苑[/color] — 私库、新军、犒赏、卖官\n" +
            "[color=#cfa860]· 起驾巡幸[/color] — 切场景（宣政殿/西园/后宫）\n\n" +

            "[b][color=#f0c85a]▍ 时间[/color][/b]\n" +
            "三旬为月，十二月为年。点 N 或 Enter 推进一旬 — 期间会触发叛乱检测、奏折过期、历史事件。\n\n" +

            "[b][color=#f0c85a]▍ 张让忠告[/color][/b]\n" +
            "1) [color=#ff7a5a]别先抄家！[/color] 10 月之前攒皇权到 50。\n" +
            "2) [color=#ff7a5a]184/4/2 黄巾必爆[/color]，提前给冀州派桥玄。\n" +
            "3) [color=#ff7a5a]189/9[/color] 何进伏诛、董卓入京。\n" +
            "4) 实时状态看 S 国势总览，9 看密札情报。";

        // 关闭提示
        var hint = new Label
        {
            Text = "— 按 Enter / Space / 点击任意位置 关闭 —",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        hint.AddThemeColorOverride("font_color", new Color(0.70f, 0.62f, 0.40f, 1.0f));
        hint.AddThemeFontSizeOverride("font_size", 16);
        vbox.AddChild(hint);

        // 关闭逻辑：键盘/鼠标任一事件触发即关闭
        void Dismiss()
        {
            if (_openingTutorialShown) return;
            _openingTutorialShown = true;
            if (IsInstanceValid(overlay))
            {
                overlay.QueueFree();
            }
            _openingTutorialOverlay = null;
        }

        // 监听键盘
        overlay.GuiInput += (InputEvent ev) =>
        {
            if (ev is InputEventKey k && (k.Pressed) &&
                (k.Keycode == Key.Enter || k.Keycode == Key.KpEnter || k.Keycode == Key.Space))
            {
                Dismiss();
                GetViewport().SetInputAsHandled();
            }
        };
        // 监听鼠标点击
        overlay.GuiInput += (InputEvent ev) =>
        {
            if (ev is InputEventMouseButton mb && mb.Pressed)
            {
                Dismiss();
                GetViewport().SetInputAsHandled();
            }
        };
        // Esc 也可关闭
        overlay.GuiInput += (InputEvent ev) =>
        {
            if (ev is InputEventKey k2 && k2.Pressed && k2.Keycode == Key.Escape)
            {
                Dismiss();
                GetViewport().SetInputAsHandled();
            }
        };

        _openingTutorialOverlay = overlay;
    }
}
