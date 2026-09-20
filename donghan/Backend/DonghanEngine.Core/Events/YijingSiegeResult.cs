namespace DonghanEngine.Core.Events;

public enum YijingSiegeOutcome
{
    RatifyYuanShaoAndElevateCaoCao, // 顺势封大将军并升曹操司空：天子敕封袁绍为大将军兼领冀州、封曹操为司空兼领兖州，南北二强互相对峙平衡，朝廷收纳河北两千万谢恩金，皇权+5
    ImperialSecretAllianceGongsun,  // 密诏救易京失利但保残部：天子遣使救公孙瓒未果，但收拢赵云等义从骑兵入西园禁军，西园禁军+2000，袁绍心怀怨望
    YuanShaoDominatesNorthUnchecked // 袁绍一统河北天下震恐：朝廷无力制衡，袁绍尽并四州自立大将军，雄视中原，皇权-10
}

public sealed record YijingSiegeResult(
    YijingSiegeOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int YuanShaoPowerDelta,
    int YuanShaoLoyaltyDelta,
    int CaoCaoPowerDelta,
    int CaoCaoLoyaltyDelta,
    int WestGardenTroopBonus,
    string YouzhouGovernorId);
