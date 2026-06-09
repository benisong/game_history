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

        var militaryNpcs = _gameState.Npcs.Values
            .Where(n => n.IsActive && n.GovernedProvinceId == null)
            .OrderByDescending(NpcTraitEvaluator.GetCombatPower)
            .Take(4)
            .ToList();
        if (militaryNpcs.Count == 0)
        {
            _intelActionsVBox.AddChild(new Label { Text = "京中暂无可派遣将领。", AutowrapMode = TextServer.AutowrapMode.WordSmart });
            return;
        }

        var cardRoot = CreateIntelDecisionCard("军令牌 · 平定州郡", $"叛乱州郡：{p.Name}\n贼势约：{p.Garrison} 人｜距京：{p.Distance}\n请陛下先定出兵之数，再择将授符。军费与胜率将随兵力即时推算。");

        var troopBox = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        troopBox.AddThemeConstantOverride("separation", 8);
        cardRoot.AddChild(troopBox);
        var troopLabel = new Label { Text = "出兵" };
        StylePopupBodyText(troopLabel, PopupSkin.Intel);
        troopBox.AddChild(troopLabel);

        var troopSpin = new SpinBox
        {
            MinValue = 1000,
            MaxValue = Math.Max(1000, _gameState.WestGardenArmy.Size),
            Step = 1000,
            Value = Math.Min(3000, Math.Max(1000, _gameState.WestGardenArmy.Size)),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        troopBox.AddChild(troopSpin);
        var troopUnit = new Label { Text = "人" };
        StylePopupBodyText(troopUnit, PopupSkin.Intel);
        troopBox.AddChild(troopUnit);

        var costLabel = CreateActionPreviewLabel(PopupSkin.Intel);
        cardRoot.AddChild(costLabel);

        foreach (var general in militaryNpcs)
        {
            double combatPower = NpcTraitEvaluator.GetCombatPower(general);
            double distancePenalty = p.Distance * 5;

            var generalCard = new Panel { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            generalCard.AddThemeStyleboxOverride("panel", CreatePopupInnerPanelStyle(PopupSkin.Intel));
            cardRoot.AddChild(generalCard);

            var generalBox = new VBoxContainer();
            SetFullRect(generalBox);
            generalBox.OffsetLeft = 10;
            generalBox.OffsetTop = 8;
            generalBox.OffsetRight = -10;
            generalBox.OffsetBottom = -8;
            generalBox.AddThemeConstantOverride("separation", 6);
            generalCard.AddChild(generalBox);

            var detail = new RichTextLabel
            {
                BbcodeEnabled = true,
                CustomMinimumSize = new Vector2(0, 70),
                FitContent = true,
                ScrollActive = false,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            generalBox.AddChild(detail);

            var dispatch = new Button { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            generalBox.AddChild(dispatch);

            void RefreshPreview(double troopsValue)
            {
                int selectedTroops = (int)troopsValue;
                int campaignCost = Math.Max(100, selectedTroops / 10);
                double troopRatio = selectedTroops / (double)Math.Max(p.Garrison, 1);
                double troopBonus = Math.Clamp((troopRatio - 1.0) * 20, -20, 25);
                double successRate = Math.Clamp(combatPower - distancePenalty + troopBonus, 5, 95);
                string risk = successRate >= 70 ? "胜算较厚" : successRate >= 45 ? "胜负未定" : "败风险高";
                costLabel.Text = $"军费预估：{campaignCost} 万钱｜西园现有：{_gameState.WestGardenArmy.Size} 人｜出征后留守：{Math.Max(0, _gameState.WestGardenArmy.Size - selectedTroops)} 人";
                detail.Text = $"[color=#2a1608][b]{general.Name}[/b]  武 {general.Martial}｜统 {general.Leadership}｜战力 {combatPower:F0}[/color]\n" +
                    $"[color=#4a2a12]距京惩罚 {distancePenalty:F0}｜兵力修正 {troopBonus:+0;-0;0}%｜胜率 {successRate:F0}%（{risk}）[/color]";
                dispatch.Text = $"授符出征 · {general.Name} 率 {selectedTroops} 人";
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

        var cardRoot = CreateIntelDecisionCard("密札朱批 · 遣使招安", $"叛乱州郡：{p.Name}\n可叠加策略以增安抚力度，但赈灾需动用国库，惩治可能激化地方。请先圈定策略，再择使臣持节出京。");

        var strategyGrid = new GridContainer { Columns = 2, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        strategyGrid.AddThemeConstantOverride("h_separation", 12);
        strategyGrid.AddThemeConstantOverride("v_separation", 4);
        cardRoot.AddChild(strategyGrid);

        var chkSowDiscord = new CheckBox { Text = "离间计" };
        var chkPersuade = new CheckBox { Text = "说服", ButtonPressed = true };
        var chkDisasterRelief = new CheckBox { Text = "赈灾" };
        var chkPunish = new CheckBox { Text = "惩治" };
        strategyGrid.AddChild(chkSowDiscord);
        strategyGrid.AddChild(chkPersuade);
        strategyGrid.AddChild(chkDisasterRelief);
        strategyGrid.AddChild(chkPunish);

        var reliefBox = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        reliefBox.AddThemeConstantOverride("separation", 8);
        cardRoot.AddChild(reliefBox);
        var reliefLabel = new Label { Text = "赈银" };
        StylePopupBodyText(reliefLabel, PopupSkin.Intel);
        reliefBox.AddChild(reliefLabel);
        var reliefSpin = new SpinBox
        {
            MinValue = 500,
            MaxValue = 1500,
            Step = 500,
            Value = 500,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        reliefBox.AddChild(reliefSpin);
        var reliefUnit = new Label { Text = "万" };
        StylePopupBodyText(reliefUnit, PopupSkin.Intel);
        reliefBox.AddChild(reliefUnit);

        var previewFrame = new Panel { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        previewFrame.AddThemeStyleboxOverride("panel", CreatePopupInnerPanelStyle(PopupSkin.Intel));
        cardRoot.AddChild(previewFrame);
        var previewMargin = new MarginContainer();
        SetFullRect(previewMargin);
        previewMargin.AddThemeConstantOverride("margin_left", 10);
        previewMargin.AddThemeConstantOverride("margin_right", 10);
        previewMargin.AddThemeConstantOverride("margin_top", 8);
        previewMargin.AddThemeConstantOverride("margin_bottom", 8);
        previewFrame.AddChild(previewMargin);
        var preview = CreateActionPreviewLabel(PopupSkin.Intel);
        previewMargin.AddChild(preview);

        void RefreshStrategyPreview()
        {
            string strategies = string.Join(" + ", new[]
            {
                chkSowDiscord.ButtonPressed ? "离间" : "",
                chkPersuade.ButtonPressed ? "说服" : "",
                chkDisasterRelief.ButtonPressed ? $"赈灾{(int)reliefSpin.Value}万" : "",
                chkPunish.ButtonPressed ? "惩治豪强" : ""
            }.Where(s => !string.IsNullOrEmpty(s)));
            string treasuryLine = chkDisasterRelief.ButtonPressed
                ? $"国库预支：{(int)reliefSpin.Value} 万钱｜事后结余约 {Math.Max(0, _gameState.Treasury - (int)reliefSpin.Value)} 万钱"
                : "国库预支：无";
            preview.Text = string.IsNullOrEmpty(strategies)
                ? "预计策略：未选策略\n黄门捧诏不敢出宫。"
                : $"预计策略：{strategies}\n{treasuryLine}";
        }

        chkSowDiscord.Toggled += _ => RefreshStrategyPreview();
        chkPersuade.Toggled += _ => RefreshStrategyPreview();
        chkDisasterRelief.Toggled += _ => RefreshStrategyPreview();
        chkPunish.Toggled += _ => RefreshStrategyPreview();
        reliefSpin.ValueChanged += _ => RefreshStrategyPreview();
        RefreshStrategyPreview();

        var envoys = _gameState.Npcs.Values
            .Where(n => n.IsActive && n.GovernedProvinceId == null)
            .OrderByDescending(NpcTraitEvaluator.GetPoliticalSkill)
            .Take(4)
            .ToList();
        foreach (var envoy in envoys)
        {
            double politicalSkill = NpcTraitEvaluator.GetPoliticalSkill(envoy);
            var pacify = new Button
            {
                Text = $"持节出使 · {envoy.Name}  政略 {politicalSkill:F0}",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
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
            cardRoot.AddChild(pacify);
        }
    }

    private VBoxContainer CreateIntelDecisionCard(string titleText, string bodyText)
    {
        if (_intelActionsVBox == null) throw new InvalidOperationException("Intel actions container is not initialized.");

        var card = new Panel { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        card.AddThemeStyleboxOverride("panel", CreatePopupInnerPanelStyle(PopupSkin.Intel));
        _intelActionsVBox.AddChild(card);

        var root = new VBoxContainer();
        SetFullRect(root);
        root.OffsetLeft = 12;
        root.OffsetTop = 10;
        root.OffsetRight = -12;
        root.OffsetBottom = -10;
        root.AddThemeConstantOverride("separation", 8);
        card.AddChild(root);

        var title = new Label { Text = titleText };
        StyleColumnTitle(title, PopupSkin.Intel);
        root.AddChild(title);

        var body = new Label { Text = bodyText };
        StylePopupBodyText(body, PopupSkin.Intel);
        root.AddChild(body);

        return root;
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
