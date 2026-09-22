namespace DonghanEngine.Core.Events;

public enum BaidiEntrustmentOutcome
{
    ImperialAppointsZhugeLiangLoyal, // 天子明诏拜诸葛亮领丞相：天子下明旨优抚西蜀，册封诸葛亮为武乡侯、领益州丞相，建立朝廷直通巴蜀公文，诸葛亮表忠纳贡四千万，皇权+15，诸葛亮忠诚100
    ZhugeLiangGovernsAutonomously,   // 诸葛亮鞠躬尽瘁领蜀：诸葛亮受托孤治蜀，上表洛阳请封，进贡两千万，朝廷追认
    ShuHanFactionalChaos            // 蜀汉内耗失序：天子势弱，刘备殁后蜀中南中并叛，朝廷难加节制
}

public sealed record BaidiEntrustmentResult(
    BaidiEntrustmentOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int ZhugeLiangLoyaltyDelta,
    int ZhugeLiangPowerDelta,
    bool LiuBeiDied);
