using System;
using System.Collections.Generic;
using DonghanEngine.Core;
using DonghanEngine.Core.Balance;

namespace DonghanEngine.Core.Politics;

/// <summary>
/// 纯领域服务：天子威望非线性政治力学评估器（紧凑黄金区 46~60，单一职责）
/// 支持 IInitializableBalance&lt;ImperialPrestigeBalanceConfig&gt; 接口，供超级控制工具动态调参
/// </summary>
public sealed class ImperialPrestigeEvaluator : IImperialPrestigeEvaluator, IInitializableBalance<IImperialPrestigeBalanceProvider>
{
    private IImperialPrestigeBalanceProvider _config;

    public ImperialPrestigeEvaluator(IImperialPrestigeBalanceProvider? config = null)
    {
        _config = config ?? new ImperialPrestigeBalanceConfig();
    }

    public void InitializeConfig(IImperialPrestigeBalanceProvider config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public IImperialPrestigeBalanceProvider GetConfig() => _config;

    public PrestigeEvaluationResult Evaluate(int currentPrestige)
    {
        int clamped = Math.Clamp(currentPrestige, 0, 100);

        // 1. 0~25: 傀儡危机区 (PuppetVulnerable)
        if (clamped <= _config.PuppetMaxThreshold)
        {
            return new PrestigeEvaluationResult(
                Prestige: clamped,
                State: PrestigeState.PuppetVulnerable,
                EdictExecutionEfficiency: _config.PuppetEfficiency,
                OppressionTransferRate: 0.0,
                RebellionChanceModifier: _config.PuppetRebellionModifier,
                IsPuppetRiskTriggered: true,
                StatusDescription: "主弱臣强 · 傀儡之虞",
                NarrativeSummary: "天子威信扫地，政令难出南宫！百官轻慢懈怠，权臣军阀暗蓄异志，朝廷随时面临【挟天子以令诸侯】之倾覆大祸！");
        }

        // 2. 26~45: 威望偏低区 (DisrespectedWeak)
        if (clamped <= _config.DisrespectedMaxThreshold)
        {
            return new PrestigeEvaluationResult(
                Prestige: clamped,
                State: PrestigeState.DisrespectedWeak,
                EdictExecutionEfficiency: _config.DisrespectedEfficiency,
                OppressionTransferRate: 0.0,
                RebellionChanceModifier: _config.DisrespectedRebellionModifier,
                IsPuppetRiskTriggered: false,
                StatusDescription: "轻慢懈弛 · 阳奉阴违",
                NarrativeSummary: "天子威严不足，尚书台与三公推诿塞责，政令执行大打折扣，地方诸侯截留贡赋，公家号令难行。");
        }

        // 3. 46~60: 【紧凑黄金平衡区】(GoldenBalance)
        if (clamped <= _config.GoldenBalanceMaxThreshold)
        {
            return new PrestigeEvaluationResult(
                Prestige: clamped,
                State: PrestigeState.GoldenBalance,
                EdictExecutionEfficiency: _config.GoldenBalanceEfficiency,
                OppressionTransferRate: 0.0,
                RebellionChanceModifier: _config.GoldenBalanceRebellionModifier,
                IsPuppetRiskTriggered: false,
                StatusDescription: "威严得体 · 垂拱而治",
                NarrativeSummary: "天子恩威并济，恰合中庸之德！百官恪尽职守，朝政畅达，下无酷吏转嫁之祸，诸侯敬畏大义，海内承平。");
        }

        // 4. 61~80: 威压过甚区 (OppressiveDread)
        if (clamped <= _config.OppressiveMaxThreshold)
        {
            return new PrestigeEvaluationResult(
                Prestige: clamped,
                State: PrestigeState.OppressiveDread,
                EdictExecutionEfficiency: _config.OppressiveEfficiency,
                OppressionTransferRate: _config.OppressiveTransferRate,
                RebellionChanceModifier: _config.OppressiveRebellionModifier,
                IsPuppetRiskTriggered: false,
                StatusDescription: "威压过重 · 伴君如虎",
                NarrativeSummary: "天子雷霆震怒频加，百官战栗自危！朝中酷吏、贪婪之臣为求自保或媚上，开始向下加派横征，底层民怨暗潮涌动。");
        }

        // 5. 81~100: 雷霆暴政区 (TyrannicalTerror)
        return new PrestigeEvaluationResult(
            Prestige: clamped,
            State: PrestigeState.TyrannicalTerror,
            EdictExecutionEfficiency: _config.TyrannicalEfficiency,
            OppressionTransferRate: _config.TyrannicalTransferRate,
            RebellionChanceModifier: _config.TyrannicalRebellionModifier,
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
            bool isOppressiveTrait = npc.Corruption >= _config.OppressiveCorruptionThreshold || npc.Ambition >= _config.OppressiveAmbitionThreshold ||
                                     npc.Personality.Contains("狠辣") || npc.Personality.Contains("贪婪") || npc.Personality.Contains("深沉");

            if (isOppressiveTrait)
            {
                oppressiveOfficials.Add(npcId);
            }
        }

        return oppressiveOfficials.AsReadOnly();
    }
}
