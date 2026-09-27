using System.Collections.Generic;

namespace DonghanEngine.Core.Settlement;

/// <summary>
/// 旬末结算幻灯片卡片种类
/// </summary>
public enum SettlementSlideKind
{
    General,            // 宏观天时与起居
    MilitaryPayroll,    // 军饷发放与禁军士气
    LandTaxAndScorching,// 田赋征收与焦土复耕
    GovernorAppraisal,  // 尚书台考课与内调
    CourtDelegation,    // 廷议代办执行成效
    SpyReport,          // 西园特务密报
    HistoricalEvent     // 重大历史因果与诸侯攻伐
}

/// <summary>
/// 单张结算卡片模型
/// </summary>
public sealed record SettlementSlide(
    SettlementSlideKind Kind,
    string Title,
    string HeaderSubtitle,
    string BodyText,
    string SummaryTag,
    IReadOnlyList<string> BulletPoints,
    string? IllustrationPath = null);

/// <summary>
/// 旬末全景结算报告
/// </summary>
public sealed record TurnSettlementPackage(
    int Year,
    int Month,
    int Xun,
    string DateText,
    IReadOnlyList<SettlementSlide> Slides,
    GameState SnapshotState);

/// <summary>
/// 旬末结算幻灯片服务契约（单一职责）
/// </summary>
public interface ITurnSettlementSlideService
{
    TurnSettlementPackage CompileTurnSettlementPackage(GameState state, IReadOnlyList<SettlementSlide>? extraSlides = null);
}
