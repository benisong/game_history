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

    private void BuildIntelBody(VBoxContainer root)
    {
        var body = new HBoxContainer();
        body.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        body.AddThemeConstantOverride("separation", 12);
        root.AddChild(body);

        var provinceColumn = CreateIntelColumn(body, "州郡密札", 285);
        _provinceItemList = new ItemList();
        _provinceItemList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        StylePopupItemList(_provinceItemList, PopupSkin.Intel);
        _provinceItemList.ItemSelected += OnProvinceSelected;
        provinceColumn.AddChild(_provinceItemList);

        var closeButton = new Button();
        closeButton.Text = "收起密札";
        StyleSceneActionButton(closeButton, ActionButtonSkin.Document);
        closeButton.Pressed += _windowManager.PopWindow;
        provinceColumn.AddChild(closeButton);

        var detailColumn = CreateIntelColumn(body, "详情与研判", 500, expand: true);
        _intelProvinceDetailsLabel = new RichTextLabel();
        _intelProvinceDetailsLabel.BbcodeEnabled = true;
        _intelProvinceDetailsLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        detailColumn.AddChild(_intelProvinceDetailsLabel);

        var actionColumn = CreateIntelColumn(body, "可行处置", 285);
        _intelActionsTitleLabel = new Label();
        _intelActionsTitleLabel.Text = "先选择州郡";
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
        RenderProvinceList();
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
            "[b][font_size=16]【黄门密札】[/font_size][/b]\n\n" +
            "天下州郡、民心、守军、太守与叛乱皆录于此。\n\n" +
            "请在左侧选择一州，查看地方风险与可行处置。\n\n" +
            "[color=yellow]黄门提醒：[/color]\n" +
            "- 叛乱州优先处置。\n" +
            "- 无太守且民心低的州，易生民变。\n" +
            "- 西园军虽为天子亲军，但出兵会消耗国库。";

        ClearIntelChildren(_intelActionsVBox);
        if (_intelActionsTitleLabel != null) _intelActionsTitleLabel.Text = "先选择州郡";
        _intelActionsVBox.AddChild(new Label { Text = "请选择左侧州郡。", AutowrapMode = TextServer.AutowrapMode.WordSmart });
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
        foreach (Node child in box.GetChildren()) child.QueueFree();
    }
}
