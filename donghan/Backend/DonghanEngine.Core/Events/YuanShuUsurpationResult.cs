namespace DonghanEngine.Core.Events;

public enum YuanShuUsurpationOutcome
{
    ImperialCoalitionCrushesYuanShu, // 天子明诏天下共讨：天子颁九道诛贼朱谕，曹操、刘备、孙策、吕布四路奉旨合围淮南，袁术势穷兵溃，淮南库府尽入朝廷国库，皇权+15，群雄忠诚暴涨
    CaoCaoIndependentCrush,         // 曹操奉旨独破袁术：曹操引军大破寿春，收降淮南诸军，袁术败死
    YuanShuHoldsHuainan             // 袁术困兽犹斗据守：朝廷号召力不足，各路诸侯观望，袁术盘踞淮南
}

public sealed record YuanShuUsurpationResult(
    YuanShuUsurpationOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int CaoCaoLoyaltyDelta,
    int LiuBeiLoyaltyDelta,
    int SunCeLoyaltyDelta,
    int LvBuLoyaltyDelta,
    int YuanShuPowerDelta);
