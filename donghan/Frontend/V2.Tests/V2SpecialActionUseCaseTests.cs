using DonghanEngine.Core;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;
using Xunit;

namespace DonghanFrontend.V2.Tests;

public sealed class V2SpecialActionUseCaseTests
{
    [Fact]
    public void Sell_office_in_west_garden_updates_private_treasury_and_power()
    {
        var state = new GameState { CurrentLocation = "西园" };
        var beforePrivate = state.PrivateTreasury;
        var beforePower = state.ImperialPower;
        var runtime = V2RuntimeFactory.CreateDefault(state);

        var result = runtime.SpecialActions.Execute(new SpecialActionCommand("sell_office"));

        Assert.True(result.Success);
        Assert.Equal(beforePrivate + 1000, runtime.State.GetSnapshot().PrivateTreasury);
        Assert.Equal(beforePower - 3, runtime.State.GetSnapshot().ImperialPower);
    }

    [Fact]
    public void Harem_rest_in_harem_updates_health()
    {
        var state = new GameState { CurrentLocation = "后宫", Health = 50 };
        var runtime = V2RuntimeFactory.CreateDefault(state);

        var result = runtime.SpecialActions.Execute(new SpecialActionCommand("harem_rest"));

        Assert.True(result.Success);
        Assert.True(runtime.State.GetSnapshot().Health > 50);
    }

    [Fact]
    public void Disaster_relief_in_court_updates_treasury()
    {
        var state = new GameState { CurrentLocation = "宣政殿", Treasury = 5000 };
        var runtime = V2RuntimeFactory.CreateDefault(state);

        var result = runtime.SpecialActions.Execute(new SpecialActionCommand("disaster_relief", 2000, "cao_cao"));

        Assert.True(result.Success);
        Assert.Equal(3000, runtime.State.GetSnapshot().Treasury);
    }

    [Fact]
    public void Disaster_relief_without_funds_returns_failure()
    {
        var state = new GameState { CurrentLocation = "宣政殿", Treasury = 0 };
        var runtime = V2RuntimeFactory.CreateDefault(state);

        var result = runtime.SpecialActions.Execute(new SpecialActionCommand("disaster_relief", 2000, "cao_cao"));

        Assert.False(result.Success);
        Assert.Equal("InsufficientTreasury", result.ErrorCode);
    }

    [Fact]
    public void Confiscation_without_accuser_returns_failure_report()
    {
        var state = new GameState { CurrentLocation = "宣政殿" };
        foreach (var npc in state.Npcs.Values)
        {
            npc.Favorability = 0;
            npc.Corruption = 100;
        }
        var runtime = V2RuntimeFactory.CreateDefault(state);

        var result = runtime.SpecialActions.Execute(new SpecialActionCommand(
            "confiscation", TargetNpcId: "zhang_rang", Destination: "西园"));

        Assert.False(result.Success, result.StoryText);
    }

    [Fact]
    public void Special_action_in_wrong_location_returns_failure()
    {
        var runtime = V2RuntimeFactory.CreateDefault(new GameState { CurrentLocation = "宣政殿" });

        var result = runtime.SpecialActions.Execute(new SpecialActionCommand("sell_office"));

        Assert.False(result.Success);
        Assert.Equal(nameof(InvalidOperationException), result.ErrorCode);
    }
}
