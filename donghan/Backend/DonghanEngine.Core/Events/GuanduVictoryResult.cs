namespace DonghanEngine.Core.Events;

public enum GuanduVictoryOutcome
{
    RatifyVictoryAndRestrainCaoCao, // 顺势封赏并御札申饬：天子准曹操官渡大捷，加封冀州牧/丞相号但命宗室进驻徐豫监军，曹操献乌巢战利金三千万，皇权+10
    CaoCaoOverwhelmingHegemony,     // 曹操大捷威震天下：曹操尽并袁绍辎重，声威赫奕，朝廷全力追认，曹操权势暴涨+35
    GuanduStalemateProlonged        // 官渡胶着相持不下：许攸未反，袁曹长久对峙，北方生灵涂炭
}

public sealed record GuanduVictoryResult(
    GuanduVictoryOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int CaoCaoPowerDelta,
    int CaoCaoLoyaltyDelta,
    int YuanShaoPowerDelta,
    int YuanShaoLoyaltyDelta);
