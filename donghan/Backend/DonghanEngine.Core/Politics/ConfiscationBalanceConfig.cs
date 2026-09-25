namespace DonghanEngine.Core.Politics;

public sealed class ConfiscationBalanceConfig
{
    public int MinBaseStash { get; set; } = 500;
    public int CorruptionGoldMultiplier { get; set; } = 80;
    public int PowerGoldMultiplier { get; set; } = 40;
    public int CorruptionGrainMultiplier { get; set; } = 150;
    public int BaseGrainSeized { get; set; } = 1000;
    public int ImperialPowerGainOnConfiscation { get; set; } = 8;
    public int PublicMoraleGainOnConfiscation { get; set; } = 12;
    public int SameFactionLoyaltyPenalty { get; set; } = -25;
    public int OtherFactionLoyaltyPenalty { get; set; } = -5;
}
