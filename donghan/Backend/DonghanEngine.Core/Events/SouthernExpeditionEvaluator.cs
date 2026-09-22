using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 225 年 3 月诸葛亮南征与七擒孟获（南中底定，滇马贡金络绎入洛）因果（单一职责）
/// </summary>
public sealed class SouthernExpeditionEvaluator : ISouthernExpeditionEvaluator
{
    public SouthernExpeditionResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool moderateCourt = state.ImperialPower >= 35;

        // 1. 天子皇权强盛 (>=50)：诸葛亮七擒七纵孟获平定南中，孟获归顺朝廷，南中出产滇马三千匹、金银丹漆入洛阳，充实西园禁军骑兵
        if (strongCourt)
        {
            return new SouthernExpeditionResult(
                SouthernExpeditionOutcome.ImperialTriumphAndNanzhongTribute,
                "【深入不毛 · 七擒孟获】诸葛孔明荡平南中诸夷！滇马贡金络绎传送洛阳太仓！",
                "【南平】建安三十年春三月，蜀相诸葛亮亲提大军五月渡泸、深入不毛，攻心为上，七擒七纵南蛮王孟获。孟获由衷感服，南中诸夷誓死永奉大汉正朔。诸葛亮将南中出产之金银、丹漆、耕牛、滇马三千匹与贡金三千万钱，尽数表奏转运洛阳太庙太仓！天子优诏赐封孟获为朝廷南中都督、加封诸葛亮殊荣，西园禁军添滇南良骑，皇权大振！",
                ImperialPowerDelta: 15,
                TreasuryGoldDelta: 3000,
                ZhugeLiangPowerDelta: 25,
                ZhugeLiangLoyaltyDelta: 30,
                WestGardenTroopBonus: 3000,
                WestGardenMoraleBonus: 20);
        }

        // 2. 朝廷中平 (35-49)：诸葛亮平定南中积蓄国力，上表洛阳进贡一千五百万
        if (moderateCourt)
        {
            return new SouthernExpeditionResult(
                SouthernExpeditionOutcome.ZhugeLiangAutonomousPacification,
                "【七擒七纵 · 蛮夷宾服】诸葛亮平定南中！上表洛阳进献方物！",
                "【服远】诸葛亮南征大胜，南中夷汉粗安。诸葛亮遣使入洛阳进纳贡赋一千五百万钱以表大汉正朔，西蜀后方彻底稳固，朝廷嘉勉其功。",
                ImperialPowerDelta: 5,
                TreasuryGoldDelta: 1500,
                ZhugeLiangPowerDelta: 30,
                ZhugeLiangLoyaltyDelta: 15,
                WestGardenTroopBonus: 1000,
                WestGardenMoraleBonus: 10);
        }

        // 3. 皇权微弱 (<35)：南中瘴疠阻滞
        return new SouthernExpeditionResult(
            SouthernExpeditionOutcome.NanzhongRebellionProtracted,
            "【瘴疠弥漫 · 西南苦战】南征大军受阻于烟瘴！南中诸蛮复叛！",
            "【受阻】南中地形险恶、水土恶劣，征南大军受挫，战事相持不下。朝廷诏命难行，西南边疆依然动荡，天子深以为忧！",
            ImperialPowerDelta: -5,
            TreasuryGoldDelta: 0,
            ZhugeLiangPowerDelta: 10,
            ZhugeLiangLoyaltyDelta: 0,
            WestGardenTroopBonus: 0,
            WestGardenMoraleBonus: -10);
    }
}
