using DonghanEngine.Core;
using DonghanFrontend;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;
using Xunit;

namespace DonghanFrontend.V2.Tests;

public sealed class V2LegacyParityTests
{
    [Fact]
    public void Travel_v2_matches_direct_engine_state()
    {
        var legacyState = new GameState();
        var legacy = CreateEngine(legacyState);
        var v2 = V2RuntimeFactory.CreateDefault(new GameState());

        legacy.TravelToLocation("西园");
        var result = v2.Travel.Travel(new TravelCommand("西园"));

        Assert.True(result.Success);
        Assert.Equal(legacyState.CurrentLocation, v2.State.GetSnapshot().CurrentLocation);
    }

    [Fact]
    public void Recruitment_v2_matches_direct_engine_resources()
    {
        var legacyState = new GameState();
        var legacy = CreateEngine(legacyState);
        legacy.TravelToLocation("西园");
        var v2 = V2RuntimeFactory.CreateDefault(new GameState());
        v2.Travel.Travel(new TravelCommand("西园"));

        legacy.ExecuteRaiseWestGardenTroopsAction(1000);
        v2.WestGarden.RecruitArmy(new RecruitArmyCommand(1000));

        var actual = v2.State.GetSnapshot();
        Assert.Equal(legacyState.WestGardenArmy.Size, actual.WestGardenArmySize);
        Assert.Equal(legacyState.Treasury, actual.Treasury);
        Assert.Equal(legacyState.PopularSupport, actual.PopularSupport);
        Assert.Equal(legacyState.WestGardenArmy.Morale, actual.WestGardenMorale);
    }

    [Fact]
    public void Governor_assignment_v2_matches_direct_engine_state()
    {
        var legacyState = new GameState();
        var legacy = CreateEngine(legacyState);
        var v2 = V2RuntimeFactory.CreateDefault(new GameState());

        legacy.AssignGovernor("sili", "cao_cao");
        v2.Intel.ExecuteProvinceAction(
            new ProvinceActionCommand("sili", ProvinceActionKind.AssignGovernor, "cao_cao"));

        Assert.Equal(
            legacyState.Provinces["sili"].GovernorId,
            v2.State.GetProvince("sili")!.GovernorId);
        Assert.Equal(
            legacyState.Provinces["sili"].LocalSupport,
            v2.State.GetProvince("sili")!.LocalSupport);
    }

    [Fact]
    public async Task Turn_advance_v2_matches_direct_engine_time_and_events()
    {
        var legacyState = new GameState();
        var legacy = CreateEngine(legacyState);
        var v2 = V2RuntimeFactory.CreateDefault(new GameState());

        await legacy.NextXunAsync();
        var result = await v2.Turns.AdvanceXunAsync();
        var actual = v2.State.GetSnapshot();

        Assert.True(result.Success);
        Assert.Equal(legacyState.Year, actual.Year);
        Assert.Equal(legacyState.Month, actual.Month);
        Assert.Equal(legacyState.Xun, actual.Xun);
        Assert.Equal(legacyState.Outcome.ToString(), actual.Outcome);
        Assert.Equal(legacyState.Chronicle, actual.Chronicle);
    }

    private static GameEngine CreateEngine(GameState state) =>
        new(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());
}
