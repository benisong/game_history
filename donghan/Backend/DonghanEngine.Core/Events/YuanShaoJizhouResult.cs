namespace DonghanEngine.Core.Events;

public enum YuanShaoJizhouOutcome
{
    RatifyAndCollectGold,    // 顺水推舟追认：天子承认既成事实，加封袁绍为冀州牧，收纳谢恩贡金 2000 万，袁绍忠诚度上升
    DenounceAndProvokeGongsun,// 明斥暗诛挑拨：天子严旨斥责袁绍擅夺同僚符节，密诏公孙瓒趁虚攻打袁绍后方（驱虎吞狼）
    DeployImperialMediator   // 持节调停庇护：天子遣光禄大夫持节入邺城，保全韩馥并划定分治，收取双方和解金
}

public sealed record YuanShaoJizhouResult(
    YuanShaoJizhouOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int YuanShaoPowerDelta,
    int YuanShaoLoyaltyDelta,
    int GongsunZanHostilityDelta,
    bool HanFuPreserved);
