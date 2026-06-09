using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DonghanEngine.Core;

namespace DonghanFrontend;

public partial class MainScene : Control
{
    private Panel? _courtPopup;
    private Label? _courtStageLabel;
    private VBoxContainer? _courtOfficialsVBox;
    private VBoxContainer? _courtTopicsVBox;
    private RichTextLabel? _courtDebateLabel;
    private VBoxContainer? _courtDecisionsVBox;
    private VBoxContainer? _courtFreeEdictVBox;
    private LineEdit? _courtInput;
    private readonly List<CourtTopicViewModel> _courtTopics = new();

    private void InitializeCourtPanel()
    {
        _courtTopics.Clear();
        _courtTopics.AddRange(CreateDefaultCourtTopics());

        _courtPopup = new Panel();
        _courtPopup.Name = "CourtPopup";
        _courtPopup.Visible = false;
        _courtPopup.CustomMinimumSize = new Vector2(1100, 640);
        _courtPopup.AnchorLeft = 0.5f;
        _courtPopup.AnchorTop = 0.5f;
        _courtPopup.AnchorRight = 0.5f;
        _courtPopup.AnchorBottom = 0.5f;
        _courtPopup.OffsetLeft = -550;
        _courtPopup.OffsetTop = -320;
        _courtPopup.OffsetRight = 550;
        _courtPopup.OffsetBottom = 320;
        _courtPopup.AddThemeStyleboxOverride("panel", CreatePopupPanelStyle(PopupSkin.Court));

        var root = new VBoxContainer();
        SetFullRect(root);
        root.OffsetLeft = 18;
        root.OffsetTop = 16;
        root.OffsetRight = -18;
        root.OffsetBottom = -16;
        root.AddThemeConstantOverride("separation", 12);
        _courtPopup.AddChild(root);

        BuildCourtHeader(root);
        BuildCourtBody(root);
        AddChild(_courtPopup);
    }

    private void ShowTravelToCourtPrompt()
    {
        ShowTravelPrompt(
            "【黄门急奏】",
            "陛下尚未驾临宣政殿，百官不可无端入朝。是否即刻起驾宣政殿，开朝听政？",
            "宣政殿",
            "起驾宣政殿");
    }

    private void BuildCourtHeader(VBoxContainer root)
    {
        var header = new VBoxContainer();
        header.AddThemeConstantOverride("separation", 3);
        root.AddChild(header);

        var title = new Label();
        title.Text = "宣政殿 · 大朝会";
        StylePopupTitle(title, PopupSkin.Court);
        header.AddChild(title);

        _courtStageLabel = new Label();
        _courtStageLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _courtStageLabel.Text = $"{FormatTimeLabel()}  ·  阶段：择议";
        header.AddChild(_courtStageLabel);
    }

    private void BuildCourtBody(VBoxContainer root)
    {
        var body = new HBoxContainer();
        body.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        body.AddThemeConstantOverride("separation", 12);
        root.AddChild(body);

        _courtOfficialsVBox = CreateCourtColumn(body, "百官班列", 230);

        var middle = CreateCourtColumn(body, "今日廷议", 560, expand: true);
        _courtTopicsVBox = new VBoxContainer();
        _courtTopicsVBox.AddThemeConstantOverride("separation", 8);
        middle.AddChild(_courtTopicsVBox);

        _courtDebateLabel = new RichTextLabel();
        _courtDebateLabel.BbcodeEnabled = true;
        _courtDebateLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _courtDebateLabel.Visible = false;
        middle.AddChild(_courtDebateLabel);

        _courtFreeEdictVBox = new VBoxContainer();
        _courtFreeEdictVBox.Visible = false;
        _courtFreeEdictVBox.AddThemeConstantOverride("separation", 8);
        middle.AddChild(_courtFreeEdictVBox);
        BuildFreeEdictBox();

        _courtDecisionsVBox = CreateCourtColumn(body, "御前裁断", 250);
    }

    private VBoxContainer CreateCourtColumn(HBoxContainer parent, string title, int width, bool expand = false)
    {
        var panel = new Panel();
        panel.CustomMinimumSize = new Vector2(width, 0);
        panel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        panel.SizeFlagsHorizontal = expand ? Control.SizeFlags.ExpandFill : Control.SizeFlags.ShrinkBegin;
        panel.AddThemeStyleboxOverride("panel", CreateCourtInnerPanelStyle());
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
        StyleColumnTitle(label, PopupSkin.Court);
        box.AddChild(label);

        return box;
    }

    private static StyleBoxFlat CreateCourtInnerPanelStyle()
    {
        return CreatePopupInnerPanelStyle(PopupSkin.Court);
    }

