using Godot;
using System;
using System.Collections.Generic;
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
        if (playerInput.Contains("赏赐何进") || playerInput.Contains("重赏何进"))
        {
            result.PrimaryIntent = "REWARD";
            result.Speeches.Add(new CourtSpeech
            {
                MinisterId = "he_jin",
                MinisterName = "何进",
                SpeechText = "臣谢陛下隆恩！臣定当整军备战，保大汉无虞！",
                ExpectedFavorabilityChange = 15,
                ExpectedPowerChange = 5
            });
            _spokenThisXun.Add("he_jin");
        }
        else if (playerInput.Contains("冷落张让") || playerInput.Contains("训诫张让"))
        {
            result.PrimaryIntent = "COLD";
            result.Speeches.Add(new CourtSpeech
            {
                MinisterId = "zhang_rang",
                MinisterName = "张让",
                SpeechText = "陛下如今薄情如此，奴才只盼着陛下龙体安康呐...",
                ExpectedFavorabilityChange = -15,
                ExpectedPowerChange = -3
            });
            _spokenThisXun.Add("zhang_rang");
        }
        else
        {
            // P2-3 兜底：玩家输入未匹配任何意图分支时，从殿中未发言 NPC 池选 1 名表态（避免朝会冷场）
            int xk = XunKeyOf(state);
            if (xk != _lastXunKey)
            {
                _spokenThisXun.Clear();
                _lastXunKey = xk;
            }

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
                result.Speeches.Add(new CourtSpeech
                {
                    MinisterId = npc.Id,
                    MinisterName = npc.Name,
                    SpeechText = "臣等谨遵圣谕。",
                    Stance = "AGREED",
                    ExpectedFavorabilityChange = 1,
                    ExpectedPowerChange = 0
                });
                _spokenThisXun.Add(npc.Id);
            }
        }
        return Task.FromResult(result);
    }

    public Task OrchestrateXunUpdateAsync(GameState state)
    {
        return Task.CompletedTask;
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
