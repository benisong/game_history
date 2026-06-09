using Godot;
using System;
using DonghanEngine.Core;

namespace DonghanFrontend;

public partial class MainScene : Control
{
    private VBoxContainer? _deskContainer;

    private void InitializeEmperorsDesk()
    {
        var centerPanel = GetNodeOrNull<Panel>("CenterPanel");
        if (centerPanel == null) return;

        // 主界面第一版视觉方案：顶部只放简要时间；主体由四张长方形入口卡片横向占据。
        EnsureMainSceneImageBackground(centerPanel);

        _mainTimeLabel = new Label();
        _mainTimeLabel.Name = "MainTimeLabel";
        _mainTimeLabel.Text = FormatTimeLabel();
        _mainTimeLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _mainTimeLabel.AnchorLeft = 0.5f;
        _mainTimeLabel.AnchorTop = 0.0f;
        _mainTimeLabel.AnchorRight = 0.5f;
        _mainTimeLabel.AnchorBottom = 0.0f;
        _mainTimeLabel.OffsetLeft = -320;
        _mainTimeLabel.OffsetTop = 18;
        _mainTimeLabel.OffsetRight = 320;
        _mainTimeLabel.OffsetBottom = 54;
        _mainTimeLabel.AddThemeFontSizeOverride("font_size", 24);
        _mainTimeLabel.AddThemeColorOverride("font_color", new Color(0.92f, 0.78f, 0.45f, 1.0f));
        centerPanel.AddChild(_mainTimeLabel);
        centerPanel.MoveChild(_mainTimeLabel, 0);

        _deskContainer = new VBoxContainer();
        _deskContainer.Name = "EmperorsDesk";
        _deskContainer.AnchorLeft = 0.5f;
        _deskContainer.AnchorTop = 0.0f;
        _deskContainer.AnchorRight = 0.5f;
        _deskContainer.AnchorBottom = 0.0f;
        _deskContainer.OffsetLeft = -620;
        _deskContainer.OffsetTop = 76;
        _deskContainer.OffsetRight = 620;
        _deskContainer.OffsetBottom = 560;
        _deskContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        _deskContainer.Alignment = BoxContainer.AlignmentMode.Center;
        _deskContainer.CustomMinimumSize = new Vector2(1240, 470);
        _deskContainer.AddThemeConstantOverride("separation", 10);
        centerPanel.AddChild(_deskContainer);
        centerPanel.MoveChild(_deskContainer, 0);

        if (_storyOutput != null)
        {
            _storyOutput.OffsetLeft = 96;
            _storyOutput.OffsetTop = 622;
            _storyOutput.OffsetRight = -96;
            _storyOutput.OffsetBottom = -30;
            _storyOutput.ZIndex = 4;
            _storyOutput.ScrollActive = false;
            _storyOutput.AddThemeColorOverride("default_color", new Color(0.90f, 0.76f, 0.48f, 1.0f));
            EnsureMainAnnualEventFrame(centerPanel, _storyOutput);
            SetAnnualMajorEventBanner();
        }

        var deskRow = new HBoxContainer();
        deskRow.Alignment = BoxContainer.AlignmentMode.Center;
        deskRow.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        deskRow.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        deskRow.AddThemeConstantOverride("separation", 28);
        _deskContainer.AddChild(deskRow);

        _btnCourtSeal = CreateMainEntryCard(
            "朝会",
            "临朝听政",
            "res://Assets/UI/cards/main_court_card.png",
            OnCourtSealPressed);
        _btnIntelToken = CreateMainEntryCard(
            "黄门密札",
            "天下情报",
            "res://Assets/UI/cards/main_intel_card.png",
            OnIntelTokenPressed);
        _btnWestGardenPalace = CreateMainEntryCard(
            "西园",
            "私军密署",
            "res://Assets/UI/cards/main_west_garden_card.png",
            OnWestGardenPressed);
        _btnTravelCarriage = CreateMainEntryCard(
            "起驾",
            "巡幸移驾",
            "res://Assets/UI/cards/main_travel_card.png",
            ShowTravelOverlay);

        deskRow.AddChild(_btnCourtSeal);
        deskRow.AddChild(_btnIntelToken);
        deskRow.AddChild(_btnWestGardenPalace);
        deskRow.AddChild(_btnTravelCarriage);
    }

