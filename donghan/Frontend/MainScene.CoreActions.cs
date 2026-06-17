using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DonghanEngine.Core;

namespace DonghanFrontend;

public partial class MainScene : Control
{
    private void UpdateUI()
    {
        if (_gameState == null) return;

        if (_reignLabel != null)
        {
            _reignLabel.Text = FormatTimeLabel();
        }
        if (_mainTimeLabel != null)
        {
            _mainTimeLabel.Text = FormatTimeLabel();
        }

        HideMainSceneStatusAndNpcChrome();
        SetAnnualMajorEventBanner();

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

    private string FormatTimeLabel()
    {
        if (_gameState == null) return string.Empty;

        string xunName = _gameState.Xun switch
        {
            1 => "上旬",
            2 => "中旬",
            3 => "下旬",
            _ => $"第{_gameState.Xun}旬"
        };

        return $"{_gameState.ReignTitle}{_gameState.ReignYear}年 · {_gameState.Year}年{_gameState.Month}月{xunName}";
    }

    private void HideMainSceneStatusAndNpcChrome()
    {
        GetNodeOrNull<Panel>("LeftPanel")?.Hide();
        GetNodeOrNull<Panel>("RightPanel")?.Hide();
        GetNodeOrNull<Panel>("BottomPanel")?.Hide();

        _imperialPowerLabel?.Hide();
        _treasuryLabel?.Hide();
        _privateTreasuryLabel?.Hide();
        _healthLabel?.Hide();
        _chronicleLog?.Hide();

        GetNodeOrNull<Label>("LeftPanel/VBoxContainer/TitleLabel")?.Hide();
        GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ChronicleTitle")?.Hide();
        GetNodeOrNull<Label>("LeftPanel/VBoxContainer/PopularSupportLabel")?.Hide();
        GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmyTitleLabel")?.Hide();
        GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmySizeLabel")?.Hide();
        GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmyMoraleLabel")?.Hide();
        GetNodeOrNull<Label>("LeftPanel/VBoxContainer/ArmyLoyaltyLabel")?.Hide();

        _sceneTitleLabel?.Hide();
        _interactiveLabel?.Hide();
        _heJinButton?.Hide();
        _zhangRangButton?.Hide();
        _caoCaoButton?.Hide();
        _jianShuoButton?.Hide();
        _travelButton?.Hide();
        _npcScrollContainer?.Hide();
    }

    // 动态控制右侧控制面板
    private void UpdateSceneButtons()
    {
        if (_gameState == null) return;

        _sceneTitleLabel?.Hide();
        _interactiveLabel?.Hide();
        _heJinButton?.Hide();
        _zhangRangButton?.Hide();
        _caoCaoButton?.Hide();
        _jianShuoButton?.Hide();

        var disasterBtn = GetNodeOrNull<Button>("RightPanel/Ministers/DisasterReliefButton");
        disasterBtn?.Hide();

        if (_actionLabel != null)
        {
            _actionLabel.Visible = false;
            _actionLabel.Text = "【西园军务】";
        }
        if (_sellOfficeButton != null) _sellOfficeButton.Visible = false;
        if (_drillArmyButton != null) _drillArmyButton.Visible = false;
        if (_recruitArmyButton != null) _recruitArmyButton.Visible = false;

        if (_haremActionLabel != null) _haremActionLabel.Visible = false;
        if (_haremRestButton != null) _haremRestButton.Visible = false;
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

            // 生成转场奏报，不再污染主界面年度事件横条
            string travelStory = location switch
            {
                "宣政殿" => "【起驾 · 宣政殿】\n\n“起驾宣政殿——！”\n内监尖细的高唱声在深宫回荡。陛下登临天子龙辇，在满朝文武的拜跪高呼声中重返宝座。大汉帝国的齿轮，将再次随着陛下的御笔而转动。",
                "后宫" => "【巡幸 · 温德殿】\n\n“天子起驾温德殿，闲人退避——！”\n车舆缓缓停在红墙绿瓦、花香袅袅的后宫。红幔轻摇，莺声燕语。陛下卸下了金銮殿上的重负，来到了属于帝王的绝对私密之所。",
                "西园" => "【起驾 · 西园别苑】\n\n“起驾西园——！”\n陛下避开了何进等人的耳目，轻车简从，来到了陛下亲自督造的西园。这里有堆积如山的金银私库，有新募组建的精锐新军，是陛下摆脱掣肘、暗中夺回大权的铁血基地。",
                _ => $"【起驾】\n\n陛下移驾{location}。"
            };
            ShowTravelReportPopup("起驾奏报", travelStory, location);

            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
        }
    }

    private static PopupSkin GetTravelReportSkin(string location)
    {
        return location switch
        {
            "西园" => PopupSkin.WestGarden,
            "后宫" => PopupSkin.Warning,
            _ => PopupSkin.Court
        };
    }

