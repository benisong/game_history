using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DonghanEngine.Core;

namespace DonghanFrontend;

public partial class MainScene : Control
{
    private void ShowMinisterDetails(string ministerId)
    {
        if (_gameState == null) return;
        if (!_gameState.Npcs.TryGetValue(ministerId, out var minister)) return;

        _currentDetailsMinisterId = ministerId;

        var panel = new Panel();
        panel.Name = "MinisterDossierPopup";
        ConfigureCenteredPopupPanel(panel, PopupSkin.Document, new Vector2(660, 500));

        var root = new VBoxContainer();
        SetFullRect(root);
        root.OffsetLeft = 24;
        root.OffsetTop = 20;
        root.OffsetRight = -24;
        root.OffsetBottom = -20;
        root.AddThemeConstantOverride("separation", 12);
        panel.AddChild(root);

        var title = new Label { Text = $"奏牍档案 · {minister.Name}" };
        StylePopupTitle(title, PopupSkin.Document);
        root.AddChild(title);

        var subtitle = new Label
        {
            Text = $"{minister.Title}｜{minister.Faction}｜{(minister.GovernedProvinceId != null && _gameState.Provinces.TryGetValue(minister.GovernedProvinceId, out var province) ? $"外任 {province.Name}" : "在京候旨")}",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        StylePopupBodyText(subtitle, PopupSkin.Document);
        root.AddChild(subtitle);

        var dossierFrame = new Panel
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        dossierFrame.AddThemeStyleboxOverride("panel", CreatePopupInnerPanelStyle(PopupSkin.Document));
        root.AddChild(dossierFrame);

        var dossier = new RichTextLabel
        {
            BbcodeEnabled = true,
            FitContent = false,
            ScrollActive = true,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            Text = BuildMinisterDossierText(minister)
        };
        dossier.AddThemeColorOverride("default_color", GetPopupBodyColor(PopupSkin.Document));
        SetFullRect(dossier);
        dossier.OffsetLeft = 16;
        dossier.OffsetTop = 14;
        dossier.OffsetRight = -16;
        dossier.OffsetBottom = -14;
        dossierFrame.AddChild(dossier);

        var row = CreateActionPopupButtonRow();
        var treasury = new Button { Text = "籍没入国库", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, Disabled = minister.StashedWealth <= 0 };
        var privateTreasury = new Button { Text = "籍没入西园私库", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, Disabled = minister.StashedWealth <= 0 };
        var close = new Button { Text = "合上奏牍", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        StyleSceneActionButton(treasury, ActionButtonSkin.Warning);
        StyleSceneActionButton(privateTreasury, ActionButtonSkin.Warning);
        StyleSceneActionButton(close, ActionButtonSkin.Document);
        treasury.Pressed += () => ShowConfiscateConfirmAction("国库");
        privateTreasury.Pressed += () => ShowConfiscateConfirmAction("私库");
        close.Pressed += _windowManager.PopWindow;
        row.AddChild(treasury);
        row.AddChild(privateTreasury);
        row.AddChild(close);
        root.AddChild(row);

        PushTemporaryPopup(panel);
    }

    private string BuildMinisterDossierText(NpcState minister)
    {
        if (_gameState == null) return string.Empty;

        string location = minister.GovernedProvinceId != null && _gameState.Provinces.TryGetValue(minister.GovernedProvinceId, out var province)
            ? $"外任记录：{province.Name} 太守"
            : "任所记录：在京候旨";
        string confiscationHint = minister.StashedWealth > 0
            ? "可由廷尉奏牍发起籍没，执行前仍需圣裁确认。"
            : "暂无可籍赃银，强行籍没收益极低。";

        string riskLine = BuildNpcRiskLine(minister);
        string relationLine = BuildNpcRelationDossierText(minister.Id);
        string sourceLine = $"登场来源：{minister.InitialLocation}｜{minister.EntryCondition}";
        string roleLine = string.IsNullOrWhiteSpace(minister.HistoricalRole) ? "史料定位：暂无" : $"史料定位：{minister.HistoricalRole}";
        string deathLine = minister.HistoricalDeathYear.HasValue ? $"史实卒年：{minister.HistoricalDeathYear.Value}（可因玩家干预改写）" : "史实卒年：暂无定论";
        string sourceNoteLine = string.IsNullOrWhiteSpace(minister.SourceNote) ? "来源说明：暂无" : $"来源说明：{minister.SourceNote}";

        return $"[b]【身分】[/b]\n" +
            $"姓名：{minister.Name}\n官职：{minister.Title}\n派系：{minister.Faction}\n{location}\n{sourceLine}\n{roleLine}\n{deathLine}\n{sourceNoteLine}\n\n" +
            $"[b]【君臣与朝局】[/b]\n" +
            $"君臣情分：{DescribeAttitudeLevel(minister.Favorability)}（{minister.Favorability}/100）\n" +
            $"朝局分量：{DescribePowerLevel(minister.Power)}（{minister.Power}/100）\n" +
            $"廉污风评：{DescribeCorruptionLevel(minister.Corruption)}（{minister.Corruption}/100）\n" +
            $"可籍赃银：{minister.StashedWealth} 万钱\n" +
            $"风险札记：{riskLine}\n\n" +
            $"[b]【五维摘录】[/b]\n" +
            $"武略 {minister.Martial}｜统御 {minister.Leadership}｜政术 {minister.Politics}\n" +
            $"声望 {minister.Charisma}｜野心 {minister.Ambition}\n\n" +
            $"[b]【关系札记】[/b]\n{relationLine}\n\n" +
            $"[b]【廷尉提示】[/b]\n{confiscationHint}";
    }

    private string BuildNpcRelationDossierText(string npcId)
    {
        if (_gameState == null) return "暂无关系记录。";

        var relations = _gameState.NpcRelations
            .Where(r => r.FromNpcId == npcId || (r.IsMutual && r.ToNpcId == npcId))
            .OrderByDescending(r => r.Strength)
            .Take(8)
            .ToList();

        if (relations.Count == 0) return "暂无关系记录。";

        var lines = new List<string>();
        foreach (var relation in relations)
        {
            string otherId = relation.FromNpcId == npcId ? relation.ToNpcId : relation.FromNpcId;
            string otherName = ResolveNpcDisplayName(otherId);
            lines.Add($"{GetRelationTypeLabel(relation.Type)}：{otherName}（{relation.Label}，强度{relation.Strength}）");
        }

        return string.Join("\n", lines);
    }

    private string ResolveNpcDisplayName(string npcId)
    {
        if (_gameState != null && _gameState.Npcs.TryGetValue(npcId, out var activeNpc)) return activeNpc.Name;
        var preset = HistoricalNpcPresets.All.FirstOrDefault(n => n.Id == npcId);
        return preset?.Name ?? npcId;
    }

    private static string GetRelationTypeLabel(NpcRelationType type)
    {
        return type switch
        {
            NpcRelationType.Kinship => "宗族姻亲",
            NpcRelationType.Patronage => "提携举荐",
            NpcRelationType.FactionAlly => "同党盟友",
            NpcRelationType.TeacherStudent => "师生门下",
            NpcRelationType.SwornBond => "义从战友",
            NpcRelationType.Rivalry => "政治竞争",
            NpcRelationType.Hostility => "宿敌仇怨",
            NpcRelationType.Command => "军中统属",
            NpcRelationType.RegionalTie => "乡党州郡",
            _ => "关系"
        };
    }

    private static string BuildNpcRiskLine(NpcState minister)
    {
        var risks = new List<string>();
        if (minister.IsHostile) risks.Add("敌对势力");
        if (minister.Ambition >= 80) risks.Add("野心极高");
        else if (minister.Ambition >= 65) risks.Add("有坐大风险");
        if (minister.Power >= 70) risks.Add("党羽炽盛");
        if (minister.Corruption >= 70) risks.Add("巨蠹可查");
        if (minister.Traits.Contains(TraitNames.ShouXiaYouBing) || minister.Traits.Contains(TraitNames.YongBingZiZhong)) risks.Add("握兵风险");
        if (minister.Traits.Contains(TraitNames.MenFaShiJia) || minister.Traits.Contains(TraitNames.ChuShenMingMen)) risks.Add("门阀牵连");
        if (minister.Traits.Contains(TraitNames.QingZhengLianJie)) risks.Add("清流名望，不宜轻动");
        if (risks.Count == 0) risks.Add("暂可驾驭");
        return string.Join("；", risks);
    }

    private void ApplyMinisterDocumentSkin()
    {
        if (_ministerPanel == null) return;
        _ministerPanel.AddThemeStyleboxOverride("panel", CreatePopupPanelStyle(PopupSkin.Document));

        var vBox = _ministerPanel.GetNodeOrNull<VBoxContainer>("VBox");
        if (vBox != null) vBox.AddThemeConstantOverride("separation", 10);

        foreach (var label in new[]
        {
            _ministerFavorabilityLabel,
            _ministerPowerLabel,
            GetNodeOrNull<Label>("MinisterOverlayPanel/VBox/MinisterCorruption"),
            GetNodeOrNull<Label>("MinisterOverlayPanel/VBox/MinisterWealth")
        })
        {
            if (label == null) continue;
            ConfigureWrappingLabel(label);
            label.AddThemeColorOverride("font_color", new Color(0.20f, 0.12f, 0.06f, 1.0f));
        }

        var actionRow = GetNodeOrNull<HBoxContainer>("MinisterOverlayPanel/VBox/HBox");
        if (actionRow != null)
        {
            foreach (var child in actionRow.GetChildren())
            {
                if (child is Button button)
                {
                    button.Text = button.Name.ToString() switch
                    {
                        "ConfiscateTreasuryBtn" => "籍没入国库",
                        "ConfiscatePrivateBtn" => "籍没入西园私库",
                        _ => button.Text
                    };
                }
            }
        }
    }

    private static string DescribeAttitudeLevel(int value)
    {
        if (value >= 75) return "亲近";
        if (value >= 55) return "可用";
        if (value >= 35) return "观望";
        return "疏离";
    }

    private static string DescribePowerLevel(int value)
    {
        if (value >= 75) return "炽盛";
        if (value >= 55) return "有势";
        if (value >= 35) return "中平";
        return "微弱";
    }

    private static string DescribeCorruptionLevel(int value)
    {
        if (value >= 75) return "巨蠹";
        if (value >= 55) return "可疑";
        if (value >= 35) return "有瑕";
        return "尚廉";
    }

    private void ShowConfiscateConfirmAction(string destination)
    {
        if (_gameState == null || string.IsNullOrEmpty(_currentDetailsMinisterId)) return;
        if (!_gameState.Npcs.TryGetValue(_currentDetailsMinisterId, out var target)) return;

        var panel = new Panel();
        panel.Name = "ConfiscationConfirmPopup";
        panel.Visible = false;
        ConfigureCenteredPopupPanel(panel, PopupSkin.Warning, new Vector2(620, 420));

        var root = new VBoxContainer();
        SetFullRect(root);
        root.OffsetLeft = 24;
        root.OffsetTop = 20;
        root.OffsetRight = -24;
        root.OffsetBottom = -20;
        root.AddThemeConstantOverride("separation", 12);
        panel.AddChild(root);

        var title = new Label { Text = "廷尉奏牍 · 籍没家产" };
        StylePopupTitle(title, PopupSkin.Warning);
        root.AddChild(title);

        var desc = new Label
        {
            Text = $"目标朝臣：{target.Name} · {target.Title}\n朱批去向：{destination}\n此令一出，即为当朝籍没。若目标党羽炽盛或其人尚廉，可能激起朝臣反噬与民怨。"
        };
        StylePopupBodyText(desc, PopupSkin.Warning);
        root.AddChild(desc);

        var previewFrame = new Panel { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        previewFrame.AddThemeStyleboxOverride("panel", CreatePopupInnerPanelStyle(PopupSkin.Warning));
        root.AddChild(previewFrame);

        var preview = new Label
        {
            Text = BuildConfiscationPreviewText(target, destination),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        StylePopupBodyText(preview, PopupSkin.Warning);
        SetFullRect(preview);
        preview.OffsetLeft = 14;
        preview.OffsetTop = 12;
        preview.OffsetRight = -14;
        preview.OffsetBottom = -12;
        previewFrame.AddChild(preview);

        var row = CreateActionPopupButtonRow();
        var confirm = new Button
        {
            Text = destination == "国库" ? "朱批籍没 · 入国库" : "朱批籍没 · 入西园私库",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            Disabled = target.StashedWealth <= 0
        };
        var cancel = new Button { Text = "暂缓不发", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        StyleSceneActionButton(confirm, ActionButtonSkin.Warning);
        StyleSceneActionButton(cancel, ActionButtonSkin.Warning);
        row.AddChild(confirm);
        row.AddChild(cancel);
        root.AddChild(row);

        confirm.Pressed += () =>
        {
            _windowManager.PopWindow();
            DoConfiscateAction(destination);
        };
        cancel.Pressed += _windowManager.PopWindow;

        PushTemporaryPopup(panel);
    }

    private string BuildConfiscationPreviewText(NpcState target, string destination)
    {
        int rawWealth = target.StashedWealth;
        int expectedTreasury = (int)(rawWealth * 0.70);
        int expectedPrivate = rawWealth - expectedTreasury;
        string destinationLine = destination == "国库"
            ? $"入账预估：国库约 +{expectedTreasury} 万钱，西园私库约 +{expectedPrivate} 万钱（按现行籍没拆分）"
            : $"入账预估：国库约 +{expectedTreasury} 万钱，西园私库约 +{expectedPrivate} 万钱（即便朱批偏私，办案仍会有国税拆分）";
        string factionRisk = target.Power >= 60 ? "高：目标朝局分量炽盛，党羽可能反噬皇权" : "可控：目标朝局分量未至炽盛";
        string justiceRisk = target.Corruption < 35 ? "高：廉污风评尚廉，强行籍没恐伤民心" : target.Corruption >= 70 ? "低：巨蠹有赃，民间或称快" : "中：需罗织罪名坐实赃款";
        string wealthLine = rawWealth > 0 ? $"可籍赃银：{rawWealth} 万钱" : "可籍赃银：无；此时强行动手收益极低";
        string locationLine = _gameState?.CurrentLocation == "宣政殿"
            ? "礼法条件：已在宣政殿，可当朝宣旨"
            : "礼法条件：不在宣政殿，强行籍没将被御史驳回";

        return $"{wealthLine}\n{destinationLine}\n党羽反噬：{factionRisk}\n清议风险：{justiceRisk}\n{locationLine}\n后果预判：目标权势与好感将大幅下挫；若朝中无近臣出列弹劾，圣旨可能流产并损失皇权。";
    }

    // 执行抄家动作
    private void DoConfiscateAction(string destination)
    {
        if (_gameEngine == null || string.IsNullOrEmpty(_currentDetailsMinisterId)) return;

        try
        {
            // 关闭详情面板
            _windowManager.PopWindow();

            // 如果不在宣政殿，发出警告
            if (_gameState?.CurrentLocation != "宣政殿")
            {
                ShowStoryReportPopup("御史弹劾", "【御史弹劾】\n\n“陛下，抄没朝臣家产兹事体大，必须在宣政殿百官大朝会上宣旨籍没，方可调动京师御林军，否则名不正言不顺！”", PopupSkin.Warning);
                return;
            }

            var result = _gameEngine.ExecuteConfiscationAction(_currentDetailsMinisterId, destination);
            ShowStoryReportPopup("廷尉回奏", result.StoryText, PopupSkin.Warning);
            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
        }
    }

    private ScrollContainer? _npcScrollContainer;
    private VBoxContainer? _npcListVBox;

    private void InitializeDynamicNpcList()
    {
        var ministersVBox = GetNodeOrNull<VBoxContainer>("RightPanel/Ministers");
        if (ministersVBox == null) return;

        // 隐藏旧的硬编码按钮，防止并立冲突
        if (_heJinButton != null) _heJinButton.Hide();
        if (_zhangRangButton != null) _zhangRangButton.Hide();
        if (_caoCaoButton != null) _caoCaoButton.Hide();
        if (_jianShuoButton != null) _jianShuoButton.Hide();

        _npcScrollContainer = new ScrollContainer();
        _npcScrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _npcScrollContainer.CustomMinimumSize = new Vector2(0, 200);

        _npcListVBox = new VBoxContainer();
        _npcListVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _npcScrollContainer.AddChild(_npcListVBox);

        // 将动态滚动列表插入到 ministersVBox 中，位于 SceneTitle 和 InteractiveLabel 之后
        ministersVBox.AddChild(_npcScrollContainer);
    }

    private void UpdateNpcList()
    {
        if (_npcListVBox == null || _gameState == null) return;

        // 清理旧节点
        foreach (Node child in _npcListVBox.GetChildren())
        {
            _npcListVBox.RemoveChild(child);
            child.QueueFree();
        }

        // 动态载入当前在朝的所有活跃大臣
        foreach (var npc in _gameState.Npcs.Values
            .Where(n => n.IsActive && !n.IsHostile)
            .OrderBy(GetNpcListFactionOrder)
            .ThenByDescending(n => n.Power)
            .ThenBy(n => n.Name))
        {
            string locationTag = npc.GovernedProvinceId != null ? $"【任{_gameState.Provinces[npc.GovernedProvinceId].Name}】" : "【在京】";
            var btn = new Button();
            btn.Text = $"[{npc.Faction}] {npc.Name} {locationTag}";
            btn.Alignment = HorizontalAlignment.Left;
            btn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            StyleSceneActionButton(btn, ActionButtonSkin.Court);
            
            string npcId = npc.Id;
            btn.Pressed += () => ShowMinisterDetails(npcId);
            _npcListVBox.AddChild(btn);
        }
    }

    private static int GetNpcListFactionOrder(NpcState npc)
    {
        return npc.Faction switch
        {
            "外戚派" => 0,
            "阉党派" => 1,
            "西园亲军" => 2,
            "清流派" => 3,
            "地方州牧" => 4,
            "割据军阀" => 5,
            _ => 6
        };
    }
}
