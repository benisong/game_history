using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 189 年 8 月中平政变因果结算（单一职责）
/// </summary>
public sealed class AugustCrisisExecutor : IAugustCrisisExecutor
{
    public TurnResult Execute(GameState state, AugustCrisisResult result)
    {
        var turnResult = new TurnResult();

        // 1. 若为天子坐镇免乱
        if (result.Outcome == AugustCrisisOutcome.ImperialSuppressed)
        {
            state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);
            state.PopularSupport = Math.Clamp(state.PopularSupport + result.PopularSupportDelta, 0, 100);
            if (state.WestGardenArmy != null)
            {
                state.WestGardenArmy.AdjustMorale(10);
                state.WestGardenArmy.AdjustLoyalty(10);
            }
        }
        else if (result.Outcome == AugustCrisisOutcome.HistoricalBloodshed)
        {
            // 史实路线：何进死亡
            if (state.Npcs.TryGetValue("he_jin", out var heJin))
            {
                heJin.IsActive = false;
                heJin.DeathReason = "【何进之死】中平六年八月，何进谋诛阉宦，反被张让等矫诏伏诛于嘉德殿前。";
                heJin.Power = 0;
            }

            state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);
            state.PopularSupport = Math.Clamp(state.PopularSupport + result.PopularSupportDelta, 0, 100);
            if (state.WestGardenArmy != null)
            {
                state.WestGardenArmy.AdjustMorale(-20);
                state.WestGardenArmy.AdjustLoyalty(-20);
            }
        }

        // 2. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
