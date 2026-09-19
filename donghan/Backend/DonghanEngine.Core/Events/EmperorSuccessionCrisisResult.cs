namespace DonghanEngine.Core.Events;

public enum EmperorCrisisCause
{
    NaturalIllness,       // 纯生理大病（好感正常，但天子自身 Health 过低）
    HeJinAssassination,   // 外戚何进集团鸩杀（何进好感过低且野心权势极大）
    EunuchAssassination,  // 十常侍张让集团下毒（宦官失宠面临清算，铤而走险）
    SafeAndHealthy        // 忠诚稳固、龙体安康，平安度过危机
}

public enum EmperorSurvivalOutcome
{
    DiedFromPoisonOrIllness, // 中毒/重病驾崩（进入崩殂/何进乱政）
    SurvivedAndDiscovered,   // 险死还生并查明真相（灵帝健康强挺过来，震怒彻查）
    PeacefulRecovery         // 盛世安康，龙体无恙
}

public sealed record EmperorSuccessionCrisisResult(
    EmperorCrisisCause Cause,
    EmperorSurvivalOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int HealthDelta,
    int ImperialPowerDelta,
    int CulpritPowerDelta,
    string CulpritNpcId,
    bool CulpritExposed);
