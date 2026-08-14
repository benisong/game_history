using Xunit;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Tests;

public sealed class V2EdictServiceTests
{
    [Fact]
    public async Task Advance_then_read_and_resolve_edict_through_interface()
    {
        var runtime = V2RuntimeFactory.CreateDefault();
        await runtime.Turns.AdvanceXunAsync();

        var pending = runtime.Edicts.GetPendingEdicts();
        Assert.NotEmpty(pending);
        Assert.NotEmpty(pending[0].Options);

        var result = runtime.Edicts.Resolve(new ResolveEdictCommand(pending[0].Id, 0));

        Assert.True(result.Success);
        Assert.Equal(ReportKind.Information, result.Kind);
        Assert.NotEmpty(result.StoryText);
        Assert.DoesNotContain(pending[0].Id, runtime.Edicts.GetPendingEdicts().Select(edict => edict.Id));
    }

    [Fact]
    public void Invalid_edict_option_returns_failure_without_throwing()
    {
        var runtime = V2RuntimeFactory.CreateDefault();

        var result = runtime.Edicts.Resolve(new ResolveEdictCommand("missing-edict", 0));

        Assert.False(result.Success);
        Assert.Equal(ReportKind.Warning, result.Kind);
    }
}
