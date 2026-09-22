using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Economy;

namespace DonghanEngine.Tests;

public class CadastralAndIrrigationTests
{
    private readonly CadastralAndIrrigationService _service = new();

    [Fact]
    public void Test_ExecuteCadastralSurvey_Standard_IncreasesCapacityAndReclaimsTax()
    {
        var state = new GameState { Treasury = 1000, PopularSupport = 50 };
        state.Provinces["yuzhou"].LandCarryingCapacity = 100000;

        var scholar = new NpcState { Id = "scholar", Name = "名士", Faction = "清流派", Favorability = 70, IsActive = true };
        state.RegisterNpc(scholar);

        var result = _service.ExecuteCadastralSurvey(state, "yuzhou", CadastralSurveyIntensity.Standard);

        Assert.True(result.Success);
        Assert.Equal(115000, state.Provinces["yuzhou"].LandCarryingCapacity); // 100000 + 15000
        Assert.Equal(1800, state.Treasury); // 1000 + 800
        Assert.Equal(58, state.PopularSupport); // 50 + 8
        Assert.Equal(55, scholar.Favorability); // 70 - 15 (世家豪族怨怼)
        Assert.Contains(state.Chronicle, c => c.Contains("度田") && c.Contains("豫州"));
    }

    [Fact]
    public void Test_ConstructIrrigation_Success_DeductsTreasury_PermanentlyBoostsCapacity()
    {
        var state = new GameState { Treasury = 5000, PopularSupport = 60 };
        state.Provinces["yizhou"].LandCarryingCapacity = 120000;

        var result = _service.ConstructIrrigation(state, "yizhou");

        Assert.True(result.Success);
        Assert.Equal("都江堰拓浚工程", result.ProjectName);
        Assert.Equal(4000, state.Treasury); // 5000 - 1000
        Assert.Equal(140000, state.Provinces["yizhou"].LandCarryingCapacity); // 120000 + 20000
        Assert.Equal(68, state.PopularSupport); // 60 + 8
        Assert.Contains(state.Chronicle, c => c.Contains("都江堰") && c.Contains("水利"));
    }

    [Fact]
    public void Test_ConstructIrrigation_InsufficientTreasury_Fails()
    {
        var state = new GameState { Treasury = 200 };
        var result = _service.ConstructIrrigation(state, "sili");

        Assert.False(result.Success);
        Assert.Equal(200, state.Treasury);
        Assert.Contains("现钱不足", result.NarrativeTitle);
    }
}
