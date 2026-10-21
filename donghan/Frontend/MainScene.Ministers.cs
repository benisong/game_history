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
        var close = new Button { Text = "合上奏牍", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        StyleSceneActionButton(close, ActionButtonSkin.Document);
        close.Pressed += _windowManager.PopWindow;
        row.AddChild(close);
        root.AddChild(row);

        PushTemporaryPopup(panel);
    }

    private string BuildMinisterDossierText(NpcState minister)
    {
        if (_gameState == null) return string.Empty;

        string location = minister.GovernedProvinceId != null && _gameState.Provinces.TryGetValue(minister.GovernedProvinceId, out var province)
            ? $"外任出处：{province.Name} 太守"
            : "任所现状：在京听宣";

        string riskLine = BuildNpcRiskLine(minister);
        string relationLine = BuildNpcRelationDossierText(minister.Id);

        string talentLine = $"武略 {DescribeStatGrade(minister.Martial)} ｜ 统御 {DescribeStatGrade(minister.Leadership)} ｜ 政术 {DescribeStatGrade(minister.Politics)} ｜ 声望 {DescribeStatGrade(minister.Charisma)}";
        string ambitionLine = DescribeAmbitionLevel(minister.Ambition);

        return $"[b]【朝官履历】[/b]\n" +
            $"姓名：{minister.Name}\n官职：{minister.Title}\n派系：{minister.Faction}\n{location}\n\n" +
            $"[b]【朝野风评与圣眷】[/b]\n" +
            $"君臣情分：{DescribeAttitudeLevel(minister.Favorability)}\n" +
            $"朝局分量：{DescribePowerLevel(minister.Power)}\n" +
            $"廉吏风评：{DescribeCorruptionLevel(minister.Corruption)}\n" +
            $"抱负意向：{ambitionLine}\n" +
            $"机要评定：{riskLine}\n\n" +
            $"[b]【经世才干】[/b]\n" +
            $"{talentLine}\n\n" +
            $"[b]【世家宗党与交往】[/b]\n{relationLine}";
    }

    private static string DescribeStatGrade(int value)
    {
        if (value >= 85) return "卓越";
        if (value >= 70) return "练达";
        if (value >= 50) return "平正";
        if (value >= 35) return "凡庸";
        return "拙劣";
    }

    private static string DescribeAmbitionLevel(int value)
    {
        if (value >= 80) return "觊觎神器，异志昭然";
        if (value >= 65) return "自视甚高，图谋自立";
        if (value >= 45) return "汲汲于功名利禄";
        if (value >= 30) return "安分守己";
        return "淡泊名利，忠贞恭谨";
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

        bool isEunuch = target.Faction == FactionCatalog.EunuchFaction;
        bool isImperialClan = target.Faction == FactionCatalog.ImperialClan;
        bool isPureStream = target.Faction == FactionCatalog.PureStream;

        string factionBacklash;
        if (isPureStream)
        {
            factionBacklash = "⚠️ 警示：目标在清流名士与天下太学生中名望极高，籍没恐引发朝臣死谏、清流反感与民怨沸腾";
        }
        else if (isImperialClan)
        {
            factionBacklash = "⚠️ 警示：目标为外戚重臣爪牙，籍没恐激化外戚大将军派系矛盾，导致内廷与外朝关系骤紧";
        }
        else if (isEunuch)
        {
            factionBacklash = "预计受到外廷清流公卿一致赞扬称颂，但内廷宦官近侍人人自危、心怀猜忌";
        }
        else
        {
            factionBacklash = "籍没将触动朝堂世家豪强之利益底线，恐招致朝臣暗中抵制与非议";
        }

        string wealthDestination = destination == "国库" 
            ? "抄家所得家私将充实大司农国库，用于平叛赈灾、补足军饷" 
            : "抄家所得家私将悉数密入西园私库，归天子内帑调度";

        ShowDecisionConfirmDialog(
            "廷尉奏牍 · 籍没家产",
            $"抄没私财 · {target.Name}",
            $"降旨籍没【{target.Name}·{target.Title}】名下田宅、私蓄与家资，明正典刑。",
            new[]
            {
                wealthDestination,
                factionBacklash,
                "被抄没官员将被革职下狱，其名下党羽势力将受重创"
            },
            () =>
            {
                _windowManager.PopWindow();
                DoConfiscateAction(destination);
            },
            destination == "国库" ? PopupSkin.Court : PopupSkin.WestGarden,
            "下诏抄没");
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
        string relationRisk = BuildConfiscationRelationPreviewText(target.Id);
        string justiceRisk = target.Corruption < 35 ? "高：廉污风评尚廉，强行籍没恐伤民心" : target.Corruption >= 70 ? "低：巨蠹有赃，民间或称快" : "中：需罗织罪名坐实赃款";
        string wealthLine = rawWealth > 0 ? $"可籍赃银：{rawWealth} 万钱" : "可籍赃银：无；此时强行动手收益极低";
        string locationLine = _gameState?.CurrentLocation == "宣政殿"
            ? "礼法条件：已在宣政殿，可当朝宣旨"
            : "礼法条件：不在宣政殿，强行籍没将被御史驳回";

        return $"{wealthLine}\n{destinationLine}\n党羽反噬：{factionRisk}\n关系牵连：{relationRisk}\n清议风险：{justiceRisk}\n{locationLine}\n后果预判：目标权势与好感将大幅下挫；若朝中无近臣出列弹劾，圣旨可能流产并损失皇权。";
    }

    private string BuildConfiscationRelationPreviewText(string targetId)
    {
        if (_gameEngine == null) return "暂无可见牵连。";

        var backlashes = _gameEngine.PreviewConfiscationRelationBacklashes(targetId);
        if (backlashes.Count == 0)
        {
            return "暂无可见牵连。";
        }

        return string.Join("；", backlashes.Take(3).Select(backlash =>
            $"{backlash.NpcName}（{backlash.Label}，好感 {(backlash.FavorabilityDelta >= 0 ? "+" : "")}{backlash.FavorabilityDelta}，权势 {(backlash.PowerDelta >= 0 ? "+" : "")}{backlash.PowerDelta}）"));
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
                ShowWarningReportPopup("御史弹劾", "【御史弹劾】\n\n“陛下，抄没朝臣家产兹事体大，必须在宣政殿百官大朝会上宣旨籍没，方可调动京师御林军，否则名不正言不顺！”");
                return;
            }

            var result = _gameEngine.ExecuteConfiscationAction(_currentDetailsMinisterId, destination);
            ShowWarningReportPopup("廷尉籍没回奏", result.StoryText);
            UpdateUI();
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex.Message);
        }
    }
}
