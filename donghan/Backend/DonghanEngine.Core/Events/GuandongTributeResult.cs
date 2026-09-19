namespace DonghanEngine.Core.Events;

public enum GuandongTributeOutcome
{
    ImperialTributeProsperity, // 诸侯尊奉纳贡：天子皇权与威严稳固，关东群雄竞相纳贡，国库大充，皇权大振
    PartialTributeDissent,     // 阳奉阴违拖欠：部分诸侯（如袁术）抗税拖欠，天子顺势挑拨邻近诸侯制衡
    FactionalBoycott           // 朝廷威权不足：群雄借口流寇断绝纳贡，朝廷被迫下严旨斥责
}

public sealed record GuandongTributeResult(
    GuandongTributeOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int CaoCaoLoyaltyDelta,
    int YuanShaoLoyaltyDelta,
    int SunJianLoyaltyDelta,
    int YuanShuLoyaltyDelta);
