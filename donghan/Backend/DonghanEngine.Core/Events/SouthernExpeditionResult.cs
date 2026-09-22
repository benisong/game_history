namespace DonghanEngine.Core.Events;

public enum SouthernExpeditionOutcome
{
    ImperialTriumphAndNanzhongTribute, // 南中底定滇马入洛：诸葛亮深入不毛七擒孟获，平定南中诸郡，孟获真心降服，南中金银丹漆滇马三千匹与贡金三千万尽输洛阳太仓，国库+3000，西园禁军+3000，皇权+15，诸葛亮忠诚满值
    ZhugeLiangAutonomousPacification,  // 诸葛亮平定南中积蓄国力：诸葛亮平南中收赋税充实蜀汉国力，上表洛阳进献方物贡金一千五百万，皇权+5
    NanzhongRebellionProtracted        // 瘴疠阻滞南中未宁：天子势弱，南中瘴气弥漫战事胶着，西南边患难平
}

public sealed record SouthernExpeditionResult(
    SouthernExpeditionOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int ZhugeLiangPowerDelta,
    int ZhugeLiangLoyaltyDelta,
    int WestGardenTroopBonus,
    int WestGardenMoraleBonus);
