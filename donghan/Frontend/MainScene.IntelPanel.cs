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
            // 1. 军事平叛：展示将领五维、战力、距京惩罚与最终胜率
            var suppressPanel = new VBoxContainer();
            suppressPanel.AddThemeConstantOverride("separation", 6);
            _provinceActionsVBox.AddChild(suppressPanel);

            var lblSup = new Label();
            lblSup.Text = "⚔️ 派兵平叛候选将领";
            suppressPanel.AddChild(lblSup);

            var militaryNpcs = _gameState.Npcs.Values.Where(n => n.IsActive && n.GovernedProvinceId == null).ToList();
            if (militaryNpcs.Count == 0)
            {
                suppressPanel.AddChild(new Label { Text = "（京中暂无可派遣将领）" });
            }

            var troopBox = new HBoxContainer();
            troopBox.AddThemeConstantOverride("separation", 8);
            suppressPanel.AddChild(troopBox);
            troopBox.AddChild(new Label { Text = "出兵规模:" });
            var troopSpin = new SpinBox();
            troopSpin.MinValue = 1000;
            troopSpin.MaxValue = Math.Max(1000, _gameState.WestGardenArmy.Size);
            troopSpin.Step = 1000;
            troopSpin.Value = Math.Min(3000, Math.Max(1000, _gameState.WestGardenArmy.Size));
            troopSpin.CustomMinimumSize = new Vector2(120, 0);
            troopBox.AddChild(troopSpin);
            troopBox.AddChild(new Label { Text = $"人（西园军现有 {_gameState.WestGardenArmy.Size} 人，军费=每千人100万）" });

            foreach (var mil in militaryNpcs.Take(3))
            {
                double combatPower = NpcTraitEvaluator.GetCombatPower(mil);
                double distancePenalty = p.Distance * 5;

                var row = new VBoxContainer();
                row.AddThemeConstantOverride("separation", 3);
                suppressPanel.AddChild(row);

                var detail = new RichTextLabel();
                detail.BbcodeEnabled = true;
                detail.CustomMinimumSize = new Vector2(0, 72);
                var btnSup = new Button();

                void RefreshSuppressPreview(double troopsValue)
                {
                    int selectedTroops = (int)troopsValue;
                    int campaignCost = Math.Max(100, selectedTroops / 10);
                    double troopRatio = selectedTroops / (double)Math.Max(p.Garrison, 1);
                    double troopBonus = Math.Clamp((troopRatio - 1.0) * 20, -20, 25);
                    double successRate = Math.Clamp(combatPower - distancePenalty + troopBonus, 5, 95);

                    detail.Text = $"[b]{mil.Name}[/b] 武力:{mil.Martial} 统帅:{mil.Leadership} | 综合战力:[color=yellow]{combatPower:F0}[/color] - 距京惩罚:{distancePenalty:F0}\n" +
                        $"出兵:{selectedTroops}人 军费:{campaignCost}万 兵力修正:{troopBonus:+0;-0;0}%\n" +
                        $"预计胜率: [color={(successRate >= 70 ? "green" : successRate >= 45 ? "yellow" : "red")}]{successRate:F0}%[/color]";
                    btnSup.Text = $"命 {mil.Name} 率 {selectedTroops} 人出征";
                }

                RefreshSuppressPreview(troopSpin.Value);
                troopSpin.ValueChanged += RefreshSuppressPreview;
                row.AddChild(detail);

                string milId = mil.Id;
                btnSup.Pressed += () =>
                {
                    int troops = (int)troopSpin.Value;
                    _windowManager.PopWindow();
                    var res = _gameEngine!.SuppressRebellion(p.Id, milId, troops);
                    if (_storyOutput != null) _storyOutput.Text = res.StoryText;
                    UpdateUI();
                };
                row.AddChild(btnSup);
            }

            // 2. 遣使招抚：由玩家勾选四策并选择赈灾金额
            var pacifyPanel = new VBoxContainer();
            pacifyPanel.AddThemeConstantOverride("separation", 6);
            _provinceActionsVBox.AddChild(pacifyPanel);

            var lblPac = new Label();
            lblPac.Text = "🌸 遣使招安四策";
            pacifyPanel.AddChild(lblPac);

            var strategyGrid = new GridContainer();
            strategyGrid.Columns = 2;
            strategyGrid.AddThemeConstantOverride("h_separation", 12);
            strategyGrid.AddThemeConstantOverride("v_separation", 4);
            pacifyPanel.AddChild(strategyGrid);

            var chkSowDiscord = new CheckBox { Text = "离间计 (政治≥50)" };
            var chkPersuade = new CheckBox { Text = "说服 (魅力≥45)", ButtonPressed = true };
            var chkDisasterRelief = new CheckBox { Text = "赈灾放粮" };
            var chkPunish = new CheckBox { Text = "惩治官员 (皇权≥35)" };
            strategyGrid.AddChild(chkSowDiscord);
            strategyGrid.AddChild(chkPersuade);
            strategyGrid.AddChild(chkDisasterRelief);
            strategyGrid.AddChild(chkPunish);

            var reliefBox = new HBoxContainer();
            reliefBox.AddThemeConstantOverride("separation", 8);
            pacifyPanel.AddChild(reliefBox);
            reliefBox.AddChild(new Label { Text = "赈灾金额:" });
            var reliefSpin = new SpinBox();
            reliefSpin.MinValue = 500;
            reliefSpin.MaxValue = 1500;
            reliefSpin.Step = 500;
            reliefSpin.Value = 500;
            reliefSpin.CustomMinimumSize = new Vector2(110, 0);
            reliefBox.AddChild(reliefSpin);
            reliefBox.AddChild(new Label { Text = "万钱（仅勾选赈灾时扣国库）" });

            var envoyBox = new HBoxContainer();
            envoyBox.AddThemeConstantOverride("separation", 10);
            pacifyPanel.AddChild(envoyBox);
            envoyBox.AddChild(new Label { Text = "选择特使:" });

            foreach (var envoy in militaryNpcs.Take(3))
            {
                var btnPac = new Button();
                double politicalSkill = NpcTraitEvaluator.GetPoliticalSkill(envoy);
                btnPac.Text = $"{envoy.Name} (政略{politicalSkill:F0})";

                string envoyId = envoy.Id;
                btnPac.Pressed += () =>
                {
                    var strategies = (GameEngine.PacifyStrategy)0;
                    if (chkSowDiscord.ButtonPressed) strategies |= GameEngine.PacifyStrategy.SowDiscord;
                    if (chkPersuade.ButtonPressed) strategies |= GameEngine.PacifyStrategy.Persuade;
                    if (chkDisasterRelief.ButtonPressed) strategies |= GameEngine.PacifyStrategy.DisasterRelief;
                    if (chkPunish.ButtonPressed) strategies |= GameEngine.PacifyStrategy.Punish;

                    if (strategies == 0)
                    {
                        if (_storyOutput != null) _storyOutput.Text = "【招安未发】\n\n陛下尚未选定任何安抚策略，黄门捧诏不敢出宫。";
                        return;
                    }

                    int reliefGold = chkDisasterRelief.ButtonPressed ? (int)reliefSpin.Value : 0;
                    _windowManager.PopWindow();
                    var res = _gameEngine!.PacifyRebellion(p.Id, envoyId, strategies, reliefGold);
                    if (_storyOutput != null) _storyOutput.Text = res.StoryText;
                    UpdateUI();
                };
                envoyBox.AddChild(btnPac);
            }
        }
    }
}
