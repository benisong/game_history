using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Tests;

public class V2NominationUiTests
{
    [Fact]
    public void Test_NominationUi_AppointAndRejectWorkflow()
    {
        var state = new GameState { ImperialPower = 50 };
        var runtime = V2RuntimeFactory.CreateDefault(state);

        var nominations = runtime.Nominations.GetPendingNominations();
        Assert.NotEmpty(nominations);

        var xunYu = nominations.FirstOrDefault(c => c.CandidateId == "xun_yu");
        Assert.NotNull(xunYu);

        // 1. 御批除官
        var appointResult = runtime.Nominations.Appoint(xunYu, "尚书令");
        Assert.True(appointResult.Success);
        Assert.Contains("荀彧", appointResult.Title);
        Assert.True(runtime.State.GetSnapshot().ImperialPower > 50);

        // 2. 驳回察举
        var xunChen = nominations.FirstOrDefault(c => c.CandidateId == "xun_chen");
        Assert.NotNull(xunChen);

        var rejectResult = runtime.Nominations.Reject(xunChen);
        Assert.True(rejectResult.Success);
        Assert.Contains("弃才", rejectResult.Title);
    }
}
