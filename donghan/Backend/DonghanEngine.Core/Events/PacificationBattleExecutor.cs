using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 184 年 10 月平定战役的数值增损、将领升迁/下狱与冀州平叛（单一职责）
/// </summary>
public sealed class PacificationBattleExecutor : IPacificationBattleExecutor
{
    public TurnResult Execute(GameState state, PacificationBattleResult battleResult)
    {
        var result = new TurnResult();

        // 1. 冀州平定处理
        if (battleResult.JizhouPacified && state.Provinces.TryGetValue("jizhou", out var jizhou))
        {
            if (jizhou.IsRebelling)
            {
                jizhou.SuppressRebellion(supportRecovery: 30, newGarrison: 3000);
            }
            else
            {
                jizhou.AdjustLocalSupport(15);
            }
        }

        // 2. 卢植蒙冤下狱处理
        if (battleResult.LuZhiImprisoned && state.Npcs.TryGetValue("lu_zhi", out var luZhi))
        {
            luZhi.AdjustPower(-15);
            luZhi.AdjustFavorability(-10);
            luZhi.RevokeGovernor();
        }
        else if (battleResult.Outcome == PacificationOutcome.LuZhiTriumph && state.Npcs.TryGetValue("lu_zhi", out var victoriousLuZhi))
        {
            victoriousLuZhi.AdjustPower(20);
            victoriousLuZhi.AdjustFavorability(15);
        }

        // 3. 皇甫嵩功勋处理
        if (battleResult.GeneralId == "huangfu_song" && state.Npcs.TryGetValue("huangfu_song", out var huangfu))
        {
            huangfu.AdjustPower(25);
            huangfu.AdjustFavorability(10);
        }

        // 4. 天子皇权与民心变更
        state.ImperialPower = Math.Clamp(state.ImperialPower + battleResult.ImperialPowerDelta, 0, 100);
        state.PopularSupport = Math.Clamp(state.PopularSupport + battleResult.PopularSupportDelta, 0, 100);

        // 5. 记入起居注
        state.AddToChronicle(battleResult.ChronicleText);

        result.StoryText = $"{battleResult.NarrativeTitle}\n\n{battleResult.ChronicleText}";
        return result;
    }
}
