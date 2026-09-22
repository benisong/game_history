using System.Collections.Generic;

namespace DonghanEngine.Core.Politics;

public enum ConfiscationTargetType
{
    CorruptOfficial, // 贪官污吏 / 专权外戚 / 贪腐宦官
    AristocratGentry // 隐匿田产豪强大族
}

public sealed record ConfiscationExecutionResult(
    bool Success,
    string TargetNpcId,
    string TargetName,
    int GoldSeized,
    int GrainSeized,
    int ImperialPowerDelta,
    int PublicMoraleDelta,
    IReadOnlyDictionary<string, int> AffiliatedLoyaltyPenalties,
    string NarrativeTitle,
    string ChronicleText);

public enum MilitaryPayrollStatus
{
    FullyPaid,       // 军饷足额发放，军心稳定
    GenerouslyRewarded, // 内库重赏三军，士气暴涨，皇权提升
    UnpaidShortage,  // 欠饷短缺，士气滑落
    MutinyTriggered  // 士气暴跌至30以下，引发禁军哗变兵谏
}

public sealed record MilitaryPayrollResult(
    MilitaryPayrollStatus Status,
    int GoldSpent,
    int MoraleDelta,
    int ImperialPowerDelta,
    int DeserterCount,
    string NarrativeTitle,
    string ChronicleText);
