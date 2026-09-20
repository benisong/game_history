using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 194 年 5 月吕布袭兖州与中原大蝗灾因果（单一职责）
/// </summary>
public sealed class LvBuYanzhouEvaluator : ILvBuYanzhouEvaluator
{
    public LvBuYanzhouResult Evaluate(GameState state)
    {
        bool hasEnoughTreasuryForRelief = state.Treasury >= 1200;
        bool strongCourt = state.ImperialPower >= 50;

        // 1. 国库充裕且天子有为：发洛阳常平仓粮赈济灾民，持节制衡曹吕两虎
        if (hasEnoughTreasuryForRelief && strongCourt)
        {
            return new LvBuYanzhouResult(
                LvBuYanzhouOutcome.ReliefAndMediateYanzhou,
                "【开仓赈饥 · 皇恩浩荡】天子开洛阳太仓赈济中原大蝗！以粮羁縻曹吕二虎！",
                "【赈济】兴平元年夏，张邈、陈宫迎吕布袭兖州，适逢中原大蝗饥馑。天子发洛阳常平仓粮二十万石顺洛水东下赈济兖豫饥民，活人无数。降旨封吕布为平东将军、曹操为兖州牧，以粮为筹码令两虎罢战互制，民心大悦，皇权赫奕！",
                ImperialPowerDelta: 10,
                PopularSupportDelta: 15,
                TreasuryFoodCost: 1200,
                CaoCaoPowerDelta: -5,
                CaoCaoLoyaltyDelta: 20,
                LvBuPowerDelta: 25,
                LvBuLoyaltyDelta: 25);
        }

        // 2. 曹操好感较高或自立破贼（朝廷未全力赈灾，但未崩坏）
        if (state.Treasury >= 500)
        {
            return new LvBuYanzhouResult(
                LvBuYanzhouOutcome.CaoCaoRecoversYanzhou,
                "【中原苦战 · 曹操逐吕】大蝗食尽草木！曹操引精兵血战击走吕布！",
                "【苦战】中原大饥，万物凋敝。曹操引残部坚守甄城、范县，与吕布相持百日，终以奇兵击溃吕布，吕布引残骑东投徐州刘备。曹操虽复兖州，兵马折损过半。",
                ImperialPowerDelta: 2,
                PopularSupportDelta: -5,
                TreasuryFoodCost: 500,
                CaoCaoPowerDelta: 10,
                CaoCaoLoyaltyDelta: 10,
                LvBuPowerDelta: 15,
                LvBuLoyaltyDelta: 5);
        }

        // 3. 朝廷空虚无力赈济：中原大饥人相食，流民四起
        return new LvBuYanzhouResult(
            LvBuYanzhouOutcome.ZhongyuanFamineCollapse,
            "【蝗旱交迫 · 赤地千里】中原大饥人相食！流民四溃朝廷震动！",
            "【凶荒】蝗虫蔽天，中原草木食尽，白骨蔽野，人相食。朝廷府库空竭无法发赈，中原百姓流离失所，流寇蜂起，民心大损！",
            ImperialPowerDelta: -10,
            PopularSupportDelta: -20,
            TreasuryFoodCost: 0,
            CaoCaoPowerDelta: -15,
            CaoCaoLoyaltyDelta: -10,
            LvBuPowerDelta: 10,
            LvBuLoyaltyDelta: -10);
    }
}
