using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        _status = new Label
        {
            Text = "V2 Runtime 已组装：UI 只依赖 Contracts 接口。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _status.CustomMinimumSize = new Vector2(0, 54);
        _status.AddThemeColorOverride("font_color", new Color(0.72f, 0.68f, 0.56f, 1f));
        root.AddChild(_status);

        _content = new VBoxContainer();
        _content.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _content.AddThemeConstantOverride("separation", 12);
        root.AddChild(_content);
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
        AddButton(actions, "打开御案折匣", ShowEdicts);
        AddButton(actions, "查看朝臣名册", ShowMinisters);
        AddButton(actions, "进入西园军务", OpenWestGarden);
        AddButton(actions, "推进一旬", ShowTurnControl);
        AddButton(actions, "进入宣政殿朝会", ShowCourt);
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

    private void ShowEdicts()
    {
        ClearContent();
        AddSectionTitle("御案折匣 · 尚书台卷宗");
        _content.AddChild(new Label
        {
            Text = "待批奏折通过 IEdictService 读取；朱批以 ResolveEdictCommand 提交。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        });

        var body = new HBoxContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        body.AddThemeConstantOverride("separation", 14);
        _content.AddChild(body);
        var list = new ItemList
        {
            CustomMinimumSize = new Vector2(360, 0),
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        body.AddChild(list);
        var detail = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        detail.AddThemeConstantOverride("separation", 8);
        body.AddChild(detail);
        var content = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        content.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        detail.AddChild(content);
        var options = new VBoxContainer();
        options.AddThemeConstantOverride("separation", 8);
        detail.AddChild(options);

        var edicts = _runtime.Edicts.GetPendingEdicts();
        for (int i = 0; i < edicts.Count; i++)
            list.AddItem($"【{edicts[i].Type}】{edicts[i].Title} · 剩余{edicts[i].ExpiryXun}旬");

        void RenderEdict(long index)
        {
            if (index < 0 || index >= edicts.Count) return;
            var edict = edicts[(int)index];
            content.Text = $"【{edict.Title}】\n\n{edict.NarrativeContent}\n\n保质期：剩余 {edict.ExpiryXun} 旬";
            ClearChildrenAfter(options, 0);
            for (int optionIndex = 0; optionIndex < edict.Options.Count; optionIndex++)
            {
                int capturedIndex = optionIndex;
                var option = edict.Options[optionIndex];
                AddButton(options, $"朱批：{option.Description}", () =>
                {
                    var result = _runtime.Edicts.Resolve(new ResolveEdictCommand(edict.Id, capturedIndex));
                    ShowResult(result);
                    ShowEdicts();
                });
            }
        }

        list.ItemSelected += RenderEdict;
        if (edicts.Count > 0) RenderEdict(0);
        else content.Text = "当前没有待批奏折。推进旬日后，旬务调度可能生成新的奏折。";
        AddButton(_content, "合上卷宗 · 返回御案", ShowHome);
        RefreshSnapshot();
    }

    private void ShowMinisters()
    {
        ClearContent();
        AddSectionTitle("百官名册 · 朝臣状态");
        _content.AddChild(new Label
        {
            Text = "名册使用 MinisterSnapshot 只读切片；数值以品阶显示，不暴露 NPC 内部五维。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        });

        var body = new HBoxContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        body.AddThemeConstantOverride("separation", 14);
        _content.AddChild(body);
        var list = new ItemList
        {
            CustomMinimumSize = new Vector2(390, 0),
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        body.AddChild(list);
        var detail = new Label
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        body.AddChild(detail);

        var ministers = _runtime.State.GetMinisters()
            .Where(minister => minister.IsActive)
            .OrderByDescending(minister => minister.Power)
            .ThenBy(minister => minister.Name)
            .ToList();
        for (int i = 0; i < ministers.Count; i++)
        {
            var minister = ministers[i];
            string status = minister.IsHostile ? "敌对" : "在朝";
            list.AddItem($"{minister.Name} · {minister.Title} · {status}");
        }

        list.ItemSelected += index => RenderMinister(index);
        void RenderMinister(long index)
        {
            if (index < 0 || index >= ministers.Count) return;
            var minister = ministers[(int)index];
            detail.Text = $"【{minister.Name}】\n\n" +
                $"官职：{minister.Title}\n" +
                $"派系：{minister.Faction}\n" +
                $"状态：{(minister.IsHostile ? "敌对" : minister.IsActive ? "在朝" : "下野")}\n\n" +
                $"圣眷：{FavorabilityGrade(minister.Favorability)}\n" +
                $"朝堂影响：{InfluenceGrade(minister.Power)}\n" +
                $"操守：{IntegrityGrade(minister.Corruption)}";
        }
        if (ministers.Count > 0)
        {
            list.Select(0, true);
            RenderMinister(0);
        }
        AddButton(_content, "合上名册 · 返回御案", ShowHome);
        RefreshSnapshot();
    }

    private static string FavorabilityGrade(int value) => value switch
    {
        < 25 => "疏冷",
        < 50 => "中立",
        < 75 => "亲近",
        _ => "倚重"
    };

    private static string InfluenceGrade(int value) => value switch
    {
        < 25 => "微弱",
        < 50 => "有限",
        < 75 => "显著",
        _ => "权重"
    };

    private static string IntegrityGrade(int value) => value switch
    {
        >= 75 => "浑浊",
        >= 50 => "有瑕",
        >= 25 => "清正",
        _ => "廉直"
    };

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

    private async void ShowCourt()
    {
        ClearContent();
        AddSectionTitle("宣政殿 · V2 大朝会");
        _content.AddChild(new Label
        {
            Text = "朝会状态、议题与裁断通过 ICourtService 管理；V2 不直接访问 GameEngine。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        });

        var opening = await _runtime.Court.StartSessionAsync();
        _content.AddChild(new Label
        {
            Text = opening,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        });

        var hostRow = new HBoxContainer();
        hostRow.AddThemeConstantOverride("separation", 8);
        _content.AddChild(hostRow);
        hostRow.AddChild(new Label { Text = "主持人：" });
        var host = new OptionButton();
        host.AddItem("何进");
        host.SetItemMetadata(0, "he_jin");
        host.AddItem("曹操");
        host.SetItemMetadata(1, "cao_cao");
        host.AddItem("张让");
        host.SetItemMetadata(2, "zhang_rang");
        host.AddItem("蹇硕");
        host.SetItemMetadata(3, "jian_shuo");
        hostRow.AddChild(host);

        var topics = new VBoxContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        topics.AddThemeConstantOverride("separation", 8);
        _content.AddChild(topics);
        AddCourtTopic(topics, "整军备寇", "军务", "military_readiness", new[]
        {
            ("准何进整北军", "military_north", "扩整北军，强化外戚军务"),
            ("命曹操整西园军", "military_garden", "将西园军务交给曹操"),
            ("令张让核军费", "military_funds", "先核军费，再定军务")
        }, host);
        AddCourtTopic(topics, "国帑筹措", "财计", "treasury", new[]
        {
            ("令张让筹措内帑", "treasury_eunuch", "让中官介入财计")
        }, host);
        AddCourtTopic(topics, "整饬宦官", "党争", "eunuchs", new[]
        {
            ("训诫张让", "eunuch_reprimand", "公开训诫中官"),
            ("安抚张让", "eunuch_reassure", "以圣眷稳定内廷")
        }, host);
        AddButton(_content, "退朝 · 返回御案", ShowHome);
        RefreshSnapshot();
    }

    private void AddCourtTopic(
        VBoxContainer parent,
        string title,
        string category,
        string topicId,
        (string Label, string Id, string Hint)[] decisions,
        OptionButton host)
    {
        var panel = new VBoxContainer();
        panel.AddThemeConstantOverride("separation", 5);
        parent.AddChild(panel);
        panel.AddChild(new Label { Text = $"【{category}】{title}" });
        foreach (var decision in decisions)
        {
            AddButton(panel, decision.Label, () => ExecuteCourtDecisionAsync(topicId, decision.Id, decision.Hint, host));
        }
    }

    private async void ExecuteCourtDecisionAsync(string topicId, string decisionId, string hint, OptionButton host)
    {
        _status.Text = $"正在处理：{hint}";
        try
        {
            var command = new CourtDecisionCommand(topicId, decisionId, host.GetSelectedMetadata().AsString());
            var task = _runtime.Court.ExecuteDecisionAsync(command);
            var completed = await Task.WhenAny(task, Task.Delay(15000));
            if (completed != task)
            {
                _status.Text = "朝议处理超时：规则服务未在 15 秒内返回。";
                return;
            }
            ShowResult(await task);
        }
        catch (Exception ex)
        {
            _status.Text = $"朝议处理异常：{ex.Message}";
        }
    }

    private void ShowTurnControl()
    {
        ClearContent();
        AddSectionTitle("时序推演 · 旬日流转");
        _content.AddChild(new Label
        {
            Text = "旬推进由 ITurnService 统一调度。每一旬可能触发奏折过期、州郡叛乱、历史事件或结局判定。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        });

        var controls = new HBoxContainer();
        controls.AddThemeConstantOverride("separation", 10);
        _content.AddChild(controls);
        AddButton(controls, "推进一旬", AdvanceOneXun);
        var count = new SpinBox
        {
            MinValue = 1,
            MaxValue = 30,
            Step = 1,
            Value = 3,
            CustomMinimumSize = new Vector2(160, 42)
        };
        controls.AddChild(count);
        AddButton(controls, "快进指定旬数", () => FastForwardXun((int)count.Value));

        var latest = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        _content.AddChild(latest);
        RenderTurnSummary(latest);
        AddButton(_content, "停止推演 · 返回御案", ShowHome);
        RefreshSnapshot();
    }

    private async void AdvanceOneXun()
    {
        _status.Text = "正在推进一旬：等待 ITurnService 完成旬结算……";
        var result = await _runtime.Turns.AdvanceXunAsync();
        _status.Text = result.Success
            ? $"旬结算完成：时间、事件与结局判定已由 ITurnService 处理。新增事件：{FormatEvents(result.Events)}"
            : $"旬推进失败：{result.ErrorMessage}";
        ShowTurnControl();
    }

    private async void FastForwardXun(int count)
    {
        _status.Text = $"正在快进 {count} 旬：每旬逐步执行，不跳过规则结算……";
        var result = await _runtime.Turns.FastForwardAsync(new FastForwardCommand(count));
        _status.Text = result.Success
            ? $"快进完成：请求 {result.RequestedXun} 旬，实际推进 {result.AdvancedXun} 旬。新增事件：{FormatEvents(result.Events)}"
            : $"快进失败：已推进 {result.AdvancedXun} 旬；{result.InterruptReason ?? "规则服务未返回原因。"}";
        if (result.Interrupted)
            _status.Text += " 游戏结局或临界状态已触发，快进已停止。";
        ShowTurnControl();
    }

    private static string FormatEvents(IReadOnlyList<string> events)
    {
        if (events.Count == 0) return "无";
        return string.Join("；", events.Count > 3 ? events.Skip(events.Count - 3) : events);
    }

    private void RenderTurnSummary(Label target)
    {
        var state = _runtime.State.GetSnapshot();
        string recent = state.Chronicle.Count == 0
            ? "暂无旬日记录。"
            : string.Join("\n", state.Chronicle.Skip(Math.Max(0, state.Chronicle.Count - 8)));
        target.Text = $"当前结局：{state.Outcome}\n\n最近旬日记录：\n{recent}";
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
        string story = StripBbCode(result.StoryText);
        _status.Text = result.Success
            ? $"{result.Title}\n{story}"
            : $"{result.Title}：{story}";
        RefreshSnapshot();
    }

    private static string StripBbCode(string text)
    {
        return Regex.Replace(text ?? string.Empty, @"\[/?(?:color(?:=[^\]]+)?|b|i|u)\]", string.Empty);
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
