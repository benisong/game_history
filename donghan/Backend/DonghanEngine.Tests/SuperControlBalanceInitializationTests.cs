using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Economy;
using DonghanEngine.Core.Health;
using DonghanEngine.Core.Politics;

namespace DonghanEngine.Tests;

public class SuperControlBalanceInitializationTests
{
    [Fact]
    public void Test_HealthService_CanBeInitializedViaInitInterface()
    {
        var service = new ImperialHealthService();
        var customHealthConfig = new HealthBalanceConfig
        {
            MonthlyRecoveryRate = 0.60,
            YangRecoveryPerXun = 5
        };

        // 超级控制工具调用 InitializeConfig
        service.InitializeConfig(customHealthConfig);

        var state = new GameState
        {
            HiddenCurrentEnergy = 50,
            HiddenMaxEnergy = 100,
            HiddenYangVitality = 80
        };

        // 验证 60% 恢复生效: 50 * 0.60 = 30 -> 80
        service.AdvanceMonthlyEnergySettlement(state);
        Assert.Equal(80, state.HiddenCurrentEnergy);

        // 验证阳气自然恢复 5 点
        service.AdvanceXunHealthSettlement(state);
        Assert.Equal(85, state.HiddenYangVitality);
    }

    [Fact]
    public void Test_LandOwnershipService_CanBeInitializedViaInitInterface()
    {
        var service = new LandOwnershipService();
        var customLandConfig = new LandBalanceConfig
        {
            GentryTaxDiscountRatio = 0.80 // 修改世家私田征税比例为 80%
        };

        service.InitializeConfig(customLandConfig);
        Assert.Equal(0.80, service.GetConfig().GentryTaxDiscountRatio);
    }

    [Fact]
    public void Test_MilitaryPayrollService_CanBeInitializedViaInitInterface()
    {
        var service = new MilitaryPayrollService();
        var customPayrollConfig = new PayrollBalanceConfig
        {
            BaseCostPerThousandSoldiers = 200,
            MutinyMoraleThreshold = 20
        };

        service.InitializeConfig(customPayrollConfig);
        Assert.Equal(200, service.GetConfig().BaseCostPerThousandSoldiers);
        Assert.Equal(20, service.GetConfig().MutinyMoraleThreshold);
    }

    [Fact]
    public void Test_ConfiscationService_CanBeInitializedViaInitInterface()
    {
        var service = new ConfiscationService();
        var customConfiscationConfig = new ConfiscationBalanceConfig
        {
            ImperialPowerGainOnConfiscation = 15,
            SameFactionLoyaltyPenalty = -40
        };

        service.InitializeConfig(customConfiscationConfig);
        Assert.Equal(15, service.GetConfig().ImperialPowerGainOnConfiscation);
        Assert.Equal(-40, service.GetConfig().SameFactionLoyaltyPenalty);
    }

    [Fact]
    public void Test_OfficialRankService_CanBeInitializedViaInitInterface()
    {
        var service = new OfficialRankService();
        var customRankConfig = new OfficialRankBalanceConfig
        {
            MaxPromotionStepAllowance = 4 // 超级控制工具临时放宽至 4 级
        };

        service.InitializeConfig(customRankConfig);
        Assert.Equal(4, service.GetConfig().MaxPromotionStepAllowance);
    }
}
