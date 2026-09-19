namespace DonghanEngine.Core.Events;

public enum GuanxiDefenseOutcome
{
    FirmBorderDeterrence,  // 关西藩屏稳固：皇甫嵩忠诚且关西守军精锐，董卓被彻底锁死在三辅关外，不敢越雷池一步
    BribeAndAppease,       // 封官加爵安抚：天子封董卓为太师/前将军，赐予虚衔以稳住其野心，相安无事
    BorderBorderSkirmish   // 边关生变冲突：天子未加犒赏且皇甫嵩兵微，董卓试探性劫掠三辅，关西告急
}

public sealed record GuanxiDefenseResult(
    GuanxiDefenseOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int HuangfuSongPowerDelta,
    int HuangfuSongLoyaltyDelta,
    int DongZhuoPowerDelta,
    int DongZhuoFavorDelta,
    int TreasuryCost,
    int PopularSupportDelta);
