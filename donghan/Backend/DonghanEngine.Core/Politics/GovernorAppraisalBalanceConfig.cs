namespace DonghanEngine.Core.Politics;

public sealed class GovernorAppraisalBalanceConfig
{
    public int InferiorSupportThreshold { get; set; } = 30;
    public int InferiorAmbitionThreshold { get; set; } = 80;
    public int InferiorFavorabilityThreshold { get; set; } = 40;
    public int SuperiorSupportThreshold { get; set; } = 60;
    public int SuperiorTaxContributionThreshold { get; set; } = 400;
    public int SuperiorFavorabilityThreshold { get; set; } = 60;
    public int PromotionImperialPowerThreshold { get; set; } = 46;
    public int PromotionFavorabilityThreshold { get; set; } = 70;
    public int PromotionMaxAmbitionThreshold { get; set; } = 80;
    public int PromotionImperialPowerGain { get; set; } = 3;
    public int PromotionGentryLoyaltyGain { get; set; } = 5;
    public int PromotionFavorabilityGain { get; set; } = 15;
    public int PromotionPowerGain { get; set; } = 10;
    public int RefusalImperialPowerPenalty { get; set; } = -3;
    public int RefusalGentryLoyaltyPenalty { get; set; } = -5;
    public int RefusalFavorabilityPenalty { get; set; } = -15;
    public int RefusalPowerGain { get; set; } = 5;
}
