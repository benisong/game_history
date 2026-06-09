using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DonghanEngine.Core;

namespace DonghanFrontend;

public partial class MainScene : Control
{
    private const int MaxWestGardenArmySize = 12000;
    private Panel? _westGardenPopup;
    private RichTextLabel? _westGardenStatsLabel;
    private VBoxContainer? _westGardenOfficersVBox;
    private RichTextLabel? _westGardenDetailsLabel;
    private VBoxContainer? _westGardenActionsVBox;
    private Button? _btnWestGardenPalace;

    private void InitializeWestGardenPanel()
    {
        _westGardenPopup = new Panel();
        _westGardenPopup.Name = "WestGardenPopup";
        _westGardenPopup.Visible = false;
        _westGardenPopup.CustomMinimumSize = new Vector2(1100, 640);
        _westGardenPopup.AnchorLeft = 0.5f;
        _westGardenPopup.AnchorTop = 0.5f;
        _westGardenPopup.AnchorRight = 0.5f;
        _westGardenPopup.AnchorBottom = 0.5f;
        _westGardenPopup.OffsetLeft = -550;
        _westGardenPopup.OffsetTop = -320;
        _westGardenPopup.OffsetRight = 550;
        _westGardenPopup.OffsetBottom = 320;
        _westGardenPopup.AddThemeStyleboxOverride("panel", CreatePopupPanelStyle(PopupSkin.WestGarden));

        var root = new VBoxContainer();
        SetFullRect(root);
        root.OffsetLeft = 18;
        root.OffsetTop = 16;
        root.OffsetRight = -18;
        root.OffsetBottom = -16;
        root.AddThemeConstantOverride("separation", 12);
        _westGardenPopup.AddChild(root);

        BuildWestGardenHeader(root);
        BuildWestGardenBody(root);
        AddChild(_westGardenPopup);
    }

    private void BuildWestGardenHeader(VBoxContainer root)
    {
        var title = new Label();
        title.Text = "西园精舍 · 天子亲军密署";
        StylePopupTitle(title, PopupSkin.WestGarden);
        root.AddChild(title);

        _westGardenStatsLabel = new RichTextLabel();
        _westGardenStatsLabel.BbcodeEnabled = true;
        _westGardenStatsLabel.CustomMinimumSize = new Vector2(0, 72);
        root.AddChild(_westGardenStatsLabel);
    }

    private void BuildWestGardenBody(VBoxContainer root)
    {
        var body = new HBoxContainer();
        body.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        body.AddThemeConstantOverride("separation", 12);
        root.AddChild(body);

        var armyColumn = CreateWestGardenColumn(body, "西园军势", 285);
        _westGardenOfficersVBox = new VBoxContainer();
        _westGardenOfficersVBox.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _westGardenOfficersVBox.AddThemeConstantOverride("separation", 8);
        armyColumn.AddChild(_westGardenOfficersVBox);

        var closeButton = new Button();
        closeButton.Text = "合上军簿";
        StyleSceneActionButton(closeButton, ActionButtonSkin.WestGarden);
        closeButton.Pressed += _windowManager.PopWindow;
        armyColumn.AddChild(closeButton);

        var detailsColumn = CreateWestGardenColumn(body, "校场与私库", 500, expand: true);
        _westGardenDetailsLabel = new RichTextLabel();
        _westGardenDetailsLabel.BbcodeEnabled = true;
        _westGardenDetailsLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        detailsColumn.AddChild(_westGardenDetailsLabel);

        var actionColumn = CreateWestGardenColumn(body, "军务处置", 285);
        var actionScroll = new ScrollContainer();
        actionScroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        actionColumn.AddChild(actionScroll);

        _westGardenActionsVBox = new VBoxContainer();
        _westGardenActionsVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _westGardenActionsVBox.AddThemeConstantOverride("separation", 9);
        actionScroll.AddChild(_westGardenActionsVBox);
    }

