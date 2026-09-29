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
        ConfigureCenteredPopupPanel(_intelPopup, PopupSkin.Intel, new Vector2(1780, 880));

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
        _intelGlobalStatsLabel.CustomMinimumSize = new Vector2(0, 84);
        _intelGlobalStatsLabel.AddThemeFontSizeOverride("normal_font_size", 16);
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

        // 左侧：东汉十三州疆域全图 (根据地理位置分布，适配 1920x1080 宏伟版面)
        var mapColumn = CreateIntelColumn(body, "🗺️ 大汉十三州疆域全图 (翠绿=富庶 ｜ 琥珀=平稳 ｜ 赤红=凋敝 ｜ 紫红=叛乱)", 1050);
        
        var mapPanel = new Panel();
        mapPanel.CustomMinimumSize = new Vector2(1020, 680);
        mapPanel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        var mapStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.06f, 0.045f, 0.035f, 0.96f),
            BorderColor = new Color(0.48f, 0.34f, 0.18f, 1f),
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8
        };
        mapStyle.SetBorderWidthAll(2);
        mapPanel.AddThemeStyleboxOverride("panel", mapStyle);
        mapColumn.AddChild(mapPanel);

        _chinaMapCanvas = new Control();
        _chinaMapCanvas.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _chinaMapCanvas.Draw += OnDrawChinaMapTopologyLines;
        mapPanel.AddChild(_chinaMapCanvas);

        BuildChinaMapButtons(_chinaMapCanvas);

        var closeButton = new Button();
        closeButton.Text = "收起密札 (ESC)";
        StyleSceneActionButton(closeButton, ActionButtonSkin.Document);
        closeButton.Pressed += _windowManager.PopWindow;
        mapColumn.AddChild(closeButton);

        // 中间：州郡详情与研判
        var detailColumn = CreateIntelColumn(body, "州郡详情与研判", 400, expand: true);
        _intelProvinceDetailsLabel = new RichTextLabel();
        _intelProvinceDetailsLabel.BbcodeEnabled = true;
        _intelProvinceDetailsLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        detailColumn.AddChild(_intelProvinceDetailsLabel);

        // 右侧：可行处置
        var actionColumn = CreateIntelColumn(body, "可行处置", 280);
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

        // 东汉十三州相对中国地图地理经纬度布局坐标 [X, Y, Width, Height] (适配 1920x1080 大画布 1020x680)
        var layout = new Dictionary<string, (int x, int y, int w, int h)>
        {
            { ProvinceCatalog.Youzhou,   (670, 50, 190, 75) },  // 幽州 (东北·燕赵幽蓟)
            { ProvinceCatalog.Bingzhou,  (430, 110, 180, 85) }, // 并州 (北方·太原上党)
            { ProvinceCatalog.Jizhou,    (640, 150, 190, 85) }, // 冀州 (华北·邺城巨鹿)
            { ProvinceCatalog.Liangzhou, (80, 180, 230, 105) }, // 凉州 (西北·西凉金城)
            { ProvinceCatalog.Sili,      (410, 230, 190, 95) }, // 司隶 (京畿·洛阳长安)
            { ProvinceCatalog.Yanzhou,   (630, 260, 175, 80) }, // 兖州 (中原·东郡濮阳)
            { ProvinceCatalog.Qingzhou,  (830, 240, 165, 80) }, // 青州 (半岛·齐鲁临淄)
            { ProvinceCatalog.Yuzhou,    (590, 365, 190, 80) }, // 豫州 (淮汝·汝南颖川)
            { ProvinceCatalog.Xuzhou,    (805, 345, 175, 80) }, // 徐州 (海岱·彭城下邳)
            { ProvinceCatalog.Yizhou,    (150, 350, 245, 135) },// 益州 (西南·巴蜀成都)
            { ProvinceCatalog.Jingzhou,  (460, 460, 215, 110) },// 荆州 (两湖·荆襄南阳)
            { ProvinceCatalog.Yangzhou,  (720, 440, 240, 120) },// 扬州 (江东·建业吴会)
            { ProvinceCatalog.Jiaozhou,  (390, 595, 310, 70) }  // 交州 (岭南·南海交趾)
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

    private void OnDrawChinaMapTopologyLines()
    {
        if (_chinaMapCanvas == null || _gameState == null) return;

        // 绘制地理驿道/地缘邻接拓扑连线 (适配 1020x680 大画布)
        var drawnEdges = new HashSet<string>();
        var lineColor = new Color(0.55f, 0.42f, 0.25f, 0.70f);
        var borderLineColor = new Color(0.35f, 0.28f, 0.20f, 0.50f);

        // 1. 绘制中国汉代疆域东南海岸与边陲轮廓线
        Vector2[] coastLine = new Vector2[]
        {
            new(860, 60),   // 辽东/幽州东
            new(920, 220),  // 渤海湾
            new(1000, 260), // 山东半岛东角
            new(970, 340),  // 淮东海岸
            new(960, 450),  // 长江口
            new(940, 530),  // 浙闽沿海
            new(720, 630),  // 岭南南海
            new(390, 640),  // 交趾北部湾
        };
        for (int i = 0; i < coastLine.Length - 1; i++)
        {
            _chinaMapCanvas.DrawLine(coastLine[i], coastLine[i + 1], borderLineColor, 3.0f, antialiased: true);
        }

        // 2. 绘制十三州之间邻接驿道网络
        foreach (var p in _gameState.Provinces.Values)
        {
            if (!_provinceMapButtons.TryGetValue(p.Id, out var srcBtn)) continue;
            Vector2 srcCenter = srcBtn.Position + srcBtn.Size / 2f;

            foreach (var nId in p.Neighbors)
            {
                if (!_provinceMapButtons.TryGetValue(nId, out var dstBtn)) continue;
                string edgeKey = string.CompareOrdinal(p.Id, nId) < 0 ? $"{p.Id}_{nId}" : $"{nId}_{p.Id}";
                if (drawnEdges.Contains(edgeKey)) continue;
                drawnEdges.Add(edgeKey);

                Vector2 dstCenter = dstBtn.Position + dstBtn.Size / 2f;
                // 绘制古代驿道交通线
                _chinaMapCanvas.DrawLine(srcCenter, dstCenter, lineColor, 2.5f, antialiased: true);
                // 驿道节点小圆点
                _chinaMapCanvas.DrawCircle((srcCenter + dstCenter) / 2f, 3.5f, new Color(0.85f, 0.70f, 0.42f, 0.85f));
            }
        }
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
        _chinaMapCanvas?.QueueRedraw();
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
        btn.Text = $"【{p.Name}】\n{statusTag} 官田{stateRatio}%\n民心{p.LocalSupport} 守军{p.Garrison:N0}";
        btn.AddThemeColorOverride("font_color", textColor);
        btn.AddThemeFontSizeOverride("font_size", 14);
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
