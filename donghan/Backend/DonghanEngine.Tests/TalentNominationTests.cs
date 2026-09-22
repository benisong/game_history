using System.Linq;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Politics;

namespace DonghanEngine.Tests;

public class TalentNominationTests
{
    [Fact]
    public void Test_GenerateAnnualNominations_ReturnsValidCandidatesFromFamilies()
    {
        var state = new GameState();
        var service = new TalentNominationService();

        var families = service.GetRegisteredFamilies();
        Assert.NotEmpty(families);
        Assert.Contains(families, f => f.FamilyId == "ru_nan_yuan");
        Assert.Contains(families, f => f.FamilyId == "ying_chuan_xun");

        var candidates = service.GenerateAnnualNominations(state);
        Assert.NotEmpty(candidates);
        Assert.Contains(candidates, c => c.CandidateId == "xun_yu");
        Assert.Contains(candidates, c => c.CandidateId == "yang_xiu");
    }

    [Fact]
    public void Test_AppointCandidate_RegistersNpc_BoostsImperialPowerAndFamilyLoyalty()
    {
        var state = new GameState { ImperialPower = 50 };
        var service = new TalentNominationService();

        var candidate = new NominationCandidate(
            CandidateId: "xun_yu",
            Name: "荀彧",
            SponsoringFamilyId: "ying_chuan_xun",
            NativeProvince: "yuzhou",
            Politics: 98,
            Intelligence: 96,
            Charisma: 94,
            Martial: 40,
            RecommendedOfficeTitle: "尚书令",
            CandidateBio: "王佐之才");

        var result = service.AppointCandidate(state, candidate, "尚书令");

        Assert.True(result.Appointed);
        Assert.True(state.Npcs.ContainsKey("xun_yu"), "荀彧应当被正式注册入朝野谱系");
        Assert.Equal("尚书令", state.Npcs["xun_yu"].Title);
        Assert.Equal(85, state.Npcs["xun_yu"].Favorability);
        Assert.Equal(53, state.ImperialPower); // 50 + 3
        Assert.Equal(15, result.SponsoringFamilyLoyaltyDelta);
        Assert.Contains(state.Chronicle, c => c.Contains("荀彧") && c.Contains("尚书令"));
    }

    [Fact]
    public void Test_RejectCandidate_ReducesFamilyLoyalty_AndCandidateDefectsToWarlord()
    {
        var state = new GameState { ImperialPower = 50 };
        var service = new TalentNominationService();

        var candidate = new NominationCandidate(
            CandidateId: "xun_chen",
            Name: "荀谌",
            SponsoringFamilyId: "ru_nan_yuan",
            NativeProvince: "yuzhou",
            Politics: 82,
            Intelligence: 85,
            Charisma: 78,
            Martial: 45,
            RecommendedOfficeTitle: "议郎",
            CandidateBio: "袁氏故吏");

        var result = service.RejectCandidate(state, candidate);

        Assert.False(result.Appointed);
        Assert.False(state.Npcs.ContainsKey("xun_chen"), "被驳回候选人不进入朝廷谱系");
        Assert.Equal(-20, result.SponsoringFamilyLoyaltyDelta);
        Assert.Equal("yuan_shao", result.DestinationFactionId); // 袁氏门生外逃投奔袁绍
        Assert.Contains(state.Chronicle, c => c.Contains("荀谌") && c.Contains("弃贤"));
    }
}
