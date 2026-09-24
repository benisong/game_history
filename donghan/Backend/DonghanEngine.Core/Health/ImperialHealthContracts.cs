namespace DonghanEngine.Core.Health;

/// <summary>
/// 玩家可见的天子精神状态（完全隐藏精力/阳气具体数值，用精神状态隐式传达）
/// </summary>
public enum ImperialMentalState
{
    Radiant,         // 龙精虎猛 · 神采奕奕 (精力 >= 80, 阳气 >= 60)
    ClearAndCalm,    // 神闲气定 · 如常视事 (精力 50~79, 阳气 >= 50)
    SlightlyFatigued,// 神思稍倦 · 案牍微劳 (精力 35~49)
    YangDeficient,   // 虚阳浮越 · 畏寒神怠 (阳气 < 50，恢复效率折半)
    DeeplyExhausted, // 虚耗神伤 · 亏蚀根本 (精力 < 35，结算面临扣减上限风险)
    CriticalCollapse // 气若游丝 · 卧榻难起 (精力最大值 < 80 染疾沉疴 或 精力 <= 10)
}

/// <summary>
/// 太医令请脉诊断报告（不对外暴露数值，只给古典医理建议）
/// </summary>
public sealed record ImperialHealthDiagnosisReport(
    ImperialMentalState MentalState,
    string MentalStateDescription, // 精神状态文风表述
    string PulseDescription,       // 脉象诊断 (如 "脉细欲绝", "六脉调匀")
    string PhysicianAdvice,        // 太医院调理建议
    bool IsAfflictedWithDisease,   // 是否因最大精力上限 < 80 染疾
    int YearsOfLifeLost,           // 累计折损预期寿数 (年)
    string NarrativeSummary);

/// <summary>
/// 调养与政务动作结算结果
/// </summary>
public sealed record HealthActionResolutionResult(
    bool Success,
    string ActionName,
    int EnergyDelta,
    int YangDelta,
    ImperialMentalState NewMentalState,
    string NarrativeTitle,
    string ChronicleText);
