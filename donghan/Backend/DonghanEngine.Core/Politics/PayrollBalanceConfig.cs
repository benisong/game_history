namespace DonghanEngine.Core.Politics;

public sealed class PayrollBalanceConfig
{
    public int BaseCostPerThousandSoldiers { get; set; } = 150; // 每1000兵每旬常规军饷
    public int BonusCostPerThousandSoldiers { get; set; } = 250; // 每1000兵额外犒赏开销
    public int BonusMoraleBoost { get; set; } = 15;             // 额外犒赏士气提升
    public int BonusLoyaltyBoost { get; set; } = 10;            // 额外犒赏忠诚提升
    public int BonusImperialPowerBoost { get; set; } = 6;       // 额外犒赏皇权提升
    public int NormalPaidMoraleBoost { get; set; } = 2;         // 常规发饷士气提升
    public int UnpaidMoralePenalty { get; set; } = -20;         // 欠饷士气惩罚
    public int MutinyMoraleThreshold { get; set; } = 30;        // 触发哗变士气阈值
    public double MutinyDesertionRatio { get; set; } = 0.25;    // 哗变逃散兵力比例 (1/4)
    public int MutinyImperialPowerPenalty { get; set; } = -15;  // 哗变皇权受损
}
