namespace DonghanEngine.Core.Events;

public enum JingzhouSuccessionOutcome
{
    ImperialMediationAndPartition, // 天子大义调解分陕：天子下诏敕命刘琦继领荆州刺史、刘备移驻江夏为藩屏，曹操虽引兵南下但奉诏止步宛襄，荆襄各献贡金入朝，国库+2500，皇权+10
    CaoCaoSubduesJingzhouDirect,   // 曹操南征纳降刘琮：刘表病亡刘琮举州降曹，曹操尽并荆襄七郡，威逼江东，刘备败退夏口
    LiuBeiConsolidatesJingxiang    // 刘备据荆襄自雄：刘备感召荆襄士民尽收江南四郡，与曹操分庭抗礼
}

public sealed record JingzhouSuccessionResult(
    JingzhouSuccessionOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int CaoCaoPowerDelta,
    int CaoCaoLoyaltyDelta,
    int LiuBeiPowerDelta,
    int LiuBeiLoyaltyDelta,
    string JingzhouGovernorId);
