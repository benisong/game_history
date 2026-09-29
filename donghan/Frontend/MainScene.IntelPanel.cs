using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DonghanEngine.Core;

namespace DonghanFrontend;

public partial class MainScene : Control
{
    private Panel? _intelPopup;
    private RichTextLabel? _intelGlobalStatsLabel;
    private ItemList? _provinceItemList;
    private RichTextLabel? _intelProvinceDetailsLabel;
    private VBoxContainer? _intelActionsVBox;
    private Label? _intelActionsTitleLabel;
    private readonly List<Province> _intelProvinceOrder = new();

    private void InitializeIntelPanel()
    {
        _intelPopup = new Panel();
        _intelPopup.Name = "IntelPopup";
        _intelPopup.Visible = false;
        _intelPopup.CustomMinimumSize = new Vector2(1100, 640);
        _intelPopup.AnchorLeft = 0.5f;
        _intelPopup.AnchorTop = 0.5f;
        _intelPopup.AnchorRight = 0.5f;
        _intelPopup.AnchorBottom = 0.5f;
        _intelPopup.OffsetLeft = -550;
        _intelPopup.OffsetTop = -320;
        _intelPopup.OffsetRight = 550;
        _intelPopup.OffsetBottom = 320;
        _intelPopup.AddThemeStyleboxOverride("panel", CreatePopupPanelStyle(PopupSkin.Intel));

        var root = new VBoxContainer();
        SetFullRect(root);
        root.OffsetLeft = 18;
        root.OffsetTop = 16;
        root.OffsetRight = -18;
        root.OffsetBottom = -16;
        root.AddThemeConstantOverride("separation", 12);
        _intelPopup.AddChild(root);

        BuildIntelHeader(root);
        BuildIntelBody(root);
        AddChild(_intelPopup);
    }

    private void BuildIntelHeader(VBoxContainer root)
    {
        var title = new Label();
        title.Text = "黄门密札 · 天下情报台";
        StylePopupTitle(title, PopupSkin.Intel);
        root.AddChild(title);

        _intelGlobalStatsLabel = new RichTextLabel();
        _intelGlobalStatsLabel.BbcodeEnabled = true;
        _intelGlobalStatsLabel.CustomMinimumSize = new Vector2(0, 72);
        root.AddChild(_intelGlobalStatsLabel);
    }

    private Control? _chinaMapCanvas;
    private readonly Dictionary<string, Button> _provinceMapButtons = new();

