using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Economy;

namespace DonghanEngine.Tests;

public class MacroEconomyAndSymbiosisTests
{
    private readonly AgriculturalCarryingEngine _carryingEngine = new();
    private readonly BanditWarlordSymbiosisEngine _symbiosisEngine = new();

    [Fact]
    public void Test_EvaluateProvince_StableAndAbundant_WhenPopulationWellUnderCapacity()
    {
        var province = new Province
        {
            Id = "yuzhou",
            Name = "豫州",
            Population = 80000,
            LandCarryingCapacity = 120000
        };

        var report = _carryingEngine.EvaluateProvince(province, weatherSeverity: 0, aristocracyLandRatio: 0.25);

        Assert.Equal(ProvinceCrisisLevel.StableAndAbundant, report.CrisisLevel);
        Assert.Equal(0, report.GrainDeficit);
        Assert.Equal(0, report.DisplacedRefugees);
        Assert.True(report.PublicMoraleDelta > 0);
        Assert.Contains("仓廪充实", report.Description);
    }

    [Fact]
    public void Test_EvaluateProvince_MalthusianCollapse_WhenOverpopulatedAndSevereWeather()
    {
        var province = new Province
        {
            Id = "qingzhou",
            Name = "青州",
            Population = 180000, // 远超承载
            LandCarryingCapacity = 100000
        };

        // 遭遇特大蝗旱灾 (weatherSeverity=35) 且豪强兼并严重 (aristocracyLandRatio=0.65)
        var report = _carryingEngine.EvaluateProvince(province, weatherSeverity: 35, aristocracyLandRatio: 0.65);

        Assert.Equal(ProvinceCrisisLevel.MalthusianCollapse, report.CrisisLevel);
        Assert.True(report.GrainDeficit > 5000, "严重超载与灾荒应出现巨大粮食缺口");
        Assert.True(report.DisplacedRefugees > 10000, "马尔萨斯崩溃应爆发数万流民");
        Assert.Equal(-20, report.PublicMoraleDelta);
        Assert.Contains("饥馑暴乱", report.Description);
    }

    [Fact]
    public void Test_BanditSpillover_WarlordAbsorbsRefugeesIntoEliteTroops()
    {
        var state = new GameState();
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", Power = 60, IsActive = true };
        state.RegisterNpc(caoCao);

        int displacedRefugees = 20000;
        var result = _symbiosisEngine.EvaluateBanditSpillover(state, "qingzhou", displacedRefugees);

        Assert.Equal("cao_cao", result.AbsorbingWarlordId);
        Assert.Equal("青州兵", result.EliteTroopTypeName);
        Assert.Equal(7000, result.TroopsAbsorbed); // 20000 * 35% = 7000
        Assert.True(result.WarlordPowerGained > 0);
        Assert.Equal(60 + result.WarlordPowerGained, caoCao.Power); // 曹操收编青州兵实力暴涨
        Assert.Contains(state.Chronicle, c => c.Contains("青州兵") && c.Contains("曹操"));
    }
}
