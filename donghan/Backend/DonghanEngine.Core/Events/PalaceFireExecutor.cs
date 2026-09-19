using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 185 年 2 月南宫大火后的数值结算、全州郡民心增损与起居注（单一职责）
/// </summary>
public sealed class PalaceFireExecutor : IPalaceFireExecutor
{
    public TurnResult Execute(GameState state, PalaceFireResult result)
    {
        var turnResult = new TurnResult();

        // 1. 天子数值结算
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);
        state.PopularSupport = Math.Clamp(state.PopularSupport + result.PopularSupportDelta, 0, 100);
        state.Treasury = Math.Clamp(state.Treasury + result.TreasuryDelta, 0, 999999);
        state.PrivateTreasury = Math.Clamp(state.PrivateTreasury + result.PrivateTreasuryDelta, 0, 999999);

        // 2. 张让好感变更
        if (state.Npcs.TryGetValue("zhang_rang", out var zr))
        {
            zr.AdjustFavorability(result.ZhangRangFavorDelta);
        }

        // 3. 全州郡民心影响
        if (result.AllProvinceSupportDelta != 0)
        {
            foreach (var province in state.Provinces.Values)
            {
                province.AdjustLocalSupport(result.AllProvinceSupportDelta);
            }
        }

        // 4. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
