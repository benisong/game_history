namespace DonghanEngine.Core.Events;

public enum XiangfanBattleOutcome
{
    ImperialMediationSavesGuanYu,   // 天子明诏持节调停救关羽：天子在洛阳遣使持节至襄阳前线宣读罢战诏，敕关羽退守江陵保全荆州免遭孙权背刺暗算，孙权受申诫罢兵，关羽免死保全，三方各输贡金，国库+3000，皇权+15，关羽/刘备忠诚大增
    HistoricalLvmengCrossesYangtze, // 史实白衣渡江走麦城：孙权吕蒙白衣渡江袭荆州，关羽败走麦城遇害，刘备痛失荆州，孙刘决裂
    GuanYuBreaksFanCity             // 关羽强克襄樊逼洛阳：天子势弱，关羽水淹七军克樊城进逼洛阳，朝廷震恐
}

public sealed record XiangfanBattleResult(
    XiangfanBattleOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int LiuBeiPowerDelta,
    int LiuBeiLoyaltyDelta,
    int CaoCaoPowerDelta,
    int SunQuanPowerDelta,
    int SunQuanLoyaltyDelta,
    bool GuanYuSaved);
