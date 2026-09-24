using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域执行器：执行 198 年 10 月下邳白门楼因果结算（单一职责）
/// </summary>
public sealed class BaimenlouExecutor : IBaimenlouExecutor
{
    public TurnResult Execute(GameState state, BaimenlouResult result)
    {
        var turnResult = new TurnResult();

        // 1. 国库收入与皇权结算
        if (result.TreasuryGoldDelta > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + result.TreasuryGoldDelta, 0, 999999);
        }
        state.ImperialPower = Math.Clamp(state.ImperialPower + result.ImperialPowerDelta, 0, 100);

        // 2. 西园禁军兵员扩充与士气提升
        if (state.WestGardenArmy != null)
        {
            if (result.WestGardenTroopBonus > 0)
            {
                state.WestGardenArmy.AdjustSize(result.WestGardenTroopBonus);
            }
            state.WestGardenArmy.AdjustMorale(result.WestGardenMoraleBonus);
        }

        // 3. 曹操与刘备好感/忠诚结算
        if (state.Npcs.TryGetValue("cao_cao", out var caoCao))
            caoCao.AdjustFavorability(result.CaoCaoLoyaltyDelta);

        if (state.Npcs.TryGetValue("liu_bei", out var liuBei))
            liuBei.AdjustFavorability(result.LiuBeiLoyaltyDelta);

        // 4. 吕布生死与去向流转
        if (state.Npcs.TryGetValue("lv_bu", out var lvBu))
        {
            if (result.LvBuExecuted)
            {
                lvBu.IsActive = false;
                lvBu.DeathReason = "【白门楼受戮】建安三年冬，下邳城破，天子密敕缢杀于白门楼。";
                lvBu.Power = 0;
                state.IsLvBuDraftedToImperialArmy = false;
            }
            else if (result.Outcome == BaimenlouOutcome.PardonLvBuDraftToWestGarden)
            {
                lvBu.Title = "西园前军校尉";
                lvBu.InitialLocation = "洛阳西园";
                lvBu.Faction = "帝党派";
                lvBu.Favorability = 90; // 感天子救命之恩
                lvBu.Power = 60;
                state.IsLvBuDraftedToImperialArmy = true;
            }
            else if (result.Outcome == BaimenlouOutcome.LvBuFleesToHebei)
            {
                lvBu.InitialLocation = "冀州邺城";
                lvBu.Faction = "军阀派";
                lvBu.Favorability = 20;
                state.IsLvBuDraftedToImperialArmy = false;
            }
        }

        // 5. 记入起居注
        state.AddToChronicle(result.ChronicleText);

        turnResult.StoryText = $"{result.NarrativeTitle}\n\n{result.ChronicleText}";
        return turnResult;
    }
}
