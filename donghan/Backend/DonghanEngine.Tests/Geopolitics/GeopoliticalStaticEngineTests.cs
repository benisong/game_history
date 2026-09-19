using System.Collections.Generic;
using DonghanEngine.Core;
using DonghanEngine.Core.Geopolitics.Contracts;
using DonghanEngine.Core.Geopolitics.MathEngine;
using DonghanEngine.Core.Geopolitics.Models;
using DonghanEngine.Core.Geopolitics.Strategies;
using Xunit;

namespace DonghanEngine.Tests.Geopolitics;

public class GeopoliticalStaticEngineTests
{
    [Fact]
    public void Test_WarlordFaction_Encapsulation_ProtectsCollection()
    {
        var faction = new WarlordFaction(
            "cao_cao_force",
            "cao_cao",
            "曹操军团",
            WarlordPosture.AmbitiousWarlord,
            new[] { "yanzhou" },
            12000,
            6000,
            initialLoyalty: 65,
            initialAmbition: 92);

        Assert.Single(faction.ControlledProvinces);
        Assert.Contains("yanzhou", faction.ControlledProvinces);

        faction.AnnexProvince("xuzhou");
        Assert.Equal(2, faction.ControlledProvinces.Count);
        Assert.Contains("xuzhou", faction.ControlledProvinces);

        faction.CedeProvince("yanzhou");
        Assert.Single(faction.ControlledProvinces);
        Assert.DoesNotContain("yanzhou", faction.ControlledProvinces);
    }

    [Fact]
    public void Test_WarlordAggressionFormula_SuppressedWhenImperialPowerAndArmyHigh()
    {
        var faction = new WarlordFaction("yuan_shao", "yuan_shao", "袁绍", WarlordPosture.AmbitiousWarlord, new[] { "jizhou" }, 20000, 8000, 50, 90);

        // 天子皇权强盛 (70) 且 西园禁军过万 (10000)
        int probSuppressed = WarlordAggressionFormula.CalculateAttackProbability(
            faction,
            targetGarrison: 3000,
            hostilityWithTarget: 50,
            emperorImperialPower: 70,
            westGardenArmySize: 10000,
            hasTruce: false);

        // 天子皇权衰颓 (20) 且 禁军仅 2000
        int probWeakCourt = WarlordAggressionFormula.CalculateAttackProbability(
            faction,
            targetGarrison: 3000,
            hostilityWithTarget: 50,
            emperorImperialPower: 20,
            westGardenArmySize: 2000,
            hasTruce: false);

        Assert.True(probSuppressed < probWeakCourt, "天子势大时应显著压制枭雄进攻概率");
    }

    [Fact]
    public void Test_AmbitiousWarlordStrategy_LaunchesCampaign_WhenDiceHits()
    {
        var mockRand = new SeededRandomProvider();
        mockRand.EnqueuePercentage(10); // 骰子摇出 10（极低，必命中）

        var caoCao = new WarlordFaction("cao_cao", "cao_cao", "曹操", WarlordPosture.AmbitiousWarlord, new[] { "yanzhou" }, 15000, 8000, 60, 95);
        var state = new GameState(); // 包含标准十三州拓扑图
        var relations = new FactionRelationGraph();

        var context = new GeopoliticalContext(
            EmperorImperialPower: 40,
            WestGardenArmySize: 4000,
            WestGardenMorale: 60,
            AllFactions: new Dictionary<string, WarlordFaction> { ["cao_cao"] = caoCao },
            AllProvinces: state.Provinces,
            Relations: relations);

        var strategy = new AmbitiousWarlordStrategy(mockRand);
        var decision = strategy.EvaluateDecision(caoCao, context);

        Assert.Equal(WarlordActionType.LaunchCampaign, decision.ActionType);
        Assert.False(string.IsNullOrEmpty(decision.TargetProvinceId));
        Assert.True(decision.CommittedTroops > 0);
    }

    [Fact]
    public void Test_LoyalistBannerStrategy_SubmitsTribute_WhenProvisionsAbundant()
    {
        var loyalist = new WarlordFaction("huangfu_song", "huangfu_song", "皇甫嵩", WarlordPosture.LoyalistBanner, new[] { "bingzhou" }, 8000, 4000, initialLoyalty: 90, initialAmbition: 20);
        var state = new GameState();
        var context = new GeopoliticalContext(50, 5000, 60, new Dictionary<string, WarlordFaction>(), state.Provinces, new FactionRelationGraph());

        var strategy = new LoyalistBannerStrategy();
        var decision = strategy.EvaluateDecision(loyalist, context);

        Assert.Equal(WarlordActionType.Tribute, decision.ActionType);
        Assert.Equal(500, decision.TributeAmount);
    }

    [Fact]
    public void Test_CampaignStepResolver_ConquersProvince_WhenOverwhelmingForce()
    {
        var attacker = new WarlordFaction("yuan_shao", "yuan_shao", "袁绍", WarlordPosture.AmbitiousWarlord, new[] { "jizhou" }, 25000, 10000, 50, 85);
        var defender = new WarlordFaction("han_fu", "han_fu", "韩馥", WarlordPosture.CautiousAutonomist, new[] { "qingzhou" }, 3000, 2000, 50, 20);

        var targetProv = new Province { Id = "qingzhou", Name = "青州", Garrison = 1500, DefenseLevel = 20 };
        var campaign = new MilitaryCampaign("camp_01", "yuan_shao", "han_fu", "qingzhou", committedTroops: 18000, durationTurns: 2);

        var resolver = new CampaignStepResolver();
        var stepResult = resolver.ResolveStep(campaign, attacker, defender, targetProv);

        Assert.Equal(CampaignStatus.Victory, stepResult.Status);
        Assert.True(stepResult.ProvinceConquered);
        Assert.Contains("qingzhou", attacker.ControlledProvinces);
        Assert.DoesNotContain("qingzhou", defender.ControlledProvinces);
    }
}
