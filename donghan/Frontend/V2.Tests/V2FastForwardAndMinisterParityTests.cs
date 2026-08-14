using DonghanEngine.Core;
using DonghanFrontend;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;
using Xunit;

namespace DonghanFrontend.V2.Tests;

public sealed class V2FastForwardAndMinisterParityTests
{
    [Fact]
    public async Task Fast_forward_v2_matches_direct_engine_until_outcome()
    {
        var legacyState = new GameState();
        var legacy = CreateEngine(legacyState, new Random(18404));
        var v2 = V2RuntimeFactory.CreateDefault(new GameState(), new Random(18404));

        int legacyAdvanced = 0;
        for (; legacyAdvanced < 3; legacyAdvanced++)
        {
            await legacy.NextXunAsync();
            if (legacyState.Outcome != GameOutcome.Playing)
            {
                legacyAdvanced++;
                break;
            }
        }

        var result = await v2.Turns.FastForwardAsync(new FastForwardCommand(3));
        var actual = v2.State.GetSnapshot();

        Assert.True(result.Success);
        Assert.Equal(legacyAdvanced, result.AdvancedXun);
        Assert.Equal(legacyState.Year, actual.Year);
        Assert.Equal(legacyState.Month, actual.Month);
        Assert.Equal(legacyState.Xun, actual.Xun);
        Assert.Equal(legacyState.Outcome.ToString(), actual.Outcome);
        Assert.Equal(legacyState.Chronicle, actual.Chronicle);
    }

    [Fact]
    public void Minister_snapshots_match_legacy_npc_state()
    {
        var legacyState = new GameState();
        var legacy = CreateEngine(legacyState);
        var v2 = V2RuntimeFactory.CreateDefault(new GameState());
        var snapshots = v2.State.GetMinisters();

        Assert.Equal(legacyState.Npcs.Count, snapshots.Count);
        foreach (var snapshot in snapshots)
        {
            var npc = legacyState.Npcs[snapshot.Id];
            Assert.Equal(npc.Name, snapshot.Name);
            Assert.Equal(npc.Title, snapshot.Title);
            Assert.Equal(npc.Faction, snapshot.Faction);
            Assert.Equal(npc.Favorability, snapshot.Favorability);
            Assert.Equal(npc.Power, snapshot.Power);
            Assert.Equal(npc.Corruption, snapshot.Corruption);
            Assert.Equal(npc.IsActive, snapshot.IsActive);
            Assert.Equal(npc.IsHostile, snapshot.IsHostile);
        }
    }

    private static GameEngine CreateEngine(GameState state, Random? rng = null) =>
        new(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator(), rng);
}
