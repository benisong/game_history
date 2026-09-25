using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Balance;
using DonghanEngine.Core.Economy;
using DonghanEngine.Core.Health;
using DonghanEngine.Core.Politics;

namespace DonghanEngine.Tests;

public class SuperControlBalanceInitializationTests
{
    // 1. 模拟开发期：超级控制工具实现类 (支持拉滑块动态 set)
    private sealed class MockSuperControlHealthProvider : IHealthBalanceProvider
    {
        public double MonthlyRecoveryRate { get; set; } = 0.60; // 调高到 60%
        public int LowEnergyThreshold1 { get; set; } = 40;
        public int LowEnergyThreshold2 { get; set; } = 20;
        public int YangHaremDrain { get; set; } = 5;
        public int YangRecoveryPerXun { get; set; } = 5;       // 调高到 5 点
        public int LowYangAffairPenaltyThreshold { get; set; } = 30;
        public double LowYangAffairCostMultiplier { get; set; } = 1.50;
        public int LowYangRecoveryThreshold1 { get; set; } = 50;
        public double LowYangRecoveryRate1 { get; set; } = 0.20;
        public int LowYangRecoveryThreshold2 { get; set; } = 20;
        public double LowYangRecoveryRate2 { get; set; } = 0.10;
        public int YangHighRateThreshold { get; set; } = 80;
        public int YangMidRateThreshold { get; set; } = 70;
        public int EnergyPerYangHighRate { get; set; } = 5;
        public int EnergyPerYangMidRate { get; set; } = 4;
        public int EnergyPerYangLowRate { get; set; } = 3;
        public int MaxEnergyDiseaseThreshold { get; set; } = 80;
        public int LifespanYearsPerStep { get; set; } = 10;
        public double DiseaseStepSize { get; set; } = 10.0;
    }

    // 2. 模拟上线期：静态固化只读类 (不可修改，安全封死)
    private sealed class MockProductionFrozenHealthProvider : IHealthBalanceProvider
    {
        public double MonthlyRecoveryRate => 0.40;
        public int LowEnergyThreshold1 => 40;
        public int LowEnergyThreshold2 => 20;
        public int YangHaremDrain => 5;
        public int YangRecoveryPerXun => 2;
        public int LowYangAffairPenaltyThreshold => 30;
        public double LowYangAffairCostMultiplier => 1.50;
        public int LowYangRecoveryThreshold1 => 50;
        public double LowYangRecoveryRate1 => 0.20;
        public int LowYangRecoveryThreshold2 => 20;
        public double LowYangRecoveryRate2 => 0.10;
        public int YangHighRateThreshold => 80;
        public int YangMidRateThreshold => 70;
        public int EnergyPerYangHighRate => 5;
        public int EnergyPerYangMidRate => 4;
        public int EnergyPerYangLowRate => 3;
        public int MaxEnergyDiseaseThreshold => 80;
        public int LifespanYearsPerStep => 10;
        public double DiseaseStepSize => 10.0;
    }

    [Fact]
    public void Test_HealthService_CanBeInitializedViaInitInterface_WithSuperControlProvider()
    {
        var service = new ImperialHealthService();
        var superControlProvider = new MockSuperControlHealthProvider();

        // 超级控制工具通过 InitializeConfig 接口注入实现类
        service.InitializeConfig(superControlProvider);

        var state = new GameState
        {
            HiddenCurrentEnergy = 50,
            HiddenMaxEnergy = 100,
            HiddenYangVitality = 80
        };

        // 验证 60% 恢复生效: 50 * 0.60 = 30 -> 80
        service.AdvanceMonthlyEnergySettlement(state);
        Assert.Equal(80, state.HiddenCurrentEnergy);

        // 验证阳气自然恢复 5 点生效
        service.AdvanceXunHealthSettlement(state);
        Assert.Equal(85, state.HiddenYangVitality);
    }

    [Fact]
    public void Test_HealthService_CanBeInitializedViaInitInterface_WithProductionFrozenProvider()
    {
        var service = new ImperialHealthService();
        var frozenProvider = new MockProductionFrozenHealthProvider();

        // 正式上线时注入纯只读静态固化实现类
        service.InitializeConfig(frozenProvider);

        var state = new GameState
        {
            HiddenCurrentEnergy = 50,
            HiddenMaxEnergy = 100,
            HiddenYangVitality = 80
        };

        // 验证 40% 恢复生效: 50 * 0.40 = 20 -> 70
        service.AdvanceMonthlyEnergySettlement(state);
        Assert.Equal(70, state.HiddenCurrentEnergy);
    }

    [Fact]
    public void Test_LandOwnershipService_CanBeInitializedViaInitInterface()
    {
        var service = new LandOwnershipService();
        var customLandConfig = new LandBalanceConfig
        {
            GentryTaxDiscountRatio = 0.80
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
            MaxPromotionStepAllowance = 4
        };

        service.InitializeConfig(customRankConfig);
        Assert.Equal(4, service.GetConfig().MaxPromotionStepAllowance);
    }
}
