using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DonghanEngine.Core;

namespace DonghanFrontend;

// Mock 调试组件以便在编译期直接提供依赖
public class MockScheduler : IAIScheduler
{
    public INpcLifecycleManager NpcManager { get; } = null!;

    public Task<AIOrchestrationResult> OrchestrateGrandCourtAsync(string playerInput, string activeOfficerId, GameState state)
    {
        var result = new AIOrchestrationResult();

        // P2-3 旬变重置
        int xk = XunKeyOf(state);
        if (xk != _lastXunKey)
        {
            _spokenThisXun.Clear();
            _lastXunKey = xk;
        }

        // P2-6 意图分类
        var cls = IntentClassifier.Classify(playerInput);
        result.PrimaryIntent = cls.Intent.ToString();

        // P2-7 完整版：动态选 2 名 NPC 发言（派系对立 + 派系相同/中立兜底）
        // 替代原 14 个硬编码 EmitFaction；台词从 FactionSpeechBank 查，桶未命中则用 GetDefault() 兜底
        SelectSpeakersForIntent(result, state, activeOfficerId, cls.Intent);

        // P2-2：让 activeOfficerId（朝廷主持人）在 result 中的发言置于队首
        MoveActiveOfficerToFront(result, activeOfficerId);

        // P2-1：台词模板占位符解析（{Treasury}/{Health}/{PopularSupport}/{Morale}）
        ResolveTemplates(result, state);

        // P2-7：过滤已下野 / 敌对 / 不在殿中的 NPC 发言，并从 _spokenThisXun 同步清理
        FilterIneligibleSpeakers(result, state);

        // P2-7：过滤后若无任何合格发言，从殿中未发言池按 Power 选 1 名兜底
        if (result.Speeches.Count == 0)
        {
            EmitFallback(result, state, activeOfficerId);
        }

        return Task.FromResult(result);
    }

    private void ResolveTemplates(AIOrchestrationResult result, GameState state)
    {
        var morale = state.WestGardenArmy?.Morale.ToString() ?? "—";
        foreach (var s in result.Speeches)
        {
            s.SpeechText = s.SpeechText
                .Replace("{Treasury}", state.Treasury.ToString())
                .Replace("{Health}", state.Health.ToString())
                .Replace("{PopularSupport}", state.PopularSupport.ToString())
                .Replace("{Morale}", morale);
        }
    }

    public Task OrchestrateXunUpdateAsync(GameState state)
    {
        return Task.CompletedTask;
    }

    private void FilterIneligibleSpeakers(AIOrchestrationResult result, GameState state)
    {
        var toRemove = new HashSet<string>();
        foreach (var s in result.Speeches)
        {
            if (!state.Npcs.TryGetValue(s.MinisterId, out var n)
                || !n.IsActive
                || n.IsHostile
                || n.InitialLocation != "洛阳朝堂")
            {
                toRemove.Add(s.MinisterId);
            }
        }
        if (toRemove.Count == 0) return;
        result.Speeches.RemoveAll(s => toRemove.Contains(s.MinisterId));
        foreach (var id in toRemove) _spokenThisXun.Remove(id);
    }

    private void MoveActiveOfficerToFront(AIOrchestrationResult result, string activeOfficerId)
    {
        if (string.IsNullOrEmpty(activeOfficerId)) return;
        if (result.Speeches.Count == 0) return;
        var existing = result.Speeches.FirstOrDefault(s => s.MinisterId == activeOfficerId);
        if (existing == null) return;
        result.Speeches.Remove(existing);
        result.Speeches.Insert(0, existing);
    }

    private void Emit(AIOrchestrationResult result, string id, string name, string stance, int favDelta, int powDelta, string text)
    {
        result.Speeches.Add(new CourtSpeech
        {
            MinisterId = id,
            MinisterName = name,
            Stance = stance,
            SpeechText = text,
            ExpectedFavorabilityChange = favDelta,
            ExpectedPowerChange = powDelta
        });
        _spokenThisXun.Add(id);
    }

    // P2-7 完整版：按"派系对立 → 派系相同 → 派系中立"3 选 2 动态挑选朝会发言 NPC
    // 选 NPC 时同时校验：(1) FactionStance 矩阵有 Stance；(2) FactionSpeechBank 有专属台词桶
    // 桶未命中但选出来 → 用 GetDefault() 兜底（不跳过这名 NPC，保留朝会张力）
    private void SelectSpeakersForIntent(AIOrchestrationResult result, GameState state, string activeOfficerId, CourtIntent intent)
    {
        if (string.IsNullOrEmpty(activeOfficerId) || !state.Npcs.TryGetValue(activeOfficerId, out var active))
        {
            // 无主持人：退化为单 NPC 兜底（保留 P2-3 兜底行为）
            EmitFallback(result, state, activeOfficerId);
            return;
        }

        var pool = state.Npcs.Values
            .Where(n => n.IsActive && !n.IsHostile && n.InitialLocation == "洛阳朝堂"
                     && !_spokenThisXun.Contains(n.Id) && n.Id != activeOfficerId)
            .OrderByDescending(n => n.Power)
            .ToList();

        var opponents = FactionStance.GetOppositionFactions(active.Faction);
        var chosen = new HashSet<string>();

        // 第 1 名：派系对立
        var opp = pool.FirstOrDefault(n =>
            opponents.Contains(n.Faction) && FactionStance.GetStance(n.Faction, intent) != null);
        if (opp != null) TryAddSpeaker(result, state, opp, intent, chosen);

        // 第 2 名：派系相同（除主持人自身）
        if (chosen.Count < 2)
        {
            var ally = pool.FirstOrDefault(n =>
                n.Faction == active.Faction && n.Id != activeOfficerId
                && FactionStance.GetStance(n.Faction, intent) != null);
            if (ally != null) TryAddSpeaker(result, state, ally, intent, chosen);
        }

        // 第 2 名兜底：剩余池中 Power 最大且不是已选 / 不是主持人
        if (chosen.Count < 2)
        {
            var neutral = pool.FirstOrDefault(n =>
                !chosen.Contains(n.Id) && FactionStance.GetStance(n.Faction, intent) != null);
            if (neutral != null) TryAddSpeaker(result, state, neutral, intent, chosen);
        }
    }

