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
            var cardRoot = CreateIntelDecisionCard("刺史敕札 · 召还地方官", $"州郡：{p.Name}\n现任地方官：{governorName}\n召还后该郡将暂无主官，无人治理会使民心加速下坠；但被召还者将重返京师，朝中权势略回升。");

            if (_gameState.Npcs.TryGetValue(p.GovernorId, out var currentGovernor))
            {
                var detailFrame = new PanelContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
                detailFrame.AddThemeStyleboxOverride("panel", CreatePopupInnerPanelStyle(PopupSkin.Intel));
                cardRoot.AddChild(detailFrame);

                var detail = new Label
                {
                    Text = $"政务能力：{DescribeGovernorPolitics(currentGovernor)}\n野心风险：{DescribeGovernorAmbition(currentGovernor)}\n派系牵连：{currentGovernor.Faction}\n回京影响：{currentGovernor.Name}权势 +3，{p.Name}暂失主官"
                };
                StylePopupBodyText(detail, PopupSkin.Intel);
                detailFrame.AddChild(detail);
            }

            var recall = new Button
            {
                Text = $"朱批召还 · {governorName} 回京述职",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            StyleSceneActionButton(recall, ActionButtonSkin.Document);
            recall.Pressed += () =>
            {
                _windowManager.PopWindow();
                var result = _gameEngine!.RecallGovernor(p.Id);
                ShowStoryReportPopup("刺史敕札", result.StoryText, PopupSkin.Intel);
                UpdateUI();
            };
            cardRoot.AddChild(recall);
            return;
        }

        var candidates = _gameState.Npcs.Values
            .Where(n => n.IsActive && !n.IsHostile && n.GovernedProvinceId == null)
            .OrderByDescending(n => n.Politics)
            .ThenBy(n => n.Ambition)
            .Take(4)
            .ToList();

        if (candidates.Count == 0)
        {
            _intelActionsVBox.AddChild(new Label { Text = "京中暂无闲置文武。", AutowrapMode = TextServer.AutowrapMode.WordSmart });
            return;
        }

        var root = CreateIntelDecisionCard("吏部铨选 · 外任太守", $"州郡：{p.Name}\n当前地方民心：{p.LocalSupport}/100｜守军：{p.Garrison}\n任命地方官可立即稳民心 +10，并使该臣远离中枢、权势 -5；高野心臣在低民心地区有割据风险。");

        foreach (var npc in candidates)
        {
            var candidateCard = new PanelContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            candidateCard.AddThemeStyleboxOverride("panel", CreatePopupInnerPanelStyle(PopupSkin.Intel));
            root.AddChild(candidateCard);

            var candidateBox = new VBoxContainer();
            candidateBox.AddThemeConstantOverride("separation", 6);
            candidateCard.AddChild(candidateBox);

            var detail = new RichTextLabel
            {
                BbcodeEnabled = true,
                FitContent = true,
                ScrollActive = false,
                CustomMinimumSize = new Vector2(0, 78),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            detail.Text = $"[color=#2a1608][b]{npc.Name}[/b]  {npc.Title}｜{npc.Faction}[/color]\n" +
                $"[color=#4a2a12]政务：{DescribeGovernorPolitics(npc)}｜野心：{DescribeGovernorAmbition(npc)}｜风格：{npc.Style}[/color]\n" +
                $"[color=#4a2a12]预判：{DescribeGovernorAppointmentImpact(p, npc)}[/color]";
            candidateBox.AddChild(detail);

            var appoint = new Button
            {
                Text = $"朱批外任 · {npc.Name} 出守 {p.Name}",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            StyleSceneActionButton(appoint, ActionButtonSkin.Document);
            string npcId = npc.Id;
            appoint.Pressed += () =>
            {
                _windowManager.PopWindow();
                var result = _gameEngine!.AssignGovernor(p.Id, npcId);
                ShowStoryReportPopup("吏部回奏", result.StoryText, PopupSkin.Intel);
                UpdateUI();
            };
            candidateBox.AddChild(appoint);
        }
    }

    private void RenderSuppressRebellionActions(Province p)
    {
        if (_intelActionsVBox == null || _gameState == null) return;
        AddIntelSectionTitle("【军事平叛】");

        var militaryNpcs = _gameState.Npcs.Values
            .Where(n => n.IsActive && !n.IsHostile && n.GovernedProvinceId == null)
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
        StylePopupInput(troopSpin, PopupSkin.Intel);
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

            var generalCard = new PanelContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            generalCard.AddThemeStyleboxOverride("panel", CreatePopupInnerPanelStyle(PopupSkin.Intel));
            cardRoot.AddChild(generalCard);

            var generalBox = new VBoxContainer();
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
            StyleSceneActionButton(dispatch, ActionButtonSkin.Document);
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
                ShowStoryReportPopup("军情战报", result.StoryText, PopupSkin.Intel);
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
        StylePopupCheckBox(chkSowDiscord, PopupSkin.Intel);
        StylePopupCheckBox(chkPersuade, PopupSkin.Intel);
        StylePopupCheckBox(chkDisasterRelief, PopupSkin.Intel);
        StylePopupCheckBox(chkPunish, PopupSkin.Intel);
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
        StylePopupInput(reliefSpin, PopupSkin.Intel);
        reliefBox.AddChild(reliefSpin);
        var reliefUnit = new Label { Text = "万" };
        StylePopupBodyText(reliefUnit, PopupSkin.Intel);
        reliefBox.AddChild(reliefUnit);

        var previewFrame = new PanelContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        previewFrame.AddThemeStyleboxOverride("panel", CreatePopupInnerPanelStyle(PopupSkin.Intel));
        cardRoot.AddChild(previewFrame);
        var preview = CreateActionPreviewLabel(PopupSkin.Intel);
        previewFrame.AddChild(preview);

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
            .Where(n => n.IsActive && !n.IsHostile && n.GovernedProvinceId == null)
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
            StyleSceneActionButton(pacify, ActionButtonSkin.Document);
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
                    ShowStoryReportPopup("招安未发", "【招安未发】\n\n陛下尚未选定任何安抚策略，黄门捧诏不敢出宫。", PopupSkin.Warning);
                    return;
                }

                int reliefGold = chkDisasterRelief.ButtonPressed ? (int)reliefSpin.Value : 0;
                _windowManager.PopWindow();
                var result = _gameEngine!.PacifyRebellion(p.Id, envoyId, strategies, reliefGold);
                ShowStoryReportPopup("黄门密奏", result.StoryText, PopupSkin.Intel);
                UpdateUI();
            };
            cardRoot.AddChild(pacify);
        }
    }

    private static string DescribeGovernorPolitics(NpcState npc)
    {
        return npc.Politics switch
        {
            >= 80 => $"{npc.Politics}（经邦治郡）",
            >= 60 => $"{npc.Politics}（足堪任事）",
            >= 40 => $"{npc.Politics}（守成尚可）",
            _ => $"{npc.Politics}（政务薄弱）"
        };
    }

    private static string DescribeGovernorAmbition(NpcState npc)
    {
        return npc.Ambition switch
        {
            >= 80 => $"{npc.Ambition}（鹰视狼顾）",
            >= 60 => $"{npc.Ambition}（需防坐大）",
            >= 40 => $"{npc.Ambition}（尚可驾驭）",
            _ => $"{npc.Ambition}（低）"
        };
    }

    private static string DescribeGovernorAppointmentImpact(Province province, NpcState npc)
    {
        string governance = npc.Politics >= 70 ? "治政可期" : npc.Politics >= 45 ? "可暂稳地方" : "恐难理繁剧";
        string ambitionRisk = npc.Ambition >= 60 && province.LocalSupport <= 30 ? "低民心下有叛离隐患" : npc.Ambition >= 60 ? "需防外任养望" : "割据风险较低";
        return $"民心 +10，{npc.Name}权势 -5；{governance}，{ambitionRisk}";
    }

    private VBoxContainer CreateIntelDecisionCard(string titleText, string bodyText)
    {
        if (_intelActionsVBox == null) throw new InvalidOperationException("Intel actions container is not initialized.");

        var card = new PanelContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        card.AddThemeStyleboxOverride("panel", CreatePopupInnerPanelStyle(PopupSkin.Intel));
        _intelActionsVBox.AddChild(card);

        var root = new VBoxContainer();
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
