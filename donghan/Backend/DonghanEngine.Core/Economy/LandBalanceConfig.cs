using DonghanEngine.Core.Balance;

namespace DonghanEngine.Core.Economy;

public sealed class LandBalanceConfig : ILandBalanceProvider
{
    public int WarGentryLossRatioPercent { get; set; } = 35; // 战后世家私田沦为焦土比例 (35%)
    public int ScorchedMonthsDuration { get; set; } = 6;      // 焦土休耕持续月数 (6个月)
    public int RepurchaseGoldPerThousandLand { get; set; } = 100; // 每1000亩赎买价格 (100万钱)
    public int RepurchaseMinGold { get; set; } = 50;          // 赎买最低缴纳金
    public int RepurchaseLoyaltyBoost { get; set; } = 15;     // 赎买士族忠诚提升点数
    public int TaxPerThousandStateLand { get; set; } = 10;    // 国家官田每1000亩季赋 (10万钱)
    public double GentryTaxDiscountRatio { get; set; } = 0.60;// 世家私田折减系数 (少收40%即按60%收)
}
