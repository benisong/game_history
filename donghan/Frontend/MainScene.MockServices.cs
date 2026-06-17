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

        // P2-5：所有朝会发言 NPC 的 Stance 一律从 FactionStance 矩阵查；矩阵当前条目严格复制原硬编码语义
        // Frontend 仍保留"赏赐何进"与"训诫张让"两条特殊文案
        switch (cls.Intent)
        {
            case CourtIntent.Reward:
                EmitFaction(result, cls.Intent, "he_jin", "何进", FactionCatalog.ImperialClan, 15, 5,
                    "臣谢陛下隆恩！臣定当整军备战，保大汉无虞！");
                break;
            case CourtIntent.EunuchReform:
                EmitFaction(result, cls.Intent, "zhang_rang", "张让", FactionCatalog.EunuchFaction, -15, -3,
                    "陛下如今薄情如此，奴才只盼着陛下龙体安康呐...");
                break;
            case CourtIntent.Relief:
                EmitFaction(result, cls.Intent, "cao_cao", "曹操", FactionCatalog.PureStream, 5, 2,
                    "陛下圣明！赈灾乃安民之本，臣愿领旨督办！");
                EmitFaction(result, cls.Intent, "zhang_rang", "张让", FactionCatalog.EunuchFaction, -3, 0,
                    "陛下，国库仅 {Treasury} 万钱……不如由奴才来经办，定能省下不少银两。");
                break;
            case CourtIntent.Execute:
                EmitFaction(result, cls.Intent, "cao_cao", "曹操", FactionCatalog.PureStream, 10, 5,
                    "臣附议！乱臣贼子，人人得而诛之！");
                break;
            case CourtIntent.Treasury:
                var zhangTreasuryText = state.Treasury < 3000
                    ? "国库仅 {Treasury} 万钱，奴才愿为陛下查核诸库，纵节衣缩食亦必保军国无误。"
                    : "国库充盈（{Treasury} 万钱），奴才愿为陛下查核诸库，绝不令军国大计因钱粮误事。";
                EmitFaction(result, cls.Intent, "zhang_rang", "张让", FactionCatalog.EunuchFaction, 3, 2, zhangTreasuryText);
                EmitFaction(result, cls.Intent, "he_jin", "何进", FactionCatalog.ImperialClan, -2, 0,
                    "军费关乎社稷，不可尽付中官之手。");
                break;
            case CourtIntent.MilitaryBuild:
                EmitFaction(result, cls.Intent, "he_jin", "何进", FactionCatalog.ImperialClan, 3, 2,
                    "黄巾虽乱，朝廷威灵尚在。臣请整北军，明示天下。");
                EmitFaction(result, cls.Intent, "jian_shuo", "蹇硕", FactionCatalog.WesternGarden, 3, 2,
                    "西园诸校尉本为陛下亲军，愿为天子先驱。");
                break;
            case CourtIntent.Talent:
                EmitFaction(result, cls.Intent, "cao_cao", "曹操", FactionCatalog.PureStream, 5, 2,
                    "臣不敢自夸，愿以实绩报陛下知遇。");
                EmitFaction(result, cls.Intent, "jian_shuo", "蹇硕", FactionCatalog.WesternGarden, 3, 2,
                    "西园诸校尉皆陛下亲擢，正可分外廷之权。");
                break;
            case CourtIntent.Decline:
            case CourtIntent.Idle:
            case CourtIntent.Intel:
            case CourtIntent.Travel:
                // 不动朝会发言
                break;
            case CourtIntent.Unknown:
            default:
                // P2-3 兜底：从殿中未发言池按 Power 选 1 名表态
                EmitFallback(result, state, activeOfficerId);
                break;
        }

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

    // P2-5：按 NPC 派系 + 当前 Intent 查 FactionStance 矩阵查 stance；矩阵未命中则不发声
    private void EmitFaction(AIOrchestrationResult result, CourtIntent intent, string id, string name, string faction, int favDelta, int powDelta, string text)
    {
        var stance = FactionStance.GetStance(faction, intent);
        if (stance == null) return;  // 该派系对此 Intent 不主动表态
        Emit(result, id, name, stance, favDelta, powDelta, text);
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
