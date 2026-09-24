using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class CrossYearMultiEraCausalBranchTests
{
    [Fact]
    public void Test_YuanShuSurvives_LeadsTo_TwoYuansAllianceAtGuandu()
    {
        var state = new GameState { ImperialPower = 20 }; // 弱势朝廷未能消灭袁术
        var ys = new NpcState { Id = "yuan_shu", Name = "袁术", IsActive = true, Power = 60 };
        state.RegisterNpc(ys);

        var ysEvaluator = new YuanShuUsurpationEvaluator();
        var ysExecutor = new YuanShuUsurpationExecutor();

        var ysResult = ysEvaluator.Evaluate(state);
        Assert.Equal(YuanShuUsurpationOutcome.YuanShuHoldsHuainan, ysResult.Outcome);
        ysExecutor.Execute(state, ysResult);

        Assert.False(state.IsYuanShuEliminatedEarly, "袁术未能被彻底剿灭");

        // 200 年官渡前夕：二袁合流，15万大军压境
        var gdEvaluator = new GuanduPreludeEvaluator();
        var gdResult = gdEvaluator.Evaluate(state);

        Assert.Contains("二袁合流", gdResult.NarrativeTitle);
        Assert.Contains("十五万大军压境", gdResult.NarrativeTitle);
        Assert.Equal(-10, gdResult.ImperialPowerDelta);
    }

    [Fact]
    public void Test_LvBuDraftedToWestGarden_LeadsTo_LvBuDominatesTongguanBattle()
    {
        var state = new GameState { ImperialPower = 40 };
        var lvBu = new NpcState { Id = "lv_bu", Name = "吕布", IsActive = true };
        state.RegisterNpc(lvBu);

        var bmlEvaluator = new BaimenlouEvaluator();
        var bmlExecutor = new BaimenlouExecutor();

        // 198 年白门楼：朝廷特赦吕布，收编为西园大将
        var bmlResult = new BaimenlouResult(
            Outcome: BaimenlouOutcome.PardonLvBuDraftToWestGarden,
            NarrativeTitle: "【特敕赦免 · 猛将归心】",
            ChronicleText: "特赦吕布归西园。",
            ImperialPowerDelta: 5,
            TreasuryGoldDelta: 1000,
            CaoCaoLoyaltyDelta: -10,
            LiuBeiLoyaltyDelta: -15,
            WestGardenTroopBonus: 1000,
            WestGardenMoraleBonus: 15,
            LvBuExecuted: false);

        bmlExecutor.Execute(state, bmlResult);
        Assert.True(state.IsLvBuDraftedToImperialArmy, "吕布已收编入西园大军");

        // 211 年潼关之战：吕布作为朝廷大将亲征大破马超，长安三辅尽归天子
        var tgEvaluator = new TongguanBattleEvaluator();
        var tgResult = tgEvaluator.Evaluate(state);

        Assert.Contains("飞将战神威", tgResult.NarrativeTitle);
        Assert.Contains("禁军克潼关", tgResult.NarrativeTitle);
        Assert.Equal(4000, tgResult.TreasuryGoldDelta);
    }

    [Fact]
    public void Test_SunCeAssassinationAverted_LeadsTo_SunCeLeadingChibiBattle()
    {
        var state = new GameState { ImperialPower = 60 };
        var scEvaluator = new SunCeJiangdongEvaluator();
        var scExecutor = new SunCeJiangdongExecutor();

        // 196 年孙策平定江东：天子优抚确立大义，保全孙策性命
        var scResult = scEvaluator.Evaluate(state);
        Assert.Equal(SunCeJiangdongOutcome.RatifyAndCollectSaltIronTax, scResult.Outcome);
        scExecutor.Execute(state, scResult);

        Assert.True(state.IsSunCeAssassinationAverted, "孙策刺杀危机已化解，孙策存活");

        // 208 年赤壁之战：孙策亲征赤壁火攻大破曹操
        var cbEvaluator = new ChibiBattleEvaluator();
        var cbResult = cbEvaluator.Evaluate(state);

        Assert.Contains("小霸王亲征", cbResult.NarrativeTitle);
        Assert.Contains("赤壁烈焰", cbResult.NarrativeTitle);
        Assert.Equal(2500, cbResult.TreasuryGoldDelta);
    }
}
