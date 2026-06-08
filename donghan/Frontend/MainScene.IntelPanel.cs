using Godot;
using System;
using System.Linq;
using DonghanEngine.Core;

namespace DonghanFrontend;

public partial class MainScene : Control
{
    private Panel? _intelPopup;
    private ItemList? _provinceItemList;
    private RichTextLabel? _provinceDetailsLabel;
    private VBoxContainer? _provinceActionsVBox;

    private RichTextLabel? _intelGlobalStatsLabel;

    private void InitializeIntelPanel()
    {
        _intelPopup = new Panel();
        _intelPopup.Name = "IntelPopup";
        _intelPopup.Visible = false;
        _intelPopup.CustomMinimumSize = new Vector2(720, 450);
        _intelPopup.SetAnchorsPreset(Control.LayoutPreset.Center);

        var hBox = new HBoxContainer();
        hBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        hBox.OffsetLeft = 15; hBox.OffsetTop = 15; hBox.OffsetRight = -15; hBox.OffsetBottom = -15;
        hBox.AddThemeConstantOverride("separation", 15);
        _intelPopup.AddChild(hBox);

        // 左半边：大汉 6 州郡总览
        var leftVBox = new VBoxContainer();
        leftVBox.CustomMinimumSize = new Vector2(250, 0);
        hBox.AddChild(leftVBox);

        var listTitle = new Label();
        listTitle.Text = "🗺️ 大汉十三州舆图情报";
        leftVBox.AddChild(listTitle);

        // 顶端天下全局态势大收纳
        _intelGlobalStatsLabel = new RichTextLabel();
        _intelGlobalStatsLabel.CustomMinimumSize = new Vector2(0, 65);
        _intelGlobalStatsLabel.BbcodeEnabled = true;
        leftVBox.AddChild(_intelGlobalStatsLabel);

        _provinceItemList = new ItemList();
        _provinceItemList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _provinceItemList.ItemSelected += OnProvinceSelected;
        leftVBox.AddChild(_provinceItemList);

        var btnClose = new Button();
        btnClose.Text = "收起舆图";
        btnClose.Pressed += _windowManager.PopWindow;
        leftVBox.AddChild(btnClose);

        // 右半边：太守任命、平叛、招抚详情与执行台
        var rightVBox = new VBoxContainer();
        rightVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        hBox.AddChild(rightVBox);

        _provinceDetailsLabel = new RichTextLabel();
        _provinceDetailsLabel.CustomMinimumSize = new Vector2(0, 100);
        _provinceDetailsLabel.BbcodeEnabled = true;
        rightVBox.AddChild(_provinceDetailsLabel);

        _provinceActionsVBox = new VBoxContainer();
        _provinceActionsVBox.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        rightVBox.AddChild(_provinceActionsVBox);

        AddChild(_intelPopup);
    }

    private void OnIntelTokenPressed()
    {
        UpdateIntelProvinceList();
        _windowManager.PushWindow(_intelPopup!);
    }

    private void UpdateIntelProvinceList()
    {
        if (_provinceItemList == null || _gameState == null) return;
        _provinceItemList.Clear();
        _provinceDetailsLabel!.Text = "请在左侧大舆图上选择一个郡县进行治理、平叛或安抚...";
        foreach (Node child in _provinceActionsVBox!.GetChildren()) child.QueueFree();

        // 刷新大局态势与西园精锐军势情报
        if (_intelGlobalStatsLabel != null)
        {
            _intelGlobalStatsLabel.Text = $"[color=gold][b]● 汉室天下大局态势[/b][/color]\n" +
                $"天下民心: [color=yellow]{_gameState.PopularSupport}[/color]/100\n" +
                $"西园新军: [color=yellow]{_gameState.WestGardenArmy.Size}[/color]兵 | 士气: [color=yellow]{_gameState.WestGardenArmy.Morale}[/color]/100 | 忠诚: [color=yellow]{_gameState.WestGardenArmy.Loyalty}[/color]/100";
        }

        foreach (var p in _gameState.Provinces.Values.OrderBy(p => p.Distance))
        {
            string govName = p.GovernorId != null && _gameState.Npcs.TryGetValue(p.GovernorId, out var g) ? g.Name : "暂无太守";
            string status = p.IsRebelling ? $"⚡【叛乱中】{p.RebelFaction}" : "○ 安定";
            _provinceItemList.AddItem($"{p.Name} (太守: {govName}) {status}");
        }
    }