    private void TryAddSpeaker(AIOrchestrationResult result, GameState state, NpcState npc, CourtIntent intent, HashSet<string> chosen)
    {
        var stance = FactionStance.GetStance(npc.Faction, intent);
        if (stance == null) return;  // 防御：上层已校验，这里再保一次

        var entry = FactionSpeechBank.TryGetSpeech(npc.Id, intent, state) ?? FactionSpeechBank.GetDefault();
        var text = FactionSpeechBank.ResolveTemplates(entry.Text, state);

        Emit(result, npc.Id, npc.Name, stance, entry.FavDelta, entry.PowDelta, text);
        chosen.Add(npc.Id);
    }

    private void EmitFallback(AIOrchestrationResult result, GameState state, string activeOfficerId)
    {
        var pool = state.Npcs.Values
            .Where(n => n.IsActive && n.InitialLocation == "洛阳朝堂" && !_spokenThisXun.Contains(n.Id))
            .OrderByDescending(n => n.Power)
            .ToList();

        string chosenId = !string.IsNullOrEmpty(activeOfficerId)
            && pool.Any(n => n.Id == activeOfficerId)
            ? activeOfficerId
            : (pool.FirstOrDefault()?.Id ?? string.Empty);

        if (!string.IsNullOrEmpty(chosenId) && state.Npcs.TryGetValue(chosenId, out var npc))
        {
            Emit(result, npc.Id, npc.Name, "AGREED", 1, 0, "臣等谨遵圣谕。");
        }
    }

    // P2-3 旬变追踪：每旬开始时清空"已发言 NPC 集合"，避免同一旬内同一 NPC 重复表态
    private int _lastXunKey = -1;
    private readonly HashSet<string> _spokenThisXun = new();
    private static int XunKeyOf(GameState s) => s.Year * 10000 + s.Month * 100 + s.Xun;
}

public class MockOracle : IEventOracle
{
    public Task<OracleEvent?> CheckRandomEventAsync(GameState state)
    {
        OracleEvent? evt = null;
        if (state.Chronicle.Count > 0 && state.Chronicle[state.Chronicle.Count - 1].Contains("天灾"))
        {
            evt = new OracleEvent
            {
                EventName = "地动山摇",
                Description = "洛阳突发地震，百姓流离失所，朝廷需开仓赈灾。",
                ImperialPowerChange = -5,
                TreasuryChange = -150,
                HealthChange = 0
            };
        }
        return Task.FromResult(evt);
    }
}

public class MockMinisterAgent : IMinisterAgent
{
    public Task<List<MinisterDialogue>> TalkToMinistersAsync(List<string> activeMinisters, string playerInput, GameState state)
    {
        var list = new List<MinisterDialogue>();
        foreach (var mId in activeMinisters)
        {
            if (mId == "he_jin")
            {
                list.Add(new MinisterDialogue
                {
                    MinisterId = "he_jin",
                    MinisterName = "何进",
                    DialogueText = "臣谢陛下隆恩！臣定当整军备战，保大汉无虞！",
                    FavorabilityChange = 15,
                    PowerChange = 5
                });
            }
            else if (mId == "zhang_rang")
            {
                list.Add(new MinisterDialogue
                {
                    MinisterId = "zhang_rang",
                    MinisterName = "张让",
                    DialogueText = "陛下如今薄情如此，奴才只盼着陛下龙体安康呐...",
                    FavorabilityChange = -15,
                    PowerChange = -3
                });
            }
        }
        return Task.FromResult(list);
    }
}

public class MockNarrator : INarrator
{
    public Task<string> RenderStoryAsync(string playerInput, OracleEvent? triggeredEvent, List<MinisterDialogue> ministerDialogues, GameState state)
    {
        string story = $"【圣旨朱批】：“[color=yellow]{playerInput}[/color]”\n\n";
        if (triggeredEvent != null)
        {
            story += $"[color=red]● 天降警示：{triggeredEvent.EventName}[/color]\n{triggeredEvent.Description}\n\n";
        }
        foreach (var dial in ministerDialogues)
        {
            story += $"[b]{dial.MinisterName}[/b]在殿前叩首，进言道: \"[i]{dial.DialogueText}[/i]\"\n\n";
        }
        story += "皇帝缓缓靠在龙椅上。朝堂波诡云谲，陛下今日的朱批将悄然重构这危如累卵的天下...";
        return Task.FromResult(story);
    }
}
