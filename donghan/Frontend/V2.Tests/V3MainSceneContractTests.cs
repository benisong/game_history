using System.Linq;
using Xunit;
using DonghanEngine.Core;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Tests;

public class V3MainSceneContractTests
{
    [Fact]
    public void Test_V3Runtime_LoadsInitialState_AndSupportsSandTableInteraction()
    {
        var state = new GameState();
        var runtime = V2RuntimeFactory.CreateDefault(state);

        var snap = runtime.State.GetSnapshot();
        Assert.Equal("光和", snap.ReignTitle);
        Assert.Equal(184, snap.Year);

        // 验证 13 州沙盘读取
        var provinces = runtime.State.GetAllProvinces();
        Assert.Equal(13, provinces.Count);

        // 验证司隶官田占多
        var sili = provinces.FirstOrDefault(p => p.Id == "sili");
        Assert.NotNull(sili);
        Assert.Equal(80000, sili.StateControlledLand);

        // 验证沙盘快捷动作轮盘可直接调用领域服务
        var surveyRes = runtime.Agriculture.SurveyLand("yuzhou", DonghanEngine.Core.Economy.CadastralSurveyIntensity.Standard);
        Assert.NotNull(surveyRes);

        var irrRes = runtime.Agriculture.BuildIrrigation("yuzhou");
        Assert.NotNull(irrRes);

        // 验证健康精神状态与温德殿静养
        var diagnosis = runtime.Health.GetPhysicianDiagnosis();
        Assert.NotNull(diagnosis);
        Assert.Equal(DonghanEngine.Core.Health.ImperialMentalState.Radiant, diagnosis.MentalState);

        var restRes = runtime.Health.RestAtWendePalace();
        Assert.True(restRes.Success);

        // 验证廷议待办读取与一键交办 (仅扣 2 精力)
        var affairs = runtime.Delegation.GetPendingAffairs();
        Assert.NotEmpty(affairs);

        var batchRes = runtime.Delegation.ExecuteBatchDelegation();
        Assert.True(batchRes.Success);
    }
}
