using System.Threading.Tasks;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;
using Xunit;

namespace DonghanFrontend.V2.Tests;

public sealed class V2CourtNavigationTests
{
    [Fact]
    public async Task Talent_cao_decision_is_supported_by_court_service()
    {
        var runtime = V2RuntimeFactory.CreateDefault();

        var result = await runtime.Court.ExecuteDecisionAsync(
            new CourtDecisionCommand("talent", "talent_cao", "he_jin"));

        Assert.True(result.Success);
        Assert.NotEmpty(result.StoryText);
    }

    [Fact]
    public async Task Talent_jian_decision_is_supported_by_court_service()
    {
        var runtime = V2RuntimeFactory.CreateDefault();

        var result = await runtime.Court.ExecuteDecisionAsync(
            new CourtDecisionCommand("talent", "talent_jian", "jian_shuo"));

        Assert.True(result.Success);
        Assert.NotEmpty(result.StoryText);
    }
}
