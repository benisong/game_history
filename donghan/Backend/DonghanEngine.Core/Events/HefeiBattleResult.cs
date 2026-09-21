namespace DonghanEngine.Core.Events;

public enum HefeiBattleOutcome
{
    ImperialMediationAndHuaiheTruce, // 天子明诏持节调停江淮：张辽逍遥津大破孙权后，天子遣使持节调停淮南兵端，孙权退军柴桑，曹操守合肥，两家各纳贡金入朝，国库+3000，皇权+10
    ZhangLiaoIndependentGlory,       // 张辽威震江东逍遥津：张辽八百破十万威震江表，曹操向朝廷献捷进贡一千五百万，孙权受创休兵
    SunQuanBreaksHefei               // 孙权强攻拔合肥：天子势弱，孙权十万大军强克合肥威逼寿春，江淮失衡
}

public sealed record HefeiBattleResult(
    HefeiBattleOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int CaoCaoLoyaltyDelta,
    int SunQuanLoyaltyDelta,
    int CaoCaoPowerDelta,
    int SunQuanPowerDelta);