    private void OnProvinceSelected(long index)
    {
        if (_gameState == null || index < 0 || index >= _gameState.Provinces.Count) return;
        var provincesList = _gameState.Provinces.Values.OrderBy(p => p.Distance).ToList();
        var p = provincesList[(int)index];

        string govName = p.GovernorId != null && _gameState.Npcs.TryGetValue(p.GovernorId, out var g) ? g.Name : "暂无";
        string rebStatus = p.IsRebelling ? $"[color=red]⚡ 叛乱中 ({p.RebelFaction})，已持续 {p.RebellionMonths} 个月[/color]" : "[color=green]○ 安定无事[/color]";

        _provinceDetailsLabel!.Text = $"[b][font_size=16]【{p.Name}】[/font_size][/b] 距京: [color=yellow]{p.Distance}[/color] 里\n" +
            $"地方民心: {p.LocalSupport} / 100 | 郡中守军: {p.Garrison} 人\n" +
            $"地方太守: {govName} | 当前局势: {rebStatus}";

        // 清空操作区
        foreach (Node child in _provinceActionsVBox!.GetChildren()) child.QueueFree();

        // === 太守召回/任命区 ===
        if (p.GovernorId != null)
        {
            var btnRecall = new Button();
            btnRecall.Text = $"召回太守【{govName}】";
            btnRecall.Pressed += () =>
            {
                _windowManager.PopWindow();
                var res = _gameEngine!.RecallGovernor(p.Id);
                if (_storyOutput != null) _storyOutput.Text = res.StoryText;
                UpdateUI();
            };
            _provinceActionsVBox.AddChild(btnRecall);
        }
        else
        {
            // 任命太守列表
            var hBoxGov = new HBoxContainer();
            hBoxGov.AddThemeConstantOverride("separation", 10);
            _provinceActionsVBox.AddChild(hBoxGov);
            var lblGov = new Label(); lblGov.Text = "外派太守: ";
            hBoxGov.AddChild(lblGov);

            var availableNpcs = _gameState.Npcs.Values.Where(n => n.IsActive && n.GovernedProvinceId == null).ToList();
            if (availableNpcs.Count == 0)
            {
                var lblNone = new Label(); lblNone.Text = "（京中暂无闲置文武）";
                hBoxGov.AddChild(lblNone);
            }
            else
            {
                foreach (var npc in availableNpcs.Take(3)) // 推荐前三位
                {
                    var btnGov = new Button();
                    btnGov.Text = $"{npc.Name} (野心 {npc.Ambition})";
                    string npcId = npc.Id;
                    btnGov.Pressed += () =>
                    {
                        _windowManager.PopWindow();
                        var res = _gameEngine!.AssignGovernor(p.Id, npcId);
                        if (_storyOutput != null) _storyOutput.Text = res.StoryText;
                        UpdateUI();
                    };
                    hBoxGov.AddChild(btnGov);
                }
            }
        }

        // === 平叛与安抚 (仅在叛乱时显示) ===
        if (p.IsRebelling)
        {
            // 1. 军事平叛
            var hBoxSuppress = new HBoxContainer();
            hBoxSuppress.AddThemeConstantOverride("separation", 10);
            _provinceActionsVBox.AddChild(hBoxSuppress);
            var lblSup = new Label(); lblSup.Text = "⚔️ 派兵平叛: ";
            hBoxSuppress.AddChild(lblSup);

            var militaryNpcs = _gameState.Npcs.Values.Where(n => n.IsActive && n.GovernedProvinceId == null).ToList();
            foreach (var mil in militaryNpcs.Take(2))
            {
                var btnSup = new Button();
                double combatPower = NpcTraitEvaluator.GetCombatPower(mil);
                double successRate = Math.Clamp(combatPower - p.Distance * 5, 5, 95);
                btnSup.Text = $"{mil.Name} (胜率{successRate:F0}%)";
                
                string milId = mil.Id;
                btnSup.Pressed += () =>
                {
                    _windowManager.PopWindow();
                    var res = _gameEngine!.SuppressRebellion(p.Id, milId);
                    if (_storyOutput != null) _storyOutput.Text = res.StoryText;
                    UpdateUI();
                };
                hBoxSuppress.AddChild(btnSup);
            }

            // 2. 遣使招抚 (使用说服与离间叠加策略)
            var hBoxPacify = new HBoxContainer();
            hBoxPacify.AddThemeConstantOverride("separation", 10);
            _provinceActionsVBox.AddChild(hBoxPacify);
            var lblPac = new Label(); lblPac.Text = "🌸 遣使招安: ";
            hBoxPacify.AddChild(lblPac);

            foreach (var envoy in militaryNpcs.Take(2))
            {
                var btnPac = new Button();
                btnPac.Text = $"{envoy.Name} (说服+离间)";
                
                string envoyId = envoy.Id;
                btnPac.Pressed += () =>
                {
                    _windowManager.PopWindow();
                    var strategies = GameEngine.PacifyStrategy.Persuade | GameEngine.PacifyStrategy.SowDiscord;
                    var res = _gameEngine!.PacifyRebellion(p.Id, envoyId, strategies, 0);
                    if (_storyOutput != null) _storyOutput.Text = res.StoryText;
                    UpdateUI();
                };
                hBoxPacify.AddChild(btnPac);
            }
        }
    }
}