    private VBoxContainer CreateWestGardenColumn(HBoxContainer parent, string title, int width, bool expand = false)
    {
        var panel = new Panel();
        panel.CustomMinimumSize = new Vector2(width, 0);
        panel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        panel.SizeFlagsHorizontal = expand ? Control.SizeFlags.ExpandFill : Control.SizeFlags.ShrinkBegin;
        panel.AddThemeStyleboxOverride("panel", CreateWestGardenInnerPanelStyle());
        parent.AddChild(panel);

        var box = new VBoxContainer();
        SetFullRect(box);
        box.OffsetLeft = 12;
        box.OffsetTop = 10;
        box.OffsetRight = -12;
        box.OffsetBottom = -10;
        box.AddThemeConstantOverride("separation", 9);
        panel.AddChild(box);

        var label = new Label();
        label.Text = title;
        StyleColumnTitle(label, PopupSkin.WestGarden);
        box.AddChild(label);

        return box;
    }

    private static StyleBoxFlat CreateWestGardenInnerPanelStyle()
    {
        return CreatePopupInnerPanelStyle(PopupSkin.WestGarden);
    }

    private void OnWestGardenPressed()
    {
        if (_gameState == null) return;
        if (_gameState.CurrentLocation != "西园")
        {
            ShowTravelToWestGardenPrompt();
            return;
        }

        RefreshWestGardenPanel();
        _windowManager.PushWindow(_westGardenPopup!);
    }

    private void ShowTravelToWestGardenPrompt()
    {
        ShowTravelPrompt(
            "【黄门密奏】",
            "西园乃天子亲军密署，陛下尚未起驾。是否移驾西园，亲阅校场与私库军簿？",
            "西园",
            "移驾西园");
    }

    private void RefreshWestGardenPanel()
    {
        if (_gameState == null) return;
        RenderWestGardenStats();
        RenderWestGardenOfficers();
        RenderWestGardenDetails();
        RenderWestGardenActions();
    }

    private void RenderWestGardenStats()
    {
        if (_westGardenStatsLabel == null || _gameState == null) return;
        var army = _gameState.WestGardenArmy;
        _westGardenStatsLabel.Text =
            $"[center][color=gray]{FormatTimeLabel()}[/color][/center]\n" +
            $"[center]所在地 [color=yellow]{_gameState.CurrentLocation}[/color]  ｜  " +
            $"私库 [color=yellow]{_gameState.PrivateTreasury}[/color]万  ｜  国库 [color=yellow]{_gameState.Treasury}[/color]万  ｜  " +
            $"皇权 {ColorNumber(_gameState.ImperialPower, danger: 25, warning: 40)}/100  ｜  民心 {ColorNumber(_gameState.PopularSupport, danger: 25, warning: 40)}/100[/center]\n" +
            $"[center]西园新军 [color=yellow]{army.Size}[/color]/{MaxWestGardenArmySize}人  ｜  " +
            $"士气 {ColorNumber(army.Morale, danger: 35, warning: 55)}/100  ｜  忠诚 {ColorNumber(army.Loyalty, danger: 35, warning: 55)}/100  ｜  基础军饷 [color=yellow]{army.BasePayPerTurn}[/color]万/旬[/center]";
    }

    private void RenderWestGardenOfficers()
    {
        if (_westGardenOfficersVBox == null || _gameState == null) return;
        ClearWestGardenChildren(_westGardenOfficersVBox);

        var armyText = new RichTextLabel();
        armyText.BbcodeEnabled = true;
        armyText.CustomMinimumSize = new Vector2(0, 145);
        armyText.Text = BuildWestGardenArmyCard();
        _westGardenOfficersVBox.AddChild(armyText);

        _westGardenOfficersVBox.AddChild(new Label
        {
            Text = "校尉帐前",
            HorizontalAlignment = HorizontalAlignment.Center
        });

        AddWestGardenOfficerButton("jian_shuo", "上军校尉");
        AddWestGardenOfficerButton("cao_cao", "典军可任");
        AddWestGardenOfficerButton("zhang_rang", "中官掌财");
    }

