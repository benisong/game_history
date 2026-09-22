using System.Collections.Generic;

namespace DonghanEngine.Core.Economy;

public enum ProvinceCrisisLevel
{
    StableAndAbundant, // 土地人口承载充裕，民心安稳
    MildPressure,       // 接近承载上限，隐匿田产轻微
    SevereShortage,     // 土地超载/旱蝗灾害，粮食短缺
    MalthusianCollapse  // 人口严重突破承载上限且兼并严重，触发饥荒暴动与流民潮
}

public sealed record ProvinceCarryingReport(
    string ProvinceId,
    string ProvinceName,
    int Population,
    int LandCarryingCapacity, // 土地最大人口承载上限
    double AristocracyLandRatio, // 世家大族土地兼并比例 (0.0 ~ 1.0)
    int GrainYield,           // 本旬粮食产出 (石)
    int GrainRequired,        // 本旬口粮需求 (石)
    int GrainDeficit,         // 粮食缺口 (0表示无缺口)
    ProvinceCrisisLevel CrisisLevel,
    int PublicMoraleDelta,    // 对民心的影响 (-20 ~ +5)
    int DisplacedRefugees,    // 产生的流民/流寇规模
    string Description);

public sealed record BanditSpilloverResult(
    string ProvinceId,
    int BanditScale,          // 本地爆发的流寇规模
    string AbsorbingWarlordId,// 借机收编流民为私兵部曲的军阀 ID (如 "cao_cao", "yuan_shao")
    string EliteTroopTypeName,// 收编转化的特殊精锐兵种名称 (如 "青州兵", "白马义从", "丹阳兵")
    int TroopsAbsorbed,       // 诸侯吸收的兵员数量
    int WarlordPowerGained,   // 诸侯增长的权势
    string NarrativeTitle,
    string ChronicleText);
