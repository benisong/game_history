using Godot;

namespace DonghanFrontend;

public partial class MainScene : Control
{
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

    private void ShowTravelOverlay()
    {
        var panel = new Panel();
        panel.Name = "TravelChoicePopup";
        panel.Visible = false;
        ConfigureCenteredPopupPanel(panel, PopupSkin.Warning, new Vector2(720, 470));

        var root = new VBoxContainer();
        SetFullRect(root);
        root.OffsetLeft = 24;
        root.OffsetTop = 20;
        root.OffsetRight = -24;
        root.OffsetBottom = -20;
        root.AddThemeConstantOverride("separation", 12);
        panel.AddChild(root);

        var title = new Label { Text = "龙辇巡幸 · 三处移驾" };
        StylePopupTitle(title, PopupSkin.Warning);
        root.AddChild(title);

        var desc = new Label
        {
            Text = "请陛下定夺今日驻跸之所。宣政殿总揽朝纲，温德殿调养龙体，西园精舍掌私库与新军。"
        };
        StylePopupBodyText(desc, PopupSkin.Warning);
        root.AddChild(desc);

        var cards = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        cards.AddThemeConstantOverride("separation", 12);
        root.AddChild(cards);

        AddTravelDestinationCard(cards,
            "宣政殿",
            "朝会理政",
            "批阅天下奏折、召见群臣、处置赈灾与地方任免。",
            "适合：总揽大权、稳定朝局",
            "起驾宣政殿");

        AddTravelDestinationCard(cards,
            "后宫",
            "温德殿",
            "暂离朝争，调养龙体；后续可扩展妃嫔、皇嗣与内廷事件。",
            "适合：恢复健康、缓冲政务压力",
            "巡幸温德殿");

        AddTravelDestinationCard(cards,
            "西园",
            "西园精舍",
            "避开外朝耳目，掌控私库、新军、阅兵发饷与募兵补军。",
            "适合：强军、蓄财、重建皇权根基",
            "起驾西园");

        var cancel = new Button
        {
            Text = "龙辇免起",
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
            CustomMinimumSize = new Vector2(180, 40)
        };
        cancel.Pressed += _windowManager.PopWindow;
        root.AddChild(cancel);

        PushTemporaryPopup(panel);
    }

    private void AddTravelDestinationCard(HBoxContainer parent, string destination, string heading, string body, string usage, string buttonText)
    {
        var card = new Panel { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        card.AddThemeStyleboxOverride("panel", CreatePopupInnerPanelStyle(PopupSkin.Warning));
        parent.AddChild(card);

        var box = new VBoxContainer();
        SetFullRect(box);
        box.OffsetLeft = 12;
        box.OffsetTop = 12;
        box.OffsetRight = -12;
        box.OffsetBottom = -12;
        box.AddThemeConstantOverride("separation", 8);
        card.AddChild(box);

        var title = new Label { Text = heading };
        StyleColumnTitle(title, PopupSkin.Warning);
        box.AddChild(title);

        var bodyLabel = new Label
        {
            Text = body,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        StylePopupBodyText(bodyLabel, PopupSkin.Warning);
        box.AddChild(bodyLabel);

        var usageLabel = CreateActionPreviewLabel(PopupSkin.Warning);
        usageLabel.Text = usage;
        box.AddChild(usageLabel);

        var go = new Button
        {
            Text = buttonText,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, 42)
        };
        go.Pressed += () => DoTravel(destination);
        box.AddChild(go);
    }

    private void ShowOpeningOverlay()
    {
        _openingOverlay = new Panel();
        _openingOverlay.Name = "OpeningOverlay";
        _openingOverlay.MouseFilter = Control.MouseFilterEnum.Stop;
        _openingOverlay.FocusMode = Control.FocusModeEnum.All;
        _openingOverlay.ZIndex = 30_000;
        SetFullRect(_openingOverlay);

        var opaqueStyle = new StyleBoxFlat();
        opaqueStyle.BgColor = new Color(0.08f, 0.08f, 0.08f, 1.0f);
        opaqueStyle.SetBorderWidthAll(4);
        opaqueStyle.BorderColor = new Color(0.72f, 0.58f, 0.12f, 1.0f);
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
            _openingOverlay.QueueFree();
        };
        vBox.AddChild(btnConfirm);

        AddChild(_openingOverlay);
        _openingOverlay.GrabFocus();
    }

    private void ShowTravelPrompt(string title, string message, string destination, string confirmText)
    {
        var prompt = new Panel();
        prompt.Name = "TravelPromptPopup";
        prompt.Visible = false;
        prompt.CustomMinimumSize = new Vector2(560, 260);
        prompt.AnchorLeft = 0.5f;
        prompt.AnchorTop = 0.5f;
        prompt.AnchorRight = 0.5f;
        prompt.AnchorBottom = 0.5f;
        prompt.OffsetLeft = -280;
        prompt.OffsetTop = -130;
        prompt.OffsetRight = 280;
        prompt.OffsetBottom = 130;
        prompt.AddThemeStyleboxOverride("panel", CreatePopupPanelStyle(PopupSkin.Warning));

        var box = new VBoxContainer();
        SetFullRect(box);
        box.OffsetLeft = 20;
        box.OffsetTop = 18;
        box.OffsetRight = -20;
        box.OffsetBottom = -18;
        box.AddThemeConstantOverride("separation", 14);
        prompt.AddChild(box);

        var titleLabel = new Label();
        titleLabel.Text = title;
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 21);
        titleLabel.AddThemeColorOverride("font_color", GetPopupTitleColor(PopupSkin.Warning));
        box.AddChild(titleLabel);

        var messageLabel = new Label();
        messageLabel.Text = message;
        messageLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        messageLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        messageLabel.AddThemeColorOverride("font_color", new Color(0.92f, 0.80f, 0.58f, 1.0f));
        box.AddChild(messageLabel);

        var row = new HBoxContainer();
        row.Alignment = BoxContainer.AlignmentMode.Center;
        row.AddThemeConstantOverride("separation", 18);
        box.AddChild(row);

        var confirm = new Button();
        confirm.Text = confirmText;
        confirm.CustomMinimumSize = new Vector2(170, 42);
        confirm.Pressed += () =>
        {
            DoTravel(destination);
            prompt.QueueFree();
        };
        row.AddChild(confirm);

        var cancel = new Button();
        cancel.Text = "暂缓";
        cancel.CustomMinimumSize = new Vector2(120, 42);
        cancel.Pressed += _windowManager.PopWindow;
        row.AddChild(cancel);

        AddChild(prompt);
        _windowManager.PushWindow(prompt);
    }
}
