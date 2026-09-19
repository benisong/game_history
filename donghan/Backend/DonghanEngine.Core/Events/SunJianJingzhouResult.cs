namespace DonghanEngine.Core.Events;

public enum SunJianJingzhouOutcome
{
    ImperialMediationTruce,   // 天子持节调停：朝廷使者持节制止跨江攻伐，孙坚退兵江东，刘表上表谢恩，双方各输纳贡金
    SunJianFallsAtXianshan,   // 史实岘山遇伏：孙坚急躁轻进，在岘山中暗箭身亡，长子孙策收拢余部投江东
    SunJianDominatesJingzhou  // 江东猛虎破城：孙坚避开伏击大破黄祖，进逼襄阳，刘表割地求和
}

public sealed record SunJianJingzhouResult(
    SunJianJingzhouOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int SunJianPowerDelta,
    int SunJianLoyaltyDelta,
    int LiuBiaoPowerDelta,
    int LiuBiaoLoyaltyDelta,
    bool SunJianDied);