    private Button CreateMainEntryCard(string title, string subtitle, string texturePath, Action pressedCallback)
    {
        var btn = new Button();
        btn.Text = string.Empty;
        btn.CustomMinimumSize = new Vector2(270, 405);
        btn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        btn.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        btn.MouseFilter = Control.MouseFilterEnum.Stop;
        btn.AddThemeStyleboxOverride("normal", CreateMainEntryCardStyle(new Color(0.10f, 0.055f, 0.035f, 1.0f), 2));
        btn.AddThemeStyleboxOverride("hover", CreateMainEntryCardStyle(new Color(0.16f, 0.095f, 0.045f, 1.0f), 4));
        btn.AddThemeStyleboxOverride("pressed", CreateMainEntryCardStyle(new Color(0.055f, 0.030f, 0.020f, 1.0f), 3));
        btn.AddThemeStyleboxOverride("focus", CreateMainEntryCardStyle(new Color(0.18f, 0.105f, 0.045f, 1.0f), 4));
        btn.Pressed += pressedCallback;

        var art = new TextureRect();
        art.Name = "CardArt";
        art.Texture = LoadTextureFromProjectFile(texturePath);
        art.MouseFilter = Control.MouseFilterEnum.Ignore;
        art.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        art.OffsetLeft = 8;
        art.OffsetTop = 8;
        art.OffsetRight = -8;
        art.OffsetBottom = -8;
        art.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        art.StretchMode = TextureRect.StretchModeEnum.Scale;
        btn.AddChild(art);

        var shade = new ColorRect();
        shade.Name = "BottomShade";
        shade.Color = new Color(0.0f, 0.0f, 0.0f, 0.62f);
        shade.MouseFilter = Control.MouseFilterEnum.Ignore;
        shade.AnchorLeft = 0.0f;
        shade.AnchorTop = 1.0f;
        shade.AnchorRight = 1.0f;
        shade.AnchorBottom = 1.0f;
        shade.OffsetLeft = 12;
        shade.OffsetTop = -86;
        shade.OffsetRight = -12;
        shade.OffsetBottom = -14;
        btn.AddChild(shade);

        var labelBox = new VBoxContainer();
        labelBox.Name = "CardLabelBox";
        labelBox.MouseFilter = Control.MouseFilterEnum.Ignore;
        labelBox.AnchorLeft = 0.0f;
        labelBox.AnchorTop = 1.0f;
        labelBox.AnchorRight = 1.0f;
        labelBox.AnchorBottom = 1.0f;
        labelBox.OffsetLeft = 14;
        labelBox.OffsetTop = -82;
        labelBox.OffsetRight = -14;
        labelBox.OffsetBottom = -16;
        labelBox.Alignment = BoxContainer.AlignmentMode.Center;
        labelBox.AddThemeConstantOverride("separation", 0);
        btn.AddChild(labelBox);

        var titleLabel = new Label();
        titleLabel.Text = title;
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", title.Length >= 4 ? 25 : 30);
        titleLabel.AddThemeColorOverride("font_color", new Color(0.96f, 0.84f, 0.52f, 1.0f));
        labelBox.AddChild(titleLabel);

        var subtitleLabel = new Label();
        subtitleLabel.Text = subtitle;
        subtitleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        subtitleLabel.AddThemeFontSizeOverride("font_size", 15);
        subtitleLabel.AddThemeColorOverride("font_color", new Color(0.72f, 0.58f, 0.34f, 1.0f));
        labelBox.AddChild(subtitleLabel);

        return btn;
    }

    private void SetAnnualMajorEventBanner()
    {
        if (_storyOutput == null) return;

        _storyOutput.Clear();
        _storyOutput.AppendText(BuildAnnualMajorEventText());
    }

    private string BuildAnnualMajorEventText()
    {
        if (_gameState == null) return "本年度特大事件：暂无奏报";

        if (_gameState.Year == 184)
        {
            return "【本年度特大事件】黄巾大起义：巨鹿张角以太平道惑众，十三州民变蜂起，朝廷根基震动。";
        }

        foreach (var province in _gameState.Provinces.Values)
        {
            if (province.IsRebelling)
            {
                string faction = string.IsNullOrWhiteSpace(province.RebelFaction) ? "地方叛军" : province.RebelFaction;
                return $"【本年度特大事件】{province.Name}{faction}作乱：州郡烽火未息，需入黄门密札详察。";
            }
        }

        if (_gameState.PopularSupport <= 15)
        {
            return "【本年度特大事件】天下饥困：民心跌入危局，流民盗贼遍起，需防大旱与民变相激。";
        }

        return "【本年度特大事件】暂无特大灾异奏报。";
    }

