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

        // 极简主界面：只保留时间、卷轴/马车入口与正文
        _mainTimeLabel = new Label();
        _mainTimeLabel.Name = "MainTimeLabel";
        _mainTimeLabel.Text = FormatTimeLabel();
        _mainTimeLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _mainTimeLabel.AnchorLeft = 0.5f;
        _mainTimeLabel.AnchorTop = 0.0f;
        _mainTimeLabel.AnchorRight = 0.5f;
        _mainTimeLabel.AnchorBottom = 0.0f;
        _mainTimeLabel.OffsetLeft = -240;
        _mainTimeLabel.OffsetTop = 16;
        _mainTimeLabel.OffsetRight = 240;
        _mainTimeLabel.OffsetBottom = 48;
        _mainTimeLabel.AddThemeFontSizeOverride("font_size", 22);
        centerPanel.AddChild(_mainTimeLabel);
        centerPanel.MoveChild(_mainTimeLabel, 0);

        _deskContainer = new VBoxContainer();
        _deskContainer.Name = "EmperorsDesk";
        _deskContainer.AnchorLeft = 0.5f;
        _deskContainer.AnchorTop = 0.0f;
        _deskContainer.AnchorRight = 0.5f;
        _deskContainer.AnchorBottom = 0.0f;
        _deskContainer.OffsetLeft = -360;
        _deskContainer.OffsetTop = 62;
        _deskContainer.OffsetRight = 360;
        _deskContainer.OffsetBottom = 118;
        _deskContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        _deskContainer.Alignment = BoxContainer.AlignmentMode.Center;
        _deskContainer.CustomMinimumSize = new Vector2(720, 100);
        _deskContainer.AddThemeConstantOverride("separation", 10);
        
        centerPanel.AddChild(_deskContainer);
        centerPanel.MoveChild(_deskContainer, 0);

        // 极简主界面：上方只保留卷轴/马车入口，正文尽量展开
        if (_storyOutput != null)
        {
            _storyOutput.OffsetTop = 150;
        }

        var deskRow = new HBoxContainer();
        deskRow.Alignment = BoxContainer.AlignmentMode.Center;
        deskRow.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        deskRow.AddThemeConstantOverride("separation", 18);
        _deskContainer.AddChild(deskRow);

        _btnCourtSeal = CreateDeskButton("📜 朝会", OnCourtSealPressed);
        _btnIntelToken = CreateDeskButton("📜 情报", OnIntelTokenPressed);
        _btnWestGardenPalace = CreateDeskButton("🏯 西园", OnWestGardenPressed);
        _btnTravelCarriage = CreateCarriageButton("🐎 起驾", () =>
        {
            if (_travelOverlayPanel != null) _windowManager.PushWindow(_travelOverlayPanel);
        });

        deskRow.AddChild(_btnCourtSeal);
        deskRow.AddChild(_btnIntelToken);
        deskRow.AddChild(_btnWestGardenPalace);
        deskRow.AddChild(_btnTravelCarriage);
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
}
