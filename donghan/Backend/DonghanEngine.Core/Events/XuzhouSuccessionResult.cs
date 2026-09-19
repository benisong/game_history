namespace DonghanEngine.Core.Events;

public enum XuzhouSuccessionOutcome
{
    ImperialRatifiesLiuBei,     // 天子诏封刘皇叔：天子下明诏敕封刘备领徐州牧，加封宗室皇叔号，刘备忠心耿耿，成为中央制衡曹操的重要屏障
    CaoCaoSubduesXuzhou,       // 曹操破城吞徐州：朝廷未加干涉，曹操大军克下下邳兼并徐州，中原势力剧增
    TaoQianHoldsAndAppeals     // 陶谦借使节调停：天子遣使持节敕令曹操罢兵，陶谦进献贡金保全徐州
}

public sealed record XuzhouSuccessionResult(
    XuzhouSuccessionOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int LiuBeiPowerDelta,
    int LiuBeiLoyaltyDelta,
    int CaoCaoPowerDelta,
    int CaoCaoLoyaltyDelta,
    string XuzhouGovernorId);
