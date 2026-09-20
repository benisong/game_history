namespace DonghanEngine.Core.Events;

public enum GuanduPreludeOutcome
{
    ImperialDualAppeasementAndTribute, // 天子居中制衡收纳巨贡：天子明诏申诫袁曹二强各守疆土不得轻动，袁绍进奉河北战马两千匹，曹操上表进纳漕粮百万石，国库+3000，皇权+10，南北对峙相持
    CaoCaoFavoredSecretSupport,        // 密诏相助曹操抗袁：天子密下朱谕于曹操许其经略中原以抗河北强虏，曹操誓死效忠，袁绍心生疑忌
    HegemonicClashUnchecked            // 袁曹私战天下大乱：朝廷号令难行，袁曹十万大军于黄河两岸列阵对峙，战火一触即发，皇权-10
}

public sealed record GuanduPreludeResult(
    GuanduPreludeOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int CaoCaoLoyaltyDelta,
    int YuanShaoLoyaltyDelta,
    int WestGardenTroopBonus,
    int WestGardenMoraleBonus);
