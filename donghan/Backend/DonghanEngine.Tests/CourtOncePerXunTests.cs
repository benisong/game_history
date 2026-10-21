using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;

namespace DonghanEngine.Tests;

public class CourtOncePerXunTests
{
    [Fact]
    public async Task Test_CourtSessionLock_IsResetOnNextXun()
    {
        var state = new GameState();
        var scheduler = new MockScheduler();
        var oracle = new MockOracle();
        var ministerAgent = new MockMinisterAgent();
        var narrator = new MockNarrator();
        var engine = new GameEngine(state, scheduler, oracle, ministerAgent, narrator);

        // 初始状态：未退朝
        Assert.False(state.IsCourtConcludedThisXun);

        // 模拟退朝
        state.IsCourtConcludedThisXun = true;
        Assert.True(state.IsCourtConcludedThisXun);

        // 推进一旬
        await engine.NextXunAsync();

        // 验证：新的一旬已重置为可开朝状态
        Assert.False(state.IsCourtConcludedThisXun);
    }
}
