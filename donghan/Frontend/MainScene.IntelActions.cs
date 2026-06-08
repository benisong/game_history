using Godot;
using System;
using System.Linq;
using DonghanEngine.Core;

namespace DonghanFrontend;

public partial class MainScene : Control
{
    private void RenderProvinceActions(Province p)
    {
        if (_intelActionsVBox == null || _gameState == null) return;
        ClearIntelChildren(_intelActionsVBox);
        if (_intelActionsTitleLabel != null) _intelActionsTitleLabel.Text = $"处置 · {p.Name}";

        RenderGovernorActions(p);
        if (p.IsRebelling)
        {
            AddIntelActionSeparator();
            RenderSuppressRebellionActions(p);
            AddIntelActionSeparator();
            RenderPacifyRebellionActions(p);
        }
        else
        {
            AddIntelActionSeparator();
            _intelActionsVBox.AddChild(new Label
            {
                Text = "【地方处置】\n该州暂未叛乱。第一轮仅支持太守任免；赈济、修城、征粮等后续接入后端规则。",
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            });
        }
    }

    private void RenderGovernorActions(Province p)
    {
        if (_intelActionsVBox == null || _gameState == null) return;
        AddIntelSectionTitle("【太守任免】");

        string governorName = p.GovernorId != null && _gameState.Npcs.TryGetValue(p.GovernorId, out var governor) ? governor.Name : "";
        if (p.GovernorId != null)
        {
            var recall = new Button();
            recall.Text = $"召回太守【{governorName}】";
            recall.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            recall.Pressed += () =>
            {
                _windowManager.PopWindow();
                var result = _gameEngine!.RecallGovernor(p.Id);
                if (_storyOutput != null) _storyOutput.Text = result.StoryText;
                UpdateUI();
            };
            _intelActionsVBox.AddChild(recall);
            return;
        }

        var candidates = _gameState.Npcs.Values
            .Where(n => n.IsActive && n.GovernedProvinceId == null)
            .OrderByDescending(n => n.Politics)
            .ThenBy(n => n.Ambition)
            .Take(4)
            .ToList();

        if (candidates.Count == 0)
        {
            _intelActionsVBox.AddChild(new Label { Text = "京中暂无闲置文武。", AutowrapMode = TextServer.AutowrapMode.WordSmart });
            return;
        }

        foreach (var npc in candidates)
        {
            var appoint = new Button();
            appoint.Text = $"任 {npc.Name}  政{npc.Politics} 野{npc.Ambition} {npc.Faction}";
            appoint.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            string npcId = npc.Id;
            appoint.Pressed += () =>
            {
                _windowManager.PopWindow();
                var result = _gameEngine!.AssignGovernor(p.Id, npcId);
                if (_storyOutput != null) _storyOutput.Text = result.StoryText;
                UpdateUI();
            };
            _intelActionsVBox.AddChild(appoint);
        }
    }

    private void RenderSuppressRebellionActions(Province p)
    {
        if (_intelActionsVBox == null || _gameState == null) return;
        AddIntelSectionTitle("【军事平叛】");

        var militaryNpcs = _gameState.Npcs.Values.Where(n => n.IsActive && n.GovernedProvinceId == null).Take(4).ToList();
        if (militaryNpcs.Count == 0)
        {
            _intelActionsVBox.AddChild(new Label { Text = "京中暂无可派遣将领。", AutowrapMode = TextServer.AutowrapMode.WordSmart });
            return;
        }

        var troopBox = new HBoxContainer();
        troopBox.AddThemeConstantOverride("separation", 8);
        _intelActionsVBox.AddChild(troopBox);
        troopBox.AddChild(new Label { Text = "出兵:" });

        var troopSpin = new SpinBox();
        troopSpin.MinValue = 1000;
        troopSpin.MaxValue = Math.Max(1000, _gameState.WestGardenArmy.Size);
        troopSpin.Step = 1000;
        troopSpin.Value = Math.Min(3000, Math.Max(1000, _gameState.WestGardenArmy.Size));
        troopSpin.CustomMinimumSize = new Vector2(115, 0);
        troopBox.AddChild(troopSpin);
        troopBox.AddChild(new Label { Text = "人" });

        var costLabel = new Label();
        costLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _intelActionsVBox.AddChild(costLabel);

        foreach (var general in militaryNpcs)
        {
            double combatPower = NpcTraitEvaluator.GetCombatPower(general);
            double distancePenalty = p.Distance * 5;

            var detail = new RichTextLabel();
            detail.BbcodeEnabled = true;
            detail.CustomMinimumSize = new Vector2(0, 76);
            _intelActionsVBox.AddChild(detail);

            var dispatch = new Button();
            dispatch.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _intelActionsVBox.AddChild(dispatch);

            void RefreshPreview(double troopsValue)
            {
                int selectedTroops = (int)troopsValue;
                int campaignCost = Math.Max(100, selectedTroops / 10);
                double troopRatio = selectedTroops / (double)Math.Max(p.Garrison, 1);
                double troopBonus = Math.Clamp((troopRatio - 1.0) * 20, -20, 25);
                double successRate = Math.Clamp(combatPower - distancePenalty + troopBonus, 5, 95);
                costLabel.Text = $"军费预估：{campaignCost} 万钱｜西园现有 {_gameState.WestGardenArmy.Size} 人";
                detail.Text = $"[b]{general.Name}[/b] 武{general.Martial} 统{general.Leadership}｜战力[color=yellow]{combatPower:F0}[/color] 距京惩罚{distancePenalty:F0}\n" +
                    $"兵力修正 {troopBonus:+0;-0;0}%｜预计胜率 [color={(successRate >= 70 ? "green" : successRate >= 45 ? "yellow" : "red")}]{successRate:F0}%[/color]";
                dispatch.Text = $"命 {general.Name} 率 {selectedTroops} 人出征";
            }

            RefreshPreview(troopSpin.Value);
            troopSpin.ValueChanged += RefreshPreview;

            string generalId = general.Id;
            dispatch.Pressed += () =>
            {
                int troops = (int)troopSpin.Value;
                _windowManager.PopWindow();
                var result = _gameEngine!.SuppressRebellion(p.Id, generalId, troops);
                if (_storyOutput != null) _storyOutput.Text = result.StoryText;
                UpdateUI();
            };
        }
    }

