using System.Collections.Generic;

namespace DonghanEngine.Core.Economy;

public enum WarScorchedState
{
    Normal,         // 正常耕作
    ScorchedEarth   // 战乱焦土（半年不产粮）
}

public sealed record ProvinceLandReport(
    string ProvinceId,
    string ProvinceName,
    int TotalArableLand,       // 州郡总耕地/土地承载 (亩/标准单位)
    int StateControlledLand,   // 国家/天子控制土地 (编户齐民/官田屯田)
    int GentryControlledLand,  // 世家豪族控制土地 (坞堡私田)
    int ScorchedLand,          // 战乱焦土无主地 (战乱后半年不产粮)
    int ScorchedMonthsRemaining,// 焦土休耕剩余月数
    int EstimatedTaxYield,     // 本季度预估纳税赋额 (万钱)
    string OwnerFactionId,     // 当前实际占领势力 ID
    string Description);

public sealed record LandRepurchaseResult(
    bool Success,
    string ProvinceId,
    string ProvinceName,
    int LandPurchased,         // 世家购回的土地规模
    int GoldPaidToTreasury,    // 支付给国库的金帛 (万钱)
    int RemainingStateLand,    // 朝廷保留的官田规模
    int GentryLoyaltyBoost,    // 世家忠诚提振
    string NarrativeTitle,
    string ChronicleText);

public sealed record PostWarLandResolutionResult(
    string ProvinceId,
    string ProvinceName,
    int NewlyScorchedLand,     // 新增焦土规模
    int StateLandGained,       // 朝廷直接收归的无主官田
    int GentryLandLost,        // 世家损失的土地
    string VictorFactionId,    // 战胜方
    string NarrativeTitle,
    string ChronicleText);
