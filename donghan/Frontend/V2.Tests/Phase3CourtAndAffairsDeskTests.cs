using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Politics;
using DonghanFrontend.V2.Contracts;
using DonghanFrontend.V2.Adapters;

namespace DonghanFrontend.V2.Tests;

public class Phase3CourtAndAffairsDeskTests
{
    [Fact]
    public void Test_CourtDelegation_BlocksNextTurn_WhenAffairsPending()
    {
        var state = new GameState();
        var runtime = V2RuntimeFactory.CreateDefault(state, new System.Random(42));

        // 1. 获取当前待办廷议列表
        var affairs = runtime.Delegation.GetPendingAffairs();
        Assert.NotEmpty(affairs);

        // 2. 亲裁其中一项政务
        var first = affairs[0];
        var directRes = runtime.Delegation.ExecuteDirectAffair(first.AffairId);
        Assert.True(directRes.Success);

        // 3. 一键分发剩余所有廷议给三司/大将军/内侍
        var batchRes = runtime.Delegation.ExecuteBatchDelegation();
        Assert.True(batchRes.Success);

        // 4. 再次获取，案头已清空
        var remaining = runtime.Delegation.GetPendingAffairs();
        Assert.Empty(remaining);
    }
}