    private void RenderPacifyRebellionActions(Province p)
    {
        if (_intelActionsVBox == null || _gameState == null) return;
        AddIntelSectionTitle("【遣使招安】");

        var strategyGrid = new GridContainer();
        strategyGrid.Columns = 2;
        strategyGrid.AddThemeConstantOverride("h_separation", 12);
        strategyGrid.AddThemeConstantOverride("v_separation", 4);
        _intelActionsVBox.AddChild(strategyGrid);

        var chkSowDiscord = new CheckBox { Text = "离间计" };
        var chkPersuade = new CheckBox { Text = "说服", ButtonPressed = true };
        var chkDisasterRelief = new CheckBox { Text = "赈灾" };
        var chkPunish = new CheckBox { Text = "惩治" };
        strategyGrid.AddChild(chkSowDiscord);
        strategyGrid.AddChild(chkPersuade);
        strategyGrid.AddChild(chkDisasterRelief);
        strategyGrid.AddChild(chkPunish);

        var reliefBox = new HBoxContainer();
        reliefBox.AddThemeConstantOverride("separation", 8);
        _intelActionsVBox.AddChild(reliefBox);
        reliefBox.AddChild(new Label { Text = "赈灾:" });
        var reliefSpin = new SpinBox();
        reliefSpin.MinValue = 500;
        reliefSpin.MaxValue = 1500;
        reliefSpin.Step = 500;
        reliefSpin.Value = 500;
        reliefSpin.CustomMinimumSize = new Vector2(105, 0);
        reliefBox.AddChild(reliefSpin);
        reliefBox.AddChild(new Label { Text = "万" });

        var preview = new Label();
        preview.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _intelActionsVBox.AddChild(preview);

        void RefreshStrategyPreview()
        {
            string strategies = string.Join(" + ", new[]
            {
                chkSowDiscord.ButtonPressed ? "离间" : "",
                chkPersuade.ButtonPressed ? "说服" : "",
                chkDisasterRelief.ButtonPressed ? $"赈灾{(int)reliefSpin.Value}万" : "",
                chkPunish.ButtonPressed ? "惩治" : ""
            }.Where(s => !string.IsNullOrEmpty(s)));
            preview.Text = string.IsNullOrEmpty(strategies) ? "预计策略：未选策略" : $"预计策略：{strategies}";
        }

        chkSowDiscord.Toggled += _ => RefreshStrategyPreview();
        chkPersuade.Toggled += _ => RefreshStrategyPreview();
        chkDisasterRelief.Toggled += _ => RefreshStrategyPreview();
        chkPunish.Toggled += _ => RefreshStrategyPreview();
        reliefSpin.ValueChanged += _ => RefreshStrategyPreview();
        RefreshStrategyPreview();

        var envoys = _gameState.Npcs.Values.Where(n => n.IsActive && n.GovernedProvinceId == null).Take(4).ToList();
        foreach (var envoy in envoys)
        {
            var pacify = new Button();
            double politicalSkill = NpcTraitEvaluator.GetPoliticalSkill(envoy);
            pacify.Text = $"遣 {envoy.Name} 招安  政略{politicalSkill:F0}";
            pacify.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            string envoyId = envoy.Id;
            pacify.Pressed += () =>
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
                var result = _gameEngine!.PacifyRebellion(p.Id, envoyId, strategies, reliefGold);
                if (_storyOutput != null) _storyOutput.Text = result.StoryText;
                UpdateUI();
            };
            _intelActionsVBox.AddChild(pacify);
        }
    }

    private void AddIntelSectionTitle(string text)
    {
        if (_intelActionsVBox == null) return;
        var label = new Label();
        label.Text = text;
        label.AddThemeColorOverride("font_color", new Color(0.95f, 0.77f, 0.28f, 1.0f));
        _intelActionsVBox.AddChild(label);
    }

    private void AddIntelActionSeparator()
    {
        if (_intelActionsVBox == null) return;
        var separator = new HSeparator();
        _intelActionsVBox.AddChild(separator);
    }
}
