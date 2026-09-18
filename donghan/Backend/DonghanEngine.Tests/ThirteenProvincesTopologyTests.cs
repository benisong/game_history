using System.Collections.Generic;
using System.Linq;
using Xunit;
using DonghanEngine.Core;

namespace DonghanEngine.Tests;

public class ThirteenProvincesTopologyTests
{
    [Fact]
    public void Test_AllThirteenProvinces_ExistWithCorrectMetadata()
    {
        var state = new GameState();
        Assert.Equal(13, state.Provinces.Count);
        
        foreach (var id in ProvinceCatalog.AllThirteenProvinceIds)
        {
            Assert.True(state.Provinces.ContainsKey(id), $"缺少州郡: {id}");
            var p = state.Provinces[id];
            Assert.False(string.IsNullOrWhiteSpace(p.Name));
            Assert.True(p.Distance >= 0);
            Assert.True(p.LocalSupport >= 0 && p.LocalSupport <= 100);
            Assert.True(p.Garrison > 0);
            Assert.True(p.Wealth > 0);
            Assert.True(p.DefenseLevel >= 0 && p.DefenseLevel <= 100);
            Assert.NotEmpty(p.Neighbors);
        }
    }

    [Fact]
    public void Test_ProvinceNeighbors_AreBidirectional()
    {
        var state = new GameState();
        
        foreach (var (id, province) in state.Provinces)
        {
            foreach (var neighborId in province.Neighbors)
            {
                Assert.True(state.Provinces.ContainsKey(neighborId), $"{id} 的邻居 {neighborId} 不存在");
                var neighbor = state.Provinces[neighborId];
                Assert.Contains(id, neighbor.Neighbors);
            }
        }
    }

    [Fact]
    public void Test_RebellionAndPacification_WorksAcrossAllProvinces()
    {
        var state = new GameState();
        
        foreach (var id in ProvinceCatalog.AllThirteenProvinceIds)
        {
            var p = state.Provinces[id];
            p.StartRebellion("地方义军", initialLocalSupport: 8, garrisonMultiplier: 2);
            Assert.True(p.IsRebelling);
            Assert.Equal("地方义军", p.RebelFaction);
            Assert.Equal(8, p.LocalSupport);

            p.PacifyRebellion(supportRecovery: 25);
            Assert.False(p.IsRebelling);
            Assert.Empty(p.RebelFaction);
            Assert.Equal(33, p.LocalSupport);
        }
    }
}
