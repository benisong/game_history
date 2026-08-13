using Xunit;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Tests;

public sealed class V2TurnServiceTests
{
    [Fact]
    public async Task AdvanceXun_returns_new_chronicle_events()
    {
        var runtime = V2RuntimeFactory.CreateDefault();

        var result = await runtime.Turns.AdvanceXunAsync();

        Assert.True(result.Success);
        Assert.Equal(2, result.Snapshot.Xun);
        Assert.Contains(result.Events, e => e.Contains("时间更迭"));
    }

    [Fact]
    public async Task FastForward_rejects_zero_xun_without_mutating_state()
    {
        var runtime = V2RuntimeFactory.CreateDefault();
        var before = runtime.State.GetSnapshot();

        var result = await runtime.Turns.FastForwardAsync(new FastForwardCommand(0));

        Assert.False(result.Success);
        Assert.Equal(0, result.AdvancedXun);
        Assert.Equal(before.Xun, result.Snapshot.Xun);
        Assert.Equal(before.Month, result.Snapshot.Month);
    }
}
