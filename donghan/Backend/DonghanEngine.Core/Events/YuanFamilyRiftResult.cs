namespace DonghanEngine.Core.Events;

public enum YuanFamilyRiftOutcome
{
    ImperialDivideAndConquer, // 天子大义分封二袁内斗：天子分别册封袁谭领青州、袁尚领冀州，撺掇二袁兄弟相残消耗河北，双方各向朝廷进贡表忠，国库+2000，皇权+10
    CaoCaoSubduesHebeiDirect, // 曹操借机北伐平定河北：曹操趁二子相争长驱直入攻陷邺城，河北尽归曹操
    YuanFamilyReconciles     // 袁氏兄弟休兵御外：朝廷号令难行，二袁暂时修好合力阻曹
}

public sealed record YuanFamilyRiftResult(
    YuanFamilyRiftOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int CaoCaoPowerDelta,
    int CaoCaoLoyaltyDelta,
    bool YuanShaoDied);
