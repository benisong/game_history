namespace DonghanEngine.Core.Events;

public enum LvBuYanzhouOutcome
{
    ReliefAndMediateYanzhou,   // 天子调粮赈济并持节制衡：天子发洛阳常平仓粮万石赈济中原饥民，降旨敕封吕布为平东将军、曹操保全兖州，以粮为筹码令两虎互制，国库支出赈济，民心暴涨+15，皇权+10
    CaoCaoRecoversYanzhou,     // 曹操自力击破吕布：曹操筹粮血战击退吕布，吕布东投徐州刘备，曹操元气受损
    ZhongyuanFamineCollapse    // 中原大饥流民四起：朝廷冷眼旁观不发赈粮，中原大饥人相食，流民四溃，民心-20，皇权-10
}

public sealed record LvBuYanzhouResult(
    LvBuYanzhouOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int PopularSupportDelta,
    int TreasuryFoodCost,
    int CaoCaoPowerDelta,
    int CaoCaoLoyaltyDelta,
    int LvBuPowerDelta,
    int LvBuLoyaltyDelta);
