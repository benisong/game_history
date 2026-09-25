using DonghanEngine.Core.Balance;

namespace DonghanEngine.Core.Economy;

public sealed class CadastralAndIrrigationBalanceConfig : ICadastralAndIrrigationBalanceProvider
{
    // 度田三种强度配置
    public double MildReduction { get; set; } = 0.05;
    public int MildCapacityBoost { get; set; } = 5000;
    public int MildTaxReclaimed { get; set; } = 300;
    public int MildLoyaltyPenalty { get; set; } = -5;
    public int MildMoraleBoost { get; set; } = 3;

    public double StandardReduction { get; set; } = 0.12;
    public int StandardCapacityBoost { get; set; } = 15000;
    public int StandardTaxReclaimed { get; set; } = 800;
    public int StandardLoyaltyPenalty { get; set; } = -15;
    public int StandardMoraleBoost { get; set; } = 8;

    public double ThoroughReduction { get; set; } = 0.20;
    public int ThoroughCapacityBoost { get; set; } = 28000;
    public int ThoroughTaxReclaimed { get; set; } = 1800;
    public int ThoroughLoyaltyPenalty { get; set; } = -30;
    public int ThoroughMoraleBoost { get; set; } = 15;

    // 水利工程配置
    public int IrrigationGoldCost { get; set; } = 1000;
    public int IrrigationCapacityIncrease { get; set; } = 20000;
    public int IrrigationMoraleBoost { get; set; } = 8;
}
