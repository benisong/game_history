using System.Collections.Generic;

namespace DonghanEngine.Core.Economy;

public enum CadastralSurveyIntensity
{
    Mild,       // 宽和核验：轻微清查，世家抵触较小
    Standard,   // 严明度田：清查隐匿田产，抑制兼并，豪强微怨
    Thorough    // 铁腕度田：掘地三尺清查坞堡隐匿人口与土地，世家震恐但国库与承载大增
}

public sealed record CadastralSurveyResult(
    bool Success,
    string ProvinceId,
    string ProvinceName,
    CadastralSurveyIntensity Intensity,
    double LandRatioReduction,    // 兼并度削减比例 (如 -0.15)
    int ReclaimedArableLand,      // 收归编户齐民的耕地/承载力提升规模
    int TaxGoldReclaimed,         // 补缴追缴税赋 (万钱)
    int AristocratLoyaltyPenalty, // 世家门阀忠诚扣减
    int PopularMoraleBoost,       // 底层自耕农与民心提振
    string NarrativeTitle,
    string ChronicleText);

public sealed record IrrigationProjectResult(
    bool Success,
    string ProvinceId,
    string ProvinceName,
    string ProjectName,           // 水利工程名称 (如 "引洛灌溉", "修白渠", "都江堰岁修", "芍陂大兴")
    int GoldCost,                 // 耗费国库资金 (万钱)
    int CapacityIncrease,         // 土地人口承载力永久提升 ($L_{\max}$)
    int PopularMoraleBoost,       // 民心提振
    string NarrativeTitle,
    string ChronicleText);
