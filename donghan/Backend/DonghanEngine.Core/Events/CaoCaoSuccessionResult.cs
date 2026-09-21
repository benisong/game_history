namespace DonghanEngine.Core.Events;

public enum CaoCaoSuccessionOutcome
{
    ImperialGrantsPosthumousAndRestrainsPi, // 天子优诏追赠并申饬曹丕：天子亲临吊唁追赠曹操殊荣，降明旨敕命世子曹丕嗣位魏王、恪守纯臣之节不得僭号，曹丕伏地受诏，献纳谢恩贡金三千万，皇权+15，曹丕忠诚+25
    CaoPiInheritsAndExpandsPower,          // 曹丕袭位权倾朝野：曹丕袭魏王爵位，尽统北方军政，朝廷厚加恩抚，曹丕权势+30，进贡一千五百万
    CaoPiDefiesImperialAuthority           // 曹丕居丧跋扈谋僭：天子势弱，曹丕拥重兵居丧擅专，朝廷节制难行
}

public sealed record CaoCaoSuccessionResult(
    CaoCaoSuccessionOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int TreasuryGoldDelta,
    int CaoPiPowerDelta,
    int CaoPiLoyaltyDelta,
    bool CaoCaoDied);
