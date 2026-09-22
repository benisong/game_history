using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Economy;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Tests;

public class V2AgriculturalPolicyUiTests
{
    [Fact]
    public void Test_AgricultureUi_SurveyLand_And_BuildIrrigation()
    {
        var state = new GameState { CurrentLocation = "宣政殿", Treasury = 5000, PopularSupport = 50 };
        state.Provinces["yuzhou"].LandCarryingCapacity = 100000;

        var runtime = V2RuntimeFactory.CreateDefault(state);

        // 1. 度田丈量
        var surveyResult = runtime.Agriculture.SurveyLand("yuzhou", CadastralSurveyIntensity.Standard);
        Assert.True(surveyResult.Success);
        Assert.Contains("度田", surveyResult.Title);
        Assert.Equal(115000, runtime.State.GetProvince("yuzhou")!.LandCarryingCapacity);
        Assert.True(runtime.State.GetSnapshot().Treasury > 5000); // 补缴税赋

        // 2. 兴修水利
        var irrigationResult = runtime.Agriculture.BuildIrrigation("yuzhou");
        Assert.True(irrigationResult.Success);
        Assert.Contains("水利", irrigationResult.Title);
        Assert.Equal(135000, runtime.State.GetProvince("yuzhou")!.LandCarryingCapacity); // 115000 + 20000
    }
}
