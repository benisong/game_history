using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 198 年 10 月下邳白门楼之役与吕布处决/收降因果（单一职责）
/// </summary>
public sealed class BaimenlouEvaluator : IBaimenlouEvaluator
{
    public BaimenlouResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool wantStrongArmy = (state.WestGardenArmy?.Size ?? 0) <= 8000;

        // 1. 天子皇权强盛 (>=50) 且禁军规模已饱和 (>8000)：明察吕布三度背主，密诏曹操决断处死，整肃纲纪
        if (strongCourt && !wantStrongArmy)
        {
            return new BaimenlouResult(
                BaimenlouOutcome.ExecuteLvBuConsolidateOrder,
                "【白门伏诛 · 纲纪肃然】天子密诏处死逆贼吕布！徐州底定！",
                "【整肃】建安三年冬，曹操、刘备决沂泗之水围下邳，生擒吕布于白门楼。天子闻奏，念及丁原、董卓前车之鉴，降密敕命曹操缢杀吕布于楼下。中原士民无不称快，曹操、刘备深服天子英断，上缴下邳战利金一千五百万，徐州全境肃清！",
                ImperialPowerDelta: 10,
                TreasuryGoldDelta: 1500,
                CaoCaoLoyaltyDelta: 20,
                LiuBeiLoyaltyDelta: 25,
                WestGardenTroopBonus: 0,
                WestGardenMoraleBonus: 10,
                LvBuExecuted: true);
        }

        // 2. 天子皇权稳固 (>=40) 且禁军求贤若渴 (<=8000)：特旨赦免死罪，拔擢为西园前锋禁军大将！
        if (state.ImperialPower >= 40)
        {
            return new BaimenlouResult(
                BaimenlouOutcome.PardonLvBuDraftToWestGarden,
                "【飞将归汉 · 特旨赦免】天子惜才赦免吕布死罪！收为西园前军先锋！",
                "【收降】白门楼下吕布乞降，天子念其弓马盖世、诛董卓有微功，特颁御札赦其死罪，诏命曹操护送吕布入洛阳，拜为西园前军校尉。吕布叩首洛阳殿阶，泣血誓死效忠，并州并骑两千人尽充西园禁军，禁军声威大震！",
                ImperialPowerDelta: 5,
                TreasuryGoldDelta: 1000,
                CaoCaoLoyaltyDelta: 5,
                LiuBeiLoyaltyDelta: -10,
                WestGardenTroopBonus: 2000,
                WestGardenMoraleBonus: 25,
                LvBuExecuted: false);
        }

        // 3. 皇权微弱 (<40)：号令迟缓，吕布突围逃往河北投奔袁绍
        return new BaimenlouResult(
            BaimenlouOutcome.LvBuFleesToHebei,
            "【飞将北奔 · 河北添翼】吕布突围北投袁本初！朝廷震恐！",
            "【遁走】朝廷号令滞塞，下邳城破之际，吕布率并州残骑奋力突围渡黄河，北投冀州袁绍。袁绍得吕布骁将，如虎添翼，河北军事威势更盛，朝廷深以为忧！",
            ImperialPowerDelta: -10,
            TreasuryGoldDelta: 0,
            CaoCaoLoyaltyDelta: -10,
            LiuBeiLoyaltyDelta: -5,
            WestGardenTroopBonus: 0,
            WestGardenMoraleBonus: -10,
            LvBuExecuted: false);
    }
}
