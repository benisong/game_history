using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 192 年 4 月曹操入主兖州与青州兵归附因果（单一职责）
/// </summary>
public sealed class CaoCaoYanzhouEvaluator : ICaoCaoYanzhouEvaluator
{
    public CaoCaoYanzhouResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 55;
        bool caoCaoLoyal = state.Npcs.TryGetValue("cao_cao", out var cc) && cc.IsActive && cc.Favorability >= 60;

        // 1. 天子皇权强盛 (>=55)：天子恩威并施，准奏曹操领兖州牧，但密诏抽调青州兵精锐 3000 人充入西园新军！
        if (strongCourt)
        {
            return new CaoCaoYanzhouResult(
                CaoCaoYanzhouOutcome.RatifyAndDraftQingzhouTroops,
                "【兖州定鼎 · 恩威兼济】曹操破百万黄巾领兖州！天子抽调青州精锐实禁军！",
                "【平乱】初平三年夏，东郡太守曹操大破青州黄巾，受降三十万众，号“青州兵”，鲍信、陈宫等奉操领兖州牧。天子下诏嘉封曹操为兖州牧，敕令挑选青州健儿三千人充实洛阳西园禁军，曹操肃然奉诏，进纳谢恩金一千五百万，皇权大振！",
                ImperialPowerDelta: 8,
                TreasuryGoldDelta: 1500,
                CaoCaoPowerDelta: 25,
                CaoCaoLoyaltyDelta: 15,
                WestGardenArmyTroopBonus: 3000,
                WestGardenMoraleBonus: 15);
        }

        // 2. 曹操好感度高 (>=60) 且朝廷中平：顺水推舟完全恩抚，加官进爵换取巨额谢恩贡金
        if (caoCaoLoyal)
        {
            return new CaoCaoYanzhouResult(
                CaoCaoYanzhouOutcome.RatifyAndAppease,
                "【顺水推舟 · 抚平中原】天子明诏加封曹操兖州牧！曹孟德上表纳巨贡！",
                "【封赏】曹操平定兖州黄巾之患，上表报捷。天子敕封曹操为镇东将军、领兖州牧，曹操感激涕零，表奏谢恩，进奉粮饷两千万钱，中原稍安！",
                ImperialPowerDelta: 2,
                TreasuryGoldDelta: 2000,
                CaoCaoPowerDelta: 30,
                CaoCaoLoyaltyDelta: 25,
                WestGardenArmyTroopBonus: 0,
                WestGardenMoraleBonus: 5);
        }

        // 3. 皇权微弱且曹操疏远：派宗室监军制衡，防范曹操尾大不掉
        return new CaoCaoYanzhouResult(
            CaoCaoYanzhouOutcome.SendImperialInspector,
            "【中原分陕 · 强藩崛起】曹操据兖州拥精兵三十万！朝廷设法防范！",
            "【藩镇】兖州刺史刘岱战死，曹操尽并其地，精甲数十万，雄视关东。朝廷虽加追认，然其部曲唯曹氏之令是从，中原霸主初露峥嵘！",
            ImperialPowerDelta: -5,
            TreasuryGoldDelta: 500,
            CaoCaoPowerDelta: 40,
            CaoCaoLoyaltyDelta: -10,
            WestGardenArmyTroopBonus: 0,
            WestGardenMoraleBonus: 0);
    }
}
