namespace DonghanEngine.Core.Events;

public enum KuangtingBattleOutcome
{
    ImperialAuthorizesCaoCao,  // 天子降诏授权：天子下明诏命曹操代天巡狩讨伐袁术，曹操大破袁术于匡亭，袁术溃逃淮南，天子皇权+8，曹操忠诚+15，国库收缴战利金
    CaoCaoIndependentVictory,  // 曹操自力破贼：曹操未经诏令破袁术，声威大震，天子顺势安抚中原
    YuanShuDominatesZhongyuan  // 袁术势大乱政：袁术部曲猖獗，中原战火蔓延，朝廷震动
}

public sealed record KuangtingBattleResult(
    KuangtingBattleOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int CaoCaoPowerDelta,
    int CaoCaoLoyaltyDelta,
    int YuanShuPowerDelta,
    int YuanShuLoyaltyDelta);
