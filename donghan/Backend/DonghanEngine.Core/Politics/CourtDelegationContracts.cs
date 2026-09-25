using System.Collections.Generic;

namespace DonghanEngine.Core.Politics;

/// <summary>
/// 廷议代办枢纽类型
/// </summary>
public enum CourtDelegationHubKind
{
    ThreeExcellencies, // 司徒/司空府 (三公·清流民政)
    GrandGeneral,      // 大将军府 (外戚军务)
    PalaceAttendants,  // 中常侍/内侍 (十常侍理财内帑)
    Secretariat        // 尚书台/中枢 (能臣考课台阁)
}

/// <summary>
/// 待办政务条目
/// </summary>
public sealed record DelegationAffairItem(
    string AffairId,
    string Title,
    string Description,
    CourtDelegationHubKind PreferredHub,
    int BaseTreasuryCost,
    int BasePopularSupportDelta,
    int BaseStateLandDelta,
    int BaseArmyMoraleDelta,
    int BaseDirectEnergyCost = 4);

/// <summary>
/// 单项政务经办人执行报告
/// </summary>
public sealed record AffairExecutionReport(
    string AffairId,
    string Title,
    string HandlerNpcId,
    string HandlerName,
    CourtDelegationHubKind HubKind,
    int ActualTreasuryCost,
    int EmbezzledAmount,         // 贪官私吞中饱私囊金额 (转入其隐匿财产，日后可抄家)
    int ActualPopularSupportDelta,
    int ActualStateLandDelta,
    int ActualArmyMoraleDelta,
    int HandlerPowerGained,       // 经办人权势增长
    string ExecutionNarrative);

/// <summary>
/// 廷议一揽子政务分发总结果
/// </summary>
public sealed record BatchDelegationResult(
    bool Success,
    int TotalEnergySpent,        // 一揽子交办固定消耗的精力 (默认 2 点)
    IReadOnlyList<AffairExecutionReport> ExecutedReports,
    int TotalEmbezzled,          // 本次所有经办人中饱私囊总额
    int TotalTreasurySpent,
    string SummaryNarrative);

/// <summary>
/// 廷议代办可配置参数矩阵（数值完全不写死，支持运行时与配置文件大幅微调）
/// </summary>
public sealed class DelegationConfig : DonghanEngine.Core.Balance.IDelegationBalanceProvider
{
    public int BatchDelegationEnergyCost { get; set; } = 2; // 一揽子批复固定精力消耗 (预设 2)
    public double AbilityEfficiencyWeight { get; set; } = 1.0; // 能力对成效的加权系数
    public double CorruptionEmbezzleRatio { get; set; } = 0.005; // 贪腐转化为中饱私囊比例
    public double CorruptionPopularityPenaltyRatio { get; set; } = 0.5; // 贪腐对民心的扣减折损系数
    public int BaseHandlerPowerGain { get; set; } = 2; // 每次经办官员权势增长基准
}