    // 执行快捷场景动作
    private void DoQuickAction(string actionId)
    {
        if (_gameEngine == null) return;

        try
        {
            var result = _gameEngine.ExecuteQuickAction(actionId);
            ShowStoryReportPopup(actionId == "sell_office" ? "西园鬻官回奏" : "温德殿起居回奏", result.StoryText, actionId == "sell_office" ? PopupSkin.WestGarden : PopupSkin.Warning);
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
            ShowWestGardenReportPopup("西园军报", result.StoryText);
            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
        }
    }

    private void DoRecruitArmyAction(int troops)
    {
        if (_gameEngine == null) return;

        try
        {
            var result = _gameEngine.ExecuteRaiseWestGardenTroopsAction(troops);
            ShowWestGardenReportPopup("西园军报", result.StoryText);
            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
            ShowWarningReportPopup("募兵未成", $"【募兵未成】\n\n{ex.Message}");
        }
    }

    private void ShowArmyDrillDialog()
    {
        if (_gameState == null) return;

        var panel = new Panel();
        ConfigureCenteredPopupPanel(panel, PopupSkin.WestGarden, new Vector2(540, 350));

        var vBox = CreateActionPopupRoot(panel, 22, 18);
        var title = new Label { Text = "西园军令 · 犒赏三军" };
        StylePopupTitle(title, PopupSkin.WestGarden);
        vBox.AddChild(title);

        var desc = new Label
        {
            Text = $"基础军饷：{_gameState.WestGardenArmy.BasePayPerTurn} 万钱\n私库现额：{_gameState.PrivateTreasury} 万钱\n军中情势：士气 {_gameState.WestGardenArmy.Morale}/100，天子忠诚 {_gameState.WestGardenArmy.Loyalty}/100"
        };
        StylePopupBodyText(desc, PopupSkin.WestGarden);
        vBox.AddChild(desc);

        var amountSpin = new SpinBox
        {
            MinValue = 0,
            MaxValue = Math.Max(0, _gameState.PrivateTreasury),
            Step = 50,
            Value = Math.Min(Math.Max(_gameState.WestGardenArmy.BasePayPerTurn + 100, 150), Math.Max(0, _gameState.PrivateTreasury)),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        StylePopupInput(amountSpin, PopupSkin.WestGarden);
        vBox.AddChild(amountSpin);

        var previewFrame = new Panel { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        previewFrame.AddThemeStyleboxOverride("panel", CreatePopupInnerPanelStyle(PopupSkin.WestGarden));
        var previewMargin = new MarginContainer();
        SetFullRect(previewMargin);
        previewMargin.AddThemeConstantOverride("margin_left", 10);
        previewMargin.AddThemeConstantOverride("margin_right", 10);
        previewMargin.AddThemeConstantOverride("margin_top", 8);
        previewMargin.AddThemeConstantOverride("margin_bottom", 8);
        previewFrame.AddChild(previewMargin);
        var preview = CreateActionPreviewLabel(PopupSkin.WestGarden);
        previewMargin.AddChild(preview);
        vBox.AddChild(previewFrame);

        void RefreshPreview(double value)
        {
            int amount = (int)value;
            int basePay = _gameState.WestGardenArmy.BasePayPerTurn;
            string verdict = amount < basePay ? "欠饷生怨，军心恐挫" : amount < basePay * 1.2 ? "仅足支应，军心微振" : "厚赏军士，可振西园锐气";
            preview.Text = $"朱批额度：{amount} 万钱\n私库结余：{Math.Max(0, _gameState.PrivateTreasury - amount)} 万钱\n预判：{verdict}";
        }

        amountSpin.ValueChanged += RefreshPreview;
        RefreshPreview(amountSpin.Value);

        var row = CreateActionPopupButtonRow();
        var confirm = new Button { Text = "朱批发饷", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, Disabled = _gameState.PrivateTreasury <= 0 };
        var cancel = new Button { Text = "收回军令", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        StyleSceneActionButton(confirm, ActionButtonSkin.WestGarden);
        StyleSceneActionButton(cancel, ActionButtonSkin.WestGarden);
        row.AddChild(confirm);
        row.AddChild(cancel);
        vBox.AddChild(row);

        confirm.Pressed += () =>
        {
            int amount = (int)amountSpin.Value;
            _windowManager.PopWindow();
            DoArmyDrillAction(amount);
        };
        cancel.Pressed += _windowManager.PopWindow;

        PushTemporaryPopup(panel);
    }

    private void ShowRecruitArmyDialog()
    {
        if (_gameState == null) return;

        const int maxArmySize = 12000;
        int capacity = Math.Max(0, maxArmySize - _gameState.WestGardenArmy.Size);
        int defaultTroops = Math.Min(2000, capacity);
        if (defaultTroops <= 0) defaultTroops = 1000;

        var panel = new Panel();
        ConfigureCenteredPopupPanel(panel, PopupSkin.WestGarden, new Vector2(540, 360));

        var vBox = CreateActionPopupRoot(panel, 22, 18);
        var title = new Label { Text = "西园军令 · 募兵补军" };
        StylePopupTitle(title, PopupSkin.WestGarden);
        vBox.AddChild(title);

        var desc = new Label
        {
            Text = $"当前兵力：{_gameState.WestGardenArmy.Size}/{maxArmySize}\n每募 1000 人：国库 -300 万钱，天下民心 -1，士气 -1。"
        };
        StylePopupBodyText(desc, PopupSkin.WestGarden);
        vBox.AddChild(desc);

        var troopSpin = new SpinBox
        {
            MinValue = 1000,
            MaxValue = Math.Max(1000, capacity),
            Step = 1000,
            Value = defaultTroops,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        StylePopupInput(troopSpin, PopupSkin.WestGarden);
        vBox.AddChild(troopSpin);

        var previewFrame = new Panel { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        previewFrame.AddThemeStyleboxOverride("panel", CreatePopupInnerPanelStyle(PopupSkin.WestGarden));
        var previewMargin = new MarginContainer();
        SetFullRect(previewMargin);
        previewMargin.AddThemeConstantOverride("margin_left", 10);
        previewMargin.AddThemeConstantOverride("margin_right", 10);
        previewMargin.AddThemeConstantOverride("margin_top", 8);
        previewMargin.AddThemeConstantOverride("margin_bottom", 8);
        previewFrame.AddChild(previewMargin);
        var preview = CreateActionPreviewLabel(PopupSkin.WestGarden);
        previewMargin.AddChild(preview);
        vBox.AddChild(previewFrame);

        void RefreshPreview(double value)
        {
            int troops = Math.Min((int)value, capacity);
            if (capacity <= 0)
            {
                preview.Text = "西园新军已满编，无需继续募兵。";
                troopSpin.Editable = false;
                return;
            }

            int batches = troops / 1000;
            preview.Text = $"预计征发：{troops} 人\n国库消耗：{batches * 300} 万钱\n民心影响：-{batches}\n募兵后兵力：{_gameState.WestGardenArmy.Size + troops}/{maxArmySize}";
        }

        troopSpin.ValueChanged += RefreshPreview;
        RefreshPreview(troopSpin.Value);

        var row = CreateActionPopupButtonRow();
        var confirm = new Button { Text = "下诏募兵", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, Disabled = capacity <= 0 };
        var cancel = new Button { Text = "暂缓", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        StyleSceneActionButton(confirm, ActionButtonSkin.WestGarden);
        StyleSceneActionButton(cancel, ActionButtonSkin.WestGarden);
        row.AddChild(confirm);
        row.AddChild(cancel);
        vBox.AddChild(row);

        confirm.Pressed += () =>
        {
            int troops = (int)troopSpin.Value;
            _windowManager.PopWindow();
            DoRecruitArmyAction(troops);
        };
        cancel.Pressed += _windowManager.PopWindow;

        PushTemporaryPopup(panel);
    }

    private void ShowDisasterReliefDialog()
    {
        if (_gameState == null) return;

        var panel = new Panel();
        ConfigureCenteredPopupPanel(panel, PopupSkin.Court, new Vector2(620, 390));

        var vBox = CreateActionPopupRoot(panel, 24, 20);
        var title = new Label { Text = "宣政殿圣裁 · 开仓赈灾" };
        StylePopupTitle(title, PopupSkin.Court);
        vBox.AddChild(title);

        var desc = new Label
        {
            Text = $"国库现额：{_gameState.Treasury} 万钱\n饥馑蔓延，赈银低于 1000 万钱恐不足以安民；经办官清浊将影响实到灾民之数。"
        };
        StylePopupBodyText(desc, PopupSkin.Court);
        vBox.AddChild(desc);

        var amountSpin = new SpinBox
        {
            MinValue = 100,
            MaxValue = Math.Max(100, _gameState.Treasury),
            Step = 100,
            Value = Math.Min(1000, Math.Max(100, _gameState.Treasury)),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        StylePopupInput(amountSpin, PopupSkin.Court);
        vBox.AddChild(amountSpin);

        var previewFrame = new Panel { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        previewFrame.AddThemeStyleboxOverride("panel", CreatePopupInnerPanelStyle(PopupSkin.Court));
        var previewMargin = new MarginContainer();
        SetFullRect(previewMargin);
        previewMargin.AddThemeConstantOverride("margin_left", 10);
        previewMargin.AddThemeConstantOverride("margin_right", 10);
        previewMargin.AddThemeConstantOverride("margin_top", 8);
        previewMargin.AddThemeConstantOverride("margin_bottom", 8);
        previewFrame.AddChild(previewMargin);
        var preview = CreateActionPreviewLabel(PopupSkin.Court);
        previewMargin.AddChild(preview);
        vBox.AddChild(previewFrame);

        void RefreshPreview(double value)
        {
            int amount = (int)value;
            preview.Text = $"拟拨赈银：{amount} 万钱\n国库结余：{Math.Max(0, _gameState.Treasury - amount)} 万钱\n预判：{(amount >= 1000 ? "足额赈济，民心可回" : "杯水车薪，恐失朝廷威信")}";
        }
        amountSpin.ValueChanged += RefreshPreview;
        RefreshPreview(amountSpin.Value);

        var envoyTitle = new Label { Text = "钦差经办" };
        StyleColumnTitle(envoyTitle, PopupSkin.Court);
        vBox.AddChild(envoyTitle);

        var officerRow = CreateActionPopupButtonRow();
        AddReliefOfficerButton(officerRow, "cao_cao", "曹操", amountSpin);
        AddReliefOfficerButton(officerRow, "he_jin", "何进", amountSpin);
        AddReliefOfficerButton(officerRow, "zhang_rang", "张让", amountSpin);
        vBox.AddChild(officerRow);

        var cancel = new Button { Text = "暂缓决策", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        StyleSceneActionButton(cancel, ActionButtonSkin.Court);
        cancel.Pressed += _windowManager.PopWindow;
        vBox.AddChild(cancel);

        PushTemporaryPopup(panel);
    }

    private void AddReliefOfficerButton(HBoxContainer row, string officerId, string fallbackName, SpinBox amountSpin)
    {
        string label = fallbackName;
        if (_gameState != null && _gameState.Npcs.TryGetValue(officerId, out var npc))
        {
            label = $"{npc.Name}\n贪墨风险 {npc.Corruption}%";
        }

        var button = new Button { Text = label, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        StyleSceneActionButton(button, ActionButtonSkin.Court);
        button.Pressed += () =>
        {
            int amount = (int)amountSpin.Value;
            _windowManager.PopWindow();
            DoDisasterReliefAction(amount, officerId);
        };
        row.AddChild(button);
    }

    private VBoxContainer CreateActionPopupRoot(Panel panel, int marginX, int marginY)
    {
        var vBox = new VBoxContainer();
        SetFullRect(vBox);
        vBox.OffsetLeft = marginX;
        vBox.OffsetTop = marginY;
        vBox.OffsetRight = -marginX;
        vBox.OffsetBottom = -marginY;
        vBox.AddThemeConstantOverride("separation", 10);
        panel.AddChild(vBox);
        return vBox;
    }

    private static HBoxContainer CreateActionPopupButtonRow()
    {
        var row = new HBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        row.AddThemeConstantOverride("separation", 12);
        return row;
    }

    private void ShowCourtReportPopup(string fallbackTitle, string storyText)
    {
        ShowStoryReportPopup(fallbackTitle, storyText, PopupSkin.Court);
    }

    private void ShowIntelReportPopup(string fallbackTitle, string storyText)
    {
        ShowStoryReportPopup(fallbackTitle, storyText, PopupSkin.Intel);
    }

    private void ShowWestGardenReportPopup(string fallbackTitle, string storyText)
    {
        ShowStoryReportPopup(fallbackTitle, storyText, PopupSkin.WestGarden);
    }

    private void ShowDocumentReportPopup(string fallbackTitle, string storyText)
    {
        ShowStoryReportPopup(fallbackTitle, storyText, PopupSkin.Document);
    }

    private void ShowWarningReportPopup(string fallbackTitle, string storyText)
    {
        ShowStoryReportPopup(fallbackTitle, storyText, PopupSkin.Warning);
    }

    private void ShowTravelReportPopup(string fallbackTitle, string storyText, string location)
    {
        ShowStoryReportPopup(fallbackTitle, storyText, GetTravelReportSkin(location));
    }

    // === P1-C2 自动推进（快进 N 旬）===
    // 主入口弹窗：步数输入（1-30，默认 3）
    // 异步循环 NextXunAsync，每旬后检测临界事件（新叛变 / 健康≤30 / 国库≤1000 / 结局已定 / 历史 trigger）
    // 命中即弹 ShowWarningReportPopup 中断 + 摘要
    private void ShowFastForwardDialog()
    {
        if (_gameState == null || _gameEngine == null) return;
        if (_gameState.Outcome != GameOutcome.Playing) return;

        var panel = new Panel();
        ConfigureCenteredPopupPanel(panel, PopupSkin.Court, new Vector2(580, 420));

        var vBox = CreateActionPopupRoot(panel, 22, 18);

        var title = new Label { Text = "御 批 · 快 进 N 旬" };
        StylePopupTitle(title, PopupSkin.Court);
        vBox.AddChild(title);

        var desc = new Label
        {
            Text = $"将连续推进 N 旬（1-30）。\n" +
                   $"遇以下情形会立即暂停弹奏报：\n" +
                   $"  · 新叛变暴起\n" +
                   $"  · 龙体欠安（健康 ≤ 30）\n" +
                   $"  · 国帑枯竭（国库 ≤ 1000 万钱）\n" +
                   $"  · 触发重大历史事件\n" +
                   $"  · 灵帝崩殂 / 亡国 / 中兴 / 续命\n\n" +
                   $"当前：{_gameState.ReignTitle}{_gameState.ReignYear}年 {_gameState.Year}年{_gameState.Month}月"
        };
        desc.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        StylePopupBodyText(desc, PopupSkin.Court);
        vBox.AddChild(desc);

        var stepSpin = new SpinBox
        {
            MinValue = 1,
            MaxValue = 30,
            Step = 1,
            Value = 3,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        StylePopupInput(stepSpin, PopupSkin.Court);
        vBox.AddChild(stepSpin);

        var previewFrame = new Panel { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        previewFrame.AddThemeStyleboxOverride("panel", CreatePopupInnerPanelStyle(PopupSkin.Court));
        var previewMargin = new MarginContainer();
        SetFullRect(previewMargin);
        previewMargin.AddThemeConstantOverride("margin_left", 10);
        previewMargin.AddThemeConstantOverride("margin_right", 10);
        previewMargin.AddThemeConstantOverride("margin_top", 8);
        previewMargin.AddThemeConstantOverride("margin_bottom", 8);
        previewFrame.AddChild(previewMargin);
        var preview = CreateActionPreviewLabel(PopupSkin.Court);
        previewMargin.AddChild(preview);
        vBox.AddChild(previewFrame);

        void RefreshPreview(double value)
        {
            int n = Math.Clamp((int)value, 1, 30);
            preview.Text = $"快进 {n} 旬 ≈ {(n + 2) / 3} 个月。\n" +
                          $"期间可能触发黄巾、何进之死、董卓入京等历史 trigger。";
        }
        stepSpin.ValueChanged += RefreshPreview;
        RefreshPreview(stepSpin.Value);

        var row = CreateActionPopupButtonRow();
        var confirm = new Button { Text = "驾临快进", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        var cancel = new Button { Text = "暂缓", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        StyleSceneActionButton(confirm, ActionButtonSkin.Court);
        StyleSceneActionButton(cancel, ActionButtonSkin.Court);
        row.AddChild(confirm);
        row.AddChild(cancel);
        vBox.AddChild(row);

        confirm.Pressed += () =>
        {
            int n = Math.Clamp((int)stepSpin.Value, 1, 30);
            _windowManager.PopWindow();
            _ = DoFastForwardAsync(n);
        };
        cancel.Pressed += _windowManager.PopWindow;

        PushTemporaryPopup(panel);
    }

    // 异步执行快进：每旬调 NextXunAsync 并检测临界事件
    private async Task DoFastForwardAsync(int n)
    {
        if (_gameState == null || _gameEngine == null) return;
        if (_gameState.Outcome != GameOutcome.Playing) return;

        // 禁用按钮避免重复点击
        if (_fastForwardButton != null) _fastForwardButton.Disabled = true;

        try
        {
            int ran = 0;
            int newRebellions = 0;
            int pacifiedRebellions = 0;
            var newRebellionNames = new List<string>();
            var pacifiedNames = new List<string>();
            var historicalEvents = new List<string>();
            bool aborted = false;
            string abortReason = "";
            string criticalTitle = "";

            for (int i = 0; i < n; i++)
            {
                if (_gameState.Outcome != GameOutcome.Playing)
                {
                    aborted = true;
                    abortReason = $"灵帝 {_gameState.GetEmperorAge()} 岁：{_gameEngine.GetOutcomeMessage()}";
                    break;
                }

                var wasRebelling = _gameState.Provinces.Values.Where(p => p.IsRebelling).Select(p => p.Id).ToHashSet();
                int chronicleBefore = _gameState.Chronicle.Count;

                await _gameEngine.NextXunAsync();
                ran++;

                var nowRebelling = _gameState.Provinces.Values.Where(p => p.IsRebelling).Select(p => p.Id).ToHashSet();
                var newlyRebelling = nowRebelling.Except(wasRebelling).ToList();
                var pacified = wasRebelling.Except(nowRebelling).ToList();
                newRebellions += newlyRebelling.Count;
                pacifiedRebellions += pacified.Count;
                newRebellionNames.AddRange(newlyRebelling.Select(id => _gameState.Provinces[id].Name));
                pacifiedNames.AddRange(pacified.Select(id => _gameState.Provinces[id].Name));

                // 历史 trigger：扫新增 chronicle
                var newChronicle = _gameState.Chronicle.Skip(chronicleBefore).ToList();
                foreach (var entry in newChronicle)
                {
                    if (entry.Contains("黄巾") || entry.Contains("何进") || entry.Contains("董卓"))
                    {
                        historicalEvents.Add(entry);
                    }
                }

                // 刷新主界面
                UpdateUI();
                SetAnnualMajorEventBanner();

                // 临界事件检查
                if (_gameState.Outcome != GameOutcome.Playing)
                {
                    aborted = true;
                    abortReason = $"灵帝 {_gameState.GetEmperorAge()} 岁：{_gameEngine.GetOutcomeMessage()}";
                    break;
                }
                if (newlyRebelling.Count > 0)
                {
                    aborted = true;
                    abortReason = string.Join("、", newRebellionNames.Distinct()) + " 起兵叛乱！请速平叛。";
                    break;
                }
                if (_gameState.Health <= 30)
                {
                    aborted = true;
                    abortReason = $"龙体欠安（健康 {_gameState.Health}）。请移驾后宫调养。";
                    break;
                }
                if (_gameState.Treasury <= 1000)
                {
                    aborted = true;
                    abortReason = $"国帑枯竭（国库 {_gameState.Treasury} 万钱）。请赈灾/抄家/卖官补充。";
                    break;
                }

                // 短暂延迟避免阻塞 UI
                await ToSignal(GetTree().CreateTimer(0.05), SceneTreeTimer.SignalName.Timeout);
            }

            // 构造奏报
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"【快进 {ran} 旬完毕】");
            sb.AppendLine($"当前：{_gameState.ReignTitle}{_gameState.ReignYear}年 {_gameState.Year}年{_gameState.Month}月{(_gameState.Xun == 1 ? "上" : _gameState.Xun == 2 ? "中" : "下")}旬");
            sb.AppendLine($"皇权：{_gameState.ImperialPower}  健康：{_gameState.Health}  民心：{_gameState.PopularSupport}  国库：{_gameState.Treasury} 万钱");
            if (newRebellions > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"⚡ 新叛乱：+{newRebellions} 郡（{string.Join("、", newRebellionNames.Distinct())}）");
            }
            if (pacifiedRebellions > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"✓ 平息：-{pacifiedRebellions} 郡（{string.Join("、", pacifiedNames.Distinct())}）");
            }
            if (historicalEvents.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("★ 历史事件：");
                foreach (var ev in historicalEvents)
                    sb.AppendLine($"  · {ev}");
            }

            if (aborted)
            {
                criticalTitle = "快进中止";
                sb.AppendLine();
                sb.AppendLine($"⚠ {abortReason}");
            }
            else if (ran < n)
            {
                criticalTitle = "快进提前结束";
                sb.AppendLine();
                sb.AppendLine($"已推进 {ran}/{n} 旬。");
            }
            else
            {
                criticalTitle = "快进完成";
                sb.AppendLine();
                sb.AppendLine("  一切平稳，未触发临界事件。");
            }

            // 中止或有事件 → 弹警告；平稳完成 → 弹 Document 奏报
            if (aborted || historicalEvents.Count > 0 || newRebellions > 0)
            {
                ShowWarningReportPopup(criticalTitle, sb.ToString());
            }
            else
            {
                ShowDocumentReportPopup(criticalTitle, sb.ToString());
            }
        }
        catch (System.Exception ex)
        {
            ShowWarningReportPopup("快进出错", $"【快进出错】\n\n{ex.Message}");
        }
        finally
        {
            if (_fastForwardButton != null) _fastForwardButton.Disabled = false;
        }
    }

    private void ShowStoryReportPopup(string fallbackTitle, string storyText, PopupSkin skin)
    {
        if (string.IsNullOrWhiteSpace(storyText)) return;

        var panel = new Panel();
        panel.Name = "StoryReportPopup";
        ConfigureCenteredPopupPanel(panel, skin, GetReportPopupSize(skin));

        var root = CreateActionPopupRoot(panel, 24, 20);
        root.AddThemeConstantOverride("separation", 12);

        var seal = new Label { Text = GetReportSealText(skin, fallbackTitle) };
        StyleReportSealLabel(seal, skin);
        root.AddChild(seal);

        var title = new Label { Text = ExtractReportTitle(fallbackTitle, storyText) };
        StylePopupTitle(title, skin);
        root.AddChild(title);

        var meta = new Label { Text = BuildReportMetaLine(skin) };
        StyleReportMetaLabel(meta, skin);
        root.AddChild(meta);

        var reportFrame = new Panel
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        reportFrame.AddThemeStyleboxOverride("panel", CreateReportBodyFrameStyle(skin));
        root.AddChild(reportFrame);

        var report = new RichTextLabel
        {
            BbcodeEnabled = true,
            Text = StripLeadingReportTitle(storyText),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            ScrollActive = true
        };
        report.AddThemeFontSizeOverride("normal_font_size", 16);
        report.AddThemeColorOverride("default_color", GetPopupBodyColor(skin));
        SetFullRect(report);
        report.OffsetLeft = 16;
        report.OffsetTop = 14;
        report.OffsetRight = -16;
        report.OffsetBottom = -14;
        reportFrame.AddChild(report);

        var footer = new Label { Text = GetReportFooterText(skin) };
        StyleReportMetaLabel(footer, skin);
        root.AddChild(footer);

        var close = new Button
        {
            Text = GetReportCloseText(skin),
            CustomMinimumSize = new Vector2(0, 42),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        StyleSceneActionButton(close, GetActionButtonSkinForPopup(skin));
        close.Pressed += _windowManager.PopWindow;
        root.AddChild(close);

        PushTemporaryPopup(panel);
    }

    private static Vector2 GetReportPopupSize(PopupSkin skin)
    {
        return skin switch
        {
            PopupSkin.Court => new Vector2(760, 500),
            PopupSkin.Intel => new Vector2(780, 510),
            PopupSkin.WestGarden => new Vector2(760, 500),
            PopupSkin.Document => new Vector2(740, 500),
            PopupSkin.Travel => new Vector2(720, 470),
            PopupSkin.Warning => new Vector2(700, 450),
            _ => new Vector2(720, 460)
        };
    }

    private static string GetReportSealText(PopupSkin skin, string fallbackTitle)
    {
        return skin switch
        {
            PopupSkin.Court => "尚书台 · 百官回奏",
            PopupSkin.Intel => fallbackTitle.Contains("军情") ? "黄门密札 · 军情战报" : "黄门密札 · 州郡回传",
            PopupSkin.WestGarden => "西园密署 · 军簿回报",
            PopupSkin.Document => "御案折匣 · 朱批回奏",
            PopupSkin.Travel => "黄门导驾 · 龙辇奏报",
            PopupSkin.Warning => fallbackTitle.Contains("御史") ? "御史台 · 风闻弹奏" : "黄门短札 · 急奏",
            _ => "内廷奏报"
        };
    }

    private static string BuildReportMetaLine(PopupSkin skin)
    {
        string source = skin switch
        {
            PopupSkin.Court => "来源：宣政殿 / 尚书台",
            PopupSkin.Intel => "来源：黄门密札 / 州郡舆图",
            PopupSkin.WestGarden => "来源：西园别苑 / 天子亲军",
            PopupSkin.Document => "来源：御案折匣 / 奏章朱批",
            PopupSkin.Travel => "来源：龙辇仪仗 / 宫门黄门",
            PopupSkin.Warning => "来源：御史台 / 黄门急奏",
            _ => "来源：内廷"
        };
        return $"{source} ｜ {DateTime.Now:HH:mm:ss}";
    }

    private static string GetReportFooterText(PopupSkin skin)
    {
        return skin switch
        {
            PopupSkin.Court => "钤印：圣裁已入起居注，百官反应将在后续旬日发酵。",
            PopupSkin.Intel => "钤印：密札已封存，地方风险请继续于黄门密札复核。",
            PopupSkin.WestGarden => "钤印：军簿已登记，兵额、军费与士气变化即时生效。",
            PopupSkin.Document => "钤印：朱批已下，尚书台据此流转卷宗。",
            PopupSkin.Travel => "钤印：导驾已毕，当前驻跸之所已经更新。",
            PopupSkin.Warning => "钤印：此为前置警示，未必消耗本旬行动。",
            _ => "钤印：奏报已收。"
        };
    }

    private static string GetReportCloseText(PopupSkin skin)
    {
        return skin switch
        {
            PopupSkin.Court => "御览毕 · 收起回奏",
            PopupSkin.Intel => "封存密札",
            PopupSkin.WestGarden => "归档军簿",
            PopupSkin.Document => "合上折匣",
            PopupSkin.Travel => "收起导驾奏报",
            PopupSkin.Warning => "朕已知晓",
            _ => "收起奏报"
        };
    }

    private static void StyleReportSealLabel(Label label, PopupSkin skin)
    {
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.AddThemeFontSizeOverride("font_size", 15);
        label.AddThemeColorOverride("font_color", GetReportAccentColor(skin));
    }

    private static void StyleReportMetaLabel(Label label, PopupSkin skin)
    {
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        label.AddThemeFontSizeOverride("font_size", 13);
        label.AddThemeColorOverride("font_color", GetPopupBodyColor(skin).Darkened(skin == PopupSkin.Intel || skin == PopupSkin.Document ? 0.16f : 0.10f));
    }

    private static Color GetReportAccentColor(PopupSkin skin)
    {
        return skin switch
        {
            PopupSkin.Intel => new Color(0.98f, 0.82f, 0.52f, 1.0f),
            PopupSkin.Document => new Color(0.28f, 0.13f, 0.04f, 1.0f),
            _ => GetPopupTitleColor(skin).Lightened(0.08f)
        };
    }

    private static StyleBoxFlat CreateReportBodyFrameStyle(PopupSkin skin)
    {
        var style = skin == PopupSkin.Document || skin == PopupSkin.Intel
            ? CreatePopupParchmentStyle()
            : CreatePopupInnerPanelStyle(skin);
        if (skin == PopupSkin.Intel)
        {
            style.BgColor = new Color(0.760f, 0.650f, 0.430f, 1.0f);
            style.BorderColor = new Color(0.42f, 0.24f, 0.10f, 1.0f);
        }
        if (skin == PopupSkin.Warning)
        {
            style.BorderColor = new Color(0.90f, 0.22f, 0.12f, 1.0f);
            style.SetBorderWidthAll(2);
        }
        return style;
    }

    private static string ExtractReportTitle(string fallbackTitle, string storyText)
    {
        if (!string.IsNullOrWhiteSpace(storyText) && storyText.StartsWith("【"))
        {
            int end = storyText.IndexOf('】');
            if (end > 1) return storyText.Substring(1, end - 1);
        }
        return fallbackTitle;
    }

    private static string StripLeadingReportTitle(string storyText)
    {
        if (!string.IsNullOrWhiteSpace(storyText) && storyText.StartsWith("【"))
        {
            int end = storyText.IndexOf('】');
            if (end >= 0)
            {
                string body = storyText.Substring(end + 1).TrimStart();
                return string.IsNullOrWhiteSpace(body) ? storyText : body;
            }
        }
        return storyText;
    }

    private void PushTemporaryPopup(Panel panel)
    {
        AddChild(panel);
        _windowManager.PushWindow(panel, freeOnClose: true);
    }

    // 执行开仓赈灾动作
    private void DoDisasterReliefAction(int amount, string officerId)
    {
        if (_gameEngine == null) return;

        try
        {
            var result = _gameEngine.ExecuteDisasterReliefAction(amount, officerId);
            ShowCourtReportPopup("宣政殿赈灾回奏", result.StoryText);
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
            ShowDocumentReportPopup("御案朱批回奏", result.StoryText);
            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Error in Turn Processing]: {ex.Message}");
            ShowWarningReportPopup("内监急奏", "【内监急奏】\n\n圣旨解析失败，AI 未响应或格式有误。");
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

    // === A1 结局面板 ===
    // _Process 每帧调用：检查 GameState.Outcome 是否已非 Playing，是则弹"圣裁已定"奏报。
    // 中兴/续命走 Document 浅纸面（吉），崩殂/亡国走 Warning 急奏（凶）。
    // _outcomeHandled 防止重弹（_Process 每帧跑）。
    private void CheckAndShowOutcomeIfGameOver()
    {
        if (_outcomeHandled) return;
        if (_gameState == null || _gameEngine == null) return;
        if (_gameState.Outcome == GameOutcome.Playing) return;

        var outcome = _gameState.Outcome;
        string body = _gameEngine.GetOutcomeMessage();
        string summary = BuildOutcomeSummary();
        string fullText = body + "\n\n" + summary;

        if (outcome == GameOutcome.ZhongXing || outcome == GameOutcome.XuMing)
        {
            ShowDocumentReportPopup("圣裁已定", fullText);
        }
        else
        {
            ShowWarningReportPopup("圣裁已定", fullText);
        }

        _handledOutcome = outcome;
        _outcomeHandled = true;
    }

    private string BuildOutcomeSummary()
    {
        if (_gameState == null) return string.Empty;
        return $"— 史官终录 —\n" +
               $"年号：{_gameState.ReignTitle}{_gameState.ReignYear}年\n" +
               $"公元：{_gameState.Year}年{_gameState.Month}月{(_gameState.Xun == 1 ? "上" : _gameState.Xun == 2 ? "中" : "下")}旬\n" +
               $"灵帝春秋：{_gameState.GetEmperorAge()} 岁\n" +
               $"皇权终值：{_gameState.ImperialPower} / 100\n" +
               $"天下民心：{_gameState.PopularSupport} / 100\n" +
               $"皇帝健康：{_gameState.Health} / 100\n" +
               $"国帑结余：{_gameState.Treasury} 万钱\n" +
               $"西园私帑：{_gameState.PrivateTreasury} 万钱\n" +
               $"叛郡计数：{CountRebellingProvinces()} / {_gameState.Provinces.Count}";
    }

    private int CountRebellingProvinces()
    {
        if (_gameState == null) return 0;
        int n = 0;
        foreach (var p in _gameState.Provinces.Values)
            if (p.IsRebelling) n++;
        return n;
    }
}
