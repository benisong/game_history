using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
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
    private ButtonGroup? _hostButtonGroup;
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

        // P2-2 主持人选择条：玩家点选 4 名主要 NPC 之一为主持人，影响 MockScheduler 发言顺序
        BuildCourtHostSelector(header);
    }

    private void BuildCourtHostSelector(VBoxContainer parent)
    {
        var hostRow = new HBoxContainer();
        hostRow.AddThemeConstantOverride("separation", 6);
        hostRow.Alignment = BoxContainer.AlignmentMode.Center;

        var hostLabel = new Label();
        hostLabel.Text = "主持人：";
        hostLabel.AddThemeColorOverride("font_color", new Color(0.95f, 0.77f, 0.28f, 1.0f));
        hostRow.AddChild(hostLabel);

        _hostButtonGroup = new ButtonGroup();
        var currentHost = _gameEngine?.ActiveOfficerId ?? "he_jin";
        foreach (var (id, name) in new (string Id, string Name)[]
        {
            ("cao_cao", "曹操"),
            ("he_jin", "何进"),
            ("zhang_rang", "张让"),
            ("jian_shuo", "蹇硕")
        })
        {
            var btn = new Button();
            btn.Text = name;
            btn.ToggleMode = true;
            btn.CustomMinimumSize = new Vector2(80, 32);
            btn.ButtonGroup = _hostButtonGroup;
            btn.ButtonPressed = (currentHost == id);
            StyleSceneActionButton(btn, ActionButtonSkin.Court);
            string capturedId = id; // 闭包捕获
            btn.Pressed += () => OnHostSelected(capturedId);
            hostRow.AddChild(btn);
        }

        parent.AddChild(hostRow);
    }

    private void OnHostSelected(string hostId)
    {
        if (_gameEngine == null) return;
        _gameEngine.ActiveOfficerId = hostId;
    }

    private void BuildCourtBody(VBoxContainer root)
    {
        var body = new HBoxContainer();
        body.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        body.AddThemeConstantOverride("separation", 12);
        root.AddChild(body);

        _courtOfficialsVBox = CreateScrollableCourtColumn(body, "百官班列", 230);

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
        var panel = CreateCourtColumnPanel(parent, width, expand);

        var box = CreateCourtColumnBox(panel);
        AddCourtColumnTitle(box, title);

        return box;
    }

    private VBoxContainer CreateScrollableCourtColumn(HBoxContainer parent, string title, int width, bool expand = false)
    {
        var panel = CreateCourtColumnPanel(parent, width, expand);

        var box = CreateCourtColumnBox(panel);
        AddCourtColumnTitle(box, title);

        var scroll = new ScrollContainer();
        scroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        scroll.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        scroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        scroll.VerticalScrollMode = ScrollContainer.ScrollMode.Auto;
        box.AddChild(scroll);

        var content = new VBoxContainer();
        content.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        content.AddThemeConstantOverride("separation", 7);
        scroll.AddChild(content);

        return content;
    }

    private Panel CreateCourtColumnPanel(HBoxContainer parent, int width, bool expand)
    {
        var panel = new Panel();
        panel.CustomMinimumSize = new Vector2(width, 0);
        panel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        panel.SizeFlagsHorizontal = expand ? Control.SizeFlags.ExpandFill : Control.SizeFlags.ShrinkBegin;
        panel.AddThemeStyleboxOverride("panel", CreateCourtInnerPanelStyle());
        parent.AddChild(panel);
        return panel;
    }

    private static VBoxContainer CreateCourtColumnBox(Panel panel)
    {
        var box = new VBoxContainer();
        SetFullRect(box);
        box.OffsetLeft = 12;
        box.OffsetTop = 10;
        box.OffsetRight = -12;
        box.OffsetBottom = -10;
        box.AddThemeConstantOverride("separation", 9);
        panel.AddChild(box);
        return box;
    }

    private void AddCourtColumnTitle(VBoxContainer box, string title)
    {
        var label = new Label();
        label.Text = title;
        StyleColumnTitle(label, PopupSkin.Court);
        box.AddChild(label);
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

        _ = OpenCourtSessionWithRitualAsync();
    }

    private async Task OpenCourtSessionWithRitualAsync()
    {
        await ShowCourtRitualSequenceAsync(BuildCourtOpeningRitualSlides(), runPreload: true);

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
        ClearChildren(_courtOfficialsVBox);

        var activeCourtNpcs = _gameState.Npcs.Values
            .Where(n => n.IsActive && !n.IsHostile)
            .OrderBy(GetCourtFactionOrder)
            .ThenByDescending(n => n.Power)
            .ThenByDescending(n => n.TitleTier)
            .ThenBy(n => n.Name)
            .GroupBy(n => GetCourtFactionLabel(n.Faction));

        foreach (var group in activeCourtNpcs)
        {
            AddCourtFactionLabel(group.Key);
            foreach (var npc in group)
            {
                AddCourtOfficialButton(npc.Id, GetCourtAttitude(npc, topicId));
            }
        }
    }

    private static int GetCourtFactionOrder(NpcState npc)
    {
        return npc.Faction switch
        {
            "外戚派" => 0,
            "阉党派" => 1,
            "西园亲军" => 2,
            "清流派" => 3,
            "地方州牧" => 4,
            "割据军阀" => 5,
            _ => 6
        };
    }

    private static string GetCourtFactionLabel(string faction)
    {
        return faction switch
        {
            "外戚派" => "【外戚武臣】",
            "阉党派" => "【中官近侍】",
            "西园亲军" => "【西园亲军】",
            "清流派" => "【清流名臣】",
            "地方州牧" => "【外镇州牧】",
            "割据军阀" => "【边军豪强】",
            _ => $"【{faction}】"
        };
    }

    private static string GetCourtAttitude(NpcState npc, string topicId)
    {
        return topicId switch
        {
            "eunuchs" when npc.Faction == "阉党派" => "辩护",
            "eunuchs" when npc.Faction == "外戚派" || npc.Faction == "清流派" => "裁抑中官",
            "treasury" when npc.Corruption >= 70 => "掌财",
            "treasury" when npc.Traits.Contains(TraitNames.QingZhengLianJie) => "请清帑藏",
            "military_readiness" when npc.Leadership >= 70 || npc.Martial >= 70 => "主军",
            "talent" when npc.Politics >= 75 || npc.Leadership >= 75 => "自请任事",
            _ when npc.GovernedProvinceId != null => "外任待召",
            _ when npc.Ambition >= 75 => "观望结势",
            _ when npc.Power >= 60 => "持重观望",
            _ => "候旨"
        };
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
        if (!_gameState.Npcs.TryGetValue(ministerId, out var npc) || !npc.IsActive || npc.IsHostile) return;

        var button = new Button();
        string external = npc.GovernedProvinceId != null && _gameState.Provinces.TryGetValue(npc.GovernedProvinceId, out var province)
            ? $"【外任{province.Name}】"
            : string.Empty;
        button.Text = $"{npc.Name}  {npc.Title}\n{attitude}{external}";
        button.Alignment = HorizontalAlignment.Left;
        button.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        button.CustomMinimumSize = new Vector2(0, 54);
        button.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        StyleSceneActionButton(button, ActionButtonSkin.Court);
        button.Pressed += () => ShowMinisterDetails(ministerId);
        _courtOfficialsVBox.AddChild(button);
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
        StylePopupInput(_courtInput, PopupSkin.Court);
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

    private async void CloseCourtSession()
    {
        _windowManager.PopWindow();
        await ShowCourtRitualSequenceAsync(BuildCourtDismissalRitualSlides(), runPreload: true);
        ShowCourtReportPopup("退朝回奏", "【退朝 · 百官散班】\n\n静鞭三响，百官次第退出宣政殿。尚书台收起奏牍，中官与外戚各自低语退下。\n\n本次朝议暂歇，天下州郡、国库民心与朝中党争仍将在旬日之间继续发酵。");
    }

    private async void OnConfirmCourtAssembly()
    {
        string txt = _courtInput!.Text;
        if (string.IsNullOrWhiteSpace(txt)) return;

        _windowManager.PopWindow();

        var resultTask = _gameEngine!.ProcessPlayerTurnAsync(txt);
        await ShowCourtRitualSequenceAsync(BuildCourtEdictRitualSlides(txt), preloadTask: resultTask);
        var result = await resultTask;
        ShowCourtReportPopup("朝议圣裁回奏", result.StoryText);
        UpdateUI();
    }

    private sealed class CourtRitualSlide
    {
        public string Title { get; init; } = string.Empty;
        public string SceneName { get; init; } = string.Empty;
        public string ImagePath { get; init; } = string.Empty;
        public string ImageText { get; init; } = string.Empty;
        public string Caption { get; init; } = string.Empty;
        public string PreloadHint { get; init; } = string.Empty;
        public float Seconds { get; init; } = 2.6f;
    }

    private CourtRitualSlide[] BuildCourtOpeningRitualSlides()
    {
        return new[]
        {
            new CourtRitualSlide
            {
                Title = "大朝仪 · 宣读上朝",
                SceneName = "宫门宣召",
                ImagePath = "res://Assets/UI/court_ritual/announce_court.png",
                ImageText = "宣政殿外，黄门持诏立于丹陛之下。朱门缓启，金线御榜映出晨色。",
                Caption = "内侍高唱：有事早奏，无事退朝。百官闻诏，自外朝趋入。",
                PreloadHint = "天机演算：预载朝会议题、百官立场与本旬奏折摘要。",
                Seconds = 2.8f
            },
            new CourtRitualSlide
            {
                Title = "大朝仪 · 响鞭肃班",
                SceneName = "静鞭三响",
                ImagePath = "res://Assets/UI/court_ritual/whip_silence.png",
                ImageText = "殿前静鞭破空，赤黑宫墙间回声如雷。羽林宿卫执戟分列，群臣顿止。",
                Caption = "一鞭止语，二鞭整冠，三鞭肃班。宣政殿内只余甲叶与玉佩轻响。",
                PreloadHint = "天机演算：整理可行动作、风险提示与派系反应缓存。",
                Seconds = 2.6f
            },
            new CourtRitualSlide
            {
                Title = "大朝仪 · 百官入朝",
                SceneName = "群臣就列",
                ImagePath = "res://Assets/UI/court_ritual/officials_enter.png",
                ImageText = "高处御座隐于冕旒之后，黑金朱红的纵深殿宇中，百官剪影层层俯伏。",
                Caption = "外戚、中官、西园校尉各归班列；天下奏牍，尽待天子一言。",
                PreloadHint = "天机演算：完成预处理，等待陛下临朝裁断。",
                Seconds = 3.0f
            }
        };
    }

    private CourtRitualSlide[] BuildCourtEdictRitualSlides(string edictText)
    {
        return new[]
        {
            new CourtRitualSlide
            {
                Title = "大朝仪 · 钟磬齐鸣",
                SceneName = "天子临殿",
                ImageText = "钟磬齐鸣，丹陛下金声层叠。宿卫亲军移戟开道，御座前灯火如鳞。",
                Caption = "黄门高唱：天子登临——百官跪迎——！",
                PreloadHint = "天机演算：提交圣旨，后台推演朝臣应对。",
                Seconds = 2.4f
            },
            new CourtRitualSlide
            {
                Title = "大朝仪 · 冕旒垂光",
                SceneName = "龙威压殿",
                ImageText = "陛下御带冕旒，龙袍玄赤相间。玉旒遮面，殿中百官不敢仰视。",
                Caption = "皇权虽衰，礼制犹在；一殿沉默，皆等御笔落下。",
                PreloadHint = "天机演算：生成群臣奏对、规则结算与叙事回奏。",
                Seconds = 2.4f
            },
            new CourtRitualSlide
            {
                Title = "大朝仪 · 宣旨群辩",
                SceneName = "黄绢展开",
                ImageText = $"内侍缓缓展开黄绢，朱砂御印压住殿中暗流。\n\n『{edictText}』",
                Caption = "众卿平身。圣旨既出，群臣或附和，或观望，或暗自盘算。",
                PreloadHint = "天机演算中。",
                Seconds = 2.8f
            }
        };
    }

    private CourtRitualSlide[] BuildCourtDismissalRitualSlides()
    {
        return new[]
        {
            new CourtRitualSlide
            {
                Title = "退朝仪 · 静鞭再响",
                SceneName = "收议",
                ImagePath = "res://Assets/UI/court_ritual/dismiss_whip.png",
                ImageText = "殿前静鞭再响，金阶上的争辩戛然而止。内侍收起奏牍，御案灯影微摇。",
                Caption = "今日朝议暂歇，未决之事仍随百官袖中暗流带出宣政殿。",
                PreloadHint = "天机演算：归档朝会记录，更新派系记忆。",
                Seconds = 2.4f
            },
            new CourtRitualSlide
            {
                Title = "退朝仪 · 百官退出",
                SceneName = "散班",
                ImagePath = "res://Assets/UI/court_ritual/officials_leave.png",
                ImageText = "百官鱼贯退下，外戚与中官各自低语。朱门半掩，殿外天色晦暗。",
                Caption = "宣政殿重归寂静；天下十三州，仍有烽烟与饥民等待下一道诏令。",
                PreloadHint = "天机演算：预热下一旬情报、奏折与地方风险摘要。",
                Seconds = 2.8f
            }
        };
    }

    private async Task ShowCourtRitualSequenceAsync(CourtRitualSlide[] slides, bool runPreload = false, Task? preloadTask = null)
    {
        if (slides.Length == 0) return;

        _isUnskippableTransitionActive = true;
        Task? backgroundWork = preloadTask ?? (runPreload ? SimulateCourtAiPreloadAsync() : null);

        EnsureCourtRitualOverlay();
        _courtRitualOverlay!.Show();
        _courtRitualOverlay.GrabFocus();

        if (_transitionMask != null) _transitionMask.Hide();

        foreach (var slide in slides)
        {
            RenderCourtRitualSlide(slide);
            await ToSignal(GetTree().CreateTimer(slide.Seconds), "timeout");
        }

        if (backgroundWork != null && !backgroundWork.IsCompleted)
        {
            RenderCourtRitualWaitingSlide(slides[^1]);
            await backgroundWork;
        }

        _courtRitualOverlay.Hide();
        _isUnskippableTransitionActive = false;
    }

    private async Task SimulateCourtAiPreloadAsync()
    {
        await ToSignal(GetTree().CreateTimer(0.8f), "timeout");
    }

    private void EnsureCourtRitualOverlay()
    {
        if (_courtRitualOverlay != null) return;

        var overlay = new Panel();
        overlay.Name = "CourtRitualOverlay";
        overlay.MouseFilter = Control.MouseFilterEnum.Stop;
        overlay.FocusMode = Control.FocusModeEnum.All;
        overlay.ZIndex = 3_000;
        SetFullRect(overlay);
        overlay.AddThemeStyleboxOverride("panel", CreateCourtRitualBackdropStyle());

        var root = new VBoxContainer { Name = "RitualRoot" };
        SetFullRect(root);
        root.OffsetLeft = 72;
        root.OffsetTop = 46;
        root.OffsetRight = -72;
        root.OffsetBottom = -46;
        root.AddThemeConstantOverride("separation", 18);
        overlay.AddChild(root);

        var title = new Label { Name = "RitualTitle" };
        StylePopupTitle(title, PopupSkin.Court);
        title.AddThemeFontSizeOverride("font_size", 28);
        root.AddChild(title);
        _ritualTitleLabel = title; // P1-B2: 缓存引用避免 ! 强制解引用

        var picture = new Panel { Name = "RitualPicture", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        picture.AddThemeStyleboxOverride("panel", CreateCourtRitualPictureStyle());
        root.AddChild(picture);

        var artwork = new TextureRect
        {
            Name = "RitualArtwork",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
            Modulate = new Color(0.92f, 0.78f, 0.52f, 0.92f),
            ZIndex = 0
        };
        SetFullRect(artwork);
        picture.AddChild(artwork);
        _ritualArtworkRect = artwork; // P1-B2: 缓存引用

        var artworkWash = new ColorRect
        {
            Name = "RitualArtworkWash",
            Color = new Color(0.02f, 0.005f, 0.0f, 0.38f),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            ZIndex = 1
        };
        SetFullRect(artworkWash);
        picture.AddChild(artworkWash);

        var pictureBox = new VBoxContainer { Name = "RitualPictureBox", ZIndex = 2 };
        SetFullRect(pictureBox);
        pictureBox.OffsetLeft = 28;
        pictureBox.OffsetTop = 24;
        pictureBox.OffsetRight = -28;
        pictureBox.OffsetBottom = -24;
        pictureBox.AddThemeConstantOverride("separation", 16);
        picture.AddChild(pictureBox);

        var sceneName = new Label { Name = "RitualSceneName" };
        StyleColumnTitle(sceneName, PopupSkin.Court);
        sceneName.AddThemeFontSizeOverride("font_size", 23);
        pictureBox.AddChild(sceneName);
        _ritualSceneNameLabel = sceneName; // P1-B2: 缓存引用

        var imageText = new RichTextLabel
        {
            Name = "RitualImageText",
            BbcodeEnabled = true,
            FitContent = false,
            ScrollActive = false,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        imageText.AddThemeColorOverride("default_color", new Color(0.91f, 0.76f, 0.46f, 1.0f));
        imageText.AddThemeFontSizeOverride("normal_font_size", 21);
        pictureBox.AddChild(imageText);
        _ritualImageTextLabel = imageText; // P1-B2: 缓存引用

        var caption = new Label { Name = "RitualCaption" };
        StylePopupBodyText(caption, PopupSkin.Court);
        caption.HorizontalAlignment = HorizontalAlignment.Center;
        caption.AddThemeFontSizeOverride("font_size", 18);
        root.AddChild(caption);
        _ritualCaptionLabel = caption; // P1-B2: 缓存引用

        var preload = CreateActionPreviewLabel(PopupSkin.Court);
        preload.Name = "RitualPreloadHint";
        preload.HorizontalAlignment = HorizontalAlignment.Center;
        root.AddChild(preload);
        _ritualPreloadHintLabel = preload; // P1-B2: 缓存引用

        var lockLabel = new Label
        {
            Text = "大朝仪进行中：不可快进／不可点击／退出键无效",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        lockLabel.AddThemeColorOverride("font_color", new Color(0.70f, 0.54f, 0.24f, 1.0f));
        lockLabel.AddThemeFontSizeOverride("font_size", 14);
        root.AddChild(lockLabel);

        AddChild(overlay);
        MoveChild(overlay, GetChildCount() - 1);
        overlay.Hide();
        _courtRitualOverlay = overlay;
    }

    private static StyleBoxFlat CreateCourtRitualBackdropStyle()
    {
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.018f, 0.010f, 0.008f, 1.0f);
        style.BorderColor = new Color(0.42f, 0.06f, 0.035f, 1.0f);
        style.SetBorderWidthAll(6);
        return style;
    }

    private static StyleBoxFlat CreateCourtRitualPictureStyle()
    {
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.095f, 0.030f, 0.020f, 1.0f);
        style.BorderColor = new Color(0.74f, 0.46f, 0.12f, 1.0f);
        style.SetBorderWidthAll(3);
        style.CornerRadiusTopLeft = 12;
        style.CornerRadiusTopRight = 12;
        style.CornerRadiusBottomLeft = 12;
        style.CornerRadiusBottomRight = 12;
        style.ContentMarginLeft = 24;
        style.ContentMarginRight = 24;
        style.ContentMarginTop = 20;
        style.ContentMarginBottom = 20;
        style.ShadowColor = new Color(0, 0, 0, 0.85f);
        style.ShadowSize = 20;
        return style;
    }

    private void RenderCourtRitualSlide(CourtRitualSlide slide)
    {
        if (_courtRitualOverlay == null) return;

        // P1-B2 修复：用 BuildCourtRitualOverlay 时缓存的引用，避免 ! 强制解引用
        if (_ritualTitleLabel != null) _ritualTitleLabel.Text = slide.Title;
        if (_ritualSceneNameLabel != null) _ritualSceneNameLabel.Text = slide.SceneName;
        if (_ritualArtworkRect != null)
        {
            _ritualArtworkRect.Texture = string.IsNullOrWhiteSpace(slide.ImagePath) ? null : LoadTextureFromProjectFile(slide.ImagePath);
            if (_ritualArtworkRect.Texture == null && !string.IsNullOrWhiteSpace(slide.ImagePath))
            {
                GD.PrintErr($"朝会仪式图片未载入：{slide.ImagePath}");
            }
        }
        if (_ritualImageTextLabel != null) _ritualImageTextLabel.Text = $"[center]{slide.ImageText}[/center]";
        if (_ritualCaptionLabel != null) _ritualCaptionLabel.Text = slide.Caption;
        if (_ritualPreloadHintLabel != null) _ritualPreloadHintLabel.Text = slide.PreloadHint;
    }

    private void RenderCourtRitualWaitingSlide(CourtRitualSlide slide)
    {
        if (_courtRitualOverlay == null) return;

        // 先用 slide 数据填充各 label，再覆盖 preload hint
        RenderCourtRitualSlide(slide);
        if (_ritualPreloadHintLabel != null) _ritualPreloadHintLabel.Text = "天机演算中。";
    }

    private static void ClearChildren(VBoxContainer box)
    {
        foreach (Node child in box.GetChildren())
        {
            box.RemoveChild(child);
            child.QueueFree();
        }
    }

    private static void ClearChildrenExceptHeader(VBoxContainer box)
    {
        var children = box.GetChildren();
        for (int i = 1; i < children.Count; i++)
        {
            var child = children[i];
            box.RemoveChild(child);
            child.QueueFree();
        }
    }

}
