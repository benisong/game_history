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

    public override void _Ready()
    {
        // 1. 初始化 C# 面向对象后端游戏实例
        _gameState = new GameState();
        
        var scheduler = new MockScheduler();
        var oracle = new MockOracle();
        var ministerAgent = new MockMinisterAgent();
        var narrator = new MockNarrator();

        _gameEngine = new GameEngine(_gameState, scheduler, oracle, ministerAgent, narrator);

        // 2. 注册窗口管理器节点到场景中
        AddChild(_windowManager);

        // 获取并绑定四个物理器物按钮（兼容旧版测试节点）
        _btnAffairsBox = GetNodeOrNull<Button>("UI_Layer/Desk/BtnAffairsBox") ?? new Button();
        _btnIntelToken = GetNodeOrNull<Button>("UI_Layer/Desk/BtnIntelToken") ?? new Button();
        _btnCourtSeal = GetNodeOrNull<Button>("UI_Layer/Desk/BtnCourtSeal") ?? new Button();
        _btnPleasureCenser = GetNodeOrNull<Button>("UI_Layer/Desk/BtnPleasureCenser") ?? new Button();

        _btnAffairsBox.Pressed += OnAffairsBoxPressed;
        _btnIntelToken.Pressed += OnIntelTokenPressed;
        _btnCourtSeal.Pressed += OnCourtSealPressed;
        _btnPleasureCenser.Pressed += OnPleasureCenserPressed;

        _panelAffairs = GetNodeOrNull<Control>("UI_Layer/PanelAffairs");
        _transitionMask = GetNodeOrNull<ColorRect>("UI_Layer/TransitionMask");
        if (_transitionMask != null)
        {
            _ritualTextLabel = _transitionMask.GetNodeOrNull<RichTextLabel>("RitualTextLabel");
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

        // 渲染初始界面状态
        UpdateUI();
        if (_storyOutput != null)
        {
            _storyOutput.Text = "汉灵帝光和七年。外戚专权，宦官秉政，百姓疾苦。大汉江山风雨飘摇，陛下当如何执掌朝政？\n请输入朱批下达圣旨...";
        }
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
        if (supportLabel != null) supportLabel.Text = $"天下民心: {_gameState.PopularSupport} / 100";

        if (_healthLabel != null) _healthLabel.Text = $"皇帝健康: {_gameState.Health} / 100";

        // 更新左侧西园军势
        var armySizeLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmySizeLabel");
        var armyMoraleLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmyMoraleLabel");
        var armyLoyaltyLabel = GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmyLoyaltyLabel");
        
        if (armySizeLabel != null) armySizeLabel.Text = $"建制人数: {_gameState.WestGardenArmy.Size}";
        if (armyMoraleLabel != null) armyMoraleLabel.Text = $"军心士气: {_gameState.WestGardenArmy.Morale} / 100";
        if (armyLoyaltyLabel != null) armyLoyaltyLabel.Text = $"天子忠诚: {_gameState.WestGardenArmy.Loyalty} / 100";

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

    private void OnAffairsBoxPressed()
    {
        GD.Print("【互动】打开雕龙漆木匣，翻开奏折...");
        if (_panelAffairs != null) _panelAffairs.Show();
    }

    private void OnIntelTokenPressed()
    {
        GD.Print("【互动】拿起漆木密札，黄门暗探呈上竹简...");
    }

    private void OnCourtSealPressed()
    {
        GD.Print("【互动】拿起天子玉玺...");
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
}
