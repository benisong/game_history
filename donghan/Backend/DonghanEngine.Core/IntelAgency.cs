using System;

namespace DonghanEngine.Core;

/// <summary>
/// 情报机构(黄门暗探)月度供养档位。天子从私库拨款，档位决定暗探"工作热情"(Zeal)。
/// </summary>
public enum IntelFundingTier
{
    None = 0,    // 不给 / 私库不足：月供 0，热情 0
    Half = 1,    // 半价克扣：月供 100 万，热情 50
    Normal = 2,  // 正常供养：月供 200 万，热情 100
    Lavish = 3   // 厚赏打赏：月供 300 万，热情 150
}

/// <summary>
/// 情报机构供养与准确率换算(纯函数，集中维护以便平衡调参)。
///
/// 链路:拨款档位 → 月供(私库扣费) → 工作热情 Zeal → 情报查探准确率。
/// 准确率公式: 准确率% = 75 + K × (Zeal − 100) × 0.25，其中平衡常数 K = 1.6。
///   不给(Zeal0)→35%  半价(50)→55%  正常(100)→75%  打赏(150)→95%
/// </summary>
public static class IntelAgency
{
    /// <summary>平衡性常数(将来调参旋钮)。当前 1.6 → 档位准确率 35/55/75/95。</summary>
    public const double AccuracyBalanceK = 1.6;

    /// <summary>各档每月私库支出(万钱)。</summary>
    public static int MonthlyCost(IntelFundingTier tier) => tier switch
    {
        IntelFundingTier.Half => 100,
        IntelFundingTier.Normal => 200,
        IntelFundingTier.Lavish => 300,
        _ => 0
    };

    /// <summary>各档激发的暗探工作热情(Zeal)。</summary>
    public static int Zeal(IntelFundingTier tier) => tier switch
    {
        IntelFundingTier.Half => 50,
        IntelFundingTier.Normal => 100,
        IntelFundingTier.Lavish => 150,
        _ => 0
    };

    /// <summary>工作热情 → 情报查探准确率(0-100)。</summary>
    public static int AccuracyFromZeal(int zeal)
    {
        double acc = 75 + AccuracyBalanceK * (zeal - 100) * 0.25;
        return (int)Math.Round(Math.Clamp(acc, 0, 100));
    }

    /// <summary>当前供养档位对应的查探准确率(0-100)。供 BuildIntelAssessment 直接使用。</summary>
    public static int CurrentAccuracy(IntelFundingTier tier) => AccuracyFromZeal(Zeal(tier));

    public static string TierLabel(IntelFundingTier tier) => tier switch
    {
        IntelFundingTier.Half => "克扣供养",
        IntelFundingTier.Normal => "正常供养",
        IntelFundingTier.Lavish => "厚赏打赏",
        _ => "暗探涣散(未供养)"
    };
}
