namespace DonghanEngine.Core.Balance;

/// <summary>
/// 核心健康与精气神平衡常数接口
/// </summary>
public interface IHealthBalanceProvider
{
    double MonthlyRecoveryRate { get; }
    int LowEnergyThreshold1 { get; }
    int LowEnergyThreshold2 { get; }
    int YangHaremDrain { get; }
    int YangRecoveryPerXun { get; }
    int LowYangAffairPenaltyThreshold { get; }
    double LowYangAffairCostMultiplier { get; }
    int LowYangRecoveryThreshold1 { get; }
    double LowYangRecoveryRate1 { get; }
    int LowYangRecoveryThreshold2 { get; }
    double LowYangRecoveryRate2 { get; }
    int YangHighRateThreshold { get; }
    int YangMidRateThreshold { get; }
    int EnergyPerYangHighRate { get; }
    int EnergyPerYangMidRate { get; }
    int EnergyPerYangLowRate { get; }
    int MaxEnergyDiseaseThreshold { get; }
    int LifespanYearsPerStep { get; }
    double DiseaseStepSize { get; }
}

/// <summary>
/// 土地产权与税赋平衡常数接口
/// </summary>
public interface ILandBalanceProvider
{
    int WarGentryLossRatioPercent { get; }
    int ScorchedMonthsDuration { get; }
    int RepurchaseGoldPerThousandLand { get; }
    int RepurchaseMinGold { get; }
    int RepurchaseLoyaltyBoost { get; }
    int TaxPerThousandStateLand { get; }
    double GentryTaxDiscountRatio { get; }
}

/// <summary>
/// 军饷与禁军士气平衡常数接口
/// </summary>
public interface IPayrollBalanceProvider
{
    int BaseCostPerThousandSoldiers { get; }
    int BonusCostPerThousandSoldiers { get; }
    int BonusMoraleBoost { get; }
    int BonusLoyaltyBoost { get; }
    int BonusImperialPowerBoost { get; }
    int NormalPaidMoraleBoost { get; }
    int UnpaidMoralePenalty { get; }
    int MutinyMoraleThreshold { get; }
    double MutinyDesertionRatio { get; }
    int MutinyImperialPowerPenalty { get; }
}

/// <summary>
/// 抄家断案平衡常数接口
/// </summary>
public interface IConfiscationBalanceProvider
{
    int MinBaseStash { get; }
    int CorruptionGoldMultiplier { get; }
    int PowerGoldMultiplier { get; }
    int CorruptionGrainMultiplier { get; }
    int BaseGrainSeized { get; }
    int ImperialPowerGainOnConfiscation { get; }
    int PublicMoraleGainOnConfiscation { get; }
    int SameFactionLoyaltyPenalty { get; }
    int OtherFactionLoyaltyPenalty { get; }
}

/// <summary>
/// 官职品阶与鬻官平衡常数接口
/// </summary>
public interface IOfficialRankBalanceProvider
{
    int MaxPromotionStepAllowance { get; }
    int ExtraordinaryPromotionStepThreshold { get; }
    int ExtraordinaryPromotionLoyaltyGain { get; }
    int StandardPromotionLoyaltyGain { get; }
    int ExtraordinaryScholarLoyaltyPenalty { get; }
    int OfficeSaleTier1MoralePenalty { get; }
    int OfficeSaleTier2MoralePenalty { get; }
    int OfficeSaleTier3MoralePenalty { get; }
    int OfficeSaleDefaultMoralePenalty { get; }
    int OfficeSaleHighRankScholarPenalty { get; }
    int OfficeSaleLowRankScholarPenalty { get; }
    int OfficeSaleBuyerFavorabilityGain { get; }
}

/// <summary>
/// 土地承载与马尔萨斯危机平衡常数接口
/// </summary>
public interface IAgriculturalCarryingBalanceProvider
{
    double MaxWeatherPenalty { get; }
    double WeatherSeverityScale { get; }
    int GrainPerThousandPeople { get; }
    double AristocracyInterceptFactor { get; }
    double CollapseDeficitRatioThreshold { get; }
    double CollapseCapacityRatioThreshold { get; }
    int CollapseMoralePenalty { get; }
    double SevereShortageDeficitThreshold { get; }
    int SevereShortageMoralePenalty { get; }
    double MildPressureAristocracyThreshold { get; }
    int MildPressureMoralePenalty { get; }
    int StableAbundantMoraleBoost { get; }
}

