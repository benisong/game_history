namespace DonghanEngine.Core.Events;

public enum PalaceFireDecision
{
    LevyTaxAndRebuild,   // 史实路线：采纳张让建议，天下开征亩税十钱，刺史太守纳修宫钱，私库大充但天下怨声载道
    RejectAndAusterity,  // 节俭抚民路线：天子下诏罪己，拒绝加税，抚恤百姓，民心大振，中官失望
    TreasuryOnlyRebuild  // 国库自支修缮：不动天下赋税，仅动用朝廷公款修缮要紧殿宇，平衡折中
}

public sealed record PalaceFireResult(
    PalaceFireDecision Decision,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int PopularSupportDelta,
    int TreasuryDelta,
    int PrivateTreasuryDelta,
    int ZhangRangFavorDelta,
    int AllProvinceSupportDelta);
