namespace DonghanEngine.Core.Events;

public enum AugustCrisisOutcome
{
    ImperialSuppressed,     // 天子亲政坐镇：灵帝在朝且皇权稳固，天子严诏节制何进与张让，西园亲军宿卫森严，动乱不发生，何进免死，董卓无召不得入京
    HistoricalBloodshed,    // 史实路线：天子已崩殂/极度势弱，何进与十常侍火并，何进伏诛，袁绍诛宦，少帝蒙尘，董卓乘虚入洛
    EmperorPurgedHeJin,     // 天子主动削藩：灵帝借十常侍或西园军削去何进兵权，收缴大将军印，改由宗室领军
    EmperorPurgedEunuchs    // 天子明正典刑：灵帝下诏清算十常侍贪腐之罪，移交廷尉依律定罪，外戚中官皆服
}

public sealed record AugustCrisisResult(
    AugustCrisisOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int PopularSupportDelta,
    bool HeJinDied,
    bool EunuchsDied,
    bool DongZhuoAllowedEntry);
