using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 207 年 8 月白狼山之战与辽东斩二袁（曹操北征乌桓，一统北方）因果（单一职责）
/// </summary>
public sealed class BailangMountainEvaluator : IBailangMountainEvaluator
{
    public BailangMountainResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool caoCaoLoyal = state.Npcs.TryGetValue("cao_cao", out var cc) && cc.IsActive && cc.Favorability >= 60;

        // 1. 天子皇权强盛 (>=50)：曹操奉诏北征白狼山斩蹋顿，辽东公孙康斩送二袁首级入洛阳称臣，天子收归幽并乌桓突骑入西园禁军
        if (strongCourt)
        {
            return new BailangMountainResult(
                BailangMountainOutcome.ImperialTriumphAndNorthernUnification,
                "【白狼斩虏 · 辽东纳首】曹操北征大破三郡乌桓！公孙康斩二袁首级入洛阳称臣！",
                "【北定】建安十二年秋八月，司空曹操引轻骑出卢龙塞，登白狼山与乌桓大战，先锋张辽阵斩乌桓单于蹋顿，胡汉降者二十余万。二袁奔逃辽东，太守公孙康惧天子声威与曹军大势，斩袁尚、袁熙首级星夜传送洛阳太庙进贡告捷！天子加封曹操殊勋，收辽东幽并降骑三千人充实西园禁军，国库受辽东朝贡三千万，北方四州彻底底定，社稷光复！",
                ImperialPowerDelta: 10,
                TreasuryGoldDelta: 3000,
                CaoCaoPowerDelta: 25,
                CaoCaoLoyaltyDelta: 20,
                WestGardenTroopBonus: 3000,
                WestGardenMoraleBonus: 20);
        }

        // 2. 曹操好感度高 (>=60) 且朝廷中平：曹操自建殊勋威震边陲，朝廷优诏赐封
        if (caoCaoLoyal)
        {
            return new BailangMountainResult(
                BailangMountainOutcome.CaoCaoIndependentConquest,
                "【北伐荡寇 · 威镇夷狄】曹操一战扫平塞北！北方全境归汉！",
                "【平塞】曹操北征乌桓大获全胜，辽东纳款，二袁伏诛。曹操上表洛阳献俘献马，北方黄河内外再无割据强藩。朝廷赐封曹操殊礼，天下诸侯莫敢不服。",
                ImperialPowerDelta: 5,
                TreasuryGoldDelta: 1500,
                CaoCaoPowerDelta: 35,
                CaoCaoLoyaltyDelta: 15,
                WestGardenTroopBonus: 1000,
                WestGardenMoraleBonus: 10);
        }

        // 3. 皇权微弱 (<35)：边塞风雪阻滞
        return new BailangMountainResult(
            BailangMountainOutcome.NorthernFrontierInstability,
            "【塞北风雪 · 边陲未宁】北伐大军受阻塞外！辽东袁氏犹存余孽！",
            "【阻滞】北征大军遭遇大雪泥泞，粮秣转运艰辛，虽击退胡骑，然辽东依然半独立自守，北方边患未除，朝廷深以为虑。",
            ImperialPowerDelta: -5,
            TreasuryGoldDelta: 0,
            CaoCaoPowerDelta: 10,
            CaoCaoLoyaltyDelta: -5,
            WestGardenTroopBonus: 0,
            WestGardenMoraleBonus: -10);
    }
}
