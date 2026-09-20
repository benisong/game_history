namespace DonghanEngine.Core.Events;

public enum TuntianSystemOutcome
{
    NationalTuntianProsperity, // 国家级官屯与民屯大获成功：天子设立典农中郎将，京畿三辅开荒屯田，洛阳太仓粟积盈亿，国库+4000，民心+20，皇权+10，禁军士气+20
    LocalTuntianAppeasement,   // 准许地方诸侯试行屯田：朝廷准曹操等在兖豫推行屯田，中原农业复苏，地方进奉粮食岁赋，国库+2000，民心+10
    TuntianStagnation          // 朝堂争论搁置阻滞：守旧公卿阻挠，屯田未能全面推广，收益寥寥
}

public sealed record TuntianSystemResult(
    TuntianSystemOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int PopularSupportDelta,
    int TreasuryGoldDelta,
    int WestGardenArmyMoraleBonus,
    int CaoCaoLoyaltyDelta);
