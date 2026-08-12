using System;
using Godot;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2;

public partial class MainSceneV2 : Control
{
    private V2Runtime _runtime = null!;
    private Label _status = null!;
    private Label _snapshot = null!;

    public override void _Ready()
    {
        _runtime = V2RuntimeFactory.CreateDefault();
        BuildUi();
        RefreshSnapshot();
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
            Position = new Vector2(60, 40),
            Size = new Vector2(1160, 640)
        };
        root.AddThemeConstantOverride("separation", 16);
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
        _snapshot.AddThemeFontSizeOverride("font_size", 18);
        root.AddChild(_snapshot);

        var actions = new HBoxContainer();
        actions.AddThemeConstantOverride("separation", 12);
        root.AddChild(actions);
        AddButton(actions, "起驾西园", TravelWestGarden);
        AddButton(actions, "西园募兵 1000", RecruitArmy);
        AddButton(actions, "推进一旬", AdvanceXun);
        AddButton(actions, "朝会占位测试", CourtNotReady);

        _status = new Label
        {
            Text = "V2 Runtime 已组装：UI 只依赖 Contracts 接口。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _status.AddThemeColorOverride("font_color", new Color(0.72f, 0.68f, 0.56f, 1f));
        root.AddChild(_status);
    }

    private static void AddButton(HBoxContainer row, string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(190, 48)
        };
        button.Pressed += action;
        row.AddChild(button);
    }

    private void TravelWestGarden()
    {
        var result = _runtime.Travel.Travel(new TravelCommand("西园"));
        ShowResult(result);
    }

    private void RecruitArmy()
    {
        var result = _runtime.WestGarden.RecruitArmy(new RecruitArmyCommand(1000));
        ShowResult(result);
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

    private void RefreshSnapshot()
    {
        var state = _runtime.State.GetSnapshot();
        _snapshot.Text =
            $"状态快照：{state.ReignTitle}{state.ReignYear}年 · {state.Year}年{state.Month}月第{state.Xun}旬\n" +
            $"所在地：{state.CurrentLocation} ｜ 皇权：{state.ImperialPower} ｜ 国库：{state.Treasury}万 ｜ 私库：{state.PrivateTreasury}万 ｜ 民心：{state.PopularSupport}\n" +
            $"西园军：{state.WestGardenArmySize}/{state.WestGardenArmyCapacity} ｜ 士气：{state.WestGardenMorale} ｜ 忠诚：{state.WestGardenLoyalty}";
    }
}
