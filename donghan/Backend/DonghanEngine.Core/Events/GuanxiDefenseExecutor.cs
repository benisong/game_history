using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 190 年 6 月关西防务因果结算（单一职责）
/// </summary>
public sealed class GuanxiDefenseExecutor : IGuanxiDefenseExecutor
{
    public TurnResult Execute(GameState state, GuanxiDefenseResult result)
    {
        var turnResult = new TurnResult();

        // 1. 扣除犒军军费与皇权民心结算
        if (result.TreasuryCost > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury - result.TreasuryCost, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);
        state.PopularSupport = Math.Clamp(state.PopularSupport + result.PopularSupportDelta, 0, 100);

        // 2. 皇甫嵩与董卓权势好感结算
        if (state.Npcs.TryGetValue("huangfu_song", out var huangfu))
        {
            huangfu.AdjustPower(result.HuangfuSongPowerDelta);
            huangfu.AdjustFavorability(result.HuangfuSongLoyaltyDelta);
        }

        if (state.Npcs.TryGetValue("dong_zhuo", out var dong))
        {
            dong.AdjustPower(result.DongZhuoPowerDelta);
            dong.AdjustFavorability(result.DongZhuoFavorDelta);
        }

        // 3. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
