using System.Linq;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Politics;
using DonghanEngine.Core.Settlement;

namespace DonghanEngine.Tests;

public class Phase1CleanupAndDomainContractsTests
{
    [Fact]
    public void Test_SpyService_DispatchesMissions_AndConsumesTreasury()
    {
        var state = new GameState
        {
            PrivateTreasury = 1000,
            Treasury = 5000
        };

        var heJin = new NpcState
        {
            Id = "he_jin",
            Name = "何进",
            Corruption = 60,
            Ambition = 75,
            Power = 80,
            StashedWealth = 2000,
            Faction = "外戚武臣"
        };
        state.RegisterNpc(heJin);

        var spyService = new SpyService();

        // 1. 验证目标列表（排除蹇硕自身）
        var targets = spyService.GetEligibleSpyTargets(state);
        Assert.Contains(targets, t => t.Id == "he_jin");

        // 2. 派遣特务刺探何进野心
        var result = spyService.DispatchSpyMission(state, SpyMissionType.InvestigateGovernorAmbition, "he_jin");
        Assert.True(result.Success);
        Assert.Equal("何进", result.TargetName);
        Assert.Equal(75, result.DiscoveredAmbition);
        Assert.Equal(700, state.PrivateTreasury); // 1000 - 300
        Assert.Contains("割据称霸之志", result.NarrativeReport);

        // 3. 派遣特务搜集贪腐罪证
        var corruptResult = spyService.DispatchSpyMission(state, SpyMissionType.GatherCorruptionEvidence, "he_jin");
        Assert.True(corruptResult.Success);
        Assert.True(corruptResult.EvidenceSecured);
        Assert.Equal(60, corruptResult.DiscoveredCorruption);
        Assert.Equal(400, state.PrivateTreasury); // 700 - 300
    }

    [Fact]
    public void Test_TurnSettlementSlideService_CompilesSlidesGracefully()
    {
        var state = new GameState
        {
            Year = 184,
            Month = 4,
            Xun = 1,
            Treasury = 12000,
            PrivateTreasury = 3500,
            ImperialPower = 65,
            PopularSupport = 70,
            CurrentLocation = "宣政殿"
        };
        state.WestGardenArmy.Size = 8000;
        state.WestGardenArmy.Morale = 75;

        var slideService = new TurnSettlementSlideService();
        var package = slideService.CompileTurnSettlementPackage(state);

        Assert.NotNull(package);
        Assert.Equal(184, package.Year);
        Assert.Equal(4, package.Month);
        Assert.Equal(1, package.Xun);
        Assert.Equal(2, package.Slides.Count); // 宏观起居卡 + 禁军军饷卡

        var genSlide = package.Slides[0];
        Assert.Equal(SettlementSlideKind.General, genSlide.Kind);
        Assert.Contains("太史令起居注", genSlide.Title);
        Assert.Contains("12000", genSlide.BodyText);

        var armySlide = package.Slides[1];
        Assert.Equal(SettlementSlideKind.MilitaryPayroll, armySlide.Kind);
        Assert.Contains("8000", armySlide.HeaderSubtitle);
    }
}
