namespace DonghanEngine.Core.Events;

public enum ChibiBattleOutcome
{
    ImperialTruceAndTributeBalance, // 天子明诏持节调停罢兵：天子遣大鸿胪持节立于赤壁前线，宣读天子罢战诏书，曹操退守江陵、孙权保全江东、刘备领江南四郡，三方竞相进贡入洛阳，国库+4000，皇权+15，三强鼎立受控于朝廷
    SunLiuChibiFireTriumph,         // 史实赤壁大捷火烧连环：孙刘联军火攻大破曹操于赤壁，曹操引残部走华容道退归北方，孙刘并起，曹操权势大幅回落
    CaoCaoCrossesYangtzeHegemony    // 曹操横渡长江席卷东南：天子势弱，曹操大军克下柴桑兼并江东，权势滔天
}

public sealed record ChibiBattleResult(
    ChibiBattleOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int CaoCaoPowerDelta,
    int CaoCaoLoyaltyDelta,
    int LiuBeiPowerDelta,
    int LiuBeiLoyaltyDelta,
    int SunQuanLoyaltyDelta);
