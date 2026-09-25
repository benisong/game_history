namespace DonghanEngine.Core.Politics;

public sealed class OfficialRankBalanceConfig
{
    public int MaxPromotionStepAllowance { get; set; } = 3;      // 正规升迁单次最大超擢跨度 (3级)
    public int ExtraordinaryPromotionStepThreshold { get; set; } = 2; // 判定为超擢拔擢的级数门槛 (>=2级)
    public int ExtraordinaryPromotionLoyaltyGain { get; set; } = 35;  // 超擢被拔擢者忠诚增量
    public int StandardPromotionLoyaltyGain { get; set; } = 15;       // 常规升迁忠诚增量
    public int ExtraordinaryScholarLoyaltyPenalty { get; set; } = -10;// 超擢清流老臣不满扣减
    public int OfficeSaleTier1MoralePenalty { get; set; } = -25;      // 卖1品三公全国民心惩罚
    public int OfficeSaleTier2MoralePenalty { get; set; } = -18;      // 卖2品州牧全国民心惩罚
    public int OfficeSaleTier3MoralePenalty { get; set; } = -12;      // 卖3品九卿全国民心惩罚
    public int OfficeSaleDefaultMoralePenalty { get; set; } = -6;     // 卖中低级官职民心惩罚
    public int OfficeSaleHighRankScholarPenalty { get; set; } = -20;  // 卖高官清流忠诚惩罚
    public int OfficeSaleLowRankScholarPenalty { get; set; } = -8;    // 卖低官清流忠诚惩罚
    public int OfficeSaleBuyerFavorabilityGain { get; set; } = 20;    // 买主忠诚增量
}
