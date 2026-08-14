using DonghanEngine.Core;
using DonghanFrontend;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;
using Xunit;

namespace DonghanFrontend.V2.Tests;

public sealed class V2CourtAndEdictParityTests
{
    [Fact]
    public async Task Fixed_court_decision_v2_matches_direct_engine_state()
    {
        var legacyState = new GameState();
        var legacy = CreateEngine(legacyState);
        legacy.TravelToLocation("宣政殿");
        legacy.StartGrandCourtSync();
        legacy.ActiveOfficerId = "cao_cao";

        var v2 = V2RuntimeFactory.CreateDefault(new GameState());
        Assert.True(v2.Travel.Travel(new TravelCommand("宣政殿")).Success);
        await v2.Court.StartSessionAsync();

        var legacyResult = await legacy.ProcessPlayerTurnAsync("命曹操整西园军");
        var v2Result = await v2.Court.ExecuteDecisionAsync(
            new CourtDecisionCommand("military_readiness", "military_garden", "cao_cao"));

        Assert.True(v2Result.Success);
        Assert.Equal(legacyResult.StoryText, v2Result.StoryText);
        Assert.Equal(legacyState.Chronicle, v2.State.GetSnapshot().Chronicle);
    }

    [Fact]
    public async Task Free_edict_v2_matches_direct_engine_state()
    {
        var legacyState = new GameState();
        var legacy = CreateEngine(legacyState);
        legacy.TravelToLocation("宣政殿");
        legacy.ActiveOfficerId = "cao_cao";

        var v2 = V2RuntimeFactory.CreateDefault(new GameState());
        Assert.True(v2.Travel.Travel(new TravelCommand("宣政殿")).Success);

        var legacyResult = await legacy.ProcessPlayerTurnAsync("命曹操整饬西园军务");
        var v2Result = await v2.Court.ExecuteFreeEdictAsync(
            new FreeEdictCommand("命曹操整饬西园军务", "cao_cao"));

        Assert.True(v2Result.Success);
        Assert.Equal(legacyResult.StoryText, v2Result.StoryText);
        Assert.Equal(legacyState.Chronicle, v2.State.GetSnapshot().Chronicle);
    }

    [Fact]
    public async Task Edict_resolution_v2_matches_direct_engine_resources()
    {
        var legacyState = new GameState();
        var legacy = CreateEngine(legacyState);
        var v2 = V2RuntimeFactory.CreateDefault(new GameState());

        await legacy.NextXunAsync();
        await v2.Turns.AdvanceXunAsync();
        Assert.NotEmpty(legacyState.ActiveEdicts);
        Assert.NotEmpty(v2.Edicts.GetPendingEdicts());
        var legacyEdict = legacyState.ActiveEdicts.First();
        var v2Edict = v2.Edicts.GetPendingEdicts().First();

        legacy.ResolveEdictAction(legacyEdict.Id, 0);
        var v2Result = v2.Edicts.Resolve(new ResolveEdictCommand(v2Edict.Id, 0));

        Assert.True(v2Result.Success);
        var actual = v2.State.GetSnapshot();
        Assert.Equal(legacyState.Treasury, actual.Treasury);
        Assert.Equal(legacyState.PrivateTreasury, actual.PrivateTreasury);
        Assert.Equal(legacyState.PopularSupport, actual.PopularSupport);
        Assert.Equal(legacyState.Health, actual.Health);
        Assert.Equal(legacyState.ActiveEdicts.Count, v2.Edicts.GetPendingEdicts().Count);
    }

    private static GameEngine CreateEngine(GameState state) =>
        new(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());
}