    private string BuildWestGardenArmyCard()
    {
        if (_gameState == null) return string.Empty;
        var army = _gameState.WestGardenArmy;
        var tags = new List<string>();
        if (army.Morale < 35) tags.Add("[color=red]军心浮动[/color]");
        if (army.Loyalty < 35) tags.Add("[color=red]忠诚堪忧[/color]");
        if (army.Size < 6000) tags.Add("[color=yellow]兵力不足[/color]");
        if (army.Size >= MaxWestGardenArmySize) tags.Add("[color=green]已近满编[/color]");
        if (tags.Count == 0) tags.Add("[color=green]尚可整顿[/color]");

        return "[b]【亲军簿】[/b]\n" +
            $"新军：{army.Size}/{MaxWestGardenArmySize} 人\n" +
            $"士气：{ColorNumber(army.Morale, danger: 35, warning: 55)}/100\n" +
            $"忠诚：{ColorNumber(army.Loyalty, danger: 35, warning: 55)}/100\n" +
            $"基础军饷：{army.BasePayPerTurn} 万/旬\n" +
            $"风险：{string.Join("、", tags)}";
    }

    private void AddWestGardenOfficerButton(string ministerId, string role)
    {
        if (_westGardenOfficersVBox == null || _gameState == null) return;
        if (!_gameState.Npcs.TryGetValue(ministerId, out var npc)) return;

        var button = new Button();
        button.Text = $"{npc.Name}｜{role}\n好感{npc.Favorability} 权势{npc.Power} 贪腐{npc.Corruption}";
        button.Alignment = HorizontalAlignment.Left;
        button.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        StyleSceneActionButton(button, ActionButtonSkin.WestGarden);
        button.Pressed += () => ShowMinisterDetails(ministerId);
        _westGardenOfficersVBox.AddChild(button);
    }

    private void RenderWestGardenDetails()
    {
        if (_westGardenDetailsLabel == null || _gameState == null) return;
        _westGardenDetailsLabel.Text =
            "[b][font_size=16]【军势研判】[/font_size][/b]\n" +
            BuildWestGardenAssessmentText() + "\n" +
            "[color=yellow][b]【私库账目提示】[/b][/color]\n" +
            "- 阅兵犒赏消耗天子私库，实际入营金额受经办人贪腐影响。\n" +
            "- 募兵补军消耗国库，并会压低天下民心与新军士气。\n" +
            "- 鬻官纳钱可快速充实私库，但损害皇权声望。\n\n" +
            "[color=yellow][b]【近日西园记】[/b][/color]\n" +
            BuildWestGardenChronicleText();
    }

    private string BuildWestGardenAssessmentText()
    {
        if (_gameState == null) return string.Empty;
        var army = _gameState.WestGardenArmy;
        var notes = new List<string>();

        if (army.Size < 6000) notes.Add("- 新军兵力不足，若地方叛乱扩大，平叛兵源将捉襟见肘。");
        else if (army.Size < MaxWestGardenArmySize) notes.Add("- 新军尚未满编，可视国库与民心情况分批募兵。");
        else notes.Add("- 新军已近满编，继续募兵意义不大，应重在稳军心。 ");

        if (army.Morale < 35) notes.Add("- 士气低迷，宜尽快发内帑犒军，否则出征战力受损。 ");
        else if (army.Morale < 55) notes.Add("- 士气中等，适度犒赏可稳住校场军心。 ");
        else notes.Add("- 士气尚可，可把私库留作危机时重赏。 ");

        if (army.Loyalty < 35) notes.Add("- 忠诚堪忧，西园亲军未必能完全为天子所用。 ");
        else if (army.Loyalty < 55) notes.Add("- 忠诚未稳，发饷与亲信经办都能影响军心归属。 ");
        else notes.Add("- 忠诚较稳，是天子压制外戚与地方叛乱的重要根基。 ");

        if (_gameState.PrivateTreasury < 500) notes.Add("- 私库告急，厚赏亲军前需先筹钱。 ");
        if (_gameState.Treasury < 1000) notes.Add("- 国库吃紧，募兵和平叛都会受到限制。 ");
        if (_gameState.PopularSupport < 25) notes.Add("- 天下民心已危，继续募兵会加重民怨。 ");

        return string.Join("\n", notes) + "\n";
    }

