using System;
using Godot;

namespace DonghanFrontend.Common;

/// <summary>
/// 汉风宫廷主题统一触控与动态反馈样式工具类
/// 为全游戏所有按键、卡片提供悬停（Hover）、按下（Pressed）、焦点（Focus）与选中态的视觉高亮与动态发光反馈
/// </summary>
public static class ImperialUiThemeHelper
{
    public enum ButtonSkin
    {
        PrimaryGold,     // 皇家御用明金（重要操作、朱批、大政令）
        SandTableLand,   // 十三州地块沙盘（领地、地缘卡片）
        ActionWheel,     // 触控决策轮盘（度田、水利、赎田、平叛）
        DarkWood,        // 宫廷乌木（常规选项、返回、导航）
        CrimsonWarning,  // 朱红戒律（抄家、征伐、重大决断）
        TealPolicy       // 苍青政务（水利、度田、安抚）
    }

    /// <summary>
    /// 为 Button 注入全套悬停、按下、焦点和普通状态的 StyleBoxFlat 与光标反馈
    /// </summary>
    public static void ApplyInteractiveFeedback(Button button, ButtonSkin skin = ButtonSkin.DarkWood, bool isSelected = false)
    {
        if (button == null) return;

        // 1. 设置光标形状为手型指针（提升触控与鼠标交互感）
        button.MouseDefaultCursorShape = Control.CursorShape.PointingHand;

        // 2. 生成各状态 StyleBox
        var normalStyle = CreateStyle(skin, hover: false, pressed: false, focused: false, selected: isSelected);
        var hoverStyle = CreateStyle(skin, hover: true, pressed: false, focused: false, selected: isSelected);
        var pressedStyle = CreateStyle(skin, hover: true, pressed: true, focused: false, selected: isSelected);
        var focusStyle = CreateStyle(skin, hover: true, pressed: false, focused: true, selected: isSelected);
        var disabledStyle = CreateDisabledStyle(skin);

        button.AddThemeStyleboxOverride("normal", normalStyle);
        button.AddThemeStyleboxOverride("hover", hoverStyle);
        button.AddThemeStyleboxOverride("pressed", pressedStyle);
        button.AddThemeStyleboxOverride("focus", focusStyle);
        button.AddThemeStyleboxOverride("disabled", disabledStyle);

        // 3. 字体与颜色高亮联动
        Color normalFont = isSelected ? new Color(1.0f, 0.94f, 0.65f) : new Color(0.92f, 0.88f, 0.82f);
        Color hoverFont = new Color(1.0f, 0.98f, 0.85f);
        Color pressedFont = new Color(1.0f, 0.80f, 0.35f);

        button.AddThemeColorOverride("font_color", normalFont);
        button.AddThemeColorOverride("font_hover_color", hoverFont);
        button.AddThemeColorOverride("font_pressed_color", pressedFont);
        button.AddThemeColorOverride("font_focus_color", hoverFont);

        // 4. 挂载鼠标进出悬停时的微缩放动态弹性反馈 (Tween Scale Animation)
        button.PivotOffset = button.CustomMinimumSize / 2f;
        
        button.MouseEntered += () =>
        {
            if (button.Disabled) return;
            var tween = button.CreateTween();
            tween.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(button, "scale", new Vector2(1.03f, 1.03f), 0.08f);
        };

        button.MouseExited += () =>
        {
            var tween = button.CreateTween();
            tween.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(button, "scale", new Vector2(1.0f, 1.0f), 0.08f);
        };

        button.ButtonDown += () =>
        {
            if (button.Disabled) return;
            var tween = button.CreateTween();
            tween.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(button, "scale", new Vector2(0.97f, 0.97f), 0.05f);
        };

        button.ButtonUp += () =>
        {
            var tween = button.CreateTween();
            tween.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(button, "scale", new Vector2(1.02f, 1.02f), 0.06f);
        };
    }

    private static StyleBoxFlat CreateStyle(ButtonSkin skin, bool hover, bool pressed, bool focused, bool selected)
    {
        var style = new StyleBoxFlat();

        // 基础底色
        Color bg = skin switch
        {
            ButtonSkin.PrimaryGold => new Color(0.24f, 0.16f, 0.05f, 0.96f),
            ButtonSkin.SandTableLand => selected ? new Color(0.26f, 0.18f, 0.08f, 0.98f) : new Color(0.12f, 0.10f, 0.09f, 0.92f),
            ButtonSkin.ActionWheel => new Color(0.14f, 0.12f, 0.10f, 0.95f),
            ButtonSkin.CrimsonWarning => new Color(0.22f, 0.06f, 0.06f, 0.96f),
            ButtonSkin.TealPolicy => new Color(0.06f, 0.16f, 0.18f, 0.96f),
            _ => new Color(0.13f, 0.11f, 0.10f, 0.95f)
        };

        // 边框主色与金线
        Color border = skin switch
        {
            ButtonSkin.PrimaryGold => new Color(0.95f, 0.78f, 0.32f, 1.0f),
            ButtonSkin.SandTableLand => selected ? new Color(0.98f, 0.82f, 0.28f, 1.0f) : new Color(0.42f, 0.35f, 0.28f, 0.8f),
            ButtonSkin.ActionWheel => new Color(0.72f, 0.58f, 0.35f, 0.9f),
            ButtonSkin.CrimsonWarning => new Color(0.85f, 0.28f, 0.22f, 1.0f),
            ButtonSkin.TealPolicy => new Color(0.25f, 0.68f, 0.72f, 1.0f),
            _ => new Color(0.55f, 0.44f, 0.30f, 0.85f)
        };

        if (hover)
        {
            bg = bg.Lightened(0.15f);
            border = border.Lightened(0.25f);
        }

        if (pressed)
        {
            bg = bg.Darkened(0.18f);
            border = border.Lightened(0.10f);
        }

        if (focused || selected)
        {
            border = new Color(1.0f, 0.88f, 0.38f, 1.0f);
        }

        style.BgColor = bg;
        style.BorderColor = border;

        // 边框厚度：悬停/选中时加粗金边
        int borderWidth = (hover || selected || focused) ? 2 : 1;
        style.SetBorderWidthAll(borderWidth);

        // 圆角
        style.CornerRadiusTopLeft = 6;
        style.CornerRadiusTopRight = 6;
        style.CornerRadiusBottomLeft = 6;
        style.CornerRadiusBottomRight = 6;

        // 内边距
        style.ContentMarginLeft = 10;
        style.ContentMarginRight = 10;
        style.ContentMarginTop = 6;
        style.ContentMarginBottom = 6;

        // 阴影与外发光
        if (hover || focused || selected)
        {
            style.ShadowColor = skin == ButtonSkin.CrimsonWarning
                ? new Color(0.85f, 0.20f, 0.15f, 0.45f)
                : new Color(0.95f, 0.75f, 0.25f, 0.38f);
            style.ShadowSize = 6;
        }
        else
        {
            style.ShadowColor = new Color(0, 0, 0, 0.35f);
            style.ShadowSize = 2;
        }

        return style;
    }

    private static StyleBoxFlat CreateDisabledStyle(ButtonSkin skin)
    {
        var style = CreateStyle(skin, hover: false, pressed: false, focused: false, selected: false);
        style.BgColor = new Color(0.08f, 0.08f, 0.08f, 0.75f);
        style.BorderColor = new Color(0.25f, 0.25f, 0.25f, 0.4f);
        style.ShadowSize = 0;
        return style;
    }
}
