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

    // 辅助面板层
    private Control? _panelAffairs;

    // 大朝仪转场遮罩
    private ColorRect? _transitionMask;
    private RichTextLabel? _ritualTextLabel;
    private Control? _courtRitualOverlay;
    private bool _isUnskippableTransitionActive;

    // A1 结局面板：游戏结局弹出后只显示一次，避免 _Process 每帧重弹
    private bool _outcomeHandled;
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

        InitializeDynamicNpcList();
        InitializeAffairsPanel();
        InitializeIntelPanel();
        InitializeCourtPanel();
        InitializeWestGardenPanel();
        ApplyOpaquePanelTheme(this);

        // 渲染初始界面状态
        UpdateUI();
        SetAnnualMajorEventBanner();
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
}
