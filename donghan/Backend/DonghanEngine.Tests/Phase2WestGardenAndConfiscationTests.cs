using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Politics;

namespace DonghanEngine.Tests;

public class Phase2WestGardenAndConfiscationTests
{
    [Fact]
    public void Test_Confiscation_ToNationalTreasury_IncreasesTreasuryAndMorale()
    {
        var state = new GameState
        {
            Treasury = 1000,
            PrivateTreasury = 500,
            ImperialPower = 50,
            PopularSupport = 50
        };

        var corruptOfficial = new NpcState
        {
            Id = "corrupt_1",
            Name = "某贪官",
            Corruption = 80,
            Power = 60,
            StashedWealth = 3000,
            Faction = "外戚武臣"
        };
        state.RegisterNpc(corruptOfficial);

        var service = new ConfiscationService();
        var result = service.ConfiscateTarget(state, "corrupt_1", ConfiscationDestination.NationalTreasury);

        Assert.True(result.Success);
        Assert.Equal("某贪官", result.TargetName);
        Assert.True(result.GoldSeized > 3000); // 包含赃银与贪腐权势折算
        Assert.True(state.Treasury > 4000);    // 缴入国库太仓
        Assert.Equal(500, state.PrivateTreasury); // 私库不变
        Assert.True(state.ImperialPower > 50); // 皇权大涨
        Assert.True(state.PopularSupport > 50); // 民心大涨
        Assert.False(corruptOfficial.IsActive);
        Assert.Contains("收押", corruptOfficial.DeathReason);
    }

    [Fact]
    public void Test_Confiscation_ToPrivateTreasury_IncreasesPrivateTreasury_SlightlyHurtsMorale()
    {
        var state = new GameState
        {
            Treasury = 1000,
            PrivateTreasury = 500,
            ImperialPower = 50,
            PopularSupport = 50
        };

        var corruptOfficial = new NpcState
        {
            Id = "corrupt_2",
            Name = "某中官",
            Corruption = 90,
            Power = 70,
            StashedWealth = 4000,
            Faction = "中官近侍"
        };
        state.RegisterNpc(corruptOfficial);

        var service = new ConfiscationService();
        var result = service.ConfiscateTarget(state, "corrupt_2", ConfiscationDestination.PrivateTreasury);

        Assert.True(result.Success);
        Assert.Equal(1000, state.Treasury);       // 国库不变
        Assert.True(state.PrivateTreasury > 4500); // 缴入西园天子私库
        Assert.True(state.ImperialPower > 50);     // 皇权提升
        Assert.True(state.PopularSupport <= 48);   // 私吞赃银民心微跌
        Assert.False(corruptOfficial.IsActive);
    }
}
