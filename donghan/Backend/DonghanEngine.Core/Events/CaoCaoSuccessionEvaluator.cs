using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 220 年 1 月曹操病逝与曹丕嗣位（天子追赠殊勋 · 申饬曹丕恪守臣节）因果（单一职责）
/// </summary>
public sealed class CaoCaoSuccessionEvaluator : ICaoCaoSuccessionEvaluator
{
    public CaoCaoSuccessionResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool moderateCourt = state.ImperialPower >= 35;

        // 1. 天子皇权强盛 (>=50)：天子亲降优诏追赠曹操殊荣，敕命曹丕嗣魏王并申饬恪守臣节，曹丕献贡金三千万
        if (strongCourt)
        {
            return new CaoCaoSuccessionResult(
                CaoCaoSuccessionOutcome.ImperialGrantsPosthumousAndRestrainsPi,
                "【一代雄杰 · 魏武星沉】魏王曹操病逝于洛阳！天子优诏追赠、申饬曹丕恪守臣节！",
                "【终老】建安二十五年春正月，魏王曹操在洛阳官邸病逝，寿六十六。天子闻奏感伤，亲临吊唁，下明诏优诏追赠曹操殊荣，谥曰武王。同时降严旨册封世子曹丕嗣位魏王、领丞相，明申《恪守纯臣封疆诏》，戒其毋蹈僭越非分之举。曹丕伏地泣拜受诏，誓死守臣节，上表进纳谢恩助国贡金三千万钱入洛阳太仓，北方人心晏然，皇权大振！",
                ImperialPowerDelta: 15,
                TreasuryGoldDelta: 3000,
                CaoPiPowerDelta: 20,
                CaoPiLoyaltyDelta: 25,
                CaoCaoDied: true);
        }

        // 2. 朝廷中平 (35-49)：曹丕袭魏王位，朝廷厚加恩抚，进贡一千五百万
        if (moderateCourt)
        {
            return new CaoCaoSuccessionResult(
                CaoCaoSuccessionOutcome.CaoPiInheritsAndExpandsPower,
                "【魏王嗣立 · 权倾北疆】曹操病逝曹丕嗣位！朝廷厚恩抚慰！",
                "【嗣位】曹操病殁洛阳，世子曹丕袭魏王爵位，统领北方大军。曹丕遣使入洛阳进贡一千五百万表奏请封，天子诏准嗣位，北方权柄尽归曹丕。",
                ImperialPowerDelta: 5,
                TreasuryGoldDelta: 1500,
                CaoPiPowerDelta: 30,
                CaoPiLoyaltyDelta: 10,
                CaoCaoDied: true);
        }

        // 3. 皇权微弱 (<35)：曹丕居丧跋扈
        return new CaoCaoSuccessionResult(
            CaoCaoSuccessionOutcome.CaoPiDefiesImperialAuthority,
            "【魏王薨逝 · 嗣子跋扈】曹丕拥重兵居丧擅专！朝廷节制难行！",
            "【跋扈】曹操病死，曹丕引重兵擅自入驻邺城，不奉朝廷节制。关东人心动荡，朝廷威严大损！",
            ImperialPowerDelta: -10,
            TreasuryGoldDelta: 0,
            CaoPiPowerDelta: 40,
            CaoPiLoyaltyDelta: -20,
            CaoCaoDied: true);
    }
}