    private void OnCourtSealPressed()
    {
        if (_gameState == null) return;
        if (_gameState.CurrentLocation != "宣政殿")
        {
            ShowTravelToCourtPrompt();
            return;
        }

        RenderCourtOfficials();
        RenderCourtTopics();
        RenderCourtDefaultDecisions();
        if (_courtStageLabel != null) _courtStageLabel.Text = $"{FormatTimeLabel()}  ·  阶段：择议";
        if (_courtInput != null) _courtInput.Text = "";
        _courtFreeEdictVBox?.Hide();
        _windowManager.PushWindow(_courtPopup!);
    }

    private void RenderCourtOfficials(string topicId = "")
    {
        if (_courtOfficialsVBox == null || _gameState == null) return;
        ClearChildrenExceptHeader(_courtOfficialsVBox);

        AddCourtFactionLabel("【外戚武臣】");
        AddCourtOfficialButton("he_jin", topicId == "eunuchs" ? "裁抑中官" : "请战");

        AddCourtFactionLabel("【中官近侍】");
        AddCourtOfficialButton("zhang_rang", topicId == "treasury" ? "掌财" : "观望");

        AddCourtFactionLabel("【西园武臣】");
        AddCourtOfficialButton("cao_cao", topicId == "talent" ? "自请任事" : "请任");
        AddCourtOfficialButton("jian_shuo", topicId == "military_readiness" ? "主军" : "侍卫宫禁");
    }

    private void AddCourtFactionLabel(string text)
    {
        if (_courtOfficialsVBox == null) return;
        var label = new Label();
        label.Text = text;
        label.AddThemeColorOverride("font_color", new Color(0.95f, 0.77f, 0.28f, 1.0f));
        _courtOfficialsVBox.AddChild(label);
    }

    private void AddCourtOfficialButton(string ministerId, string attitude)
    {
        if (_courtOfficialsVBox == null || _gameState == null) return;
        if (!_gameState.Npcs.TryGetValue(ministerId, out var npc) || !npc.IsActive) return;

        var button = new Button();
        string external = npc.GovernedProvinceId != null && _gameState.Provinces.TryGetValue(npc.GovernedProvinceId, out var province)
            ? $"【外任{province.Name}】"
            : string.Empty;
        button.Text = $"{npc.Name}  {GetCourtOfficeName(ministerId)}  {attitude}{external}";
        button.Alignment = HorizontalAlignment.Left;
        button.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        StyleSceneActionButton(button, ActionButtonSkin.Court);
        button.Pressed += () => ShowMinisterDetails(ministerId);
        _courtOfficialsVBox.AddChild(button);
    }

    private static string GetCourtOfficeName(string ministerId)
    {
        return ministerId switch
        {
            "he_jin" => "大将军",
            "zhang_rang" => "中常侍",
            "cao_cao" => "典军校尉",
            "jian_shuo" => "上军校尉",
            _ => "朝臣"
        };
    }

    private void RenderCourtTopics()
    {
        if (_courtTopicsVBox == null || _courtDebateLabel == null) return;
        _courtDebateLabel.Hide();
        _courtTopicsVBox.Show();
        _courtFreeEdictVBox?.Hide();
        ClearChildren(_courtTopicsVBox);

        var intro = new Label();
        intro.Text = "今日可议：";
        intro.AddThemeColorOverride("font_color", new Color(0.95f, 0.77f, 0.28f, 1.0f));
        _courtTopicsVBox.AddChild(intro);

        foreach (var topic in _courtTopics)
        {
            var button = new Button();
            button.Text = $"【{topic.Category}】{topic.Title}\n{topic.Summary}";
            button.Alignment = HorizontalAlignment.Left;
            button.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            button.CustomMinimumSize = new Vector2(0, 70);
            button.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            StyleSceneActionButton(button, ActionButtonSkin.Court);
            button.CustomMinimumSize = new Vector2(0, 70);
            button.Pressed += () => SelectCourtTopic(topic);
            _courtTopicsVBox.AddChild(button);
        }
    }

    private void SelectCourtTopic(CourtTopicViewModel topic)
    {
        if (_courtTopicsVBox == null || _courtDebateLabel == null) return;

        RenderCourtOfficials(topic.Id);
        _courtTopicsVBox.Hide();
        _courtFreeEdictVBox?.Hide();
        _courtDebateLabel.Show();
        if (_courtStageLabel != null) _courtStageLabel.Text = $"{FormatTimeLabel()}  ·  阶段：群臣奏对";

        string text = $"[b]【议题】{topic.Title}[/b]\n[color=gray]{topic.Summary}[/color]\n\n";
        foreach (var speech in topic.Speeches)
        {
            text += $"[color=yellow]{speech.MinisterName}[/color]（{speech.Faction}）：\n“{speech.Speech}”\n[color=gray]倾向：{speech.Attitude}[/color]\n\n";
        }
        _courtDebateLabel.Text = text;
        RenderCourtDecisions(topic);
    }

