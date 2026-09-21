namespace DonghanEngine.Core.Events;

public enum LiuBeiYizhouOutcome
{
    ImperialRatifiesYizhouAndCollectsTribute, // 天子明诏加封皇叔领益州：天子下明诏敕封刘备为益州牧，准其节制巴蜀，刘备表奏谢恩，进纳蜀锦与天府贡金三千万，皇权+10，刘备忠诚+30
    CaoCaoInterferesHanzhong,                // 曹操警惕进兵汉中：曹操闻刘备得益州，引兵西征汉中张鲁以扼蜀道门户，朝廷受贡一千五百万
    LiuBeiAutonomousDefiance                 // 刘备割据巴蜀关锁剑阁：天子势弱，刘备据险自雄，朝廷诏命难入剑门关
}

public sealed record LiuBeiYizhouResult(
    LiuBeiYizhouOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int LiuBeiPowerDelta,
    int LiuBeiLoyaltyDelta,
    int CaoCaoPowerDelta,
    string YizhouGovernorId);
