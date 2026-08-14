using DonghanEngine.Core;
using DonghanFrontend;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;
using Xunit;

namespace DonghanFrontend.V2.Tests;

public sealed class V2WestGardenFailureTests
{
    [Fact]
    public void Pay_army_with_insufficient_private_treasury_returns_failure()
    {
        var state = new GameState { CurrentLocation = "西园", PrivateTreasury = 0 };
        var runtime = V2RuntimeFactory.CreateDefault(state);

        var result = runtime.WestGarden.PayArmy(new ArmyPayCommand(100, "jian_shuo"));

        Assert.False(result.Success);
        Assert.Equal("InsufficientPrivateTreasury", result.ErrorCode);
    }

    [Fact]
    public void Recruit_army_with_insufficient_treasury_returns_failure()
    {
        var state = new GameState { CurrentLocation = "西园", Treasury = 0 };
        var runtime = V2RuntimeFactory.CreateDefault(state);

        var result = runtime.WestGarden.RecruitArmy(new RecruitArmyCommand(1000));

        Assert.False(result.Success);
        Assert.Equal("InsufficientTreasury", result.ErrorCode);
    }

    [Fact]
    public void Recruit_army_when_full_returns_failure()
    {
        var state = new GameState { CurrentLocation = "西园" };
        state.WestGardenArmy.Size = 12000;
        var runtime = V2RuntimeFactory.CreateDefault(state);

        var result = runtime.WestGarden.RecruitArmy(new RecruitArmyCommand(1000));

        Assert.False(result.Success);
        Assert.Equal("WestGardenArmyFull", result.ErrorCode);
    }

    [Fact]
    public void Recruit_army_with_non_batch_amount_returns_failure()
    {
        var state = new GameState { CurrentLocation = "西园" };
        var runtime = V2RuntimeFactory.CreateDefault(state);

        var result = runtime.WestGarden.RecruitArmy(new RecruitArmyCommand(500));

        Assert.False(result.Success);
        Assert.Equal(nameof(ArgumentException), result.ErrorCode);
    }
}
