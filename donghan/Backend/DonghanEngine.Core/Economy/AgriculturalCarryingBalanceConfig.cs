using DonghanEngine.Core.Balance;

namespace DonghanEngine.Core.Economy;

public sealed class AgriculturalCarryingBalanceConfig : IAgriculturalCarryingBalanceProvider
{
    public double MaxWeatherPenalty { get; set; } = 0.60;
    public double WeatherSeverityScale { get; set; } = 0.015;
    public int GrainPerThousandPeople { get; set; } = 100;
    public double AristocracyInterceptFactor { get; set; } = 0.50;
    public double CollapseDeficitRatioThreshold { get; set; } = 0.40;
    public double CollapseCapacityRatioThreshold { get; set; } = 1.35;
    public int CollapseMoralePenalty { get; set; } = -20;
    public double SevereShortageDeficitThreshold { get; set; } = 0.15;
    public int SevereShortageMoralePenalty { get; set; } = -10;
    public double MildPressureAristocracyThreshold { get; set; } = 0.60;
    public int MildPressureMoralePenalty { get; set; } = -3;
    public int StableAbundantMoraleBoost { get; set; } = 3;
}
