using System.Threading.Tasks;
using DonghanEngine.Core;
using DonghanEngine.Core.Geopolitics.Contracts;
using Xunit;

namespace DonghanEngine.Tests.Geopolitics;

public class GameEngineGeopoliticalIntegrationTests
{
    [Fact]
    public async Task Test_GameEngine_AdvancesTo189_PopulatesGeopoliticalMemorials()
    {
        var state = new GameState { Year = 189, Month = 1, Xun = 1, Treasury = 5000 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync();

        // 189 年推进后，应当自动执行地缘推演并产出地缘奏折（如忠臣纳贡）
        Assert.True(state.PendingGeopoliticalMemorials.Count > 0, "189年推进后尚书台应接收到地缘奏折");
        Assert.Contains(state.PendingGeopoliticalMemorials, m => m.MemorialType == GeopoliticalMemorialType.TributeOffered || m.MemorialType == GeopoliticalMemorialType.PetitionRank);
    }

    [Fact]
    public async Task Test_GameEngine_ResolvesGeopoliticalMemorial_ExecutesEdictAndRemovesFromPending()
    {
        var state = new GameState { Year = 189, Month = 1, Xun = 1, Treasury = 5000 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync();

        var memorial = state.PendingGeopoliticalMemorials[0];
        string memorialId = memorial.MemorialId;
        string optionId = memorial.Options[0].OptionId;

        int initialTreasury = state.Treasury;
        var result = engine.ResolveGeopoliticalMemorial(memorialId, optionId);

        Assert.True(result.Success);
        Assert.DoesNotContain(state.PendingGeopoliticalMemorials, m => m.MemorialId == memorialId);
        Assert.True(state.Treasury >= initialTreasury);
    }
}