/// <summary>
/// 刺史年终考课平衡常数接口
/// </summary>
public interface IGovernorAppraisalBalanceProvider
{
    int InferiorSupportThreshold { get; }
    int InferiorAmbitionThreshold { get; }
    int InferiorFavorabilityThreshold { get; }
    int SuperiorSupportThreshold { get; }
    int SuperiorTaxContributionThreshold { get; }
    int SuperiorFavorabilityThreshold { get; }
    int PromotionImperialPowerThreshold { get; }
    int PromotionFavorabilityThreshold { get; }
    int PromotionMaxAmbitionThreshold { get; }
    int PromotionImperialPowerGain { get; }
    int PromotionGentryLoyaltyGain { get; }
    int PromotionFavorabilityGain { get; }
    int PromotionPowerGain { get; }
    int RefusalImperialPowerPenalty { get; }
    int RefusalGentryLoyaltyPenalty { get; }
    int RefusalFavorabilityPenalty { get; }
    int RefusalPowerGain { get; }
}

/// <summary>
/// 廷议代办平衡常数接口
/// </summary>
public interface IDelegationBalanceProvider
{
    int BatchDelegationEnergyCost { get; }
    double AbilityEfficiencyWeight { get; }
    double CorruptionEmbezzleRatio { get; }
    double CorruptionPopularityPenaltyRatio { get; }
    int BaseHandlerPowerGain { get; }
}

/// <summary>
/// 天子威望平衡常数接口
/// </summary>
public interface IImperialPrestigeBalanceProvider
{
    int PuppetMaxThreshold { get; }
    double PuppetEfficiency { get; }
    double PuppetRebellionModifier { get; }
    int DisrespectedMaxThreshold { get; }
    double DisrespectedEfficiency { get; }
    double DisrespectedRebellionModifier { get; }
    int GoldenBalanceMaxThreshold { get; }
    double GoldenBalanceEfficiency { get; }
    double GoldenBalanceRebellionModifier { get; }
    int OppressiveMaxThreshold { get; }
    double OppressiveEfficiency { get; }
    double OppressiveTransferRate { get; }
    double OppressiveRebellionModifier { get; }
    double TyrannicalEfficiency { get; }
    double TyrannicalTransferRate { get; }
    double TyrannicalRebellionModifier { get; }
    int OppressiveCorruptionThreshold { get; }
    int OppressiveAmbitionThreshold { get; }
}

/// <summary>
/// 察举荐辟平衡常数接口
/// </summary>
public interface ITalentNominationBalanceProvider
{
    int AppointFamilyLoyaltyGain { get; }
    int AppointFamilyPowerGain { get; }
    int AppointImperialPowerGain { get; }
    int RejectFamilyLoyaltyPenalty { get; }
    int RejectFamilyPowerPenalty { get; }
}

/// <summary>
/// 流寇反哺平衡常数接口
/// </summary>
public interface IBanditBalanceProvider
{
    int AbsorbedTroopsPercent { get; }
    int PowerGainDivisor { get; }
    int MinPowerGain { get; }
}

/// <summary>
/// 度田水利平衡常数接口
/// </summary>
public interface ICadastralAndIrrigationBalanceProvider
{
    double MildReduction { get; }
    int MildCapacityBoost { get; }
    int MildTaxReclaimed { get; }
    int MildLoyaltyPenalty { get; }
    int MildMoraleBoost { get; }
    double StandardReduction { get; }
    int StandardCapacityBoost { get; }
    int StandardTaxReclaimed { get; }
    int StandardLoyaltyPenalty { get; }
    int StandardMoraleBoost { get; }
    double ThoroughReduction { get; }
    int ThoroughCapacityBoost { get; }
    int ThoroughTaxReclaimed { get; }
    int ThoroughLoyaltyPenalty { get; }
    int ThoroughMoraleBoost { get; }
    int IrrigationGoldCost { get; }
    int IrrigationCapacityIncrease { get; }
    int IrrigationMoraleBoost { get; }
}
