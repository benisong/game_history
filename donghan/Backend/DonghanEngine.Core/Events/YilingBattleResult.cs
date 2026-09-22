namespace DonghanEngine.Core.Events;

public enum YilingBattleOutcome
{
    ImperialMediationSunLiuAlliance, // 天子明诏持节调停夷陵：天子以宗室尊长与天下共主身份，遣使持节急赴夷陵，降旨严命孙刘罢斗修好、合力屏藩汉室，刘备还师白帝、孙权送还降俘，两家各进贡金三千万，国库+3000，皇权+15，孙刘同盟重归于好
    LuXunFireAttackTriumph,          // 陆逊火烧连营：史实陆逊火攻大破蜀军连营七百里，刘备败退白帝城，天下震动，孙权遣使向朝廷献捷进贡一千五百万
    LiuBeiCrushesEasternWu           // 刘备水陆并进破吴：天子势弱，刘备含怒大破东吴兼并荆扬，东南局势大乱
}

public sealed record YilingBattleResult(
    YilingBattleOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int LiuBeiPowerDelta,
    int LiuBeiLoyaltyDelta,
    int SunQuanPowerDelta,
    int SunQuanLoyaltyDelta);