    private string BuildWestGardenChronicleText()
    {
        if (_gameState == null) return "- 暂无西园记录。";
        string[] keywords = { "西园", "募兵", "阅兵", "鬻官", "平叛" };
        var records = _gameState.Chronicle
            .Where(r => keywords.Any(k => r.Contains(k)))
            .TakeLast(5)
            .ToList();

        if (records.Count == 0)
        {
            return "- 陛下轻车简从入西园，校场旗鼓未动，诸校尉静候圣裁。";
        }

        return string.Join("\n", records.Select(r => $"- {r}"));
    }

    private void RenderWestGardenActions()
    {
        if (_westGardenActionsVBox == null || _gameState == null) return;
        ClearWestGardenChildren(_westGardenActionsVBox);

        AddWestGardenSectionTitle("私库经营");
        _westGardenActionsVBox.AddChild(new Label
        {
            Text = "预览：私库 +1000 万，皇权 -3。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        });
        AddWestGardenActionButton("鬻官纳钱", DoWestGardenSellOfficeAction);

        AddWestGardenSectionTitle("阅兵犒赏");
        var officerOption = new OptionButton();
        AddOfficerOption(officerOption, "jian_shuo");
        AddOfficerOption(officerOption, "cao_cao");
        AddOfficerOption(officerOption, "zhang_rang");
        StylePopupInput(officerOption, PopupSkin.WestGarden);
        _westGardenActionsVBox.AddChild(officerOption);

        var paySpin = new SpinBox
        {
            MinValue = 0,
            MaxValue = Math.Max(0, _gameState.PrivateTreasury),
            Step = 100,
            Value = Math.Min(1000, Math.Max(0, _gameState.PrivateTreasury)),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        StylePopupInput(paySpin, PopupSkin.WestGarden);
        _westGardenActionsVBox.AddChild(paySpin);

        var drillPreview = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _westGardenActionsVBox.AddChild(drillPreview);

        void RefreshDrillPreview()
        {
            string officerId = GetSelectedOfficerId(officerOption);
            string risk = GetOfficerCorruptionRisk(officerId);
            drillPreview.Text = $"发饷：{(int)paySpin.Value} 万钱\n经办风险：{risk}\n实际效果以后端结算为准。";
        }

        officerOption.ItemSelected += _ => RefreshDrillPreview();
        paySpin.ValueChanged += _ => RefreshDrillPreview();
        RefreshDrillPreview();
        AddWestGardenActionButton("发内帑犒军", () => DoWestGardenDrillAction((int)paySpin.Value, GetSelectedOfficerId(officerOption)), disabled: _gameState.PrivateTreasury <= 0);

        AddWestGardenSectionTitle("募兵补军");
        int capacity = Math.Max(0, MaxWestGardenArmySize - _gameState.WestGardenArmy.Size);
        int defaultTroops = capacity > 0 ? Math.Min(2000, capacity) : 1000;
        var troopSpin = new SpinBox
        {
            MinValue = 1000,
            MaxValue = Math.Max(1000, capacity),
            Step = 1000,
            Value = defaultTroops,
            Editable = capacity > 0,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        StylePopupInput(troopSpin, PopupSkin.WestGarden);
        _westGardenActionsVBox.AddChild(troopSpin);

        var recruitPreview = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _westGardenActionsVBox.AddChild(recruitPreview);

        void RefreshRecruitPreview(double value)
        {
            if (capacity <= 0)
            {
                recruitPreview.Text = "西园新军已满编。";
                return;
            }
            int troops = Math.Min((int)value, capacity);
            int batches = troops / 1000;
            recruitPreview.Text = $"预计征发：{troops} 人\n国库 -{batches * 300} 万，民心 -{batches}，士气 -{batches}。";
        }

        troopSpin.ValueChanged += RefreshRecruitPreview;
        RefreshRecruitPreview(troopSpin.Value);
        AddWestGardenActionButton("下诏募兵", () => DoWestGardenRecruitAction((int)troopSpin.Value), disabled: capacity <= 0);

        AddWestGardenSectionTitle("召见校尉");
        AddWestGardenActionButton("召见蹇硕", () => ShowMinisterDetails("jian_shuo"));
        AddWestGardenActionButton("召见曹操", () => ShowMinisterDetails("cao_cao"));
        AddWestGardenActionButton("召见张让", () => ShowMinisterDetails("zhang_rang"));
    }

    private void AddWestGardenSectionTitle(string text)
    {
        if (_westGardenActionsVBox == null) return;
        var label = new Label();
        label.Text = $"【{text}】";
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.AddThemeColorOverride("font_color", GetPopupTitleColor(PopupSkin.WestGarden));
        _westGardenActionsVBox.AddChild(label);
    }

    private void AddWestGardenActionButton(string text, Action action, bool disabled = false)
    {
        if (_westGardenActionsVBox == null) return;
        var button = new Button();
        button.Text = text;
        button.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        button.Disabled = disabled;
        StyleSceneActionButton(button, ActionButtonSkin.WestGarden);
        button.Pressed += action;
        _westGardenActionsVBox.AddChild(button);
    }

    private void AddOfficerOption(OptionButton option, string ministerId)
    {
        if (_gameState == null || !_gameState.Npcs.TryGetValue(ministerId, out var npc)) return;
        int index = option.ItemCount;
        option.AddItem($"{npc.Name}｜贪腐{npc.Corruption}");
        option.SetItemMetadata(index, ministerId);
    }

    private static string GetSelectedOfficerId(OptionButton option)
    {
        if (option.ItemCount == 0) return "jian_shuo";
        int selected = option.Selected >= 0 ? option.Selected : 0;
        return option.GetItemMetadata(selected).AsString();
    }

    private string GetOfficerCorruptionRisk(string officerId)
    {
        if (_gameState == null || !_gameState.Npcs.TryGetValue(officerId, out var npc)) return "未知";
        string level = npc.Corruption >= 70 ? "高" : npc.Corruption >= 40 ? "中" : "低";
        return $"{level}（{npc.Name} 贪腐 {npc.Corruption}/100）";
    }

    private void DoWestGardenSellOfficeAction()
    {
        if (_gameEngine == null) return;
        try
        {
            var result = _gameEngine.ExecuteQuickAction("sell_office");
            ShowStoryReportPopup("西园回奏", result.StoryText, PopupSkin.WestGarden);
            UpdateUI();
            RefreshWestGardenPanel();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
            ShowStoryReportPopup("西园军务未成", $"【西园军务未成】\n\n{ex.Message}", PopupSkin.Warning);
        }
    }

    private void DoWestGardenDrillAction(int amount, string officerId)
    {
        if (_gameEngine == null) return;
        try
        {
            var result = _gameEngine.ExecuteDrillArmyActionWithOfficer(amount, officerId);
            ShowStoryReportPopup("西园军报", result.StoryText, PopupSkin.WestGarden);
            UpdateUI();
            RefreshWestGardenPanel();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
            ShowStoryReportPopup("阅兵未成", $"【阅兵未成】\n\n{ex.Message}", PopupSkin.Warning);
        }
    }

    private void DoWestGardenRecruitAction(int troops)
    {
        if (_gameEngine == null) return;
        try
        {
            var result = _gameEngine.ExecuteRaiseWestGardenTroopsAction(troops);
            ShowStoryReportPopup("西园军报", result.StoryText, PopupSkin.WestGarden);
            UpdateUI();
            RefreshWestGardenPanel();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
            ShowStoryReportPopup("募兵未成", $"【募兵未成】\n\n{ex.Message}", PopupSkin.Warning);
        }
    }

    private static void ClearWestGardenChildren(VBoxContainer box)
    {
        foreach (Node child in box.GetChildren()) child.QueueFree();
    }
}
