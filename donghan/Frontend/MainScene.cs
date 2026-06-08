using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DonghanEngine.Core;

namespace DonghanFrontend;

// Mock 调试组件以便在编译期直接提供依赖
public class MockScheduler : IAIScheduler
{
    public INpcLifecycleManager NpcManager { get; } = null!;

    public Task<AIOrchestrationResult> OrchestrateGrandCourtAsync(string playerInput, string activeOfficerId, GameState state)
    {
        var result = new AIOrchestrationResult();
        if (playerInput.Contains("赏赐何进") || playerInput.Contains("重赏何进"))
        {
            result.PrimaryIntent = "REWARD";
            result.Speeches.Add(new CourtSpeech
            {
                MinisterId = "he_jin",
                MinisterName = "何进",
                SpeechText = "臣谢陛下隆恩！臣定当整军备战，保大汉无虞！",
                ExpectedFavorabilityChange = 15,
                ExpectedPowerChange = 5
            });
        }
        else if (playerInput.Contains("冷落张让") || playerInput.Contains("训诫张让"))
        {
            result.PrimaryIntent = "COLD";
            result.Speeches.Add(new CourtSpeech
            {
                MinisterId = "zhang_rang",
                MinisterName = "张让",
                SpeechText = "陛下如今薄情如此，奴才只盼着陛下龙体安康呐...",
                ExpectedFavorabilityChange = -15,
                ExpectedPowerChange = -3
            });
        }
        return Task.FromResult(result);
    }

    public Task OrchestrateXunUpdateAsync(GameState state)
    {
        return Task.CompletedTask;
    }
}

public class MockOracle : IEventOracle
{
    public Task<OracleEvent?> CheckRandomEventAsync(GameState state)
    {
        OracleEvent? evt = null;
        if (state.Chronicle.Count > 0 && state.Chronicle[state.Chronicle.Count - 1].Contains("天灾"))
        {
            evt = new OracleEvent
            {
                EventName = "地动山摇",
                Description = "洛阳突发地震，百姓流离失所，朝廷需开仓赈灾。",
                ImperialPowerChange = -5,
                TreasuryChange = -150,
                HealthChange = 0
            };
        }
        return Task.FromResult(evt);
    }
}

public class MockMinisterAgent : IMinisterAgent
{
    public Task<List<MinisterDialogue>> TalkToMinistersAsync(List<string> activeMinisters, string playerInput, GameState state)
    {
        var list = new List<MinisterDialogue>();
        foreach (var mId in activeMinisters)
        {
            if (mId == "he_jin")
            {
                list.Add(new MinisterDialogue
                {
                    MinisterId = "he_jin",
                    MinisterName = "何进",
                    DialogueText = "臣谢陛下隆恩！臣定当整军备战，保大汉无虞！",
                    FavorabilityChange = 15,
                    PowerChange = 5
                });
            }
            else if (mId == "zhang_rang")
            {
                list.Add(new MinisterDialogue
                {
                    MinisterId = "zhang_rang",
                    MinisterName = "张让",
                    DialogueText = "陛下如今薄情如此，奴才只盼着陛下龙体安康呐...",
                    FavorabilityChange = -15,
                    PowerChange = -3
                });
            }
        }
        return Task.FromResult(list);
    }
}

