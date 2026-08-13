using System;
using System.Collections.Generic;
using Godot;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2;

public partial class MainSceneV2 : Control
{
    private V2Runtime _runtime = null!;
    private Label _status = null!;
    private Label _snapshot = null!;
    private VBoxContainer _content = null!;

    public override void _Ready()
    {
        _runtime = V2RuntimeFactory.CreateDefault();
        BuildUi();
        ShowHome();
    }

    private void BuildUi()
    {
        var background = new ColorRect
        {
            Color = new Color(0.035f, 0.025f, 0.018f, 1f),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        background.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(background);

        var root = new VBoxContainer
        {
            Position = new Vector2(60, 34),
            Size = new Vector2(1160, 650)
        };
        root.AddThemeConstantOverride("separation", 12);
        AddChild(root);

        var title = new Label
        {
            Text = "东汉末年灵帝传 · 平行玩法链路 V2",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.AddThemeFontSizeOverride("font_size", 28);
        title.AddThemeColorOverride("font_color", new Color(0.95f, 0.77f, 0.28f, 1f));
        root.AddChild(title);

        _snapshot = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _snapshot.AddThemeFontSizeOverride("font_size", 17);
        root.AddChild(_snapshot);

        _content = new VBoxContainer();
        _content.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _content.AddThemeConstantOverride("separation", 12);
        root.AddChild(_content);

        _status = new Label
        {
            Text = "V2 Runtime 已组装：UI 只依赖 Contracts 接口。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _status.CustomMinimumSize = new Vector2(0, 70);
        _status.AddThemeColorOverride("font_color", new Color(0.72f, 0.68f, 0.56f, 1f));
        root.AddChild(_status);
    }

    private void ShowHome()
    {
        ClearContent();
        AddSectionTitle("御案四入口 · V2 平行链路");
        var actions = new HBoxContainer();
        actions.AddThemeConstantOverride("separation", 12);
        _content.AddChild(actions);
        AddButton(actions, "起驾西园", TravelWestGarden);
        AddButton(actions, "进入西园军务", OpenWestGarden);
        AddButton(actions, "推进一旬", AdvanceXun);
        AddButton(actions, "朝会占位测试", CourtNotReady);
        _status.Text = "V2 Runtime 已组装：UI 只依赖 Contracts 接口。Legacy 链路未修改。";
        RefreshSnapshot();
    }

    private void ShowWestGarden()
    {
        ClearContent();
        AddSectionTitle("西园别苑 · 天子亲军密署");

        var body = new HBoxContainer();
        body.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        body.AddThemeConstantOverride("separation", 16);
        _content.AddChild(body);

        var overview = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        overview.AddThemeConstantOverride("separation", 10);
        body.AddChild(overview);
        overview.AddChild(new Label
        {
            Text = "西园军势\n\n状态来自 IGameStateReader 快照。\n军务动作通过 IWestGardenService 执行。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        });
        AddButton(overview, "刷新军簿", RefreshWestGarden);

        var actions = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        actions.AddThemeConstantOverride("separation", 8);
        body.AddChild(actions);
        actions.AddChild(new Label { Text = "军务处置" });

        var officer = new OptionButton { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        officer.AddItem("蹇硕 · 西园上军校尉");
        officer.SetItemMetadata(0, "jian_shuo");
        officer.AddItem("曹操 · 典军校尉");
        officer.SetItemMetadata(1, "cao_cao");
        officer.AddItem("张让 · 中官校尉");
        officer.SetItemMetadata(2, "zhang_rang");
        actions.AddChild(officer);

        var paySpin = new SpinBox
        {
            MinValue = 0,
            MaxValue = Math.Max(0, _runtime.State.GetSnapshot().PrivateTreasury),
            Step = 100,
            Value = 1000,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        actions.AddChild(paySpin);
        AddButton(actions, "发内帑犒军", () =>
        {
            string officerId = officer.GetSelectedMetadata().AsString();
            ShowResult(_runtime.WestGarden.PayArmy(new ArmyPayCommand((int)paySpin.Value, officerId)));
        });

        var recruitSpin = new SpinBox
        {
            MinValue = 1000,
            MaxValue = Math.Max(1000, _runtime.State.GetSnapshot().WestGardenArmyCapacity - _runtime.State.GetSnapshot().WestGardenArmySize),
            Step = 1000,
            Value = 1000,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        actions.AddChild(recruitSpin);
        AddButton(actions, "下诏募兵", () =>
        {
            ShowResult(_runtime.WestGarden.RecruitArmy(new RecruitArmyCommand((int)recruitSpin.Value)));
            ShowWestGarden();
        });

        AddButton(actions, "合上军簿并返回御案", ShowHome);
        RefreshSnapshot();
    }

    private void RefreshWestGarden()
    {
        ShowWestGarden();
        _status.Text = "军簿已从 IGameStateReader 重新读取。";
    }

    private void TravelWestGarden()
    {
        var result = _runtime.Travel.Travel(new TravelCommand("西园"));
        ShowResult(result);
        if (result.Success) ShowWestGarden();
    }

    private void OpenWestGarden()
    {
        if (_runtime.State.GetSnapshot().CurrentLocation != "西园")
        {
            _status.Text = "当前尚未驻跸西园，请先执行“起驾西园”。";
            return;
        }
        ShowWestGarden();
    }

    private async void AdvanceXun()
    {
        var result = await _runtime.Turns.AdvanceXunAsync();
        _status.Text = result.Success
            ? "V2 已通过 ITurnService 推进一旬。"
            : $"推进失败：{result.ErrorMessage}";
        RefreshSnapshot();
    }

    private void CourtNotReady()
    {
        _status.Text = "朝会 V2 尚未接入，已明确返回失败，不会静默执行旧链路。";
    }

    private void ShowResult(ActionResult result)
    {
        _status.Text = result.Success
            ? $"{result.Title}\n{result.StoryText}"
            : $"{result.Title}：{result.StoryText}";
        RefreshSnapshot();
    }

    private void AddSectionTitle(string text)
    {
        var title = new Label
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.AddThemeFontSizeOverride("font_size", 21);
        title.AddThemeColorOverride("font_color", new Color(0.86f, 0.68f, 0.30f, 1f));
        _content.AddChild(title);
    }

    private static void AddButton(Container parent, string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(190, 42),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        button.Pressed += action;
        parent.AddChild(button);
    }

    private void ClearContent()
    {
        foreach (Node child in _content.GetChildren())
            child.QueueFree();
    }

    private void RefreshSnapshot()
    {
        var state = _runtime.State.GetSnapshot();
        _snapshot.Text =
            $"状态快照：{state.ReignTitle}{state.ReignYear}年 · {state.Year}年{state.Month}月第{state.Xun}旬\n" +
            $"所在地：{state.CurrentLocation} ｜ 皇权：{state.ImperialPower} ｜ 国库：{state.Treasury}万 ｜ 私库：{state.PrivateTreasury}万 ｜ 民心：{state.PopularSupport}\n" +
            $"西园军：{state.WestGardenArmySize}/{state.WestGardenArmyCapacity} ｜ 士气：{state.WestGardenMorale} ｜ 忠诚：{state.WestGardenLoyalty}";
    }
}
