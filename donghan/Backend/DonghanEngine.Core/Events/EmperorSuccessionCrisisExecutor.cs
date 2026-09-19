using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 189 年 4 月灵帝大病与鸩杀危机结算（单一职责）
/// </summary>
public sealed class EmperorSuccessionCrisisExecutor : IEmperorSuccessionCrisisExecutor
{
    public TurnResult Execute(GameState state, EmperorSuccessionCrisisResult result)
    {
        var turnResult = new TurnResult();

        // 1. 天子健康与皇权结算
        state.Health = Math.Clamp(state.Health + result.HealthDelta, 0, 100);
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 行凶者权势与罪责结算
        if (!string.IsNullOrEmpty(result.CulpritNpcId) && state.Npcs.TryGetValue(result.CulpritNpcId, out var culprit))
        {
            culprit.AdjustPower(result.CulpritPowerDelta);
            if (result.CulpritExposed)
            {
                culprit.AdjustFavorability(-50);
            }
        }

        // 3. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        // 4. 若驾崩，结局判定为崩殂
        if (state.Health <= 0)
        {
            state.Outcome = GameOutcome.Collapse;
            state.AddToChronicle("【国难】天子崩殂，大汉社稷倾覆！");
        }

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