    private void BuildIntelBody(VBoxContainer root)
    {
        var body = new HBoxContainer();
        body.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        body.AddThemeConstantOverride("separation", 12);
        root.AddChild(body);

        // 左侧：东汉十三州疆域全图 (根据地理位置分布)
        var mapColumn = CreateIntelColumn(body, "🗺️ 大汉十三州舆图 (绿色=富庶/黄色=中等/红色=凋敝/紫色=叛乱)", 580);
        
        var mapPanel = new Panel();
        mapPanel.CustomMinimumSize = new Vector2(560, 430);
        mapPanel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        var mapStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.08f, 0.055f, 0.04f, 0.95f),
            BorderColor = new Color(0.42f, 0.28f, 0.16f, 1f),
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6
        };
        mapStyle.SetBorderWidthAll(2);
        mapPanel.AddThemeStyleboxOverride("panel", mapStyle);
        mapColumn.AddChild(mapPanel);

        _chinaMapCanvas = new Control();
        _chinaMapCanvas.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        mapPanel.AddChild(_chinaMapCanvas);

        BuildChinaMapButtons(_chinaMapCanvas);

        var closeButton = new Button();
        closeButton.Text = "收起密札";
        StyleSceneActionButton(closeButton, ActionButtonSkin.Document);
        closeButton.Pressed += _windowManager.PopWindow;
        mapColumn.AddChild(closeButton);

        // 中间：州郡详情与研判
        var detailColumn = CreateIntelColumn(body, "详情与研判", 320, expand: true);
        _intelProvinceDetailsLabel = new RichTextLabel();
        _intelProvinceDetailsLabel.BbcodeEnabled = true;
        _intelProvinceDetailsLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        detailColumn.AddChild(_intelProvinceDetailsLabel);

        // 右侧：可行处置
        var actionColumn = CreateIntelColumn(body, "可行处置", 220);
        _intelActionsTitleLabel = new Label();
        _intelActionsTitleLabel.Text = "先点击地图州郡";
        _intelActionsTitleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _intelActionsTitleLabel.AddThemeColorOverride("font_color", GetPopupTitleColor(PopupSkin.Intel));
        actionColumn.AddChild(_intelActionsTitleLabel);

        var actionScroll = new ScrollContainer();
        actionScroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        actionColumn.AddChild(actionScroll);

        _intelActionsVBox = new VBoxContainer();
        _intelActionsVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _intelActionsVBox.AddThemeConstantOverride("separation", 8);
        actionScroll.AddChild(_intelActionsVBox);
    }

    private void BuildChinaMapButtons(Control canvas)
    {
        _provinceMapButtons.Clear();

        // 东汉十三州相对中国地图地理经纬度布局坐标 [X, Y, Width, Height]
        var layout = new Dictionary<string, (int x, int y, int w, int h)>
        {
            { ProvinceCatalog.Youzhou,   (360, 40, 110, 50) },  // 幽州 (东北)
            { ProvinceCatalog.Bingzhou,  (230, 80, 100, 60) },  // 并州 (北部)
            { ProvinceCatalog.Jizhou,    (340, 110, 110, 60) }, // 冀州 (华北)
            { ProvinceCatalog.Liangzhou, (60, 130, 130, 70) },  // 凉州 (西北)
            { ProvinceCatalog.Sili,      (220, 160, 105, 65) }, // 司隶 (京畿洛阳)
            { ProvinceCatalog.Yanzhou,   (340, 180, 95, 55) },  // 兖州 (中原)
            { ProvinceCatalog.Qingzhou,  (445, 160, 95, 55) },  // 青州 (山东半岛)
            { ProvinceCatalog.Yuzhou,    (320, 245, 105, 55) }, // 豫州 (淮汝)
            { ProvinceCatalog.Xuzhou,    (435, 225, 95, 55) },  // 徐州 (淮东海岱)
            { ProvinceCatalog.Yizhou,    (100, 240, 140, 90) }, // 益州 (西南巴蜀)
            { ProvinceCatalog.Jingzhou,  (260, 310, 120, 75) }, // 荆州 (荆襄两湖)
            { ProvinceCatalog.Yangzhou,  (400, 290, 130, 80) }, // 扬州 (江东江南)
            { ProvinceCatalog.Jiaozhou,  (230, 395, 180, 50) }  // 交州 (岭南南部)
        };

        foreach (var kvp in layout)
        {
            string pId = kvp.Key;
            var pos = kvp.Value;

            var btn = new Button
            {
                Name = $"MapBtn_{pId}",
                Position = new Vector2(pos.x, pos.y),
                Size = new Vector2(pos.w, pos.h),
                Text = pId
            };
            btn.Pressed += () => OnProvinceMapClicked(pId);
            canvas.AddChild(btn);
            _provinceMapButtons[pId] = btn;
        }
    }

    private void OnProvinceMapClicked(string provinceId)
    {
        if (_gameState == null || !_gameState.Provinces.TryGetValue(provinceId, out var p)) return;
        RenderProvinceDetails(p);
        RenderProvinceActions(p);
        HighlightSelectedMapProvince(provinceId);
    }

    private void HighlightSelectedMapProvince(string selectedId)
    {
        foreach (var kvp in _provinceMapButtons)
        {
            string pId = kvp.Key;
            var btn = kvp.Value;
            if (_gameState != null && _gameState.Provinces.TryGetValue(pId, out var p))
            {
                ApplyMapButtonStyling(btn, p, isSelected: pId == selectedId);
            }
        }
    }

    private void ApplyMapButtonStyling(Button btn, Province p, bool isSelected)
    {
        // 繁荣度/状态颜色计算：
        // 叛乱 -> 紫红色；
        // 丰裕富庶 (民心>=45 且 财富>=3500) -> 翠绿；
        // 中等平稳 (民心>=30 且 财富>=2000) -> 琥珀金；
        // 凋敝危急 (民心<30 或 财富<2000) -> 赤红；
        Color bgColor;
        Color textColor = new Color(0.96f, 0.94f, 0.88f);
        string statusTag;

        if (p.IsRebelling)
        {
            bgColor = new Color(0.48f, 0.08f, 0.18f, 0.95f);
            statusTag = "⚡叛乱";
        }
        else if (p.LocalSupport >= 45 && p.Wealth >= 3500)
        {
            bgColor = new Color(0.12f, 0.36f, 0.18f, 0.92f); // 繁荣充盈
            statusTag = "🟢富庶";
        }
        else if (p.LocalSupport >= 30 && p.Wealth >= 2000)
        {
            bgColor = new Color(0.38f, 0.28f, 0.08f, 0.92f); // 中等
            statusTag = "🟡平稳";
        }
        else
        {
            bgColor = new Color(0.46f, 0.16f, 0.12f, 0.92f); // 凋敝
            statusTag = "🔴凋敝";
        }

        var style = new StyleBoxFlat
        {
            BgColor = bgColor,
            BorderColor = isSelected ? new Color(1f, 0.88f, 0.35f, 1f) : new Color(0.25f, 0.18f, 0.12f, 0.85f),
            CornerRadiusBottomLeft = 4,
            CornerRadiusBottomRight = 4,
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4
        };
        style.SetBorderWidthAll(isSelected ? 3 : 1);
        btn.AddThemeStyleboxOverride("normal", style);

        var hoverStyle = (StyleBoxFlat)style.Duplicate();
        hoverStyle.BorderColor = new Color(1f, 0.92f, 0.5f, 1f);
        hoverStyle.SetBorderWidthAll(3);
        btn.AddThemeStyleboxOverride("hover", hoverStyle);

        int stateRatio = (p.StateControlledLand * 100) / Math.Max(1, p.StateControlledLand + p.GentryControlledLand);
        btn.Text = $"【{p.Name}】\n{statusTag} 官田{stateRatio}%\n民心{p.LocalSupport} 守军{p.Garrison}";
        btn.AddThemeColorOverride("font_color", textColor);
        btn.AddThemeFontSizeOverride("font_size", 11);
    }

    private VBoxContainer CreateIntelColumn(HBoxContainer parent, string title, int width, bool expand = false)
    {
        var panel = new Panel();
        panel.CustomMinimumSize = new Vector2(width, 0);
        panel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        panel.SizeFlagsHorizontal = expand ? Control.SizeFlags.ExpandFill : Control.SizeFlags.ShrinkBegin;
        panel.AddThemeStyleboxOverride("panel", CreateIntelInnerPanelStyle());
        parent.AddChild(panel);

        var box = new VBoxContainer();
        SetFullRect(box);
        box.OffsetLeft = 12;
        box.OffsetTop = 10;
        box.OffsetRight = -12;
        box.OffsetBottom = -10;
        box.AddThemeConstantOverride("separation", 9);
        panel.AddChild(box);

        var label = new Label();
        label.Text = title;
        StyleColumnTitle(label, PopupSkin.Intel);
        box.AddChild(label);

        return box;
    }

    private static StyleBoxFlat CreateIntelInnerPanelStyle()
    {
        return CreatePopupInnerPanelStyle(PopupSkin.Intel);
    }

    private void OnIntelTokenPressed()
    {
        RefreshIntelPanel();
        _windowManager.PushWindow(_intelPopup!);
    }

    private void RefreshIntelPanel()
    {
        if (_gameState == null) return;
        RenderIntelGlobalStats();
        HighlightSelectedMapProvince(string.Empty);
        ShowIntelEmptyState();
    }

    private void RenderIntelGlobalStats()
    {
        if (_intelGlobalStatsLabel == null || _gameState == null) return;
        int rebellionCount = _gameState.Provinces.Values.Count(p => p.IsRebelling);
        int noGovernorCount = _gameState.Provinces.Values.Count(p => p.GovernorId == null);
        _intelGlobalStatsLabel.Text =
            $"[center][color=gray]{FormatTimeLabel()}[/color][/center]\n" +
            $"[center]皇权 {ColorNumber(_gameState.ImperialPower, danger: 25, warning: 40)}/100  ｜  " +
            $"国库 [color=yellow]{_gameState.Treasury}[/color]万  ｜  私库 [color=yellow]{_gameState.PrivateTreasury}[/color]万  ｜  " +
            $"民心 {ColorNumber(_gameState.PopularSupport, danger: 25, warning: 40)}/100  ｜  " +
            $"龙体 {ColorNumber(_gameState.Health, danger: 30, warning: 50)}/100  ｜  所在地 [color=yellow]{_gameState.CurrentLocation}[/color][/center]\n" +
            $"[center]叛乱州 {ColorCount(rebellionCount)}/{_gameState.Provinces.Count}  ｜  无太守州 {ColorCount(noGovernorCount)}/{_gameState.Provinces.Count}  ｜  " +
            $"西园新军 [color=yellow]{_gameState.WestGardenArmy.Size}[/color]人  ｜  士气 {ColorNumber(_gameState.WestGardenArmy.Morale, danger: 30, warning: 55)}  ｜  忠诚 {ColorNumber(_gameState.WestGardenArmy.Loyalty, danger: 30, warning: 55)}[/center]";
    }

    private static string ColorNumber(int value, int danger, int warning)
    {
        string color = value < danger ? "red" : value < warning ? "yellow" : "green";
        return $"[color={color}]{value}[/color]";
    }

    private static string ColorCount(int count)
    {
        return count > 0 ? $"[color=red]{count}[/color]" : $"[color=green]{count}[/color]";
    }

    private void RenderProvinceList()
    {
        if (_provinceItemList == null || _gameState == null) return;
        _provinceItemList.Clear();
        _intelProvinceOrder.Clear();
        _intelProvinceOrder.AddRange(_gameState.Provinces.Values
            .OrderByDescending(p => p.IsRebelling)
            .ThenByDescending(GetProvinceRiskScore)
            .ThenBy(p => p.Distance));

        foreach (var p in _intelProvinceOrder)
        {
            string governor = p.GovernorId != null && _gameState.Npcs.TryGetValue(p.GovernorId, out var g) ? g.Name : "无太守";
            string status = GetProvinceRiskLabel(p);
            _provinceItemList.AddItem($"{status} {p.Name}\n民心{p.LocalSupport}｜守军{p.Garrison}｜{governor}｜距京{p.Distance}");
        }
    }

    private void ShowIntelEmptyState()
    {
        if (_intelProvinceDetailsLabel == null || _intelActionsVBox == null) return;
        _intelProvinceDetailsLabel.Text =
            "[b][font_size=16]【黄门密札 · 天下舆图】[/font_size][/b]\n\n" +
            "天下十三州山川险要、户口民心、守军与官田私田皆标注于左侧地图。\n\n" +
            "[color=yellow]地图色块说明：[/color]\n" +
            "- [color=green]■ 翠绿[/color]：富庶繁荣（民心>=45 且 府库充实）\n" +
            "- [color=yellow]■ 琥珀[/color]：中等平稳（民心>=30）\n" +
            "- [color=red]■ 赤红[/color]：凋敝危急（民心<30，民变风险极高）\n" +
            "- [color=#ff55aa]■ 紫红[/color]：⚡ 已起兵叛乱\n\n" +
            "👉 请点击左侧地图中任意一块州郡，调阅地方军政实情录与下达诏令。";

        ClearIntelChildren(_intelActionsVBox);
        if (_intelActionsTitleLabel != null) _intelActionsTitleLabel.Text = "先点击地图州郡";
        _intelActionsVBox.AddChild(new Label { Text = "请点击左侧地图州郡。", AutowrapMode = TextServer.AutowrapMode.WordSmart });
    }

    private void OnProvinceSelected(long index)
    {
        if (_gameState == null || index < 0 || index >= _intelProvinceOrder.Count) return;
        var province = _intelProvinceOrder[(int)index];
        RenderProvinceDetails(province);
        RenderProvinceActions(province);
    }

    private void RenderProvinceDetails(Province p)
    {
        if (_intelProvinceDetailsLabel == null || _gameState == null) return;
        string governor = p.GovernorId != null && _gameState.Npcs.TryGetValue(p.GovernorId, out var g) ? $"{g.Name}（{g.Title}）" : "暂无";
        string rebellion = p.IsRebelling
            ? $"[color=red]⚡ {p.RebelFaction}叛乱，已持续 {p.RebellionMonths} 个月[/color]"
            : "[color=green]○ 安定无事[/color]";
        string neighbors = p.Neighbors.Count == 0
            ? "无"
            : string.Join("、", p.Neighbors.Select(id => _gameState.Provinces.TryGetValue(id, out var n) ? n.Name : id));

        _intelProvinceDetailsLabel.Text =
            $"[b][font_size=16]【{p.Name}】[/font_size][/b]  {GetProvinceRiskLabel(p)}\n" +
            $"当前局势：{rebellion}\n" +
            $"距京：{p.Distance}  ｜  邻接：{neighbors}\n" +
            $"地方太守：{governor}\n" +
            $"地方民心：{ColorNumber(p.LocalSupport, danger: 25, warning: 40)} / 100\n" +
            $"郡中守军：[color=yellow]{p.Garrison}[/color] 人  ｜  财富：[color=yellow]{p.Wealth}[/color] 万\n" +
            $"防务等级：{ColorNumber(p.DefenseLevel, danger: 30, warning: 45)} / 100\n\n" +
            "[color=yellow][b]【黄门研判】[/b][/color]\n" +
            BuildRiskAssessment(p) + "\n" +
            "[color=yellow][b]【近日密录】[/b][/color]\n" +
            BuildRecentIntelText(p);
    }

    private string BuildRiskAssessment(Province p)
    {
        var notes = new List<string>();
        if (p.IsRebelling) notes.Add($"- {p.Name}已陷叛乱，应优先平叛或招安，久拖或波及邻郡。");
        if (p.LocalSupport < 25) notes.Add("- 民心低于 25，黄巾响应和民变风险极高。");
        else if (p.LocalSupport < 40) notes.Add("- 民心低于 40，地方不稳，需要太守或赈济稳定。 ");
        if (p.GovernorId == null) notes.Add("- 当前无太守，地方恢复能力不足，低民心州更易出事。");
        if (p.DefenseLevel < 35) notes.Add("- 防务薄弱，一旦起乱将更难压制。");
        if (p.Garrison < 2500) notes.Add("- 守军偏少，军事平叛可能需要更多西园兵力支援。");
        if (notes.Count == 0) notes.Add("- 暂无迫切危机，可作为朝廷稳定腹地。 ");
        return string.Join("\n", notes) + "\n";
    }

    private string BuildRecentIntelText(Province p)
    {
        if (_gameState == null) return "- 暂无密录。";
        var reports = _gameState.IntelReports.TakeLast(3).ToList();
        if (reports.Count == 0)
        {
            return p.IsRebelling
                ? $"- 黄门报称：{p.Name}{p.RebelFaction}声势未息，地方粮道与守军皆需详查。"
                : "- 暂无专属密录。";
        }
        return string.Join("\n", reports.Select(r => $"- {r}"));
    }

    private static string GetProvinceRiskLabel(Province p)
    {
        if (p.IsRebelling) return $"⚡ 叛乱";
        if (p.LocalSupport < 25 || (p.GovernorId == null && p.LocalSupport < 35)) return "⚠ 危急";
        if (p.LocalSupport < 40 || p.DefenseLevel < 40) return "△ 不稳";
        return "○ 安定";
    }

    private static int GetProvinceRiskScore(Province p)
    {
        int score = 0;
        if (p.IsRebelling) score += 100;
        score += Math.Max(0, 50 - p.LocalSupport);
        score += Math.Max(0, 45 - p.DefenseLevel) / 2;
        if (p.GovernorId == null) score += 15;
        if (p.Garrison < 2500) score += 8;
        return score;
    }

    private static void ClearIntelChildren(VBoxContainer box)
    {
        foreach (Node child in box.GetChildren())
        {
            box.RemoveChild(child);
            child.QueueFree();
        }
    }
}
