using System;
using DonghanEngine.Core.Geopolitics.Models;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 191 年 4 月袁绍兼并冀州因果结算（单一职责）
/// </summary>
public sealed class YuanShaoJizhouExecutor : IYuanShaoJizhouExecutor
{
    public TurnResult Execute(GameState state, YuanShaoJizhouResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 袁绍权势与好感结算
        if (state.Npcs.TryGetValue("yuan_shao", out var yuanShao))
        {
            yuanShao.AdjustPower(result.YuanShaoPowerDelta);
            yuanShao.AdjustFavorability(result.YuanShaoLoyaltyDelta);
        }

        // 3. 冀州领地归属与守军状态同步
        if (state.Provinces.TryGetValue("jizhou", out var jizhou))
        {
            jizhou.GovernorId = "yuan_shao";
            jizhou.AdjustGarrison(4000); // 接收韩馥部曲
        }

        // 4. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
