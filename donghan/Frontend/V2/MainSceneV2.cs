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
        var actions = new FlowContainer();
        actions.AddThemeConstantOverride("separation", 12);
        _content.AddChild(actions);
        AddButton(actions, "起驾巡幸", ShowTravel);
        AddButton(actions, "大汉十三州全景沙盘", ShowGeopoliticsMap);
        AddButton(actions, "尚书台刺史大考课", ShowGovernorAppraisal);
        AddButton(actions, "进入黄门密札", ShowIntel);
        AddButton(actions, "打开御案折匣", ShowEdicts);
        AddButton(actions, "尚书台察举名册", ShowNominations);
        AddButton(actions, "查看朝臣名册", ShowMinisters);
        AddButton(actions, "进入西园军务", OpenWestGarden);
        AddButton(actions, "推进一旬", ShowTurnControl);
        AddButton(actions, "进入宣政殿朝会", ShowCourt);
        var location = _runtime.State.GetSnapshot().CurrentLocation;
        if (location == "后宫")
            AddButton(actions, "后宫休养", () => ExecuteSpecialAction(new SpecialActionCommand("harem_rest")));
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
        var detail = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        detail.AddThemeConstantOverride("separation", 10);
        body.AddChild(detail);

        var detailText = new Label
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        detail.AddChild(detailText);

        var actionBox = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        actionBox.AddThemeConstantOverride("separation", 8);
        detail.AddChild(actionBox);

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
            string traitsText = minister.Traits != null && minister.Traits.Count > 0
                ? string.Join(" · ", minister.Traits)
                : "无";

            detailText.Text = $"【{minister.Name}】\n\n" +
                $"官职：{minister.Title}\n" +
                $"派系：{minister.Faction} ｜ 性格：{minister.Personality} ｜ 风格：{minister.Style}\n" +
                $"状态：{(minister.IsHostile ? "敌对" : minister.IsActive ? "在朝" : "下野")}\n\n" +
                $"五维：武力 {minister.Martial} ｜ 统帅 {minister.Leadership} ｜ 政治 {minister.Politics} ｜ 魅力 {minister.Charisma} ｜ 野心 {minister.Ambition}\n" +
                $"特质：{traitsText}\n\n" +
                $"圣眷：{FavorabilityGrade(minister.Favorability)} ｜ 朝堂影响：{InfluenceGrade(minister.Power)} ｜ 操守：{IntegrityGrade(minister.Corruption)}";

            foreach (var child in actionBox.GetChildren())
            {
                child.QueueFree();
            }

            var btnBox = new HBoxContainer();
            btnBox.AddThemeConstantOverride("separation", 10);
            actionBox.AddChild(btnBox);

            AddButton(btnBox, $"直接籍没查抄（立威归国库）", () =>
            {
                ExecuteSpecialAction(new SpecialActionCommand("confiscate_direct", TargetNpcId: minister.Id));
                ShowMinisters();
            });

            AddButton(btnBox, $"正道升迁除官", () =>
            {
                ShowPromotionDialog(minister);
            });

            AddButton(btnBox, $"西园开榜卖官", () =>
            {
                ShowOfficeSaleDialog(minister);
            });

            AddButton(btnBox, $"命其经办赈灾（1000万）", () =>
            {
                ExecuteSpecialAction(new SpecialActionCommand("disaster_relief", 1000, minister.Id));
                ShowMinisters();
            });
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

    private void ShowPromotionDialog(MinisterSnapshot minister)
    {
        ClearContent();
        AddSectionTitle($"正道阶梯升迁 · 考课除官【{minister.Name}】");
        _content.AddChild(new Label
        {
            Text = $"当前官职：{minister.Title}（品阶：{minister.Traits?.FirstOrDefault() ?? "未定"}）。\n" +
                   "升迁铁律：按部就班一级一级升；若特旨超擢，破格拔擢一次最多不得跨越三品（超擢引发清流老臣微词）。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        });

        var positions = _runtime.Ranks.GetAllPositions();
        var scroll = new ScrollContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill, CustomMinimumSize = new Vector2(0, 320) };
        _content.AddChild(scroll);

        var list = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        list.AddThemeConstantOverride("separation", 8);
        scroll.AddChild(list);

        foreach (var pos in positions)
        {
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 10);

            string branchText = pos.Branch == DonghanEngine.Core.Politics.OfficialBranch.Civilian ? "文官" : "武官";
            var label = new Label
            {
                Text = $"【{pos.RankTier}品】{pos.Title}（{branchText}） ｜ 职责：{pos.Description}",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            row.AddChild(label);

            AddButton(row, $"诏拜【{pos.Title}】", () =>
            {
                var result = _runtime.Ranks.Promote(minister.Id, pos.Title);
                ShowResult(result);
                ShowMinisters();
            });

            list.AddChild(row);
        }

        AddButton(_content, "返回名册", ShowMinisters);
        RefreshSnapshot();
    }

    private void ShowOfficeSaleDialog(MinisterSnapshot minister)
    {
        ClearContent();
        AddSectionTitle($"西园万金堂 · 卖官鬻爵【{minister.Name}】");
        _content.AddChild(new Label
        {
            Text = "西园卖官通天法：只要钱给够，无视任何等级资历，哪怕白丁直接拜一品三公！\n" +
                   "代价警示：所得巨款直接进入天子私库，但天下民心大跌、清流士林耻与为伍！",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        });

        var positions = _runtime.Ranks.GetAllPositions();
        var scroll = new ScrollContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill, CustomMinimumSize = new Vector2(0, 320) };
        _content.AddChild(scroll);

        var list = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        list.AddThemeConstantOverride("separation", 8);
        scroll.AddChild(list);

        foreach (var pos in positions)
        {
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 10);

            var label = new Label
            {
                Text = $"【{pos.RankTier}品】{pos.Title} ｜ 明码标价：{pos.PriceInWan}万钱 ｜ 职责：{pos.Description}",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            row.AddChild(label);

            AddButton(row, $"卖官【{pos.PriceInWan}万】", () =>
            {
                var result = _runtime.Ranks.SellOffice(minister.Id, pos.Title);
                ShowResult(result);
                ShowMinisters();
            });

            list.AddChild(row);
        }

        AddButton(_content, "返回名册", ShowMinisters);
        RefreshSnapshot();
    }

    private void ShowGovernorAppraisal()
    {
        ClearContent();
        AddSectionTitle("尚书台年终大考课 · 刺史太守上计册");

        _content.AddChild(new Label
        {
            Text = "【上计考课】司徒府与尚书台每年岁终严考十三州封疆大吏：评定治绩民心、输赋额度与户口增殖。\n" +
                   "• 【上考】长吏治绩昭著，天子可诏令征拜内调入京任九卿（执金吾/太常/少府），收回地方兵印归中央直辖。\n" +
                   "• 刺史野心与朝廷威望博弈：若刺史野心高企（>=80）或朝廷威望微弱，刺史将称病抗旨拥兵自重！",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        });

        var report = _runtime.Appraisal.GetAnnualAppraisal();

        var scroll = new ScrollContainer
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, 400)
        };
        _content.AddChild(scroll);

        var list = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        list.AddThemeConstantOverride("separation", 10);
        scroll.AddChild(list);

        foreach (var r in report.Records)
        {
            var card = new VBoxContainer();
            card.AddThemeConstantOverride("separation", 6);
            list.AddChild(card);

            string gradeBadge = r.Grade switch
            {
                DonghanEngine.Core.Politics.AppraisalGrade.Superior => "🌟【上考·政绩优异】",
                DonghanEngine.Core.Politics.AppraisalGrade.Standard => "📜【中考·平庸尽职】",
                _ => "⚠️【下考·治下凋敝/跋扈】"
            };

            var title = new Label
            {
                Text = $"{gradeBadge} {r.ProvinceName}太守【{r.GovernorName}】 ｜ 治安民心：{r.LocalSupport} ｜ 岁纳田赋：{r.TaxContribution}万钱 ｜ 野心：{r.Ambition} ｜ 忠诚：{r.Favorability}"
            };
            card.AddChild(title);

            var desc = new Label
            {
                Text = $"• 考评奏疏：{r.EvaluationReport}\n• 尚书台建议：{r.RecommendedAction}",
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            card.AddChild(desc);

            var btnRow = new HBoxContainer();
            btnRow.AddThemeConstantOverride("separation", 10);
            card.AddChild(btnRow);

            AddButton(btnRow, $"征拜【执金吾】内调还京", () =>
            {
                var res = _runtime.Appraisal.PromoteGovernorToCourt(r.GovernorId, "执金吾");
                ShowResult(res);
                ShowGovernorAppraisal();
            });

            AddButton(btnRow, $"征拜【太常卿】入阁辅政", () =>
            {
                var res = _runtime.Appraisal.PromoteGovernorToCourt(r.GovernorId, "太常");
                ShowResult(res);
                ShowGovernorAppraisal();
            });
        }

        AddButton(_content, "返回御案", ShowHome);
        RefreshSnapshot();
    }

    private void ShowGeopoliticsMap()
    {
        ClearContent();
        AddSectionTitle("大汉十三州全景沙盘 · 地缘与土地产权图谱");

        _content.AddChild(new Label
        {
            Text = "【帝王舆图】实时呈现天下十三州割据归属、土地产权格局（国家官田 vs 世家私田）与人口承载负荷。\n" +
                   "• 产权规则：国家官田纳全赋，世家私田少收四成；战乱焦土半年休耕；天子亲军平叛收归官田。\n" +
                   "• 点击各州郡可一键下诏执行度田、水利、赎田或遣将平叛。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        });

        var scroll = new ScrollContainer
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, 420)
        };
        _content.AddChild(scroll);

        var grid = new GridContainer
        {
            Columns = 2,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        grid.AddThemeConstantOverride("h_separation", 14);
        grid.AddThemeConstantOverride("v_separation", 14);
        scroll.AddChild(grid);

        var provinces = GetProvinceList();
        foreach (var p in provinces)
        {
            var card = new VBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(380, 180)
            };
            card.AddThemeConstantOverride("separation", 6);
            grid.AddChild(card);

            // 承载负荷计算
            int loadPercent = p.LandCarryingCapacity > 0 ? (p.Population * 100) / p.LandCarryingCapacity : 100;
            string loadStatus = loadPercent switch
            {
                > 120 => "【红色重度超载·流民四起】",
                > 100 => "【黄色轻度超载·隐忧】",
                _ => "【绿色沃野充盈·宜居】"
            };

            // 状态标签
            string statusTag = p.IsRebelling ? " ｜ [🚨 烽烟叛乱中]" : "";
            string scorchTag = p.ScorchedMonthsRemaining > 0 ? $" ｜ [🔥 战乱焦土休耕中（余{p.ScorchedMonthsRemaining}月）]" : "";

            var titleLabel = new Label
            {
                Text = $"【{p.Name}】 归属：{p.ControllingFactionName}{statusTag}{scorchTag}",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            card.AddChild(titleLabel);

            int stateRatio = (p.StateControlledLand * 100) / Math.Max(1, p.StateControlledLand + p.GentryControlledLand);
            int gentryRatio = 100 - stateRatio;

            var detailsLabel = new Label
            {
                Text = $"• 人口：{p.Population:N0} / 承载上限：{p.LandCarryingCapacity:N0} ({loadPercent}%) {loadStatus}\n" +
                       $"• 土地产权：国家官田 {p.StateControlledLand:N0} 顷 ({stateRatio}%) ｜ 世家私田 {p.GentryControlledLand:N0} 顷 ({gentryRatio}%)\n" +
                       $"• 焦土荒田：{p.ScorchedLand:N0} 顷 ｜ 太守：{(string.IsNullOrEmpty(p.GovernorName) ? "暂缺" : p.GovernorName)} ｜ 驻军：{p.Garrison} 人 ｜ 治安民心：{p.LocalSupport}",
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            card.AddChild(detailsLabel);

            // 快捷行政动作按钮排
            var btnRow = new FlowContainer();
            btnRow.AddThemeConstantOverride("separation", 6);
            card.AddChild(btnRow);

            AddButton(btnRow, $"度田清查", () =>
            {
                var res = _runtime.Agriculture.SurveyLand(p.Id, DonghanEngine.Core.Economy.CadastralSurveyIntensity.Standard);
                ShowResult(res);
                ShowGeopoliticsMap();
            });

            AddButton(btnRow, $"大兴水利", () =>
            {
                var res = _runtime.Agriculture.BuildIrrigation(p.Id);
                ShowResult(res);
                ShowGeopoliticsMap();
            });

            if (p.StateControlledLand >= 1000)
            {
                AddButton(btnRow, $"准世家赎田", () =>
                {
                    var res = _runtime.Agriculture.RepurchaseGentryLand(p.Id, 5000);
                    ShowResult(res);
                    ShowGeopoliticsMap();
                });
            }

            if (p.IsRebelling)
            {
                AddButton(btnRow, $"出兵平叛", () =>
                {
                    ExecuteIntelAction(new ProvinceActionCommand(p.Id, ProvinceActionKind.SuppressRebellion, "cao_cao", 3000));
                    ShowGeopoliticsMap();
                });
            }
        }

        AddButton(_content, "返回御案", ShowHome);
        RefreshSnapshot();
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

    private IReadOnlyList<ProvinceSnapshot> GetProvinceList() => _runtime.State.GetAllProvinces();

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
        var governorSelector = new OptionButton();
        foreach (var candidate in new[] { ("曹操", "cao_cao"), ("何进", "he_jin"), ("张让", "zhang_rang"), ("蹇硕", "jian_shuo") })
        {
            governorSelector.AddItem(candidate.Item1);
            governorSelector.SetItemMetadata(governorSelector.ItemCount - 1, candidate.Item2);
        }
        actionBox.AddChild(governorSelector);
        AddButton(actionBox, "任命所选朝臣", () => ExecuteIntelAction(new ProvinceActionCommand(
            province.Id,
            ProvinceActionKind.AssignGovernor,
            governorSelector.GetSelectedMetadata().AsString())));
        if (!string.IsNullOrEmpty(province.GovernorId))
            AddButton(actionBox, "召还现任太守", () => ExecuteIntelAction(new ProvinceActionCommand(province.Id, ProvinceActionKind.RecallGovernor)));

        AddButton(actionBox, "度田丈量（严明清查）", () =>
        {
            var result = _runtime.Agriculture.SurveyLand(province.Id, DonghanEngine.Core.Economy.CadastralSurveyIntensity.Standard);
            ShowResult(result);
            ShowIntel();
        });

        AddButton(actionBox, "兴修水利（大兴官渠1000万）", () =>
        {
            var result = _runtime.Agriculture.BuildIrrigation(province.Id);
            ShowResult(result);
            ShowIntel();
        });

        AddButton(actionBox, "准世家赎买官田（5000顷）", () =>
        {
            var result = _runtime.Agriculture.RepurchaseGentryLand(province.Id, 5000);
            ShowResult(result);
            ShowIntel();
        });

        if (province.IsRebelling)
        {
            AddButton(actionBox, "出兵平叛（3000人）", () => ExecuteIntelAction(new ProvinceActionCommand(province.Id, ProvinceActionKind.SuppressRebellion, "cao_cao", 3000)));

            actionBox.AddChild(new Label { Text = "遣使招安策略（可多选）" });
            var sowDiscord = new CheckBox { Text = "离间计" };
            var persuade = new CheckBox { Text = "说服", ButtonPressed = true };
            var disasterRelief = new CheckBox { Text = "赈灾" };
            var punish = new CheckBox { Text = "惩治" };
            actionBox.AddChild(sowDiscord);
            actionBox.AddChild(persuade);
            actionBox.AddChild(disasterRelief);
            actionBox.AddChild(punish);

            var reliefSpin = new SpinBox
            {
                MinValue = 500,
                MaxValue = 10000,
                Step = 500,
                Value = 500,
                Prefix = "赈银：",
                Suffix = " 万钱",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            actionBox.AddChild(reliefSpin);
            AddButton(actionBox, "遣使招安", () =>
            {
                var strategies = string.Join(",", new[]
                {
                    sowDiscord.ButtonPressed ? "离间" : "",
                    persuade.ButtonPressed ? "说服" : "",
                    disasterRelief.ButtonPressed ? "赈灾" : "",
                    punish.ButtonPressed ? "惩治" : ""
                }.Where(value => !string.IsNullOrEmpty(value)));
                var relief = disasterRelief.ButtonPressed ? (int)reliefSpin.Value : 0;
                ExecuteIntelAction(new ProvinceActionCommand(
                    province.Id,
                    ProvinceActionKind.PacifyRebellion,
                    "cao_cao",
                    Strategy: strategies,
                    ReliefGold: relief));
            });
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

    private void ExecuteSpecialAction(SpecialActionCommand command)
    {
        ShowResult(_runtime.SpecialActions.Execute(command));
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

        var freeRow = new HBoxContainer();
        freeRow.AddThemeConstantOverride("separation", 8);
        _content.AddChild(freeRow);
        var freeInput = new LineEdit
        {
            PlaceholderText = "亲拟圣旨，例如：命曹操整饬西园军务",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        freeRow.AddChild(freeInput);
        AddButton(freeRow, "宣旨", () => ExecuteFreeEdictAsync(freeInput, host));

        AddButton(_content, "举荐将才 · 打开专议", ShowTalentCourtTopic);
        var courtScroll = new ScrollContainer
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        _content.AddChild(courtScroll);
        var topics = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, 900)
        };
        topics.AddThemeConstantOverride("separation", 8);
        courtScroll.AddChild(topics);
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
        AddCourtTopic(topics, "举荐将才", "用人", "talent", new[]
        {
            ("召见曹操", "talent_cao", "查看曹操任事意见"),
            ("召见蹇硕", "talent_jian", "查看蹇硕任事意见"),
            ("转黄门密札任官", "intel", "转往州郡情报处理地方任免")
        }, host);
        AddButton(_content, "开仓赈灾（曹操经办）", () => ExecuteSpecialAction(new SpecialActionCommand("disaster_relief", 1000, "cao_cao")));
        AddButton(_content, "抄家籍没张让", () => ExecuteSpecialAction(new SpecialActionCommand("confiscation", TargetNpcId: "zhang_rang", Destination: "西园")));
        AddButton(_content, "退朝 · 返回御案", ShowHome);
        RefreshSnapshot();
    }

    private void ShowTalentCourtTopic()
    {
        ClearContent();
        AddSectionTitle("宣政殿 · 举荐将才");
        _content.AddChild(new Label
        {
            Text = "乱世将起，朝廷需择可用之才。请陛下选择召见对象，或转入黄门密札处理地方任官。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        });

        var host = new OptionButton();
        foreach (var pair in new[] { ("何进", "he_jin"), ("曹操", "cao_cao"), ("张让", "zhang_rang"), ("蹇硕", "jian_shuo") })
        {
            host.AddItem(pair.Item1);
            host.SetItemMetadata(host.ItemCount - 1, pair.Item2);
        }
        _content.AddChild(host);
        AddButton(_content, "召见曹操", () => ExecuteCourtDecisionAsync("talent", "talent_cao", "查看曹操任事意见", host));
        AddButton(_content, "召见蹇硕", () => ExecuteCourtDecisionAsync("talent", "talent_jian", "查看蹇硕任事意见", host));
        AddButton(_content, "转黄门密札任官", ShowIntel);
        AddButton(_content, "返回朝会议题", ShowCourt);
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
            AddButton(panel, decision.Label, () =>
            {
                switch (decision.Id)
                {
                    case "intel":
                        ShowIntel();
                        break;
                    case "show_cao":
                    case "talent_cao":
                        ShowMinisters();
                        break;
                    case "show_jian":
                    case "talent_jian":
                        ShowMinisters();
                        break;
                    case "travel_garden":
                        TravelTo("西园");
                        break;
                    case "back_topics":
                        ShowCourt();
                        break;
                    default:
                        ExecuteCourtDecisionAsync(topicId, decision.Id, decision.Hint, host);
                        break;
                }
            });
        }
    }

    private async void ExecuteFreeEdictAsync(LineEdit input, OptionButton host)
    {
        if (string.IsNullOrWhiteSpace(input.Text))
        {
            _status.Text = "亲拟圣旨未成：诏令内容不能为空。";
            return;
        }
        _status.Text = "正在传旨：等待朝会回奏……";
        var result = await _runtime.Court.ExecuteFreeEdictAsync(
            new FreeEdictCommand(input.Text.Trim(), host.GetSelectedMetadata().AsString()));
        ShowResult(result);
        if (result.Success) input.Clear();
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

        var scroll = new ScrollContainer
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        _content.AddChild(scroll);

        var body = new HBoxContainer();
        body.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        body.AddThemeConstantOverride("separation", 16);
        scroll.AddChild(body);

        var overview = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        overview.AddThemeConstantOverride("separation", 10);
        body.AddChild(overview);
        overview.AddChild(new Label
        {
            Text = "西园军势\n\n状态来自 IGameStateReader 快照。\n军务动作通过 IWestGardenService 执行。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        });
        AddButton(overview, "刷新军簿", RefreshWestGarden);
        AddButton(overview, "鬻官纳钱", () => ExecuteSpecialAction(new SpecialActionCommand("sell_office")));

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

        AddButton(actions, "校场大阅（大治军演）", () =>
        {
            string officerId = officer.GetSelectedMetadata().AsString();
            ShowResult(_runtime.WestGarden.DrillArmy(new ArmyDrillCommand((int)paySpin.Value, officerId)));
        });

        AddButton(actions, "内库大赏三军（鼓舞士气立威）", () =>
        {
            var result = _runtime.SpecialActions.Execute(new SpecialActionCommand("grant_military_bonus"));
            ShowResult(result);
            ShowWestGarden();
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

    private void ShowNominations()
    {
        ClearContent();
        AddSectionTitle("尚书台折匣 · 岁举孝廉名册");
        _content.AddChild(new Label
        {
            Text = "各大世家门阀所荐孝廉名册。陛下可御批除官以固门阀归心，亦可驳回以抑豪强（被弃名士或出走关东）。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        });

        var nominations = _runtime.Nominations.GetPendingNominations();
        if (nominations.Count == 0)
        {
            _content.AddChild(new Label { Text = "暂无世家呈递孝廉名册。" });
            AddButton(_content, "返回御案", ShowHome);
            return;
        }

        var scroll = new ScrollContainer
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, 320)
        };
        _content.AddChild(scroll);

        var listContainer = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        listContainer.AddThemeConstantOverride("separation", 10);
        scroll.AddChild(listContainer);

        foreach (var candidate in nominations)
        {
            var card = new PanelContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            var cardBox = new VBoxContainer();
            cardBox.AddThemeConstantOverride("separation", 6);
            card.AddChild(cardBox);

            var titleLabel = new Label
            {
                Text = $"【{candidate.Name}】（举荐门阀：{candidate.SponsoringFamilyId} ｜ 原籍：{candidate.NativeProvince}）",
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            titleLabel.AddThemeColorOverride("font_color", new Color(0.95f, 0.77f, 0.28f, 1f));
            cardBox.AddChild(titleLabel);

            cardBox.AddChild(new Label
            {
                Text = $"五维属性：政治 {candidate.Politics} ｜ 智谋 {candidate.Intelligence} ｜ 魅力 {candidate.Charisma} ｜ 武力 {candidate.Martial}\n" +
                       $"荐辟拟拜：{candidate.RecommendedOfficeTitle} ｜ 评语：{candidate.CandidateBio}",
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            });

            var btnRow = new HBoxContainer();
            btnRow.AddThemeConstantOverride("separation", 12);
            cardBox.AddChild(btnRow);

            AddButton(btnRow, $"御批除拜【{candidate.RecommendedOfficeTitle}】", () =>
            {
                var result = _runtime.Nominations.Appoint(candidate, candidate.RecommendedOfficeTitle);
                ShowResult(result);
                ShowNominations();
            });

            AddButton(btnRow, "驳回奏请（弃贤外流）", () =>
            {
                var result = _runtime.Nominations.Reject(candidate);
                ShowResult(result);
                ShowNominations();
            });

            listContainer.AddChild(card);
        }

        AddButton(_content, "合上折匣 · 返回御案", ShowHome);
        RefreshSnapshot();
    }

    private void RefreshSnapshot()
    {
        var state = _runtime.State.GetSnapshot();
        _snapshot.Text =
            $"状态快照：{state.ReignTitle}{state.ReignYear}年 · {state.Year}年{state.Month}月第{state.Xun}旬\n" +
            $"所在地：{state.CurrentLocation} ｜ 皇权威望：{state.ImperialPower}（{state.PrestigeDescription}） ｜ 国库：{state.Treasury}万 ｜ 私库：{state.PrivateTreasury}万 ｜ 民心：{state.PopularSupport}\n" +
            $"西园军：{state.WestGardenArmySize}/{state.WestGardenArmyCapacity} ｜ 士气：{state.WestGardenMorale} ｜ 忠诚：{state.WestGardenLoyalty}";
    }
}
