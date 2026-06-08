using Godot;
using System;
using DonghanEngine.Core;

namespace DonghanFrontend;

public partial class MainScene : Control
{
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

    private static void ApplyOpaquePanelTheme(Node root)
    {
        if (root is Panel panel)
        {
            panel.AddThemeStyleboxOverride("panel", CreateOpaquePanelStyle(panel.Name.ToString()));
            panel.MouseFilter = Control.MouseFilterEnum.Stop;
        }

        if (root is ColorRect colorRect && root.Name.ToString().Contains("TransitionMask"))
        {
            ConfigureFullScreenBlocker(colorRect, zIndex: 20_000);
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

    private static void ConfigureWrappingLabel(Label? label, HorizontalAlignment alignment = HorizontalAlignment.Left)
    {
        if (label == null) return;

        label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        label.HorizontalAlignment = alignment;
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        label.ClipText = false;
    }
}
