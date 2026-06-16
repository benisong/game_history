using Godot;

namespace DonghanFrontend;

public partial class MainScene : Control
{
    private void OnPleasureCenserPressed()
    {
        ShowTravelPrompt(
            "【温德殿宣召】",
            "博山炉紫烟初起，黄门伏地奏请：陛下可暂离外朝，移驾温德殿调养龙体。是否即刻巡幸后宫？",
            "后宫",
            "巡幸温德殿");
    }

    private Panel? _openingOverlay;

    private void ShowTravelOverlay()
    {
        var panel = new Panel();
        panel.Name = "TravelChoicePopup";
        panel.Visible = false;
        ConfigureCenteredPopupPanel(panel, PopupSkin.Travel, new Vector2(880, 560));

        var root = new VBoxContainer();
        SetFullRect(root);
        root.OffsetLeft = 24;
        root.OffsetTop = 20;
        root.OffsetRight = -24;
        root.OffsetBottom = -20;
        root.AddThemeConstantOverride("separation", 12);
        panel.AddChild(root);

        var title = new Label { Text = "龙辇巡幸 · 驻跸择所" };
        StylePopupTitle(title, PopupSkin.Travel);
        root.AddChild(title);

        var routeFrame = new PanelContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        routeFrame.AddThemeStyleboxOverride("panel", CreateTravelRouteBannerStyle());
        root.AddChild(routeFrame);

        var routeBox = new VBoxContainer();
        routeBox.AddThemeConstantOverride("separation", 4);
        routeFrame.AddChild(routeBox);

        var routeStrip = new Label
        {
            Text = "宫门黄门执节  ━  宣政殿  ━  温德殿  ━  西园别苑",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        StyleColumnTitle(routeStrip, PopupSkin.Travel);
        routeStrip.AddThemeFontSizeOverride("font_size", 16);
        routeBox.AddChild(routeStrip);

        var desc = new Label
        {
            Text = "黄门执节，羽林夹道。请陛下定夺今日驻跸之所：临朝则百官肃立，入内则炉烟养神，幸西园则私库与新军皆待圣裁。",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        StylePopupBodyText(desc, PopupSkin.Travel);
        routeBox.AddChild(desc);

        var cards = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        cards.AddThemeConstantOverride("separation", 12);
        root.AddChild(cards);

        AddTravelDestinationCard(cards,
            "宣政殿",
            "玉阶临朝",
            "王座居中，群臣趋伏；适合批阅天下奏折、召见重臣、处置赈灾与地方任免。",
            "权力效果：总揽大权、稳定朝局",
            "起驾宣政殿");

        AddTravelDestinationCard(cards,
            "后宫",
            "温德炉烟",
            "朱帘低垂，博山炉紫烟绕殿；暂离朝争，调养龙体，缓冲政务压力。",
            "内廷效果：恢复健康、蓄养精神",
            "巡幸温德殿");

        AddTravelDestinationCard(cards,
            "西园",
            "西园秘营",
            "外朝目光止于宫门；私库、新军、阅兵发饷与募兵补军，皆可在此秘密铺陈。",
            "根基效果：强军、蓄财、重建皇权",
            "起驾西园");

        var cancel = new Button
        {
            Text = "龙辇免起",
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
            CustomMinimumSize = new Vector2(180, 40)
        };
        StyleSceneActionButton(cancel, ActionButtonSkin.Travel);
        cancel.Pressed += _windowManager.PopWindow;
        root.AddChild(cancel);

        PushTemporaryPopup(panel);
    }

    private void AddTravelDestinationCard(HBoxContainer parent, string destination, string heading, string body, string usage, string buttonText)
    {
        var card = new Panel { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        card.AddThemeStyleboxOverride("panel", CreateTravelDestinationCardStyle(destination));
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
        StyleColumnTitle(title, PopupSkin.Travel);
        title.AddThemeFontSizeOverride("font_size", 20);
        box.AddChild(title);

        var sigilFrame = new PanelContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, CustomMinimumSize = new Vector2(0, 66) };
        sigilFrame.AddThemeStyleboxOverride("panel", CreateTravelSigilFrameStyle(destination));
        box.AddChild(sigilFrame);

        var sigil = new Label
        {
            Text = GetTravelDestinationSigil(destination),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        sigil.AddThemeFontSizeOverride("font_size", 24);
        sigil.AddThemeColorOverride("font_color", GetPopupTitleColor(PopupSkin.Travel).Lightened(0.13f));
        sigilFrame.AddChild(sigil);

        var bodyLabel = new Label
        {
            Text = body,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        StylePopupBodyText(bodyLabel, PopupSkin.Travel);
        box.AddChild(bodyLabel);

        var usageLabel = CreateActionPreviewLabel(PopupSkin.Travel);
        usageLabel.Text = usage;
        usageLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        box.AddChild(usageLabel);

        var go = new Button
        {
            Text = buttonText,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, 42)
        };
        StyleSceneActionButton(go, ActionButtonSkin.Travel);
        go.Pressed += () => DoTravel(destination);
        box.AddChild(go);
    }

    private static string GetTravelDestinationSigil(string destination)
    {
        return destination switch
        {
            "宣政殿" => "▰ 御座 · 百官 ▰",
            "后宫" => "✦ 博山炉烟 ✦",
            "西园" => "◆ 羽林虎符 ◆",
            _ => "◇ 龙辇 ◇"
        };
    }

    private static StyleBoxFlat CreateTravelDestinationCardStyle(string destination)
    {
        var style = CreatePopupInnerPanelStyle(PopupSkin.Travel);
        style.BgColor = destination switch
        {
            "宣政殿" => new Color(0.150f, 0.060f, 0.038f, 1.0f),
            "后宫" => new Color(0.120f, 0.066f, 0.056f, 1.0f),
            "西园" => new Color(0.094f, 0.080f, 0.052f, 1.0f),
            _ => style.BgColor
        };
        style.BorderColor = destination switch
        {
            "宣政殿" => new Color(0.84f, 0.52f, 0.16f, 1.0f),
            "后宫" => new Color(0.72f, 0.38f, 0.28f, 1.0f),
            "西园" => new Color(0.58f, 0.46f, 0.22f, 1.0f),
            _ => style.BorderColor
        };
        style.SetBorderWidthAll(2);
        style.ShadowColor = new Color(0.0f, 0.0f, 0.0f, 0.42f);
        style.ShadowSize = 8;
        return style;
    }

    private static StyleBoxFlat CreateTravelRouteBannerStyle()
    {
        var style = CreatePopupInnerPanelStyle(PopupSkin.Travel);
        style.BgColor = new Color(0.115f, 0.060f, 0.030f, 1.0f);
        style.BorderColor = new Color(0.82f, 0.54f, 0.18f, 1.0f);
        style.SetBorderWidthAll(2);
        style.CornerRadiusTopLeft = 14;
        style.CornerRadiusTopRight = 14;
        style.CornerRadiusBottomLeft = 14;
        style.CornerRadiusBottomRight = 14;
        style.ContentMarginLeft = 16;
        style.ContentMarginRight = 16;
        style.ContentMarginTop = 10;
        style.ContentMarginBottom = 10;
        style.ShadowColor = new Color(0.0f, 0.0f, 0.0f, 0.46f);
        style.ShadowSize = 8;
        return style;
    }

    private static StyleBoxFlat CreateTravelSigilFrameStyle(string destination)
    {
        var style = CreatePopupInnerPanelStyle(PopupSkin.Travel);
        style.BgColor = destination switch
        {
            "宣政殿" => new Color(0.190f, 0.050f, 0.032f, 1.0f),
            "后宫" => new Color(0.145f, 0.067f, 0.060f, 1.0f),
            "西园" => new Color(0.082f, 0.075f, 0.050f, 1.0f),
            _ => style.BgColor
        };
        style.BorderColor = destination switch
        {
            "宣政殿" => new Color(0.90f, 0.58f, 0.18f, 1.0f),
            "后宫" => new Color(0.78f, 0.42f, 0.32f, 1.0f),
            "西园" => new Color(0.62f, 0.50f, 0.26f, 1.0f),
            _ => style.BorderColor
        };
        style.SetBorderWidthAll(2);
        style.CornerRadiusTopLeft = 10;
        style.CornerRadiusTopRight = 10;
        style.CornerRadiusBottomLeft = 10;
        style.CornerRadiusBottomRight = 10;
        style.ContentMarginLeft = 8;
        style.ContentMarginRight = 8;
        style.ContentMarginTop = 6;
        style.ContentMarginBottom = 6;
        return style;
    }

    private void ShowOpeningOverlay()
    {
        _openingOverlay = new Panel();
        _openingOverlay.Name = "OpeningOverlay";
        _openingOverlay.MouseFilter = Control.MouseFilterEnum.Stop;
        _openingOverlay.FocusMode = Control.FocusModeEnum.All;
        _openingOverlay.ZIndex = 4_000;
        SetFullRect(_openingOverlay);

        var backdropStyle = new StyleBoxFlat();
        backdropStyle.BgColor = new Color(0.025f, 0.014f, 0.012f, 1.0f);
        backdropStyle.BorderColor = new Color(0.34f, 0.05f, 0.03f, 1.0f);
        backdropStyle.SetBorderWidthAll(5);
        _openingOverlay.AddThemeStyleboxOverride("panel", backdropStyle);

        var edictPanel = new Panel();
        edictPanel.Name = "OpeningEdictPanel";
        ConfigureCenteredPopupPanel(edictPanel, PopupSkin.Court, new Vector2(760, 470));
        _openingOverlay.AddChild(edictPanel);

        var box = new VBoxContainer();
        SetFullRect(box);
        box.OffsetLeft = 34;
        box.OffsetTop = 28;
        box.OffsetRight = -34;
        box.OffsetBottom = -28;
        box.AddThemeConstantOverride("separation", 16);
        edictPanel.AddChild(box);

        var seal = new Label { Text = "黄巾乱起 · 天子临朝" };
        StylePopupTitle(seal, PopupSkin.Court);
        seal.AddThemeFontSizeOverride("font_size", 27);
        box.AddChild(seal);

        var dateLabel = new Label { Text = "光和七年 · 春" };
        StyleColumnTitle(dateLabel, PopupSkin.Court);
        dateLabel.AddThemeColorOverride("font_color", new Color(0.88f, 0.62f, 0.20f, 1.0f));
        box.AddChild(dateLabel);

        var edictFrame = new Panel { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        edictFrame.AddThemeStyleboxOverride("panel", CreatePopupInnerPanelStyle(PopupSkin.Court));
        box.AddChild(edictFrame);

        var edictText = new RichTextLabel
        {
            BbcodeEnabled = true,
            ScrollActive = false,
            FitContent = true,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        SetFullRect(edictText);
        edictText.OffsetLeft = 18;
        edictText.OffsetTop = 14;
        edictText.OffsetRight = -18;
        edictText.OffsetBottom = -14;
        edictText.Text = "[center][font_size=18][color=#f0c85a]苍天已死，黄天当立。岁在甲子，天下大吉。[/color][/font_size][/center]\n\n" +
                         "[color=#e0c48c]张角妖术惑众，巨鹿黄巾并起；外戚何进拥兵坐镇，十常侍张让把持禁中。[/color]\n" +
                         "[color=#e0c48c]汉室倾颓，累卵之危。天下百姓，倒悬之急。[/color]\n\n" +
                         "[center][color=#f0c85a]陛下，大汉八百载基业与十三州舆图，将自今日重入御笔。[/color][/center]";
        edictFrame.AddChild(edictText);

        var warning = CreateActionPreviewLabel(PopupSkin.Court);
        warning.HorizontalAlignment = HorizontalAlignment.Center;
        warning.Text = "开局态势：皇权衰微｜国库窘迫｜民心跌破活命红线｜黄巾将起";
        box.AddChild(warning);

        var btnConfirm = new Button
        {
            Text = "临朝理政",
            CustomMinimumSize = new Vector2(260, 46),
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
        };
        StyleSceneActionButton(btnConfirm, ActionButtonSkin.Court);
        btnConfirm.Pressed += () =>
        {
            _openingOverlay.ReleaseFocus();
            _openingOverlay.QueueFree();
        };
        box.AddChild(btnConfirm);

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
        StyleSceneActionButton(confirm, ActionButtonSkin.Warning);
        confirm.Pressed += () =>
        {
            DoTravel(destination);
        };
        row.AddChild(confirm);

        var cancel = new Button();
        cancel.Text = "暂缓";
        cancel.CustomMinimumSize = new Vector2(120, 42);
        StyleSceneActionButton(cancel, ActionButtonSkin.Warning);
        cancel.Pressed += _windowManager.PopWindow;
        row.AddChild(cancel);

        PushTemporaryPopup(prompt);
    }
}
