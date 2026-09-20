namespace DonghanEngine.Core.Events;

public enum BaimenlouOutcome
{
    ExecuteLvBuConsolidateOrder, // 密诏赐死诛桀骜：天子明察吕布反复无常，密诏曹操处死吕布于白门楼，整肃朝纲，皇权+10，曹操/刘备好感大增，徐州平定
    PardonLvBuDraftToWestGarden, // 赦免收降充西园：天子惜其勇武，特旨赦免吕布死罪，收为西园禁军前军先锋大将（西园禁军+2000，士气+25），吕布感激涕零誓死效忠，但微损法度
    LvBuFleesToHebei            // 吕布突围投袁绍：朝廷号令滞后，吕布率并州狼骑突围北投袁绍，河北军力大增
}

public sealed record BaimenlouResult(
    BaimenlouOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int CaoCaoLoyaltyDelta,
    int LiuBeiLoyaltyDelta,
    int WestGardenTroopBonus,
    int WestGardenMoraleBonus,
    bool LvBuExecuted);
