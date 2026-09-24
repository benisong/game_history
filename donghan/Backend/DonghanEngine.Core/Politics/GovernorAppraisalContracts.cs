using System.Collections.Generic;

namespace DonghanEngine.Core.Politics;

public enum AppraisalGrade
{
    Superior,   // 上考 (优异)：治绩斐然，户口增殖，朝贡完备
    Standard,   // 中考 (平庸)：勉强称职，岁赋平平
    Inferior    // 下考 (黜落)：治下凋敝，抗税隐匿，甚至有割据异志
}

public sealed record GovernorAppraisalRecord(
    string ProvinceId,
    string ProvinceName,
    string GovernorId,
    string GovernorName,
    AppraisalGrade Grade,
    int LocalSupport,
    int TaxContribution,       // 本年上缴太仓赋税 (万钱)
    int PopulationGrowth,      // 户口增殖规模
    int Ambition,              // 刺史/太守割据野心
    int Favorability,          // 对天子忠诚好感
    string RecommendedAction,  // 尚书台考课建议 (如 "宜征拜九卿入朝", "宜赐玺书勉谕", "宜罢黜削爵")
    string EvaluationReport);

public sealed record AnnualAppraisalReport(
    int Year,
    IReadOnlyList<GovernorAppraisalRecord> Records,
    int TotalTaxCollected,
    string SummaryText);

public sealed record GovernorPromotionResolutionResult(
    bool Success,
    string GovernorId,
    string GovernorName,
    string ProvinceId,
    bool AcceptedRecall,       // 是否顺服内调入朝
    string TargetCourtTitle,   // 拟任朝堂官职 (如 九卿/太常/少府)
    int ImperialPowerDelta,
    int GentryLoyaltyDelta,
    string NarrativeTitle,
    string ChronicleText,
    string? ErrorCode = null);
