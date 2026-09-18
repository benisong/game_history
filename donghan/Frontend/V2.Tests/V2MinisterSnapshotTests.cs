using System.Linq;
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

    [Fact]
    public void Special_action_confiscation_and_relief_from_minister_selection()
    {
        var runtime = V2RuntimeFactory.CreateDefault();

        var zhangRang = runtime.State.GetMinisters().FirstOrDefault(m => m.Name == "张让");
        Assert.NotNull(zhangRang);

        var confiscateResult = runtime.SpecialActions.Execute(new DonghanFrontend.V2.Contracts.SpecialActionCommand("confiscation", TargetNpcId: zhangRang.Id, Destination: "西园"));
        Assert.NotNull(confiscateResult);

        var caoCao = runtime.State.GetMinisters().FirstOrDefault(m => m.Name == "曹操");
        Assert.NotNull(caoCao);

        var reliefResult = runtime.SpecialActions.Execute(new DonghanFrontend.V2.Contracts.SpecialActionCommand("disaster_relief", 1000, caoCao.Id));
        Assert.NotNull(reliefResult);
        Assert.True(reliefResult.Success);
    }
}
