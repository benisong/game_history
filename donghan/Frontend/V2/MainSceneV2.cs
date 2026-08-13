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
        AddButton(actions, "起驾巡幸", ShowTravel);
        AddButton(actions, "进入黄门密札", ShowIntel);
        AddButton(actions, "进入西园军务", OpenWestGarden);
        AddButton(actions, "推进一旬", AdvanceXun);
        AddButton(actions, "朝会占位测试", CourtNotReady);
        _status.Text = "V2 Runtime 已组装：UI 只依赖 Contracts 接口。Legacy 链路未修改。";
        RefreshSnapshot();
    }

    private void ShowTravel()
    {
        ClearContent();
        AddSectionTitle("龙辇巡幸 · 驻跸择所");
        _content.AddChild(new Label
        {
            Text = "请陛下定夺今日驻跸之所。目的地确认后，V2 将通过 ITravelService 执行并返回 ActionResult。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        });

        var destinations = new HBoxContainer
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        destinations.AddThemeConstantOverride("separation", 12);
        _content.AddChild(destinations);

        AddTravelCard(destinations, "宣政殿", "玉阶临朝", "临朝听政，批阅奏折，召见百官。", () => TravelTo("宣政殿"));
        AddTravelCard(destinations, "后宫", "温德炉烟", "暂离外朝，调养龙体，恢复精神。", () => TravelTo("后宫"));
        AddTravelCard(destinations, "西园", "西园秘营", "亲阅私库与新军，处理军务。", () => TravelTo("西园"));

        AddButton(_content, "龙辇免起 · 返回御案", ShowHome);
        RefreshSnapshot();
    }

    private void AddTravelCard(Container parent, string destination, string heading, string description, Action travel)
    {
        var card = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        card.AddThemeConstantOverride("separation", 10);
        parent.AddChild(card);
        card.AddChild(new Label
        {
            Text = $"{heading}\n【{destination}】",
            HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        });
        card.AddChild(new Label
        {
            Text = description,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        });
        AddButton(card, $"起驾{destination}", travel);
    }

    private void ShowIntel()
    {
        ClearContent();
        AddSectionTitle("黄门密札 · 天下情报台");
        _content.AddChild(new Label
        {
            Text = "州郡情报由 IGameStateReader 提供；处置命令统一提交给 IIntelService。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        });

        var body = new HBoxContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        body.AddThemeConstantOverride("separation", 14);
        _content.AddChild(body);

        var provinces = new ItemList
        {
            CustomMinimumSize = new Vector2(300, 0),
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        body.AddChild(provinces);
        var provinceList = new List<ProvinceSnapshot>();
        foreach (var province in GetProvinceList())
        {
            provinceList.Add(province);
            provinces.AddItem($"{(province.IsRebelling ? "⚡" : "○")} {province.Name}\n民心{province.LocalSupport}｜守军{province.Garrison}｜{province.GovernorName switch { "" => "无太守", _ => province.GovernorName }}");
        }

        var detail = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        detail.AddThemeConstantOverride("separation", 8);
        body.AddChild(detail);
        var detailLabel = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        detailLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        detail.AddChild(detailLabel);

        var actionBox = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        actionBox.AddThemeConstantOverride("separation", 8);
        body.AddChild(actionBox);
        actionBox.AddChild(new Label { Text = "可行处置" });
        var actionHint = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        actionBox.AddChild(actionHint);

        void SelectProvince(long index)
        {
            if (index < 0 || index >= provinceList.Count) return;
            var province = provinceList[(int)index];
            var intel = _runtime.Intel.InspectProvince(new InspectProvinceCommand(province.Id));
            if (!intel.Success || intel.Province == null)
            {
                detailLabel.Text = intel.ErrorMessage ?? "州郡情报读取失败。";
                actionHint.Text = "暂无可用处置。";
                return;
            }
            RenderProvinceDetail(detailLabel, intel.Province);
            RenderIntelActions(actionBox, actionHint, intel.Province);
        }

        provinces.ItemSelected += SelectProvince;
        if (provinceList.Count > 0) SelectProvince(0);
        AddButton(_content, "收起密札 · 返回御案", ShowHome);
        RefreshSnapshot();
    }

    private IReadOnlyList<ProvinceSnapshot> GetProvinceList()
    {
        var result = new List<ProvinceSnapshot>();
        foreach (var candidate in new[] { "sili", "jizhou", "bingzhou", "yanzhou", "yuzhou", "jingzhou" })
        {
            if (_runtime.Intel.InspectProvince(new InspectProvinceCommand(candidate)).Province is { } snapshot)
                result.Add(snapshot);
        }
        return result;
    }

    private static void RenderProvinceDetail(Label target, ProvinceSnapshot province)
    {
        string rebellion = province.IsRebelling
            ? $"⚡ {province.RebelFaction}叛乱，已持续 {province.RebellionMonths} 个月"
            : "○ 安定无事";
        target.Text =
            $"【{province.Name}】\n\n当前局势：{rebellion}\n" +
            $"地方太守：{(string.IsNullOrEmpty(province.GovernorName) ? "暂无" : province.GovernorName)}\n" +
            $"地方民心：{province.LocalSupport}/100\n郡中守军：{province.Garrison} 人\n" +
            $"财富：{province.Wealth} 万\n防务等级：{province.DefenseLevel}/100\n距京：{province.Distance}";
    }

    private void RenderIntelActions(VBoxContainer actionBox, Label hint, ProvinceSnapshot province)
    {
        ClearChildrenAfter(actionBox, 2);
        hint.Text = province.IsRebelling ? "该州正在叛乱，可选择平叛或招安。" : "可进行太守任免。";
        AddButton(actionBox, "任命曹操为太守", () => ExecuteIntelAction(new ProvinceActionCommand(province.Id, ProvinceActionKind.AssignGovernor, "cao_cao")));
        if (!string.IsNullOrEmpty(province.GovernorId))
            AddButton(actionBox, "召还现任太守", () => ExecuteIntelAction(new ProvinceActionCommand(province.Id, ProvinceActionKind.RecallGovernor)));
        if (province.IsRebelling)
        {
            AddButton(actionBox, "出兵平叛（3000人）", () => ExecuteIntelAction(new ProvinceActionCommand(province.Id, ProvinceActionKind.SuppressRebellion, "cao_cao", 3000)));
            AddButton(actionBox, "遣使招安（说服）", () => ExecuteIntelAction(new ProvinceActionCommand(province.Id, ProvinceActionKind.PacifyRebellion, "cao_cao", Strategy: "说服")));
        }
    }

    private void ExecuteIntelAction(ProvinceActionCommand command)
    {
        ShowResult(_runtime.Intel.ExecuteProvinceAction(command));
        ShowIntel();
    }

    private static void ClearChildrenAfter(Container container, int keepCount)
    {
        var children = container.GetChildren();
        for (int i = keepCount; i < children.Count; i++)
            children[i].QueueFree();
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
        TravelTo("西园");
    }

    private void TravelTo(string destination)
    {
        var result = _runtime.Travel.Travel(new TravelCommand(destination));
        ShowResult(result);
        if (result.Success && destination == "西园") ShowWestGarden();
        else if (result.Success) ShowHome();
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
