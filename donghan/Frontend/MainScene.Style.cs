using Godot;
using System;
using DonghanEngine.Core;

namespace DonghanFrontend;

public partial class MainScene : Control
{
    private enum PopupSkin
    {
        Court,
        Intel,
        WestGarden,
        Document,
        Warning
    }

    private enum ActionButtonSkin
    {
        Court,
        WestGarden,
        Harem,
        Travel,
        Warning,
        Document
    }

    private static void ForceExclusiveFullscreen()
    {
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
    }

    private static void ConfigureFullScreenBlocker(ColorRect blocker, int zIndex)
    {
        blocker.Color = new Color(0.04f, 0.035f, 0.03f, 1.0f);
        blocker.MouseFilter = Control.MouseFilterEnum.Stop;
        blocker.ZIndex = zIndex;
        SetFullRect(blocker);
    }

    private void EnsureOpaqueSceneBackground()
    {
        if (GetNodeOrNull<ColorRect>("OpaqueSceneBackground") != null) return;

        var background = new ColorRect();
        background.Name = "OpaqueSceneBackground";
        background.Color = new Color(0.055f, 0.045f, 0.04f, 1.0f);
        background.MouseFilter = Control.MouseFilterEnum.Ignore;
        background.ZIndex = -100;
        SetFullRect(background);

        AddChild(background);
        MoveChild(background, 0);
    }

    private static void EnsureMainSceneImageBackground(Panel centerPanel)
    {
        if (centerPanel.GetNodeOrNull<TextureRect>("MainLacquerBackground") != null) return;

        var texture = new TextureRect();
        texture.Name = "MainLacquerBackground";
        texture.Texture = LoadTextureFromProjectFile("res://Assets/UI/backgrounds/main_lacquer_bg.png");
        texture.MouseFilter = Control.MouseFilterEnum.Ignore;
        texture.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        texture.StretchMode = TextureRect.StretchModeEnum.Scale;
        texture.ZIndex = -20;
        SetFullRect(texture);
        centerPanel.AddChild(texture);
        centerPanel.MoveChild(texture, 0);

        var darkWash = new ColorRect();
        darkWash.Name = "MainBackgroundDarkWash";
        darkWash.Color = new Color(0.0f, 0.0f, 0.0f, 0.34f);
        darkWash.MouseFilter = Control.MouseFilterEnum.Ignore;
        darkWash.ZIndex = -19;
        SetFullRect(darkWash);
        centerPanel.AddChild(darkWash);
        centerPanel.MoveChild(darkWash, 1);
    }

    private static void EnsureMainAnnualEventFrame(Panel centerPanel, RichTextLabel storyOutput)
    {
        if (centerPanel.GetNodeOrNull<Panel>("MainAnnualEventFrame") != null) return;

        var frame = new Panel();
        frame.Name = "MainAnnualEventFrame";
        frame.MouseFilter = Control.MouseFilterEnum.Ignore;
        frame.ZIndex = 3;
        frame.AnchorLeft = storyOutput.AnchorLeft;
        frame.AnchorTop = storyOutput.AnchorTop;
        frame.AnchorRight = storyOutput.AnchorRight;
        frame.AnchorBottom = storyOutput.AnchorBottom;
        frame.OffsetLeft = storyOutput.OffsetLeft - 18;
        frame.OffsetTop = storyOutput.OffsetTop - 8;
        frame.OffsetRight = storyOutput.OffsetRight + 18;
        frame.OffsetBottom = storyOutput.OffsetBottom + 8;

        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.035f, 0.018f, 0.014f, 0.78f);
        style.BorderColor = new Color(0.78f, 0.46f, 0.12f, 0.95f);
        style.SetBorderWidthAll(2);
        style.CornerRadiusTopLeft = 12;
        style.CornerRadiusTopRight = 12;
        style.CornerRadiusBottomLeft = 12;
        style.CornerRadiusBottomRight = 12;
        style.ContentMarginLeft = 16;
        style.ContentMarginRight = 16;
        style.ContentMarginTop = 8;
        style.ContentMarginBottom = 8;
        frame.AddThemeStyleboxOverride("panel", style);

