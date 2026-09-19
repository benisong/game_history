using System.Collections.Generic;

namespace DonghanEngine.Core.Events;

public enum WestGardenEstablishmentOutcome
{
    ImperialTriumph,     // 史实成功建军：私库充裕且蹇硕在朝，西园八校尉成立，平乐观大阅兵，灵帝自称无上将军，收回部分军权
    InfiltratedByHeJin,  // 大将军府渗透：何进权势过大 (>=80)，八校尉中军/典军皆受何进节制，分权受阻
    FundShortageAborted  // 私库不足夭折：私库匮乏 (<800)，无力置办军械营舍，建军延后
}

public sealed record WestGardenEstablishmentResult(
    WestGardenEstablishmentOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int HeJinPowerDelta,
    int JianShuoPowerDelta,
    int CaoCaoPowerDelta,
    int YuanShaoPowerDelta,
    int ArmySizeBonus,
    int ArmyMoraleBonus,
    int ArmyLoyaltyBonus,
    int PrivateTreasuryCost);
