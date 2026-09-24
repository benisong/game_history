using System.Linq;
using Xunit;
using DonghanEngine.Core;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Tests;

public class V2GeopoliticsMapViewTests
{
    [Fact]
    public void Test_StateReader_GetAllProvincesAndFactions_ReturnsFullThirteenProvinces()
    {
        var state = new GameState();
        var runtime = V2RuntimeFactory.CreateDefault(state);

        var provinces = runtime.State.GetAllProvinces();
        Assert.Equal(13, provinces.Count);

        // 验证司隶数据
        var sili = provinces.FirstOrDefault(p => p.Id == "sili");
        Assert.NotNull(sili);
        Assert.Equal("司隶", sili.Name);
        Assert.Equal(80000, sili.StateControlledLand);
        Assert.Equal(40000, sili.GentryControlledLand);
        Assert.Equal("court", sili.ControllingFactionId);
        Assert.Equal("朝廷直辖", sili.ControllingFactionName);

        // 验证豫州数据
        var yuzhou = provinces.FirstOrDefault(p => p.Id == "yuzhou");
        Assert.NotNull(yuzhou);
        Assert.Equal("豫州", yuzhou.Name);
        Assert.Equal(25000, yuzhou.StateControlledLand);
        Assert.Equal(115000, yuzhou.GentryControlledLand);

        // 验证各诸侯势力快照
        var factions = runtime.State.GetAllFactions();
        Assert.NotEmpty(factions);
        Assert.Contains(factions, f => f.FactionId == "cao_cao");
        Assert.Contains(factions, f => f.FactionId == "yuan_shao");
    }
}
