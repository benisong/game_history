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
                "西园" => "【起驾 · 西园精舍】\n\n“起驾西园——！”\n陛下避开了何进等人的耳目，轻车简从，来到了陛下亲自督造的西园。这里有堆积如山的金银私库，有新募组建的精锐新军，是陛下摆脱掣肘、暗中夺回大权的铁血基地。",
                _ => $"【起驾】\n\n陛下移驾{location}。"
            };
            ShowStoryReportPopup("起驾奏报", travelStory, location == "西园" ? PopupSkin.WestGarden : PopupSkin.Court);

            UpdateUI();
            if (location == "西园" && _westGardenPopup != null)
            {
                RefreshWestGardenPanel();
                _windowManager.PushWindow(_westGardenPopup);
            }
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
            ShowStoryReportPopup("行动奏报", result.StoryText, actionId == "sell_office" ? PopupSkin.WestGarden : PopupSkin.Court);
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
            ShowStoryReportPopup("西园军报", result.StoryText, PopupSkin.WestGarden);
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
            ShowStoryReportPopup("西园军报", result.StoryText, PopupSkin.WestGarden);
            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
            ShowStoryReportPopup("募兵未成", $"【募兵未成】\n\n{ex.Message}", PopupSkin.Warning);
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

    private void ShowStoryReportPopup(string fallbackTitle, string storyText, PopupSkin skin)
    {
        if (string.IsNullOrWhiteSpace(storyText)) return;

        var panel = new Panel();
        panel.Name = "StoryReportPopup";
        ConfigureCenteredPopupPanel(panel, skin, new Vector2(720, 460));

        var root = CreateActionPopupRoot(panel, 24, 20);
        root.AddThemeConstantOverride("separation", 12);

        var title = new Label { Text = ExtractReportTitle(fallbackTitle, storyText) };
        StylePopupTitle(title, skin);
        root.AddChild(title);

        var reportFrame = new Panel
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        reportFrame.AddThemeStyleboxOverride("panel", CreatePopupInnerPanelStyle(skin));
        root.AddChild(reportFrame);

        var report = new RichTextLabel
        {
            BbcodeEnabled = true,
            Text = StripLeadingReportTitle(storyText),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            ScrollActive = true
        };
        report.AddThemeColorOverride("default_color", GetPopupBodyColor(skin));
        SetFullRect(report);
        report.OffsetLeft = 14;
        report.OffsetTop = 12;
        report.OffsetRight = -14;
        report.OffsetBottom = -12;
        reportFrame.AddChild(report);

        var close = new Button
        {
            Text = "收起奏报",
            CustomMinimumSize = new Vector2(0, 42),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        close.Pressed += _windowManager.PopWindow;
        root.AddChild(close);

        PushTemporaryPopup(panel);
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
        panel.VisibilityChanged += () =>
        {
            if (!panel.Visible && IsInstanceValid(panel)) panel.QueueFree();
        };
        AddChild(panel);
        _windowManager.PushWindow(panel);
    }

    // 执行开仓赈灾动作
    private void DoDisasterReliefAction(int amount, string officerId)
    {
        if (_gameEngine == null) return;

        try
        {
            var result = _gameEngine.ExecuteDisasterReliefAction(amount, officerId);
            ShowStoryReportPopup("宣政殿奏报", result.StoryText, PopupSkin.Court);
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
            ShowStoryReportPopup("朱批回奏", result.StoryText, PopupSkin.Document);
            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Error in Turn Processing]: {ex.Message}");
            ShowStoryReportPopup("内监急奏", "【内监急奏】\n\n圣旨解析失败，AI 未响应或格式有误。", PopupSkin.Warning);
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
}
