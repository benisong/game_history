namespace DonghanEngine.Core.Health;

public sealed class HealthBalanceConfig
{
    public double MonthlyRecoveryRate { get; set; } = 0.40;
    public int LowEnergyThreshold1 { get; set; } = 40;
    public int LowEnergyThreshold2 { get; set; } = 20;
    public int YangHaremDrain { get; set; } = 5;
    public int YangRecoveryPerXun { get; set; } = 2;
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
