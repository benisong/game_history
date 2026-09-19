namespace DonghanEngine.Core.Geopolitics.Contracts;

public enum GeopoliticalEdictType
{
    RatifyAndReward,   // 官爵追认（承认既成事实，加封官职，收缴谢恩钱）
    DenounceAndProvoke,// 明旨斥责并密诏背刺（驱虎吞狼：令第三者趁虚袭其后方）
    MediateTruce,      // 遣使持节调停罢兵（居中收贡，划界休战）
    AcceptTribute,     // 嘉奖纳贡忠臣（纳金入国库，赐锦袍御酒提升忠诚）
    DeployLoyalTroops  // 奉旨征调忠臣出兵（命忠义军团拔营解围）
}

public sealed record GeopoliticalImperialEdict(
    GeopoliticalEdictType EdictType,
    string TargetFactionId,
    string ThirdPartyFactionId,
    string TargetProvinceId,
    int GoldDeductionOrReward,
    string EdictText);

public sealed record EdictExecutionResult(
    bool Success,
    string NarrativeSummary,
    int ImperialPowerDelta,
    int TreasuryDelta,
    int PrivateTreasuryDelta,
    int TargetLoyaltyDelta,
    int TargetAmbitionDelta,
    string ChronicleEntry);
