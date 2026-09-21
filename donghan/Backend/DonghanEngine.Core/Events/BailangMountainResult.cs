namespace DonghanEngine.Core.Events;

public enum BailangMountainOutcome
{
    ImperialTriumphAndNorthernUnification, // 顺天北伐辽东献首：曹操白狼山阵斩蹋顿，公孙康斩送二袁首级入洛阳以表顺命，北方全境底定，天子加封曹操并收归幽并马政，国库+3000，皇权+10
    CaoCaoIndependentConquest,             // 曹操自建殊勋威震边陲：曹操北征乌桓威震夷狄，朝廷优诏赐封，曹操权势倾动天下
    NorthernFrontierInstability            // 边塞风雪阻滞：天子势弱，北伐遭塞外风雪阻截，幽冀边疆依然动荡
}

public sealed record BailangMountainResult(
    BailangMountainOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int CaoCaoPowerDelta,
    int CaoCaoLoyaltyDelta,
    int WestGardenTroopBonus,
    int WestGardenMoraleBonus);
