using System.Collections.Generic;

namespace DonghanEngine.Core.Health;

/// <summary>
/// 玩家可见的古典气色体征表象（完全隐藏底层具体数值）
/// </summary>
public enum ImperialVitalityAura
{
    RadiantDragon,     // 龙精虎猛 · 神采奕奕 (精力 >= 80, 阳气 >= 60)
    StableHarmonious,  // 神闲气定 · 如常视事 (精力 50~79, 阳气 >= 50)
    SlightlyWeary,     // 神思稍倦 · 案牍微劳 (精力 35~49)
    YangDepleted,      // 虚阳浮越 · 畏寒面青 (阳气 < 50，恢复折损)
    SeverelyExhausted, // 虚耗神伤 · 亏蚀根本 (精力 < 35，结算面临扣减上限风险)
    BedriddenCritical  // 气若游丝 · 卧榻难起 (精力最大值 < 80 染疾沉疴 或 精力 <= 10)
}

/// <summary>
/// 隐藏的太医令请脉诊断报告（不对外暴露数值，只给古典医理建议）
/// </summary>
public sealed record ImperialHealthDiagnosisReport(
    ImperialVitalityAura Aura,
    string PulseDescription,       // 脉象诊断 (如 "脉细欲绝", "气血充盈")
    string ImperialPhysicianAdvice,// 太医院调理建议
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
    ImperialVitalityAura NewAura,
    string NarrativeTitle,
    string ChronicleText);
