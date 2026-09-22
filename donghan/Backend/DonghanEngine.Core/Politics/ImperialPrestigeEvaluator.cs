using System;
using System.Collections.Generic;
using DonghanEngine.Core;

namespace DonghanEngine.Core.Politics;

/// <summary>
/// 纯领域服务：天子威望非线性政治力学评估器（紧凑黄金区 46~60，单一职责）
/// </summary>
public sealed class ImperialPrestigeEvaluator : IImperialPrestigeEvaluator
{
    public PrestigeEvaluationResult Evaluate(int currentPrestige)
    {
        int clamped = Math.Clamp(currentPrestige, 0, 100);

        // 1. 0~25: 傀儡危机区 (PuppetVulnerable)
        if (clamped <= 25)
        {
            return new PrestigeEvaluationResult(
                Prestige: clamped,
                State: PrestigeState.PuppetVulnerable,
                EdictExecutionEfficiency: 0.35,
                OppressionTransferRate: 0.0,
                RebellionChanceModifier: 0.20, // 盗贼四起无视朝廷
                IsPuppetRiskTriggered: true,
                StatusDescription: "主弱臣强 · 傀儡之虞",
                NarrativeSummary: "天子威信扫地，政令难出南宫！百官轻慢懈怠，权臣军阀暗蓄异志，朝廷随时面临【挟天子以令诸侯】之倾覆大祸！");
        }

        // 2. 26~45: 威望偏低区 (DisrespectedWeak)
        if (clamped <= 45)
        {
            return new PrestigeEvaluationResult(
                Prestige: clamped,
                State: PrestigeState.DisrespectedWeak,
                EdictExecutionEfficiency: 0.65,
                OppressionTransferRate: 0.0,
                RebellionChanceModifier: 0.05,
                IsPuppetRiskTriggered: false,
                StatusDescription: "轻慢懈弛 · 阳奉阴违",
                NarrativeSummary: "天子威严不足，尚书台与三公推诿塞责，政令执行大打折扣，地方诸侯截留贡赋，公家号令难行。");
        }

        // 3. 46~60: 【紧凑黄金平衡区】(GoldenBalance)
        if (clamped <= 60)
        {
            return new PrestigeEvaluationResult(
                Prestige: clamped,
                State: PrestigeState.GoldenBalance,
                EdictExecutionEfficiency: 1.0,
                OppressionTransferRate: 0.0,
                RebellionChanceModifier: -0.15, // 天下民心最稳，暴乱几率降低
                IsPuppetRiskTriggered: false,
                StatusDescription: "威严得体 · 垂拱而治",
                NarrativeSummary: "天子恩威并济，恰合中庸之德！百官恪尽职守，朝政畅达，下无酷吏转嫁之祸，诸侯敬畏大义，海内承平。");
        }

        // 4. 61~80: 威压过甚区 (OppressiveDread)
        if (clamped <= 80)
        {
            return new PrestigeEvaluationResult(
                Prestige: clamped,
                State: PrestigeState.OppressiveDread,
                EdictExecutionEfficiency: 1.15,
                OppressionTransferRate: 0.35,
                RebellionChanceModifier: 0.25, // 酷吏开始压迫底层
                IsPuppetRiskTriggered: false,
                StatusDescription: "威压过重 · 伴君如虎",
                NarrativeSummary: "天子雷霆震怒频加，百官战栗自危！朝中酷吏、贪婪之臣为求自保或媚上，开始向下加派横征，底层民怨暗潮涌动。");
        }

        // 5. 81~100: 雷霆暴政区 (TyrannicalTerror)
        return new PrestigeEvaluationResult(
            Prestige: clamped,
            State: PrestigeState.TyrannicalTerror,
            EdictExecutionEfficiency: 1.25,
            OppressionTransferRate: 0.80,
            RebellionChanceModifier: 0.60, // 暴乱几率暴增
            IsPuppetRiskTriggered: false,
            StatusDescription: "雷霆之威 · 敢怒不敢言",
            NarrativeSummary: "天子威权达到极致，朝野敢怒不敢言！官员人人自危，将压力疯狂转嫁于升斗小民，苛政如虎，天下万民逼上梁山，暴乱四起！");
    }

    public IReadOnlyList<string> EvaluateOppressionTransferOfficials(GameState state, PrestigeEvaluationResult prestigeResult)
    {
        var oppressiveOfficials = new List<string>();

        // 仅在高威望 (>=61) 时官员才会向下转嫁压迫
        if (prestigeResult.OppressionTransferRate <= 0.0)
            return oppressiveOfficials.AsReadOnly();

        foreach (var (npcId, npc) in state.Npcs)
        {
            if (!npc.IsActive) continue;

            // 拥有特定词条/性格（贪腐高、或野心高、或性格为狠辣/贪婪/钻营）的官员优先转嫁压迫
            bool isOppressiveTrait = npc.Corruption >= 40 || npc.Ambition >= 70 ||
                                     npc.Personality.Contains("狠辣") || npc.Personality.Contains("贪婪") || npc.Personality.Contains("深沉");

            if (isOppressiveTrait)
            {
                oppressiveOfficials.Add(npcId);
            }
        }

        return oppressiveOfficials.AsReadOnly();
    }
}