    private static StyleBoxFlat CreateMainEntryCardStyle(Color bgColor, int borderWidth)
    {
        var style = new StyleBoxFlat();
        style.BgColor = bgColor;
        style.BorderColor = new Color(0.83f, 0.60f, 0.20f, 1.0f);
        style.SetBorderWidthAll(borderWidth);
        style.CornerRadiusTopLeft = 18;
        style.CornerRadiusTopRight = 18;
        style.CornerRadiusBottomLeft = 18;
        style.CornerRadiusBottomRight = 18;
        style.ContentMarginLeft = 8;
        style.ContentMarginRight = 8;
        style.ContentMarginTop = 8;
        style.ContentMarginBottom = 8;
        return style;
    }

    private Button CreateDeskButton(string text, Action pressedCallback)
    {
        var btn = new Button();
        btn.Text = text;
        btn.CustomMinimumSize = new Vector2(170, 58);
        btn.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        btn.AddThemeFontSizeOverride("font_size", 19);
        btn.AddThemeStyleboxOverride("normal", CreateScrollButtonStyle(new Color(0.47f, 0.33f, 0.14f, 1.0f)));
        btn.AddThemeStyleboxOverride("hover", CreateScrollButtonStyle(new Color(0.58f, 0.42f, 0.18f, 1.0f)));
        btn.AddThemeStyleboxOverride("pressed", CreateScrollButtonStyle(new Color(0.30f, 0.20f, 0.09f, 1.0f)));
        btn.Pressed += pressedCallback;
        return btn;
    }

