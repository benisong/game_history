using System.Linq;
using Xunit;
using DonghanEngine.Core;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Tests;

public class V2GovernorAppraisalUiTests
{
    [Fact]
    public void Test_AppraisalUi_GetAnnualReport_And_PromoteToCourt()
    {
        var state = new GameState { CurrentLocation = "宣政殿", ImperialPower = 60 };
        var governor = new NpcState
        {
            Id = "huangfu_song",
            Name = "皇甫嵩",
            GovernedProvinceId = "bingzhou",
            Favorability = 85,
            Ambition = 20,
            IsActive = true
        };
        state.RegisterNpc(governor);
        state.Provinces["bingzhou"].GovernorId = "huangfu_song";

        var runtime = V2RuntimeFactory.CreateDefault(state);

        // 1. 获取考课报告
        var report = runtime.Appraisal.GetAnnualAppraisal();
        Assert.NotEmpty(report.Records);

        // 2. 征拜皇甫嵩为执金吾内调回京
        var result = runtime.Appraisal.PromoteGovernorToCourt("huangfu_song", "执金吾");
        Assert.True(result.Success);
        Assert.Contains("顺服还朝", result.Title);
        Assert.Null(state.Npcs["huangfu_song"].GovernedProvinceId);
        Assert.Equal("执金吾", state.Npcs["huangfu_song"].Title);
    }
}
