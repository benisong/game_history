using System.Linq;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Politics;

namespace DonghanEngine.Tests;

public class GovernorAppraisalAndRecallTests
{
    private readonly GovernorAppraisalService _appraisalService = new();

    [Fact]
    public void Test_EvaluateAnnualAppraisal_GeneratesGradesBasedOnSupportAndTax()
    {
        var state = new GameState();
        // 豫州太守卢植：民心 55，忠诚高，纳税正常
        var report = _appraisalService.EvaluateAnnualAppraisal(state);

        Assert.NotEmpty(report.Records);
        Assert.True(report.TotalTaxCollected > 0);

        var luZhiRecord = report.Records.FirstOrDefault(r => r.GovernorId == "lu_zhi");
        Assert.NotNull(luZhiRecord);
        Assert.Equal("豫州", luZhiRecord.ProvinceName);
    }

    [Fact]
    public void Test_PromoteGovernorToCourt_LoyalGovernor_AcceptsRecall()
    {
        var state = new GameState { ImperialPower = 55 };
        var governor = new NpcState
        {
            Id = "lu_zhi",
            Name = "卢植",
            GovernedProvinceId = "yuzhou",
            Favorability = 80,
            Ambition = 30,
            IsActive = true
        };
        state.RegisterNpc(governor);
        state.Provinces["yuzhou"].GovernorId = "lu_zhi";

        // 征拜卢植为九卿太常
        var result = _appraisalService.PromoteGovernorToCourt(state, "lu_zhi", "太常");

        Assert.True(result.Success);
        Assert.True(result.AcceptedRecall);
        Assert.Null(governor.GovernedProvinceId);
        Assert.Null(state.Provinces["yuzhou"].GovernorId);
        Assert.Equal("太常", governor.Title);
        Assert.Equal(58, state.ImperialPower); // 55 + 3
        Assert.Contains(state.Chronicle, c => c.Contains("顺服还朝") || c.Contains("还朝"));
    }

    [Fact]
    public void Test_PromoteGovernorToCourt_HighAmbitionGovernor_RefusesRecall()
    {
        var state = new GameState { ImperialPower = 40 };
        var warlord = new NpcState
        {
            Id = "rebel_warlord",
            Name = "割据军阀",
            GovernedProvinceId = "jizhou",
            Favorability = 35,
            Ambition = 95, // 野心极大
            IsActive = true
        };
        state.RegisterNpc(warlord);
        state.Provinces["jizhou"].GovernorId = "rebel_warlord";

        // 征拜割据太守入京
        var result = _appraisalService.PromoteGovernorToCourt(state, "rebel_warlord", "太常");

        Assert.False(result.Success);
        Assert.False(result.AcceptedRecall);
        Assert.Equal("jizhou", warlord.GovernedProvinceId); // 仍旧霸占冀州
        Assert.Equal("rebel_warlord", state.Provinces["jizhou"].GovernorId);
        Assert.Equal(37, state.ImperialPower); // 40 - 3 皇权受损
        Assert.Contains(state.Chronicle, c => c.Contains("拥兵自重") || c.Contains("抗拒内调"));
    }
}