public class MockNarrator : INarrator
{
    public Task<string> RenderStoryAsync(string playerInput, OracleEvent? triggeredEvent, List<MinisterDialogue> ministerDialogues, GameState state)
    {
        string story = $"【圣旨朱批】：“[color=yellow]{playerInput}[/color]”\n\n";
        if (triggeredEvent != null)
        {
            story += $"[color=red]● 天降警示：{triggeredEvent.EventName}[/color]\n{triggeredEvent.Description}\n\n";
        }
        foreach (var dial in ministerDialogues)
        {
            story += $"[b]{dial.MinisterName}[/b]在殿前叩首，进言道: \"[i]{dial.DialogueText}[/i]\"\n\n";
        }
        story += "皇帝缓缓靠在龙椅上。朝堂波诡云谲，陛下今日的朱批将悄然重构这危如累卵的天下...";
        return Task.FromResult(story);
    }
}

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

    private static void ForceExclusiveFullscreen()
    {
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
    }

    private static void ConfigureFullScreenBlocker(ColorRect blocker, int zIndex)
    {
        blocker.Color = new Color(0.04f, 0.035f, 0.03f, 1.0f);
        blocker.MouseFilter = Control.MouseFilterEnum.Stop;
        blocker.ZIndex = zIndex;
        SetFullRect(blocker);
    }

    private void EnsureOpaqueSceneBackground()
    {
        if (GetNodeOrNull<ColorRect>("OpaqueSceneBackground") != null) return;

        var background = new ColorRect();
        background.Name = "OpaqueSceneBackground";
        background.Color = new Color(0.055f, 0.045f, 0.04f, 1.0f);
        background.MouseFilter = Control.MouseFilterEnum.Ignore;
        background.ZIndex = -100;
        SetFullRect(background);

        AddChild(background);
        MoveChild(background, 0);
    }

    private static void ApplyOpaquePanelTheme(Node root)
    {
        if (root is Panel panel)
        {
            panel.AddThemeStyleboxOverride("panel", CreateOpaquePanelStyle(panel.Name.ToString()));
            panel.MouseFilter = Control.MouseFilterEnum.Stop;
        }

        if (root is ColorRect colorRect && root.Name.ToString().Contains("TransitionMask"))
        {
            ConfigureFullScreenBlocker(colorRect, zIndex: 20_000);
        }

        foreach (var child in root.GetChildren())
        {
            ApplyOpaquePanelTheme(child);
        }
    }

    private static StyleBoxFlat CreateOpaquePanelStyle(string panelName)
    {
        bool isPopup = panelName.Contains("Popup") || panelName.Contains("Overlay");
        var style = new StyleBoxFlat();
        style.BgColor = isPopup
            ? new Color(0.10f, 0.095f, 0.085f, 1.0f)
            : new Color(0.075f, 0.068f, 0.06f, 1.0f);
        style.SetBorderWidthAll(isPopup ? 2 : 1);
        style.BorderColor = isPopup
            ? new Color(0.84f, 0.67f, 0.12f, 1.0f)
            : new Color(0.40f, 0.32f, 0.10f, 1.0f);
        style.ContentMarginLeft = 8;
        style.ContentMarginRight = 8;
        style.ContentMarginTop = 8;
        style.ContentMarginBottom = 8;
        return style;
    }

    private static void SetFullRect(Control control)
    {
        control.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        control.OffsetLeft = 0;
        control.OffsetTop = 0;
        control.OffsetRight = 0;
        control.OffsetBottom = 0;
    }

    private void ConfigureMinisterPanelLayout()
    {
        if (_ministerPanel == null) return;

        _ministerPanel.CustomMinimumSize = new Vector2(520, 360);
        _ministerPanel.AnchorLeft = 0.5f;
        _ministerPanel.AnchorTop = 0.5f;
        _ministerPanel.AnchorRight = 0.5f;
        _ministerPanel.AnchorBottom = 0.5f;
        _ministerPanel.OffsetLeft = -260;
        _ministerPanel.OffsetTop = -180;
        _ministerPanel.OffsetRight = 260;
        _ministerPanel.OffsetBottom = 180;

        var vBox = _ministerPanel.GetNodeOrNull<VBoxContainer>("VBox");
        if (vBox != null)
        {
            vBox.AddThemeConstantOverride("separation", 8);
        }

        ConfigureWrappingLabel(_ministerTitleLabel, HorizontalAlignment.Center);
        ConfigureWrappingLabel(_ministerFavorabilityLabel);
        ConfigureWrappingLabel(_ministerPowerLabel);
        ConfigureWrappingLabel(GetNodeOrNull<Label>("MinisterOverlayPanel/VBox/MinisterCorruption"));
        ConfigureWrappingLabel(GetNodeOrNull<Label>("MinisterOverlayPanel/VBox/MinisterWealth"));

        var actionRow = GetNodeOrNull<HBoxContainer>("MinisterOverlayPanel/VBox/HBox");
        if (actionRow != null)
        {
            actionRow.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            actionRow.Alignment = BoxContainer.AlignmentMode.Center;
            actionRow.AddThemeConstantOverride("separation", 12);
            foreach (var child in actionRow.GetChildren())
            {
                if (child is Button button)
                {
                    button.CustomMinimumSize = new Vector2(0, 42);
                    button.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                }
            }
        }
    }

    private static void ConfigureWrappingLabel(Label? label, HorizontalAlignment alignment = HorizontalAlignment.Left)
    {
        if (label == null) return;

        label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        label.HorizontalAlignment = alignment;
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        label.ClipText = false;
    }

    private void UpdateUI()
    {
        if (_gameState == null) return;

        // 更新左侧数值
        if (_reignLabel != null) _reignLabel.Text = $"年号: {_gameState.ReignTitle} {_gameState.ReignYear} 年";
        if (_imperialPowerLabel != null) _imperialPowerLabel.Text = $"皇权值: {_gameState.ImperialPower} / 100";
        if (_treasuryLabel != null) _treasuryLabel.Text = $"国库资金: {_gameState.Treasury} 万钱";
        if (_privateTreasuryLabel != null) _privateTreasuryLabel.Text = $"西园私库: {_gameState.PrivateTreasury} 万钱";
        
        // 更新天下民心
        var supportLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/PopularSupportLabel");
        if (supportLabel != null) supportLabel.Hide();

        if (_healthLabel != null) _healthLabel.Text = $"皇帝健康: {_gameState.Health} / 100";

        // 更新左侧西园军势
        var armyTitleLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmyTitleLabel");
        if (armyTitleLabel != null) armyTitleLabel.Hide();

        var armySizeLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmySizeLabel");
        if (armySizeLabel != null) armySizeLabel.Hide();

        var armyMoraleLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmyMoraleLabel");
        if (armyMoraleLabel != null) armyMoraleLabel.Hide();

        var armyLoyaltyLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmyLoyaltyLabel");
        if (armyLoyaltyLabel != null) armyLoyaltyLabel.Hide();

        // 更新起居注/编年史
        if (_chronicleLog != null)
        {
            _chronicleLog.Clear();
            foreach (var record in _gameState.Chronicle)
            {
                _chronicleLog.AppendText(record + "\n");
            }
        }

        // 根据当前所在场景，动态控制右侧控制面板按钮的显示隐藏！
        UpdateSceneButtons();
        UpdateNpcList();
    }

    // 动态控制右侧控制面板
    private void UpdateSceneButtons()
    {
        if (_gameState == null) return;

        string loc = _gameState.CurrentLocation;

        if (_sceneTitleLabel != null) _sceneTitleLabel.Text = $"当前：{loc}";

        // 1. 宣政殿显示：何进、张让，及赈灾快捷按钮
        bool isCourt = loc == "宣政殿";
        if (_heJinButton != null) _heJinButton.Visible = isCourt;
        if (_zhangRangButton != null) _zhangRangButton.Visible = isCourt;
        
        var disasterBtn = GetNodeOrNull<Button>("RightPanel/Ministers/DisasterReliefButton");
        if (disasterBtn != null) disasterBtn.Visible = isCourt;

        // 2. 西园显示：曹操、蹇硕，以及西园专属操作按钮
        bool isGarden = loc == "西园";
        if (_caoCaoButton != null) _caoCaoButton.Visible = isGarden;
        if (_jianShuoButton != null) _jianShuoButton.Visible = isGarden;
        
        if (_actionLabel != null) _actionLabel.Visible = isGarden;
        if (_sellOfficeButton != null) _sellOfficeButton.Visible = isGarden;
        if (_drillArmyButton != null) _drillArmyButton.Visible = isGarden;

        // 3. 后宫显示：后宫专属按钮（隐藏所有大臣，后宫不准外臣涉足）
        bool isHarem = loc == "后宫";
        if (_haremActionLabel != null) _haremActionLabel.Visible = isHarem;
        if (_haremRestButton != null) _haremRestButton.Visible = isHarem;

        // 如果是后宫或西园，隐藏通用“召见群臣”文字标签
        if (_interactiveLabel != null)
        {
            _interactiveLabel.Visible = isCourt || isGarden;
            _interactiveLabel.Text = isCourt ? "【召见朝臣】" : "【召见将领】";
        }
    }

    // 处理起驾
    private void DoTravel(string location)
    {
        if (_gameEngine == null || _gameState == null) return;

        try
        {
            _gameEngine.TravelToLocation(location);
            
            // 关闭起驾弹窗
            _windowManager.PopWindow();

            // 生成转场故事文本
            if (_storyOutput != null)
            {
                if (location == "宣政殿")
                {
                    _storyOutput.Text = "【起驾 · 宣政殿】\n\n“起驾宣政殿——！”\n内监尖细的高唱声在深宫回荡。陛下登临天子龙辇，在满朝文武的拜跪高呼声中重返宝座。大汉帝国的齿轮，将再次随着陛下的御笔而转动。";
                }
                else if (location == "后宫")
                {
                    _storyOutput.Text = "【巡幸 · 温德殿】\n\n“天子起驾温德殿，闲人退避——！”\n车舆缓缓停在红墙绿瓦、花香袅袅的后宫。红幔轻摇，莺声燕语。陛下卸下了金銮殿上的重负，来到了属于帝王的绝对私密之所。";
                }
                else if (location == "西园")
                {
                    _storyOutput.Text = "【起驾 · 西园精舍】\n\n“起驾西园——！”\n陛下避开了何进等人的耳目，轻车简从，来到了陛下亲自督造的西园。这里有堆积如山的金银私库，有新募组建的精锐新军，是陛下摆脱掣肘、暗中夺回大权的铁血基地。";
                }
            }

            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
        }
    }

    // 执行快捷场景动作
    private void DoQuickAction(string actionId)
    {
        if (_gameEngine == null) return;

        try
        {
            var result = _gameEngine.ExecuteQuickAction(actionId);
            if (_storyOutput != null)
            {
                _storyOutput.Text = result.StoryText;
            }
            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
        }
    }

    // 执行发饷动作
    private void DoArmyDrillAction(int amount)
    {
        if (_gameEngine == null || _gameState == null) return;

        try
        {
            string officerId = "jian_shuo"; // 默认蹇硕（西园上军校尉）
            
            if (_gameState.Npcs.TryGetValue("cao_cao", out var cao) && cao.Favorability > 50)
            {
                officerId = "cao_cao";
            }
            else if (_gameState.Npcs.TryGetValue("zhang_rang", out var zhang) && zhang.Power > 75)
            {
                officerId = "zhang_rang";
            }

            var result = _gameEngine.ExecuteDrillArmyActionWithOfficer(amount, officerId);
            if (_storyOutput != null)
            {
                _storyOutput.Text = result.StoryText;
            }
            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
        }
    }

    // 执行开仓赈灾动作
    private void DoDisasterReliefAction(int amount, string officerId)
    {
        if (_gameEngine == null) return;

        try
        {
            var result = _gameEngine.ExecuteDisasterReliefAction(amount, officerId);
            if (_storyOutput != null)
            {
                _storyOutput.Text = result.StoryText;
            }
            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
        }
    }

    // 处理朱批下旨
    private async void OnSubmitButtonPressed()
    {
        if (_playerInputEdit == null || _gameEngine == null) return;
        string text = _playerInputEdit.Text;
        if (string.IsNullOrWhiteSpace(text)) return;

        _playerInputEdit.Text = "";
        _playerInputEdit.Editable = false;
        if (_submitButton != null) _submitButton.Disabled = true;

        try
        {
            var result = await _gameEngine.ProcessPlayerTurnAsync(text);
            if (_storyOutput != null)
            {
                _storyOutput.Text = result.StoryText;
            }
            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Error in Turn Processing]: {ex.Message}");
            if (_storyOutput != null)
            {
                _storyOutput.Text += $"\n【内监急奏：圣旨解析失败，AI未响应或格式有误。】";
            }
        }
        finally
        {
            _playerInputEdit.Editable = true;
            if (_submitButton != null) _submitButton.Disabled = false;
            _playerInputEdit.GrabFocus();
        }
    }

    private void OnPlayerInputSubmitted(string text)
    {
        OnSubmitButtonPressed();
    }

    private void ShowMinisterDetails(string ministerId)
    {
        if (_gameState == null || _ministerPanel == null) return;

        if (_gameState.Npcs.TryGetValue(ministerId, out var minister))
        {
            _currentDetailsMinisterId = ministerId; // 记录当前正在查看的大臣ID，用于抄家指令

            if (_ministerTitleLabel != null) _ministerTitleLabel.Text = $"{minister.Name} ({minister.Title})";
            if (_ministerFavorabilityLabel != null) _ministerFavorabilityLabel.Text = $"好感度: {minister.Favorability} / 100";
            if (_ministerPowerLabel != null) _ministerPowerLabel.Text = $"朝堂权力: {minister.Power} / 100";

            // 显示贪腐度
            var corruptionLabel = GetNodeOrNull<Label>("MinisterOverlayPanel/VBox/MinisterCorruption");
            if (corruptionLabel != null)
            {
                corruptionLabel.Text = $"官员贪腐度: {minister.Corruption} / 100";
            }

            // 显示贪腐存银
            var wealthLabel = GetNodeOrNull<Label>("MinisterOverlayPanel/VBox/MinisterWealth");
            if (wealthLabel != null)
            {
                wealthLabel.Text = $"私蓄赃款: {minister.StashedWealth} 万钱";
            }

            // 新增或更新五维属性标签
            var fiveAttributesLabel = _ministerPanel.GetNodeOrNull<Label>("VBox/FiveAttributes");
            if (fiveAttributesLabel == null)
            {
                fiveAttributesLabel = new Label();
                fiveAttributesLabel.Name = "FiveAttributes";
                ConfigureWrappingLabel(fiveAttributesLabel, HorizontalAlignment.Center);
                _ministerPanel.GetNode<VBoxContainer>("VBox").AddChild(fiveAttributesLabel);
                // 移动到 CloseButton 之前
                _ministerPanel.GetNode<VBoxContainer>("VBox").MoveChild(fiveAttributesLabel, 5);
            }
            else
            {
                ConfigureWrappingLabel(fiveAttributesLabel, HorizontalAlignment.Center);
            }
            string govText = minister.GovernedProvinceId != null ? $"【外任 {_gameState.Provinces[minister.GovernedProvinceId].Name} 太守】\n" : "【在京闲置】\n";
            fiveAttributesLabel.Text = govText +
                $"武力: {minister.Martial,-3} | 统帅: {minister.Leadership,-3} | 政治: {minister.Politics,-3}\n" +
                $"魅力: {minister.Charisma,-3} | 野心: {minister.Ambition,-3}";

            _windowManager.PushWindow(_ministerPanel);
        }
    }

    // 执行抄家动作
    private void DoConfiscateAction(string destination)
    {
        if (_gameEngine == null || string.IsNullOrEmpty(_currentDetailsMinisterId)) return;

        try
        {
            // 关闭详情面板
            _windowManager.PopWindow();

            // 如果不在宣政殿，发出警告
            if (_gameState?.CurrentLocation != "宣政殿")
            {
                if (_storyOutput != null)
                {
                    _storyOutput.Text = "【御史弹劾】\n\n“陛下，抄没朝臣家产兹事体大，必须在宣政殿百官大朝会上宣旨籍没，方可调动京师御林军，否则名不正言不顺！”";
                }
                return;
            }

            var result = _gameEngine.ExecuteConfiscationAction(_currentDetailsMinisterId, destination);
            if (_storyOutput != null)
            {
                _storyOutput.Text = result.StoryText;
            }
            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
        }
    }

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
        _npcScrollContainer.CustomMinimumSize = new Vector2(0, 200);

        _npcListVBox = new VBoxContainer();
        _npcListVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _npcScrollContainer.AddChild(_npcListVBox);

        // 将动态滚动列表插入到 ministersVBox 中，位于 SceneTitle 和 InteractiveLabel 之后
        ministersVBox.AddChild(_npcScrollContainer);
    }

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

    private VBoxContainer? _deskContainer;

    private void InitializeEmperorsDesk()
    {
        var centerPanel = GetNodeOrNull<Panel>("CenterPanel");
        if (centerPanel == null) return;

        _deskContainer = new VBoxContainer();
        _deskContainer.Name = "EmperorsDesk";
        _deskContainer.AnchorLeft = 0.5f;
        _deskContainer.AnchorTop = 0.0f;
        _deskContainer.AnchorRight = 0.5f;
        _deskContainer.AnchorBottom = 0.0f;
        _deskContainer.OffsetLeft = -120;
        _deskContainer.OffsetTop = 18;
        _deskContainer.OffsetRight = 120;
        _deskContainer.OffsetBottom = 250;
        _deskContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        _deskContainer.Alignment = BoxContainer.AlignmentMode.Center;
        _deskContainer.CustomMinimumSize = new Vector2(240, 230);
        _deskContainer.AddThemeConstantOverride("separation", 10);
        
        centerPanel.AddChild(_deskContainer);
        centerPanel.MoveChild(_deskContainer, 0);

        // 调整 StoryOutput，给居中的竖向卷轴入口留出空间
        if (_storyOutput != null)
        {
            _storyOutput.OffsetTop = 280;
        }

        // 创建四大按钮
        _btnCourtSeal = CreateDeskButton("大朝会", OnCourtSealPressed);
        _btnAffairsBox = CreateDeskButton("尚书台", OnAffairsBoxPressed);
        _btnIntelToken = CreateDeskButton("黄门密札", OnIntelTokenPressed);
        _btnPleasureCenser = CreateDeskButton("起驾巡幸", OnPleasureCenserPressed);

        _deskContainer.AddChild(_btnCourtSeal);
        _deskContainer.AddChild(_btnAffairsBox);
        _deskContainer.AddChild(_btnIntelToken);
        _deskContainer.AddChild(_btnPleasureCenser);
    }

    private Button CreateDeskButton(string text, Action pressedCallback)
    {
        var btn = new Button();
        btn.Text = text;
        btn.CustomMinimumSize = new Vector2(220, 46);
        btn.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        btn.AddThemeFontSizeOverride("font_size", 17);
        btn.AddThemeStyleboxOverride("normal", CreateScrollButtonStyle(new Color(0.42f, 0.30f, 0.13f, 1.0f)));
        btn.AddThemeStyleboxOverride("hover", CreateScrollButtonStyle(new Color(0.52f, 0.38f, 0.16f, 1.0f)));
        btn.AddThemeStyleboxOverride("pressed", CreateScrollButtonStyle(new Color(0.30f, 0.20f, 0.09f, 1.0f)));
        btn.Pressed += pressedCallback;
        return btn;
    }

    private static StyleBoxFlat CreateScrollButtonStyle(Color bgColor)
    {
        var style = new StyleBoxFlat();
        style.BgColor = bgColor;
        style.BorderColor = new Color(0.92f, 0.76f, 0.33f, 1.0f);
        style.SetBorderWidthAll(2);
        style.CornerRadiusTopLeft = 4;
        style.CornerRadiusTopRight = 4;
        style.CornerRadiusBottomLeft = 4;
        style.CornerRadiusBottomRight = 4;
        style.ContentMarginLeft = 18;
        style.ContentMarginRight = 18;
        style.ContentMarginTop = 8;
        style.ContentMarginBottom = 8;
        return style;
    }

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
        hBox.AddThemeConstantOverride("separation", 15);
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
        btnClose.Text = "合上卷宗";
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
                EdictType.Remonstrance => "[劝诫]",
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
            btn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            
            int optIndex = i;
            btn.Pressed += async () =>
            {
                _windowManager.PopWindow(); // 关闭尚书台弹窗
                
                string pInput = $"批阅奏折 {edict.Title} 选项 {optIndex + 1}";
                
                if (_storyOutput != null) _storyOutput.Text = "正在起草朱批，下达圣旨...";
                var result = await _gameEngine!.ProcessPlayerTurnAsync(pInput);
                if (_storyOutput != null) _storyOutput.Text = result.StoryText;
                UpdateUI();
            };
            _edictOptionsVBox.AddChild(btn);
        }
    }

    private Panel? _intelPopup;
    private ItemList? _provinceItemList;
    private RichTextLabel? _provinceDetailsLabel;
    private VBoxContainer? _provinceActionsVBox;

    private RichTextLabel? _intelGlobalStatsLabel;

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
        hBox.AddThemeConstantOverride("separation", 15);
        _intelPopup.AddChild(hBox);

        // 左半边：大汉 6 州郡总览
        var leftVBox = new VBoxContainer();
        leftVBox.CustomMinimumSize = new Vector2(250, 0);
        hBox.AddChild(leftVBox);

        var listTitle = new Label();
        listTitle.Text = "🗺️ 大汉十三州舆图情报";
        leftVBox.AddChild(listTitle);

        // 顶端天下全局态势大收纳
        _intelGlobalStatsLabel = new RichTextLabel();
        _intelGlobalStatsLabel.CustomMinimumSize = new Vector2(0, 65);
        _intelGlobalStatsLabel.BbcodeEnabled = true;
        leftVBox.AddChild(_intelGlobalStatsLabel);

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

        // 刷新大局态势与西园精锐军势情报
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
            hBoxGov.AddThemeConstantOverride("separation", 10);
            _provinceActionsVBox.AddChild(hBoxGov);
            var lblGov = new Label(); lblGov.Text = "外派太守: ";
            hBoxGov.AddChild(lblGov);

            var availableNpcs = _gameState.Npcs.Values.Where(n => n.IsActive && n.GovernedProvinceId == null).ToList();
            if (availableNpcs.Count == 0)
            {
                var lblNone = new Label(); lblNone.Text = "（京中暂无闲置文武）";
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
            hBoxSuppress.AddThemeConstantOverride("separation", 10);
            _provinceActionsVBox.AddChild(hBoxSuppress);
            var lblSup = new Label(); lblSup.Text = "⚔️ 派兵平叛: ";
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

            // 2. 遣使招抚 (使用说服与离间叠加策略)
            var hBoxPacify = new HBoxContainer();
            hBoxPacify.AddThemeConstantOverride("separation", 10);
            _provinceActionsVBox.AddChild(hBoxPacify);
            var lblPac = new Label(); lblPac.Text = "🌸 遣使招安: ";
            hBoxPacify.AddChild(lblPac);

            foreach (var envoy in militaryNpcs.Take(2))
            {
                var btnPac = new Button();
                btnPac.Text = $"{envoy.Name} (说服+离间)";
                
                string envoyId = envoy.Id;
                btnPac.Pressed += () =>
                {
                    _windowManager.PopWindow();
                    var strategies = GameEngine.PacifyStrategy.Persuade | GameEngine.PacifyStrategy.SowDiscord;
                    var res = _gameEngine!.PacifyRebellion(p.Id, envoyId, strategies, 0);
                    if (_storyOutput != null) _storyOutput.Text = res.StoryText;
                    UpdateUI();
                };
                hBoxPacify.AddChild(btnPac);
            }
        }
    }

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
        vBox.AddThemeConstantOverride("separation", 15);
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
        hBox.AddThemeConstantOverride("separation", 20);
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

    private void OnPleasureCenserPressed()
    {
        GD.Print("【互动】点燃博山炉，紫烟升起，起驾后宫/西园...");
        if (_gameEngine != null)
        {
            _gameEngine.TravelToLocation("后宫");
            UpdateUI();
        }
    }

    private Panel? _openingOverlay;

    private void ShowOpeningOverlay()
    {
        _openingOverlay = new Panel();
        _openingOverlay.Name = "OpeningOverlay";
        _openingOverlay.MouseFilter = Control.MouseFilterEnum.Stop;
        _openingOverlay.FocusMode = Control.FocusModeEnum.All;
        _openingOverlay.ZIndex = 30_000;
        SetFullRect(_openingOverlay);

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
                    "张角妖术惑众，巨鹿黄巾并起。外戚何进拥兵坐镇，十常侍张让把持禁中。\n" +
                    "汉室倾颓，累卵之危。天下百姓，倒悬之急。\n\n" +
                    "[color=yellow]陛下，大汉的八百载基业与十三州舆图，您将如何执掌重构？[/color][/center]";
        vBox.AddChild(desc);

        var btnConfirm = new Button();
        btnConfirm.Text = "—— 临 朝 理 政 ——";
        btnConfirm.CustomMinimumSize = new Vector2(250, 45);
        btnConfirm.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        btnConfirm.Pressed += () =>
        {
            _openingOverlay.ReleaseFocus();
            _openingOverlay.QueueFree(); // 玩家确认后彻底销毁，显示主场景
        };
        vBox.AddChild(btnConfirm);

        AddChild(_openingOverlay);
        _openingOverlay.GrabFocus();
    }
}
