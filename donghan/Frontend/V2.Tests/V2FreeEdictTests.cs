using Xunit;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Tests;

public sealed class V2FreeEdictTests
{
    [Fact]
    public async Task Free_edict_submits_input_through_court_service()
    {
        var runtime = V2RuntimeFactory.CreateDefault();
        Assert.True(runtime.Travel.Travel(new TravelCommand("宣政殿")).Success);
        await runtime.Court.StartSessionAsync();

        var result = await runtime.Court.ExecuteFreeEdictAsync(
            new FreeEdictCommand("命曹操整饬西园军务", "cao_cao"));

        Assert.True(result.Success);
        Assert.Equal(ReportKind.Court, result.Kind);
        Assert.NotEmpty(result.StoryText);
    }

    [Fact]
    public async Task Free_edict_rejects_empty_input_without_changing_state()
    {
        var runtime = V2RuntimeFactory.CreateDefault();
        var before = runtime.State.GetSnapshot();

        var result = await runtime.Court.ExecuteFreeEdictAsync(new FreeEdictCommand(" ", "cao_cao"));

        Assert.False(result.Success);
        Assert.Equal(before.Xun, runtime.State.GetSnapshot().Xun);
    }
}
