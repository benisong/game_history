namespace DonghanEngine.Core.Events;

public enum HanzhongBattleOutcome
{
    ImperialRatifiesKingOfHanzhong, // 天子明诏册封汉中王：天子顺势颁九锡金策加封皇叔刘备为“汉中王”，赐天子旌旗遥制北方，刘备感恩誓死匡扶汉室，进献汉中战利与蜀锦金帛四千万，国库+4000，皇权+15，刘备忠诚+30
    CaoCaoHoldsHanzhongFrontier,    // 曹操退守关中并蓄力：曹操撤军关中陇右以避锋芒，刘备自领汉中王上表称臣进贡两千万，皇权+5
    LiuBeiBreaksIntoGuanzhong       // 刘备席卷关中威胁京畿：天子势弱，刘备拔汉中后直逼三辅，朝廷震恐
}

public sealed record HanzhongBattleResult(
    HanzhongBattleOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int LiuBeiPowerDelta,
    int LiuBeiLoyaltyDelta,
    int CaoCaoPowerDelta,
    int CaoCaoLoyaltyDelta,
    string LiuBeiTitle);
