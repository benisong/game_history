using DonghanEngine.Core.Balance;

namespace DonghanEngine.Core.Politics;

public sealed class TalentNominationBalanceConfig : ITalentNominationBalanceProvider
{
    public int AppointFamilyLoyaltyGain { get; set; } = 15;
    public int AppointFamilyPowerGain { get; set; } = 10;
    public int AppointImperialPowerGain { get; set; } = 3;
    public int RejectFamilyLoyaltyPenalty { get; set; } = -20;
    public int RejectFamilyPowerPenalty { get; set; } = -5;
}
