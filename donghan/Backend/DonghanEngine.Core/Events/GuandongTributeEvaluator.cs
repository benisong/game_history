using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 190 年 1 月关东诸侯纳贡与朝堂大辩论因果（单一职责）
/// </summary>
public sealed class GuandongTributeEvaluator : IGuandongTributeEvaluator
{
    public GuandongTributeResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50 && (state.WestGardenArmy?.Size ?? 0) >= 5000;
        bool moderateCourt = state.ImperialPower >= 35;

        // 1. 朝廷强盛：关东诸侯竞相上表纳贡，争求天子敕命封赏
        if (strongCourt)
        {
            return new GuandongTributeResult(
                GuandongTributeOutcome.ImperialTributeProsperity,
                "【关东来朝 · 万国输贡】关东群雄遣使入洛，上表称臣进纳岁赋！",
                "【大典】初平元年正月，关东曹操、袁绍、孙坚等遣使入洛朝贺，贡金帛粮饷数千车。天子降旨优渥封赏，朝野称颂，国库大充！",
                ImperialPowerDelta: 10,
                TreasuryGoldDelta: 3000,
                CaoCaoLoyaltyDelta: 15,
                YuanShaoLoyaltyDelta: 10,
                SunJianLoyaltyDelta: 15,
                YuanShuLoyaltyDelta: 5);
        }

        // 2. 朝廷中平：多数诸侯纳贡，淮南袁术阳奉阴违截留赋税
        if (moderateCourt)
        {
            return new GuandongTributeResult(
                GuandongTributeOutcome.PartialTributeDissent,
                "【关东修贡 · 淮南违抗】曹操袁绍如期输贡，袁术截留淮南漕赋！",
                "【朝争】关东诸侯多遣使纳贡，唯后将军袁术托词兵荒截留江淮财税。朝堂大议，天子明抚曹袁以孤立淮南！",
                ImperialPowerDelta: 3,
                TreasuryGoldDelta: 1500,
                CaoCaoLoyaltyDelta: 10,
                YuanShaoLoyaltyDelta: 5,
                SunJianLoyaltyDelta: 5,
                YuanShuLoyaltyDelta: -20);
        }

        // 3. 朝廷微弱：关东诸侯集体截留赋税，以兵荒流寇为由断绝纳贡
        return new GuandongTributeResult(
            GuandongTributeOutcome.FactionalBoycott,
            "【群雄桀骜 · 贡道受阻】关东各路借口盗匪闭关自守，岁赋中断！",
            "【虚竭】关东群雄观望洛阳虚实，皆称兵疲饷匮断绝输贡，朝廷度支见窘，天子降旨严责！",
            ImperialPowerDelta: -10,
            TreasuryGoldDelta: 0,
            CaoCaoLoyaltyDelta: -5,
            YuanShaoLoyaltyDelta: -10,
            SunJianLoyaltyDelta: -5,
            YuanShuLoyaltyDelta: -25);
    }
}
