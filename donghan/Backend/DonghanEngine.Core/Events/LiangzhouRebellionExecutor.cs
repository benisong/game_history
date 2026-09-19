using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 184 年 11 月凉州叛乱状态、人物部署与起居注记录（单一职责）
/// </summary>
public sealed class LiangzhouRebellionExecutor : ILiangzhouRebellionExecutor
{
    public TurnResult Execute(GameState state, LiangzhouRebellionResult result)
    {
        var turnResult = new TurnResult();

        // 1. 凉州叛乱状态更新
        if (result.LiangzhouRebelling && state.Provinces.TryGetValue("liangzhou", out var liangzhou))
        {
            int multiplier = result.Outcome == LiangzhouRebellionOutcome.GarrisonSurrendered ? 3 : 2;
            liangzhou.StartRebellion("凉州羌胡与叛军", initialLocalSupport: 5, garrisonMultiplier: multiplier);
        }

        // 2. 边将按需登庸部署（董卓/马腾入局）
        if (result.DeployDongZhuo && !state.Npcs.ContainsKey("dong_zhuo"))
        {
            if (HistoricalNpcPresets.All.Find(n => n.Id == "dong_zhuo") is { } preset)
            {
                state.RegisterNpc(HistoricalNpcPresets.Clone(preset));
            }
        }

        if (result.DeployMaTeng && !state.Npcs.ContainsKey("ma_teng"))
        {
            if (HistoricalNpcPresets.All.Find(n => n.Id == "ma_teng") is { } preset)
            {
                state.RegisterNpc(HistoricalNpcPresets.Clone(preset));
            }
        }

        // 3. 皇权与民心变更
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);
        state.PopularSupport = Math.Clamp(state.PopularSupport + result.PopularSupportDelta, 0, 100);

        // 4. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