        centerPanel.AddChild(frame);
        centerPanel.MoveChild(frame, Math.Max(0, storyOutput.GetIndex()));
    }

    private static Texture2D? LoadTextureFromProjectFile(string resourcePath)
    {
        var importedTexture = GD.Load<Texture2D>(resourcePath);
        if (importedTexture != null) return importedTexture;

        string filePath = ProjectSettings.GlobalizePath(resourcePath);
        var image = Image.LoadFromFile(filePath);
        if (image == null || image.IsEmpty())
        {
            GD.PrintErr($"无法载入图片资源：{resourcePath}");
            return null;
        }

        return ImageTexture.CreateFromImage(image);
    }

    private static void ApplyOpaquePanelTheme(Node root)
    {
        if (root is Panel panel)
        {
            panel.AddThemeStyleboxOverride("panel", CreateOpaquePanelStyle(panel.Name.ToString()));
            panel.MouseFilter = Control.MouseFilterEnum.Stop;
        }

        if (root is ColorRect colorRect && root.Name.ToString().Contains("TransitionMask"))
        {
            ConfigureFullScreenBlocker(colorRect, zIndex: 3_000);
        }

        foreach (var child in root.GetChildren())
        {
            ApplyOpaquePanelTheme(child);
        }
    }

    private static StyleBoxFlat CreateOpaquePanelStyle(string panelName)
    {
        bool isPopup = panelName.Contains("Popup") || panelName.Contains("Overlay");
        var style = new StyleBoxFlat();
        style.BgColor = isPopup
            ? new Color(0.10f, 0.095f, 0.085f, 1.0f)
            : new Color(0.075f, 0.068f, 0.06f, 1.0f);
        style.SetBorderWidthAll(isPopup ? 2 : 1);
        style.BorderColor = isPopup
            ? new Color(0.84f, 0.67f, 0.12f, 1.0f)
            : new Color(0.40f, 0.32f, 0.10f, 1.0f);
        style.ContentMarginLeft = 8;
        style.ContentMarginRight = 8;
        style.ContentMarginTop = 8;
        style.ContentMarginBottom = 8;
        return style;
    }


    private static StyleBoxFlat CreatePopupPanelStyle(PopupSkin skin)
    {
        var style = new StyleBoxFlat();
        style.BgColor = skin switch
        {
            PopupSkin.Court => new Color(0.070f, 0.030f, 0.020f, 1.0f),
            PopupSkin.Intel => new Color(0.055f, 0.043f, 0.030f, 1.0f),
            PopupSkin.WestGarden => new Color(0.070f, 0.060f, 0.044f, 1.0f),
            PopupSkin.Warning => new Color(0.100f, 0.045f, 0.035f, 1.0f),
            PopupSkin.Document => new Color(0.520f, 0.430f, 0.270f, 1.0f),
            _ => new Color(0.100f, 0.095f, 0.085f, 1.0f)
        };
        style.BorderColor = skin switch
        {
            PopupSkin.Court => new Color(0.70f, 0.46f, 0.11f, 1.0f),
            PopupSkin.Intel => new Color(0.58f, 0.42f, 0.22f, 1.0f),
            PopupSkin.WestGarden => new Color(0.54f, 0.42f, 0.20f, 1.0f),
            PopupSkin.Warning => new Color(0.72f, 0.16f, 0.10f, 1.0f),
            PopupSkin.Document => new Color(0.34f, 0.18f, 0.08f, 1.0f),
            _ => new Color(0.84f, 0.67f, 0.12f, 1.0f)
        };
        style.SetBorderWidthAll(3);
        style.CornerRadiusTopLeft = 10;
        style.CornerRadiusTopRight = 10;
        style.CornerRadiusBottomLeft = 10;
        style.CornerRadiusBottomRight = 10;
        style.ContentMarginLeft = 10;
        style.ContentMarginRight = 10;
        style.ContentMarginTop = 10;
        style.ContentMarginBottom = 10;
        style.ShadowColor = new Color(0, 0, 0, 0.70f);
        style.ShadowSize = 18;
        return style;
    }

    private static StyleBoxFlat CreatePopupInnerPanelStyle(PopupSkin skin)
    {
        var style = new StyleBoxFlat();
        style.BgColor = skin switch
        {
            PopupSkin.Court => new Color(0.120f, 0.055f, 0.030f, 1.0f),
            PopupSkin.Intel => new Color(0.690f, 0.590f, 0.390f, 1.0f),
            PopupSkin.WestGarden => new Color(0.125f, 0.105f, 0.075f, 1.0f),
            PopupSkin.Warning => new Color(0.160f, 0.070f, 0.050f, 1.0f),
            _ => new Color(0.110f, 0.085f, 0.060f, 1.0f)
        };
        style.BorderColor = skin switch
        {
            PopupSkin.Court => new Color(0.62f, 0.38f, 0.10f, 1.0f),
            PopupSkin.Intel => new Color(0.46f, 0.27f, 0.13f, 1.0f),
            PopupSkin.WestGarden => new Color(0.48f, 0.38f, 0.22f, 1.0f),
            PopupSkin.Warning => new Color(0.72f, 0.18f, 0.10f, 1.0f),
            _ => new Color(0.58f, 0.43f, 0.14f, 1.0f)
        };
        style.SetBorderWidthAll(1);
        style.CornerRadiusTopLeft = skin == PopupSkin.Court ? 2 : 7;
        style.CornerRadiusTopRight = skin == PopupSkin.Court ? 2 : 7;
        style.CornerRadiusBottomLeft = skin == PopupSkin.Court ? 2 : 7;
        style.CornerRadiusBottomRight = skin == PopupSkin.Court ? 2 : 7;
        style.ContentMarginLeft = 8;
        style.ContentMarginRight = 8;
        style.ContentMarginTop = 8;
        style.ContentMarginBottom = 8;
        return style;
    }

    private static Color GetPopupTitleColor(PopupSkin skin)
    {
        return skin switch
        {
            PopupSkin.Intel => new Color(0.16f, 0.09f, 0.04f, 1.0f),
            PopupSkin.WestGarden => new Color(0.86f, 0.66f, 0.28f, 1.0f),
            PopupSkin.Warning => new Color(0.95f, 0.35f, 0.20f, 1.0f),
            PopupSkin.Document => new Color(0.18f, 0.09f, 0.04f, 1.0f),
            _ => new Color(0.92f, 0.70f, 0.25f, 1.0f)
        };
    }

    private static void StylePopupTitle(Label label, PopupSkin skin)
    {
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.AddThemeFontSizeOverride("font_size", 24);
        label.AddThemeColorOverride("font_color", skin == PopupSkin.Intel
            ? new Color(0.88f, 0.73f, 0.46f, 1.0f)
            : GetPopupTitleColor(skin));
    }

    private static void StyleColumnTitle(Label label, PopupSkin skin)
    {
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.AddThemeFontSizeOverride("font_size", 18);
        label.AddThemeColorOverride("font_color", GetPopupTitleColor(skin));
    }

    private static void ConfigureCenteredPopupPanel(Panel panel, PopupSkin skin, Vector2 size)
    {
        panel.CustomMinimumSize = size;
        panel.AnchorLeft = 0.5f;
        panel.AnchorTop = 0.5f;
        panel.AnchorRight = 0.5f;
        panel.AnchorBottom = 0.5f;
        panel.OffsetLeft = -size.X / 2.0f;
        panel.OffsetTop = -size.Y / 2.0f;
        panel.OffsetRight = size.X / 2.0f;
        panel.OffsetBottom = size.Y / 2.0f;
        panel.MouseFilter = Control.MouseFilterEnum.Stop;
        panel.AddThemeStyleboxOverride("panel", CreatePopupPanelStyle(skin));
    }

    private static Label CreateActionPreviewLabel(PopupSkin skin)
    {
        var preview = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        preview.AddThemeColorOverride("font_color", skin == PopupSkin.WestGarden
            ? new Color(0.88f, 0.78f, 0.58f, 1.0f)
            : GetPopupTitleColor(skin));
        return preview;
    }

    private static void StylePopupBodyText(Label label, PopupSkin skin)
    {
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        label.AddThemeColorOverride("font_color", skin switch
        {
            PopupSkin.Document => new Color(0.20f, 0.11f, 0.05f, 1.0f),
            PopupSkin.Intel => new Color(0.88f, 0.73f, 0.46f, 1.0f),
            PopupSkin.WestGarden => new Color(0.82f, 0.74f, 0.58f, 1.0f),
            _ => new Color(0.86f, 0.74f, 0.50f, 1.0f)
        });
    }

    private static Color GetPopupBodyColor(PopupSkin skin)
    {
        return skin switch
        {
            PopupSkin.Document => new Color(0.20f, 0.11f, 0.05f, 1.0f),
            PopupSkin.Intel => new Color(0.88f, 0.73f, 0.46f, 1.0f),
            PopupSkin.WestGarden => new Color(0.82f, 0.74f, 0.58f, 1.0f),
            _ => new Color(0.86f, 0.74f, 0.50f, 1.0f)
        };
    }

    private static void StylePopupInput(Control? control, PopupSkin skin)
    {
        if (control == null) return;

        control.MouseFilter = Control.MouseFilterEnum.Stop;
        control.AddThemeFontSizeOverride("font_size", 15);
        control.AddThemeColorOverride("font_color", GetPopupBodyColor(skin));
        control.AddThemeColorOverride("font_hover_color", GetPopupBodyColor(skin).Lightened(0.18f));
        control.AddThemeColorOverride("font_focus_color", GetPopupBodyColor(skin).Lightened(0.24f));
        control.AddThemeColorOverride("font_disabled_color", GetPopupBodyColor(skin).Darkened(0.42f));
        control.AddThemeColorOverride("caret_color", GetPopupTitleColor(skin));
        control.AddThemeColorOverride("selection_color", GetPopupTitleColor(skin).Darkened(0.20f));
        control.AddThemeStyleboxOverride("normal", CreatePopupInputStyle(skin, false, false));
        control.AddThemeStyleboxOverride("hover", CreatePopupInputStyle(skin, false, false));
        control.AddThemeStyleboxOverride("focus", CreatePopupInputStyle(skin, true, false));
        control.AddThemeStyleboxOverride("read_only", CreatePopupInputStyle(skin, false, true));
        control.AddThemeStyleboxOverride("disabled", CreatePopupInputStyle(skin, false, true));
    }

    private static void StylePopupCheckBox(CheckBox? checkBox, PopupSkin skin)
    {
        if (checkBox == null) return;

        checkBox.MouseFilter = Control.MouseFilterEnum.Stop;
        checkBox.AddThemeFontSizeOverride("font_size", 15);
        checkBox.AddThemeColorOverride("font_color", GetPopupBodyColor(skin));
        checkBox.AddThemeColorOverride("font_hover_color", GetPopupBodyColor(skin).Lightened(0.18f));
        checkBox.AddThemeColorOverride("font_pressed_color", GetPopupTitleColor(skin));
        checkBox.AddThemeColorOverride("font_focus_color", GetPopupTitleColor(skin));
        checkBox.AddThemeColorOverride("font_disabled_color", GetPopupBodyColor(skin).Darkened(0.42f));
    }

    private static void StylePopupItemList(ItemList? list, PopupSkin skin)
    {
        if (list == null) return;

        list.MouseFilter = Control.MouseFilterEnum.Stop;
        list.AddThemeFontSizeOverride("font_size", 15);
        list.AddThemeColorOverride("font_color", GetPopupBodyColor(skin));
        list.AddThemeColorOverride("font_hovered_color", GetPopupBodyColor(skin).Lightened(0.18f));
        list.AddThemeColorOverride("font_selected_color", skin == PopupSkin.Intel
            ? new Color(0.15f, 0.07f, 0.03f, 1.0f)
            : GetPopupTitleColor(skin));
        list.AddThemeColorOverride("guide_color", GetPopupTitleColor(skin).Darkened(0.25f));
        list.AddThemeStyleboxOverride("panel", CreatePopupInputStyle(skin, false, false));
        list.AddThemeStyleboxOverride("focus", CreatePopupInputStyle(skin, true, false));
        list.AddThemeStyleboxOverride("cursor", CreatePopupListSelectionStyle(skin, false));
        list.AddThemeStyleboxOverride("cursor_unfocused", CreatePopupListSelectionStyle(skin, false));
        list.AddThemeStyleboxOverride("selected", CreatePopupListSelectionStyle(skin, true));
        list.AddThemeStyleboxOverride("selected_focus", CreatePopupListSelectionStyle(skin, true));
    }

    private static StyleBoxFlat CreatePopupListSelectionStyle(PopupSkin skin, bool selected)
    {
        var style = new StyleBoxFlat();
        Color accent = GetPopupTitleColor(skin);
        style.BgColor = selected ? accent.Darkened(skin == PopupSkin.Intel ? 0.05f : 0.35f) : accent.Darkened(0.55f);
        style.BorderColor = accent;
        style.SetBorderWidthAll(selected ? 2 : 1);
        style.CornerRadiusTopLeft = 5;
        style.CornerRadiusTopRight = 5;
        style.CornerRadiusBottomLeft = 5;
        style.CornerRadiusBottomRight = 5;
        style.ContentMarginLeft = 6;
        style.ContentMarginRight = 6;
        style.ContentMarginTop = 4;
        style.ContentMarginBottom = 4;
        return style;
    }

    private static StyleBoxFlat CreatePopupInputStyle(PopupSkin skin, bool focused, bool disabled)
    {
        var style = new StyleBoxFlat();
        Color bg = skin switch
        {
            PopupSkin.Intel => new Color(0.760f, 0.650f, 0.430f, 1.0f),
            PopupSkin.Document => new Color(0.700f, 0.585f, 0.365f, 1.0f),
            PopupSkin.WestGarden => new Color(0.095f, 0.080f, 0.056f, 1.0f),
            PopupSkin.Warning => new Color(0.120f, 0.050f, 0.038f, 1.0f),
            _ => new Color(0.105f, 0.040f, 0.028f, 1.0f)
        };
        if (focused) bg = bg.Lightened(0.08f);
        if (disabled) bg = bg.Darkened(0.28f);
        style.BgColor = bg;
        style.BorderColor = focused ? GetPopupTitleColor(skin) : skin switch
        {
            PopupSkin.Intel => new Color(0.42f, 0.24f, 0.10f, 1.0f),
            PopupSkin.Document => new Color(0.34f, 0.18f, 0.08f, 1.0f),
            PopupSkin.WestGarden => new Color(0.48f, 0.38f, 0.20f, 1.0f),
            PopupSkin.Warning => new Color(0.62f, 0.14f, 0.09f, 1.0f),
            _ => new Color(0.58f, 0.34f, 0.10f, 1.0f)
        };
        style.SetBorderWidthAll(focused ? 2 : 1);
        style.CornerRadiusTopLeft = 6;
        style.CornerRadiusTopRight = 6;
        style.CornerRadiusBottomLeft = 6;
        style.CornerRadiusBottomRight = 6;
        style.ContentMarginLeft = 10;
        style.ContentMarginRight = 10;
        style.ContentMarginTop = 7;
        style.ContentMarginBottom = 7;
        return style;
    }

    private static void SetFullRect(Control control)
    {
        control.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        control.OffsetLeft = 0;
        control.OffsetTop = 0;
        control.OffsetRight = 0;
        control.OffsetBottom = 0;
    }

    private void ConfigureMinisterPanelLayout()
    {
        if (_ministerPanel == null) return;

        _ministerPanel.CustomMinimumSize = new Vector2(520, 360);
        _ministerPanel.AnchorLeft = 0.5f;
        _ministerPanel.AnchorTop = 0.5f;
        _ministerPanel.AnchorRight = 0.5f;
        _ministerPanel.AnchorBottom = 0.5f;
        _ministerPanel.OffsetLeft = -260;
        _ministerPanel.OffsetTop = -180;
        _ministerPanel.OffsetRight = 260;
        _ministerPanel.OffsetBottom = 180;

        var vBox = _ministerPanel.GetNodeOrNull<VBoxContainer>("VBox");
        if (vBox != null)
        {
            vBox.AddThemeConstantOverride("separation", 8);
        }

        ConfigureWrappingLabel(_ministerTitleLabel, HorizontalAlignment.Center);
        ConfigureWrappingLabel(_ministerFavorabilityLabel);
        ConfigureWrappingLabel(_ministerPowerLabel);
        ConfigureWrappingLabel(GetNodeOrNull<Label>("MinisterOverlayPanel/VBox/MinisterCorruption"));
        ConfigureWrappingLabel(GetNodeOrNull<Label>("MinisterOverlayPanel/VBox/MinisterWealth"));

        var actionRow = GetNodeOrNull<HBoxContainer>("MinisterOverlayPanel/VBox/HBox");
        if (actionRow != null)
        {
            actionRow.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            actionRow.Alignment = BoxContainer.AlignmentMode.Center;
            actionRow.AddThemeConstantOverride("separation", 12);
            foreach (var child in actionRow.GetChildren())
            {
                if (child is Button button)
                {
                    button.CustomMinimumSize = new Vector2(0, 42);
                    button.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                }
            }
        }
    }


    private static void StyleSceneActionButton(Button? button, ActionButtonSkin skin)
    {
        if (button == null) return;

        button.CustomMinimumSize = new Vector2(0, 42);
        button.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        button.MouseFilter = Control.MouseFilterEnum.Stop;
        button.AddThemeFontSizeOverride("font_size", 15);
        button.AddThemeColorOverride("font_color", GetActionButtonTextColor(skin));
        button.AddThemeColorOverride("font_hover_color", new Color(1.0f, 0.86f, 0.48f, 1.0f));
        button.AddThemeColorOverride("font_pressed_color", new Color(0.98f, 0.72f, 0.24f, 1.0f));
        button.AddThemeColorOverride("font_focus_color", new Color(1.0f, 0.90f, 0.58f, 1.0f));
        button.AddThemeColorOverride("font_disabled_color", GetActionButtonTextColor(skin).Darkened(0.42f));
        button.AddThemeStyleboxOverride("normal", CreateActionButtonStyle(skin, false, false));
        button.AddThemeStyleboxOverride("hover", CreateActionButtonStyle(skin, true, false));
        button.AddThemeStyleboxOverride("pressed", CreateActionButtonStyle(skin, false, true));
        button.AddThemeStyleboxOverride("focus", CreateActionButtonFocusStyle(skin));
        button.AddThemeStyleboxOverride("disabled", CreateActionButtonDisabledStyle(skin));
    }

    private static ActionButtonSkin GetActionButtonSkinForPopup(PopupSkin skin)
    {
        return skin switch
        {
            PopupSkin.Court => ActionButtonSkin.Court,
            PopupSkin.WestGarden => ActionButtonSkin.WestGarden,
            PopupSkin.Warning => ActionButtonSkin.Warning,
            _ => ActionButtonSkin.Document
        };
    }

    private static Color GetActionButtonTextColor(ActionButtonSkin skin)
    {
        return skin switch
        {
            ActionButtonSkin.Court => new Color(0.94f, 0.75f, 0.36f, 1.0f),
            ActionButtonSkin.WestGarden => new Color(0.86f, 0.70f, 0.42f, 1.0f),
            ActionButtonSkin.Harem => new Color(0.92f, 0.74f, 0.62f, 1.0f),
            ActionButtonSkin.Travel => new Color(0.96f, 0.84f, 0.46f, 1.0f),
            ActionButtonSkin.Warning => new Color(0.95f, 0.45f, 0.30f, 1.0f),
            ActionButtonSkin.Document => new Color(0.24f, 0.12f, 0.05f, 1.0f),
            _ => new Color(0.92f, 0.74f, 0.42f, 1.0f)
        };
    }

    private static StyleBoxFlat CreateActionButtonStyle(ActionButtonSkin skin, bool hover, bool pressed)
    {
        var style = new StyleBoxFlat();
        Color bg = skin switch
        {
            ActionButtonSkin.Court => new Color(0.105f, 0.034f, 0.024f, 0.96f),
            ActionButtonSkin.WestGarden => new Color(0.092f, 0.074f, 0.050f, 0.96f),
            ActionButtonSkin.Harem => new Color(0.115f, 0.060f, 0.055f, 0.96f),
            ActionButtonSkin.Travel => new Color(0.090f, 0.052f, 0.030f, 0.98f),
            ActionButtonSkin.Warning => new Color(0.130f, 0.045f, 0.034f, 0.98f),
            ActionButtonSkin.Document => new Color(0.610f, 0.500f, 0.315f, 0.98f),
            _ => new Color(0.090f, 0.060f, 0.040f, 0.96f)
        };
        if (hover) bg = bg.Lightened(0.12f);
        if (pressed) bg = bg.Darkened(0.18f);
        style.BgColor = bg;
        style.BorderColor = skin switch
        {
            ActionButtonSkin.Court => new Color(0.64f, 0.36f, 0.10f, 1.0f),
            ActionButtonSkin.WestGarden => new Color(0.52f, 0.40f, 0.21f, 1.0f),
            ActionButtonSkin.Harem => new Color(0.58f, 0.30f, 0.24f, 1.0f),
            ActionButtonSkin.Travel => new Color(0.74f, 0.50f, 0.16f, 1.0f),
            ActionButtonSkin.Warning => new Color(0.72f, 0.16f, 0.10f, 1.0f),
            ActionButtonSkin.Document => new Color(0.37f, 0.20f, 0.09f, 1.0f),
            _ => new Color(0.55f, 0.36f, 0.12f, 1.0f)
        };
        style.SetBorderWidthAll(1);
        if (hover) style.SetBorderWidthAll(2);
        style.CornerRadiusTopLeft = 6;
        style.CornerRadiusTopRight = 6;
        style.CornerRadiusBottomLeft = 6;
        style.CornerRadiusBottomRight = 6;
        style.ContentMarginLeft = 12;
        style.ContentMarginRight = 12;
        style.ContentMarginTop = 8;
        style.ContentMarginBottom = 8;
        style.ShadowColor = new Color(0, 0, 0, hover ? 0.42f : 0.25f);
        style.ShadowSize = hover ? 6 : 3;
        return style;
    }

    private static StyleBoxFlat CreateActionButtonFocusStyle(ActionButtonSkin skin)
    {
        var style = CreateActionButtonStyle(skin, true, false);
        style.BorderColor = new Color(1.0f, 0.82f, 0.30f, 1.0f);
        style.SetBorderWidthAll(2);
        style.ShadowColor = new Color(1.0f, 0.62f, 0.12f, 0.36f);
        style.ShadowSize = 8;
        return style;
    }

    private static StyleBoxFlat CreateActionButtonDisabledStyle(ActionButtonSkin skin)
    {
        var style = CreateActionButtonStyle(skin, false, false);
        style.BgColor = style.BgColor.Darkened(0.36f);
        style.BorderColor = style.BorderColor.Darkened(0.45f);
        style.ShadowSize = 0;
        return style;
    }

    private void ApplySceneActionButtonStyles()
    {
        StyleSceneActionButton(GetNodeOrNull<Button>("RightPanel/Ministers/DisasterReliefButton"), ActionButtonSkin.Court);
        StyleSceneActionButton(_sellOfficeButton, ActionButtonSkin.WestGarden);
        StyleSceneActionButton(_drillArmyButton, ActionButtonSkin.WestGarden);
        StyleSceneActionButton(_recruitArmyButton, ActionButtonSkin.WestGarden);
        StyleSceneActionButton(_haremRestButton, ActionButtonSkin.Harem);
        StyleSceneActionButton(_travelButton, ActionButtonSkin.Travel);

        StyleSceneActionButton(GetNodeOrNull<Button>("MinisterOverlayPanel/VBox/HBox/ConfiscateTreasuryBtn"), ActionButtonSkin.Warning);
        StyleSceneActionButton(GetNodeOrNull<Button>("MinisterOverlayPanel/VBox/HBox/ConfiscatePrivateBtn"), ActionButtonSkin.Warning);
    }

    private static void ConfigureWrappingLabel(Label? label, HorizontalAlignment alignment = HorizontalAlignment.Left)
    {
        if (label == null) return;

        label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        label.HorizontalAlignment = alignment;
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        label.ClipText = false;
    }
}