    private Button CreateCarriageButton(string text, Action pressedCallback)
    {
        var btn = new Button();
        btn.Text = text;
        btn.CustomMinimumSize = new Vector2(170, 58);
        btn.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        btn.AddThemeFontSizeOverride("font_size", 19);
        btn.AddThemeStyleboxOverride("normal", CreateCarriageButtonStyle(new Color(0.20f, 0.13f, 0.07f, 1.0f)));
        btn.AddThemeStyleboxOverride("hover", CreateCarriageButtonStyle(new Color(0.30f, 0.20f, 0.10f, 1.0f)));
        btn.AddThemeStyleboxOverride("pressed", CreateCarriageButtonStyle(new Color(0.12f, 0.08f, 0.04f, 1.0f)));
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

    private static StyleBoxFlat CreateCarriageButtonStyle(Color bgColor)
    {
        var style = new StyleBoxFlat();
        style.BgColor = bgColor;
        style.BorderColor = new Color(0.72f, 0.50f, 0.20f, 1.0f);
        style.SetBorderWidthAll(2);
        style.CornerRadiusTopLeft = 14;
        style.CornerRadiusTopRight = 14;
        style.CornerRadiusBottomLeft = 22;
        style.CornerRadiusBottomRight = 22;
        style.ContentMarginLeft = 20;
        style.ContentMarginRight = 20;
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
        ConfigureCenteredPopupPanel(_affairsPopup, PopupSkin.Document, new Vector2(920, 560));

        var root = new VBoxContainer();
        SetFullRect(root);
        root.OffsetLeft = 24;
        root.OffsetTop = 20;
        root.OffsetRight = -24;
        root.OffsetBottom = -20;
        root.AddThemeConstantOverride("separation", 12);
        _affairsPopup.AddChild(root);

        var title = new Label { Text = "御案折匣 · 尚书台卷宗" };
        StylePopupTitle(title, PopupSkin.Document);
        root.AddChild(title);

        var subtitle = new Label
        {
            Text = "左列为待批折签，右展奏章正文；朱批一落，即入本旬政令。",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        StylePopupBodyText(subtitle, PopupSkin.Document);
        root.AddChild(subtitle);

        var hBox = new HBoxContainer();
        hBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        hBox.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        hBox.AddThemeConstantOverride("separation", 16);
        root.AddChild(hBox);

        // 左半边：漆木折匣中的奏折签条
        var leftFrame = new PanelContainer
        {
            CustomMinimumSize = new Vector2(285, 0),
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        leftFrame.AddThemeStyleboxOverride("panel", CreatePopupInnerPanelStyle(PopupSkin.Document));
        hBox.AddChild(leftFrame);

        var leftVBox = new VBoxContainer();
        leftVBox.AddThemeConstantOverride("separation", 10);
        leftFrame.AddChild(leftVBox);

        var listTitle = new Label { Text = "折匣签条" };
        StyleColumnTitle(listTitle, PopupSkin.Document);
        leftVBox.AddChild(listTitle);

        _edictsItemList = new ItemList();
        _edictsItemList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        StylePopupItemList(_edictsItemList, PopupSkin.Document);
        _edictsItemList.ItemSelected += OnEdictSelected;
        leftVBox.AddChild(_edictsItemList);

        var btnClose = new Button { Text = "合上卷宗" };
        StyleSceneActionButton(btnClose, ActionButtonSkin.Document);
        btnClose.Pressed += _windowManager.PopWindow;
        leftVBox.AddChild(btnClose);

        // 右半边：展开的奏章与朱批区
        var rightFrame = new PanelContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        rightFrame.AddThemeStyleboxOverride("panel", CreatePopupInnerPanelStyle(PopupSkin.Document));
        hBox.AddChild(rightFrame);

        var rightVBox = new VBoxContainer();
        rightVBox.AddThemeConstantOverride("separation", 10);
        rightFrame.AddChild(rightVBox);

        var contentTitle = new Label { Text = "展开奏章" };
        StyleColumnTitle(contentTitle, PopupSkin.Document);
        rightVBox.AddChild(contentTitle);

        var contentFrame = new PanelContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        contentFrame.AddThemeStyleboxOverride("panel", CreatePopupParchmentStyle());
        rightVBox.AddChild(contentFrame);

        _edictContentLabel = new RichTextLabel();
        _edictContentLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _edictContentLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _edictContentLabel.BbcodeEnabled = true;
        _edictContentLabel.ScrollActive = true;
        _edictContentLabel.AddThemeColorOverride("default_color", GetPopupBodyColor(PopupSkin.Document));
        contentFrame.AddChild(_edictContentLabel);

        var optionTitle = new Label { Text = "御笔朱批" };
        StyleColumnTitle(optionTitle, PopupSkin.Document);
        rightVBox.AddChild(optionTitle);

        _edictOptionsVBox = new VBoxContainer();
        _edictOptionsVBox.CustomMinimumSize = new Vector2(0, 126);
        _edictOptionsVBox.AddThemeConstantOverride("separation", 8);
        rightVBox.AddChild(_edictOptionsVBox);

        AddChild(_affairsPopup);
    }

    private void OnAffairsBoxPressed()
    {
        if (_gameState == null) return;
        if (_gameState.CurrentLocation != "宣政殿")
        {
            ShowWarningReportPopup("太监急奏", "【太监急奏】\n\n“陛下，漆木折匣重器存放在宣政殿案上，请移驾宣政殿再行批阅批示！”");
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
        ClearEdictOptions();

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
        if (_gameState == null || _edictOptionsVBox == null || index < 0 || index >= _gameState.ActiveEdicts.Count) return;
        var edict = _gameState.ActiveEdicts[(int)index];

        _edictContentLabel!.Text = $"[b]【{edict.Title}】[/b]\n\n{edict.NarrativeContent}";

        // 清空选项
        ClearEdictOptions();

        // 动态生成选项
        for (int i = 0; i < edict.Options.Count; i++)
        {
            var option = edict.Options[i];
            var btn = new Button();
            btn.Text = $"朱批：{option.Description}";
            btn.Alignment = HorizontalAlignment.Left;
            btn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            StyleSceneActionButton(btn, ActionButtonSkin.Document);
            
            int optIndex = i;
            btn.Pressed += () =>
            {
                _windowManager.PopWindow(); // 关闭尚书台弹窗

                var result = _gameEngine!.ResolveEdictAction(edict.Id, optIndex);
                ShowDocumentReportPopup("尚书台朱批回奏", result.StoryText);
                UpdateUI();
            };
            _edictOptionsVBox.AddChild(btn);
        }
    }

    private void ClearEdictOptions()
    {
        if (_edictOptionsVBox == null) return;

        foreach (Node opt in _edictOptionsVBox.GetChildren())
        {
            _edictOptionsVBox.RemoveChild(opt);
            opt.QueueFree();
        }
    }
}
