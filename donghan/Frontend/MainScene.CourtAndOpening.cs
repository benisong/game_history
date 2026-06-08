using Godot;
using System;
using System.Threading.Tasks;
using DonghanEngine.Core;

namespace DonghanFrontend;

public partial class MainScene : Control
{
    private Panel? _courtPopup;
    private LineEdit? _courtInput;

    private void InitializeCourtPanel()
    {
        _courtPopup = new Panel();
        _courtPopup.Name = "CourtPopup";
        _courtPopup.Visible = false;
        _courtPopup.CustomMinimumSize = new Vector2(400, 200);
        _courtPopup.SetAnchorsPreset(Control.LayoutPreset.Center);

        var vBox = new VBoxContainer();
        vBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vBox.OffsetLeft = 20; vBox.OffsetTop = 20; vBox.OffsetRight = -20; vBox.OffsetBottom = -20;
        vBox.AddThemeConstantOverride("separation", 15);
        _courtPopup.AddChild(vBox);

        var lbl = new Label();
        lbl.Text = "👑 宣政殿 · 鸣磬起朝会";
        lbl.HorizontalAlignment = HorizontalAlignment.Center;
        vBox.AddChild(lbl);

        _courtInput = new LineEdit();
        _courtInput.PlaceholderText = "输入圣旨口召，如：重赏曹操、弹劾张让";
        vBox.AddChild(_courtInput);

        var hBox = new HBoxContainer();
        hBox.Alignment = BoxContainer.AlignmentMode.Center;
        hBox.AddThemeConstantOverride("separation", 20);
        vBox.AddChild(hBox);

        var btnConfirm = new Button();
        btnConfirm.Text = "宣旨起大朝仪";
        btnConfirm.Pressed += OnConfirmCourtAssembly;
        hBox.AddChild(btnConfirm);

        var btnCancel = new Button();
        btnCancel.Text = "暂缓朝会";
        btnCancel.Pressed += _windowManager.PopWindow;
        hBox.AddChild(btnCancel);

        AddChild(_courtPopup);
    }

    private void OnCourtSealPressed()
    {
        if (_gameState == null) return;
        if (_gameState.CurrentLocation != "宣政殿")
        {
            if (_storyOutput != null) _storyOutput.Text = "【太监急奏】\n\n“陛下，天子玉玺非大朝会宣政殿不可轻动！”";
            return;
        }
        _courtInput!.Text = "";
        _windowManager.PushWindow(_courtPopup!);
    }

    private async void OnConfirmCourtAssembly()
    {
        string txt = _courtInput!.Text;
        if (string.IsNullOrWhiteSpace(txt)) return;

        _windowManager.PopWindow(); // 关闭发起弹窗

        // 触发大朝会三阶段过渡动画/文字效果
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

        // 执行 ProcessPlayerTurnAsync
        if (_storyOutput != null) _storyOutput.Text = "百官正在唇枪舌战，商议对策...";
        var result = await _gameEngine!.ProcessPlayerTurnAsync(txt);
        if (_storyOutput != null) _storyOutput.Text = result.StoryText;
        UpdateUI();
    }

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
        opaqueStyle.BgColor = new Color(0.08f, 0.08f, 0.08f, 1.0f); // 厚重墨黑褐色
        opaqueStyle.SetBorderWidthAll(4);
        opaqueStyle.BorderColor = new Color(0.72f, 0.58f, 0.12f, 1.0f); // 暗亮金边
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
            _openingOverlay.QueueFree(); // 玩家确认后彻底销毁，显示主场景
        };
        vBox.AddChild(btnConfirm);

        AddChild(_openingOverlay);
        _openingOverlay.GrabFocus();
    }
}
