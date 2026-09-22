using System;
using DonghanEngine.Core;

namespace DonghanEngine.Core.Politics;

/// <summary>
/// 纯领域服务：执行常规军饷发放、内库额外犒赏与禁军哗变判定（单一职责）
/// </summary>
public sealed class MilitaryPayrollService : IMilitaryPayrollService
{
    public MilitaryPayrollResult ProcessPayroll(GameState state, bool grantExtraBonus = false)
    {
        int armySize = state.WestGardenArmy?.Size ?? 0;
        if (armySize <= 0)
        {
            return new MilitaryPayrollResult(
                MilitaryPayrollStatus.FullyPaid,
                GoldSpent: 0,
                MoraleDelta: 0,
                ImperialPowerDelta: 0,
                DeserterCount: 0,
                NarrativeTitle: "【军备如常】禁军尚无规模。",
                ChronicleText: "西园军营平稳。");
        }

        // 基础军饷开销：每 1000 兵每旬需 150 贯军饷
        int baseCost = (armySize * 150) / 1000;
        int bonusCost = grantExtraBonus ? (armySize * 250) / 1000 : 0;
        int totalRequired = baseCost + bonusCost;

        // 1. 资金充裕且选择额外犒赏三军 (内库重赏 -> 士气大涨 + 皇权控制力提升)
        if (state.Treasury >= totalRequired && grantExtraBonus)
        {
            state.Treasury -= totalRequired;
            state.WestGardenArmy!.AdjustMorale(15);
            state.WestGardenArmy.AdjustLoyalty(10);
            state.ImperialPower = Math.Clamp(state.ImperialPower + 6, 0, 100); // 兵为将帅死节，天子威令大振

            string title = "【天恩犒赏 · 禁军归心】天子大出内帑赏赐西园三军！将士山呼万岁！";
            string text = $"【犒赏】天子念三军宿卫辛劳，特拨内库重金{totalRequired}贯犒赏西园八校尉将士。营中椎牛飨士、酒肉充溢，将士无不感泣，愿为陛下效死！禁军士气飙升，天子军权牢不可破！";
            state.AddToChronicle(text);

            return new MilitaryPayrollResult(
                MilitaryPayrollStatus.GenerouslyRewarded,
                GoldSpent: totalRequired,
                MoraleDelta: 15,
                ImperialPowerDelta: 6,
                DeserterCount: 0,
                NarrativeTitle: title,
                ChronicleText: text);
        }

        // 2. 常规足额发饷
        if (state.Treasury >= baseCost)
        {
            state.Treasury -= baseCost;
            state.WestGardenArmy!.AdjustMorale(2); // 维持微升

            string title = "【军饷足额 · 军心如山】西园禁军按期支领月俸。";
            string text = $"【发饷】西园军营如期核发军饷{baseCost}贯，三军按律操练，宿卫森严，营中肃然。";

            return new MilitaryPayrollResult(
                MilitaryPayrollStatus.FullyPaid,
                GoldSpent: baseCost,
                MoraleDelta: 2,
                ImperialPowerDelta: 0,
                DeserterCount: 0,
                NarrativeTitle: title,
                ChronicleText: text);
        }

        // 3. 欠饷短缺 (国库不足)
        int actualPaid = state.Treasury;
        state.Treasury = 0;
        state.WestGardenArmy!.AdjustMorale(-20); // 士气暴跌
        int currentMorale = state.WestGardenArmy.Morale;

        // 4. 士气跌破 30：触发禁军哗变兵谏
        if (currentMorale < 30)
        {
            int deserters = Math.Max(1000, armySize / 4); // 逃兵/溃散四分之一
            state.WestGardenArmy.AdjustSize(-deserters);
            state.ImperialPower = Math.Clamp(state.ImperialPower - 15, 0, 100); // 皇权受损

            string title = "【禁军哗变 · 兵谏洛阳】欠饷断炊！西园将士鼓噪哗变！";
            string text = $"【兵变】西园军营数月欠饷，军中怨声载道。是夜，校尉将士鼓噪持刀出营，围逼平乐观抢夺府库，逃散者多达{deserters}人！天子大惊，严饬有司筹款安抚。京畿震动，皇权受创！";
            state.AddToChronicle(text);

            return new MilitaryPayrollResult(
                MilitaryPayrollStatus.MutinyTriggered,
                GoldSpent: actualPaid,
                MoraleDelta: -20,
                ImperialPowerDelta: -15,
                DeserterCount: deserters,
                NarrativeTitle: title,
                ChronicleText: text);
        }

        // 常规欠饷
        string shortTitle = "【军饷短缺 · 士气滑落】国库空虚致禁军军心浮动。";
        string shortText = $"【欠饷】国库金帛告罄，仅支发{actualPaid}贯，将士面有菜色，军营怨声渐起，士气下挫。";
        state.AddToChronicle(shortText);

        return new MilitaryPayrollResult(
            MilitaryPayrollStatus.UnpaidShortage,
            GoldSpent: actualPaid,
            MoraleDelta: -20,
            ImperialPowerDelta: -5,
            DeserterCount: 0,
            NarrativeTitle: shortTitle,
            ChronicleText: shortText);
    }
}
