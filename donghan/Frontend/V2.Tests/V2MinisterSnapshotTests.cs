using Xunit;
using DonghanFrontend.V2.Adapters;

namespace DonghanFrontend.V2.Tests;

public sealed class V2MinisterSnapshotTests
{
    [Fact]
    public void State_reader_returns_read_only_minister_snapshots()
    {
        var runtime = V2RuntimeFactory.CreateDefault();

        var ministers = runtime.State.GetMinisters();

        Assert.NotEmpty(ministers);
        Assert.Contains(ministers, minister => minister.Name == "曹操");
        Assert.All(ministers, minister =>
        {
            Assert.False(string.IsNullOrWhiteSpace(minister.Faction));
            Assert.False(string.IsNullOrWhiteSpace(minister.Title));
        });
    }
}
