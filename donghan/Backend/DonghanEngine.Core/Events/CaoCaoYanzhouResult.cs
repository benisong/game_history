namespace DonghanEngine.Core.Events;

public enum CaoCaoYanzhouOutcome
{
    RatifyAndDraftQingzhouTroops, // 准奏封牧并抽调青州精锐：天子准奏曹操领兖州牧，但诏令抽调青州兵精锐 3000 人充实洛阳西园禁军，收纳谢恩金
    RatifyAndAppease,             // 顺水推舟完全恩抚：天子全权追认曹操为兖州牧，曹操感激涕零，忠诚度大幅提升，进奉巨额贡金
    SendImperialInspector         // 派遣宗室刺史监军：天子派汉室宗亲进驻东郡监军，曹操受制但暗生芥蒂
}

public sealed record CaoCaoYanzhouResult(
    CaoCaoYanzhouOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int CaoCaoPowerDelta,
    int CaoCaoLoyaltyDelta,
    int WestGardenArmyTroopBonus,
    int WestGardenMoraleBonus);
