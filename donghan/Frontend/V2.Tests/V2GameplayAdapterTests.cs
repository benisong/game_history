using Xunit;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Tests;

public sealed class V2GameplayAdapterTests
{
    [Fact]
    public void Travel_service_changes_location_and_returns_report()
    {
        var runtime = V2RuntimeFactory.CreateDefault();

        var result = runtime.Travel.Travel(new TravelCommand("西园"));

        Assert.True(result.Success);
        Assert.Equal("西园", runtime.State.GetSnapshot().CurrentLocation);
        Assert.Contains("西园", result.StoryText);
    }

    [Fact]
    public void WestGarden_service_recruit_updates_army_and_treasury()
    {
        var runtime = V2RuntimeFactory.CreateDefault();
        runtime.Travel.Travel(new TravelCommand("西园"));
        var before = runtime.State.GetSnapshot();

        var result = runtime.WestGarden.RecruitArmy(new RecruitArmyCommand(1000));
        var after = runtime.State.GetSnapshot();

        Assert.True(result.Success);
        Assert.Equal(before.WestGardenArmySize + 1000, after.WestGardenArmySize);
        Assert.True(after.Treasury < before.Treasury);
    }

    [Fact]
    public void Intel_service_assigns_governor_and_returns_changes()
    {
        var runtime = V2RuntimeFactory.CreateDefault();

        var result = runtime.Intel.ExecuteProvinceAction(
            new ProvinceActionCommand("sili", ProvinceActionKind.AssignGovernor, "cao_cao"));
        var province = runtime.Intel.InspectProvince(new InspectProvinceCommand("sili"));

        Assert.True(result.Success);
        Assert.True(province.Success);
        Assert.Equal("cao_cao", province.Province!.GovernorId);
        Assert.Contains("任命", result.StoryText);
    }

    [Fact]
    public async Task Court_service_returns_story_after_decision()
    {
        var runtime = V2RuntimeFactory.CreateDefault();
        var travel = runtime.Travel.Travel(new TravelCommand("宣政殿"));
        Assert.True(travel.Success);
        var opening = await runtime.Court.StartSessionAsync();

        var result = await runtime.Court.ExecuteDecisionAsync(
            new CourtDecisionCommand("military_readiness", "military_garden", "cao_cao"));

        Assert.NotEmpty(opening);
        Assert.True(result.Success);
        Assert.Equal(ReportKind.Court, result.Kind);
        Assert.NotEmpty(result.StoryText);
    }
}