    private void RenderCourtDefaultDecisions()
    {
        if (_courtDecisionsVBox == null) return;
        ClearChildrenExceptHeader(_courtDecisionsVBox);
        AddDecisionButton("亲拟圣旨", "展开自由输入，直接口召百官。", ShowFreeEdictBox);
        AddDecisionButton("退朝", "关闭朝会，回到御案。", CloseCourtSession);
    }

    private void RenderCourtDecisions(CourtTopicViewModel topic)
    {
        if (_courtDecisionsVBox == null) return;
        ClearChildrenExceptHeader(_courtDecisionsVBox);

        foreach (var decision in topic.Decisions)
        {
            AddDecisionButton(decision.Label, decision.Hint, () => ExecuteCourtDecision(decision));
        }

        AddDecisionButton("亲拟圣旨", "展开自由输入，乾纲独断。", ShowFreeEdictBox);
        AddDecisionButton("退朝", "结束本次朝议。", CloseCourtSession);
    }

    private void AddDecisionButton(string label, string hint, Action callback)
    {
        if (_courtDecisionsVBox == null) return;
        var button = new Button();
        button.Text = label;
        button.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        StyleSceneActionButton(button, ActionButtonSkin.Court);
        button.Pressed += callback;
        _courtDecisionsVBox.AddChild(button);

        if (!string.IsNullOrWhiteSpace(hint))
        {
            var hintLabel = new Label();
            hintLabel.Text = hint;
            hintLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            hintLabel.AddThemeColorOverride("font_color", new Color(0.70f, 0.66f, 0.58f, 1.0f));
            _courtDecisionsVBox.AddChild(hintLabel);
        }
    }

    private void BuildFreeEdictBox()
    {
        if (_courtFreeEdictVBox == null) return;
        ClearChildren(_courtFreeEdictVBox);

        var title = new Label();
        title.Text = "【亲拟圣旨】";
        title.AddThemeColorOverride("font_color", new Color(0.95f, 0.77f, 0.28f, 1.0f));
        _courtFreeEdictVBox.AddChild(title);

        _courtInput = new LineEdit();
        _courtInput.PlaceholderText = "如：重赏何进，命其整军备寇";
        _courtFreeEdictVBox.AddChild(_courtInput);

        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 10);
        _courtFreeEdictVBox.AddChild(row);

        var confirm = new Button();
        confirm.Text = "宣旨起大朝仪";
        StyleSceneActionButton(confirm, ActionButtonSkin.Court);
        confirm.Pressed += OnConfirmCourtAssembly;
        row.AddChild(confirm);

        var collapse = new Button();
        collapse.Text = "收起";
        StyleSceneActionButton(collapse, ActionButtonSkin.Court);
        collapse.Pressed += () => _courtFreeEdictVBox.Hide();
        row.AddChild(collapse);
    }

    private void ShowFreeEdictBox()
    {
        _courtFreeEdictVBox?.Show();
        _courtInput?.GrabFocus();
    }

    private void ExecuteCourtDecision(CourtDecisionViewModel decision)
    {
        switch (decision.Id)
        {
            case "intel":
                _windowManager.PopWindow();
                OnIntelTokenPressed();
                break;
            case "travel_garden":
                DoTravel("西园");
                break;
            case "show_cao":
                ShowMinisterDetails("cao_cao");
                break;
            case "show_jian":
                ShowMinisterDetails("jian_shuo");
                break;
            case "back_topics":
                RenderCourtTopics();
                RenderCourtDefaultDecisions();
                if (_courtStageLabel != null) _courtStageLabel.Text = $"{FormatTimeLabel()}  ·  阶段：择议";
                break;
            case "free_edict":
                ShowFreeEdictBox();
                break;
            default:
                RunCourtCommand(decision.Id);
                break;
        }
    }

    private void RunCourtCommand(string command)
    {
        if (_courtInput == null) return;
        _courtInput.Text = command;
        OnConfirmCourtAssembly();
    }

    private void CloseCourtSession()
    {
        _windowManager.PopWindow();
        ShowStoryReportPopup("退朝", "【退朝】\n\n静鞭三响，百官次第退出宣政殿。陛下暂收朝议，天下仍在暗流中流转。", PopupSkin.Court);
    }

    private async void OnConfirmCourtAssembly()
    {
        string txt = _courtInput!.Text;
        if (string.IsNullOrWhiteSpace(txt)) return;

        _windowManager.PopWindow();

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

        var result = await _gameEngine!.ProcessPlayerTurnAsync(txt);
        ShowStoryReportPopup("朝议回奏", result.StoryText, PopupSkin.Court);
        UpdateUI();
    }

    private static void ClearChildren(VBoxContainer box)
    {
        foreach (Node child in box.GetChildren()) child.QueueFree();
    }

    private static void ClearChildrenExceptHeader(VBoxContainer box)
    {
        var children = box.GetChildren();
        for (int i = 1; i < children.Count; i++)
        {
            children[i].QueueFree();
        }
    }

}
