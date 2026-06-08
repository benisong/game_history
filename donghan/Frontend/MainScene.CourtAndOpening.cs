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
}
