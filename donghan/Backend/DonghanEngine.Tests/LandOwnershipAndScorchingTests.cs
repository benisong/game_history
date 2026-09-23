using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Economy;

namespace DonghanEngine.Tests;

public class LandOwnershipAndScorchingTests
{
    private readonly LandOwnershipService _landService = new();
    private readonly AgriculturalCarryingEngine _carryingEngine = new();

    [Fact]
    public void Test_WarLandScorching_ImperialDirectArmy_TurnsGentryLandIntoStateLandAndScorched()
    {
        var state = new GameState();
        var yuzhou = state.Provinces["yuzhou"];
        yuzhou.StateControlledLand = 40000;
        yuzhou.GentryControlledLand = 60000;
        yuzhou.LandCarryingCapacity = 100000;

        // 1. 天子亲军平定豫州战乱
        var result = _landService.ResolveWarLandScorching(state, "yuzhou", victorFactionId: "court", isImperialDirectArmy: true);

        Assert.Equal(21000, result.NewlyScorchedLand); // 60000 * 35% = 21000
        Assert.Equal(21000, result.StateLandGained);    // 天子亲军平叛，无主地全数收归国家官田
        Assert.Equal(39000, yuzhou.GentryControlledLand); // 60000 - 21000
        Assert.Equal(61000, yuzhou.StateControlledLand);  // 40000 + 21000
        Assert.Equal(21000, yuzhou.ScorchedLand);
        Assert.Equal(6, yuzhou.ScorchedMonthsRemaining); // 焦土休耕6个月

        // 2. 验证焦土期间粮食承载直接扣除焦土规模 (半年不产粮)
        var carryReport = _carryingEngine.EvaluateProvince(yuzhou, weatherSeverity: 0);
        Assert.Equal(79000, carryReport.LandCarryingCapacity); // 100000 - 21000 = 79000
    }

    [Fact]
    public void Test_AdvanceScorchedLandMonthly_RestoresFertilityAfter6Months()
    {
        var state = new GameState();
        var yuzhou = state.Provinces["yuzhou"];
        yuzhou.ScorchedLand = 20000;
        yuzhou.ScorchedMonthsRemaining = 2; // 剩余2个月

        _landService.AdvanceScorchedLandMonthly(state);
        Assert.Equal(1, yuzhou.ScorchedMonthsRemaining);
        Assert.Equal(20000, yuzhou.ScorchedLand);

        _landService.AdvanceScorchedLandMonthly(state);
        Assert.Equal(0, yuzhou.ScorchedMonthsRemaining);
        Assert.Equal(0, yuzhou.ScorchedLand); // 焦土期满，恢复正常耕作
        Assert.Contains(state.Chronicle, c => c.Contains("焦土休耕期满"));
    }

    [Fact]
    public void Test_RepurchaseStateLandByGentry_PaysGoldToTreasury_BoostsLoyalty()
    {
        var state = new GameState { Treasury = 1000 };
        var yuzhou = state.Provinces["yuzhou"];
        yuzhou.StateControlledLand = 50000;
        yuzhou.GentryControlledLand = 30000;

        var scholar = new NpcState { Id = "gentry_leader", Name = "世家宗主", Faction = "豪强派", Favorability = 60, IsActive = true };
        state.RegisterNpc(scholar);

        // 世家出资赎回 10000 顷官田 (支付 1000 万钱入国库)
        var result = _landService.RepurchaseStateLandByGentry(state, "yuzhou", purchaseAmount: 10000);

        Assert.True(result.Success);
        Assert.Equal(10000, result.LandPurchased);
        Assert.Equal(1000, result.GoldPaidToTreasury);
        Assert.Equal(2000, state.Treasury); // 1000 + 1000
        Assert.Equal(40000, yuzhou.StateControlledLand); // 50000 - 10000
        Assert.Equal(40000, yuzhou.GentryControlledLand); // 30000 + 10000
        Assert.Equal(75, scholar.Favorability); // 60 + 15
        Assert.Contains(state.Chronicle, c => (c.Contains("赎买") || c.Contains("赎回") || c.Contains("纳金赎田")) && c.Contains("豫州"));
    }
}
