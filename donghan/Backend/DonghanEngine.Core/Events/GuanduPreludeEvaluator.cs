using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 200 年 2 月官渡之战前夕与天下诸侯表态（南北对峙，天子大义制衡）因果（单一职责）
/// </summary>
public sealed class GuanduPreludeEvaluator : IGuanduPreludeEvaluator
{
    public GuanduPreludeResult Evaluate(GameState state)
    {
        // 1. 若 197 年袁术未被彻底剿灭 (二袁合流)：袁绍得淮南呼应，兵力暴涨至 15 万压境官渡，曹操濒死绝境
        if (!state.IsYuanShuEliminatedEarly && state.Npcs.TryGetValue("yuan_shu", out var ys) && ys.IsActive)
        {
            return new GuanduPreludeResult(
                GuanduPreludeOutcome.HegemonicClashUnchecked,
                "【二袁合流 · 十五万大军压境】袁术残部北投袁绍！河北淮南合流围攻中原！",
                "【危局】因朝廷此前未能彻底剿灭淮南袁术，袁术率残部部曲与巨粮北投大将军袁绍，达成“二袁合流”！袁绍兵力激增至十五万大军，水陆并进直扑官渡！司空曹操军粮匮竭濒临绝境，中原震恐，洛阳京畿面临空前地缘威逼！",
                ImperialPowerDelta: -10,
                TreasuryGoldDelta: 0,
                CaoCaoLoyaltyDelta: 30, // 曹操孤立无援，极端渴求朝廷支持
                YuanShaoLoyaltyDelta: -30,
                WestGardenTroopBonus: 0,
                WestGardenMoraleBonus: -15);
        }

        bool strongCourt = state.ImperialPower >= 50;
        bool moderateCourt = state.ImperialPower >= 35;

        // 1. 天子皇权强盛 (>=50)：天子敕令南北各守疆界，袁曹争先输贡表忠以求天子正朔加持，国库大充
        if (strongCourt)
        {
            return new GuanduPreludeResult(
                GuanduPreludeOutcome.ImperialDualAppeasementAndTribute,
                "【官渡前夕 · 帝策居中】袁曹十万大军列阵黄河！天子下明诏敕两家各保疆界！",
                "【对峙】建安五年春，大将军袁绍治兵黎阳，步卒十万、骑万匹，欲南渡黄河；司空曹操勒兵官渡相拒。天子登洛阳北芒山阅视边防，下《诫分陕大臣罢斗诏》，严命两强各守封疆。袁绍遣使进奉河北良马两千匹，曹操上表输纳屯田精谷百万石以示恭顺。两强相持，国库收纳贡赋三千万钱，天子威名远播！",
                ImperialPowerDelta: 10,
                TreasuryGoldDelta: 3000,
                CaoCaoLoyaltyDelta: 20,
                YuanShaoLoyaltyDelta: 15,
                WestGardenTroopBonus: 2000,
                WestGardenMoraleBonus: 20);
        }

        // 2. 朝廷中平 (35-49)：天子密诏扶持曹操抗袁
        if (moderateCourt)
        {
            return new GuanduPreludeResult(
                GuanduPreludeOutcome.CaoCaoFavoredSecretSupport,
                "【密诏扶曹 · 孤立河北】天子密札授司空曹操！中原官渡布防！",
                "【密授】袁绍南下之势汹汹，天子密遣侍中赐曹操金斧白旄，许其便宜行事抵御河北强虏。曹操受命泣拜，誓死固守官渡，袁绍得悉朝廷偏向曹操，心怀怨望。",
                ImperialPowerDelta: 5,
                TreasuryGoldDelta: 1000,
                CaoCaoLoyaltyDelta: 25,
                YuanShaoLoyaltyDelta: -20,
                WestGardenTroopBonus: 1000,
                WestGardenMoraleBonus: 10);
        }

        // 3. 皇权微弱 (<35)：袁曹大战一触即发，朝廷号令难出京畿
        return new GuanduPreludeResult(
            GuanduPreludeOutcome.HegemonicClashUnchecked,
            "【烽火连天 · 黄河雷动】袁曹私战天下大乱！朝廷号令难行！",
            "【凶险】袁绍、曹操引大军隔黄河激战，中原士民震恐。朝廷号令皆成具文，天子深居禁中，唯有坐观龙虎相争！",
            ImperialPowerDelta: -10,
            TreasuryGoldDelta: 0,
            CaoCaoLoyaltyDelta: -10,
            YuanShaoLoyaltyDelta: -25,
            WestGardenTroopBonus: 0,
            WestGardenMoraleBonus: -10);
    }
}
