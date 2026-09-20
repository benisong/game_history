namespace DonghanEngine.Core.Events;

public enum SunCeJiangdongOutcome
{
    RatifyAndCollectSaltIronTax, // 顺水推舟追认封赏：天子明诏加封孙策为折冲校尉、领会稽太守，收纳江东巨额盐铁特产贡金（国库+2000），孙策忠诚大增，江东藩屏成型
    StrictImperialCommission,    // 派遣宗室扬州刺史：天子封孙策为将军，但保留刘繇等宗室节制之权，孙策虽服但暗中扩军
    JiangdongAutonomousDefiance  // 江东割据脱节：天子势弱，孙策席卷六郡自领会稽，朝廷无力收税，仅象征性纳贡
}

public sealed record SunCeJiangdongResult(
    SunCeJiangdongOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int SunCePowerDelta,
    int SunCeLoyaltyDelta,
    string YangzhouGovernorId);
