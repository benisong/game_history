namespace DonghanEngine.Core.Events;

public enum TongguanBattleOutcome
{
    ImperialReclaimsGuanzhongDirect, // 天子直辖关中三辅：曹操破关中联军后，天子顺势收回长安、扶风、安定防务归洛阳京畿直辖，降诏封马超为偏将军以羁縻凉州，国库收缴战利金三千万，皇权+12，凉州太守归顺
    CaoCaoSubduesGuanzhongHegemony,  // 曹操尽并关中陇右：曹操大破马超韩遂，尽收关中十部领地，设夏侯渊督凉州，曹操权势大增，向朝廷献贡一千五百万
    MaSuperDominatesChangAn          // 西凉联军据关中割据：天子势弱，马超韩遂攻破潼关进逼长安，关中再度脱离朝廷节制
}

public sealed record TongguanBattleResult(
    TongguanBattleOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int CaoCaoPowerDelta,
    int CaoCaoLoyaltyDelta,
    int MaChaoLoyaltyDelta,
    string LiangzhouGovernorId);
