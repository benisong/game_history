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
    private Button? _btnAffairsBox;      // 雕龙漆木匣 (政务)
    private Button? _btnIntelToken;      // 漆木密札 (情报)
    private Button? _btnCourtSeal;       // 天子玉玺 (朝会)
    private Button? _btnPleasureCenser;  // 铜制博山炉 (娱乐)

    // 辅助面板层
    private Control? _panelAffairs;

    // 大朝仪转场遮罩
    private ColorRect? _transitionMask;
    private RichTextLabel? _ritualTextLabel;

    // 场景专属 Action 节点引用
    private Label? _actionLabel;
    private Button? _sellOfficeButton;
    private Button? _drillArmyButton;
    private Button? _recruitArmyButton;
    private Label? _haremActionLabel;
    private Button? _haremRestButton;

    // 弹窗与管理器
    private Button? _travelButton;
    private Panel? _travelOverlayPanel;
    private Panel? _ministerPanel;
    private Label? _ministerTitleLabel;
    private Label? _ministerFavorabilityLabel;
    private Label? _ministerPowerLabel;
    
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
            ConfigureFullScreenBlocker(_transitionMask, zIndex: 20_000);
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

        // 起驾与弹窗
        _travelButton = GetNodeOrNull<Button>("RightPanel/Ministers/TravelButton");
        _travelOverlayPanel = GetNodeOrNull<Panel>("TravelOverlayPanel");

        _ministerPanel = GetNodeOrNull<Panel>("MinisterOverlayPanel");
        _ministerTitleLabel = GetNodeOrNull<Label>("MinisterOverlayPanel/VBox/MinisterTitle");
        _ministerFavorabilityLabel = GetNodeOrNull<Label>("MinisterOverlayPanel/VBox/MinisterFavor");
        _ministerPowerLabel = GetNodeOrNull<Label>("MinisterOverlayPanel/VBox/MinisterPower");
        ConfigureMinisterPanelLayout();

        InitializeEmperorsDesk();

        // 4. 绑定交互事件
        if (_submitButton != null) _submitButton.Pressed += OnSubmitButtonPressed;
        if (_playerInputEdit != null) _playerInputEdit.TextSubmitted += OnPlayerInputSubmitted;

        // 大臣头像详情点击
        if (_heJinButton != null) _heJinButton.Pressed += () => ShowMinisterDetails("he_jin");
        if (_zhangRangButton != null) _zhangRangButton.Pressed += () => ShowMinisterDetails("zhang_rang");
        if (_caoCaoButton != null) _caoCaoButton.Pressed += () => ShowMinisterDetails("cao_cao");
        if (_jianShuoButton != null) _jianShuoButton.Pressed += () => ShowMinisterDetails("jian_shuo");

        // 弹窗关闭
        var closeOverlayBtn = GetNodeOrNull<Button>("MinisterOverlayPanel/VBox/CloseButton");
        if (closeOverlayBtn != null) closeOverlayBtn.Pressed += _windowManager.PopWindow;

        // 绑定抄家动作
        var btnConfTreasury = GetNodeOrNull<Button>("MinisterOverlayPanel/VBox/HBox/ConfiscateTreasuryBtn");
        if (btnConfTreasury != null) btnConfTreasury.Pressed += () => DoConfiscateAction("国库");

        var btnConfPrivate = GetNodeOrNull<Button>("MinisterOverlayPanel/VBox/HBox/ConfiscatePrivateBtn");
        if (btnConfPrivate != null) btnConfPrivate.Pressed += () => DoConfiscateAction("私库");

        // 场景切换按钮绑定
        if (_travelButton != null && _travelOverlayPanel != null)
        {
            _travelButton.Pressed += () => _windowManager.PushWindow(_travelOverlayPanel);
        }

        var btnGoCourt = GetNodeOrNull<Button>("TravelOverlayPanel/VBox/GoCourtButton");
        if (btnGoCourt != null) btnGoCourt.Pressed += () => DoTravel("宣政殿");

        var btnGoHarem = GetNodeOrNull<Button>("TravelOverlayPanel/VBox/GoHaremButton");
        if (btnGoHarem != null) btnGoHarem.Pressed += () => DoTravel("后宫");

        var btnGoGarden = GetNodeOrNull<Button>("TravelOverlayPanel/VBox/GoGardenButton");
        if (btnGoGarden != null) btnGoGarden.Pressed += () => DoTravel("西园");

        var btnCancelTravel = GetNodeOrNull<Button>("TravelOverlayPanel/VBox/CancelTravelButton");
        if (btnCancelTravel != null) btnCancelTravel.Pressed += _windowManager.PopWindow;

        // 阅兵弹窗 UI 及事件绑定
        var armyDrillPopup = GetNodeOrNull<Panel>("ArmyDrillPopupPanel");
        var btnConfirmPay = GetNodeOrNull<Button>("ArmyDrillPopupPanel/VBox/ConfirmPayButton");
        var btnCancelPay = GetNodeOrNull<Button>("ArmyDrillPopupPanel/VBox/CancelPayButton");
        var payInput = GetNodeOrNull<LineEdit>("ArmyDrillPopupPanel/VBox/PayInput");

        if (_drillArmyButton != null && armyDrillPopup != null)
        {
            _drillArmyButton.Pressed += () =>
            {
                if (payInput != null) payInput.Text = "";
                _windowManager.PushWindow(armyDrillPopup);
            };
        }

        if (btnCancelPay != null)
        {
            btnCancelPay.Pressed += _windowManager.PopWindow;
        }

        if (btnConfirmPay != null && payInput != null)
        {
            btnConfirmPay.Pressed += () =>
            {
                if (int.TryParse(payInput.Text, out int amount))
                {
                    _windowManager.PopWindow();
                    DoArmyDrillAction(amount);
                }
                else
                {
                    payInput.Text = "请输入有效数字！";
                }
            };
        }

        if (_recruitArmyButton != null)
        {
            _recruitArmyButton.Pressed += ShowRecruitArmyDialog;
        }

        // 宣政殿：开仓赈灾弹窗 UI 绑定
        var disasterBtn = GetNodeOrNull<Button>("RightPanel/Ministers/DisasterReliefButton");
        var reliefPopup = GetNodeOrNull<Panel>("DisasterReliefPopupPanel");
        var reliefInput = GetNodeOrNull<LineEdit>("DisasterReliefPopupPanel/VBox/ReliefAmountInput");
        
        var btnCaoRelief = GetNodeOrNull<Button>("DisasterReliefPopupPanel/VBox/HBox/ReliefCaoButton");
        var btnHeRelief = GetNodeOrNull<Button>("DisasterReliefPopupPanel/VBox/HBox/ReliefHeButton");
        var btnZhangRelief = GetNodeOrNull<Button>("DisasterReliefPopupPanel/VBox/HBox/ReliefZhangButton");
        var btnCancelRelief = GetNodeOrNull<Button>("DisasterReliefPopupPanel/VBox/CancelReliefButton");

        if (disasterBtn != null && reliefPopup != null)
        {
            disasterBtn.Pressed += () =>
            {
                if (reliefInput != null) reliefInput.Text = "";
                _windowManager.PushWindow(reliefPopup);
            };
        }

        if (btnCancelRelief != null) btnCancelRelief.Pressed += _windowManager.PopWindow;

        // 绑定指派各经办官员出发
        if (btnCaoRelief != null && reliefInput != null)
        {
            btnCaoRelief.Pressed += () =>
            {
                if (int.TryParse(reliefInput.Text, out int amt))
                {
                    _windowManager.PopWindow();
                    DoDisasterReliefAction(amt, "cao_cao");
                }
                else
                {
                    reliefInput.Text = "输入数字不合法！";
                }
            };
        }

        if (btnHeRelief != null && reliefInput != null)
        {
            btnHeRelief.Pressed += () =>
            {
                if (int.TryParse(reliefInput.Text, out int amt))
                {
                    _windowManager.PopWindow();
                    DoDisasterReliefAction(amt, "he_jin");
                }
                else
                {
                    reliefInput.Text = "输入数字不合法！";
                }
            };
        }

        if (btnZhangRelief != null && reliefInput != null)
        {
            btnZhangRelief.Pressed += () =>
            {
                if (int.TryParse(reliefInput.Text, out int amt))
                {
                    _windowManager.PopWindow();
                    DoDisasterReliefAction(amt, "zhang_rang");
                }
                else
                {
                    reliefInput.Text = "输入数字不合法！";
                }
            };
        }

        // 场景专属快捷指令事件绑定
        if (_sellOfficeButton != null) _sellOfficeButton.Pressed += () => DoQuickAction("sell_office");
        if (_haremRestButton != null) _haremRestButton.Pressed += () => DoQuickAction("harem_rest");

        InitializeDynamicNpcList();
        InitializeAffairsPanel();
        InitializeIntelPanel();
        InitializeCourtPanel();
        ApplyOpaquePanelTheme(this);

        // 渲染初始界面状态
        UpdateUI();
        if (_storyOutput != null)
        {
            _storyOutput.Text = "陛下已经驾临宣政殿，请在上方抚摩御案物理器物，或在下方朱批下旨，乾纲独断...";
        }
        ShowOpeningOverlay();
    }

    public override void _Process(double delta)
    {
        if (!_forceFullscreen) return;

        var mode = DisplayServer.WindowGetMode();
        if (mode != DisplayServer.WindowMode.ExclusiveFullscreen)
        {
            ForceExclusiveFullscreen();
        }
    }
}
