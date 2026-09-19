using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 193 年 9 月曹操征徐州与陶谦三让徐州（刘备入主徐州）因果（单一职责）
/// </summary>
public sealed class XuzhouSuccessionEvaluator : IXuzhouSuccessionEvaluator
{
    public XuzhouSuccessionResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool caoCaoOverpowered = state.Npcs.TryGetValue("cao_cao", out var cc) && cc.IsActive && cc.Power >= 85;

        // 1. 曹操权势滔天 (>=85) 且朝廷偏弱 (<50)：曹操横扫徐州，兼并其地
        if (caoCaoOverpowered && !strongCourt)
        {
            return new XuzhouSuccessionResult(
                XuzhouSuccessionOutcome.CaoCaoSubduesXuzhou,
                "【徐州沦陷 · 强藩兼并】曹操大军克陷下邳！尽收徐州沃野！",
                "【兼并】初平四年秋，曹操引大军席卷徐州，下邳城陷，陶谦忧惧而死。曹操兼领兖徐二州，中原强藩势力大增，朝廷难加制衡！",
                ImperialPowerDelta: -5,
                TreasuryGoldDelta: 500,
                LiuBeiPowerDelta: 10,
                LiuBeiLoyaltyDelta: 10,
                CaoCaoPowerDelta: 35,
                CaoCaoLoyaltyDelta: -10,
                XuzhouGovernorId: "cao_cao");
        }

        // 2. 天子皇权强盛 (>=50)：陶谦让徐州于平原相刘备，天子明诏敕封刘备为徐州牧、加认宗室皇叔！
        if (strongCourt)
        {
            return new XuzhouSuccessionResult(
                XuzhouSuccessionOutcome.ImperialRatifiesLiuBei,
                "【宗室屏藩 · 皇叔领徐】陶谦让徐州于刘玄德！天子明诏加封徐州牧！",
                "【承袭】初平四年冬，徐州牧陶谦病笃，感平原相刘备仁义驰援，遂以徐州相让。天子明察宗谱，认刘备为宗室皇叔，降旨敕封为徐州牧以制曹操。刘备上表谢恩，誓死效忠汉室，进献贡金一千五百万！",
                ImperialPowerDelta: 10,
                TreasuryGoldDelta: 1500,
                LiuBeiPowerDelta: 30,
                LiuBeiLoyaltyDelta: 30,
                CaoCaoPowerDelta: 10,
                CaoCaoLoyaltyDelta: -15,
                XuzhouGovernorId: "liu_bei");
        }

        // 3. 朝廷中平：遣使调停，刘备暂领徐州，各安其位
        return new XuzhouSuccessionResult(
            XuzhouSuccessionOutcome.ImperialRatifiesLiuBei,
            "【陶谦托孤 · 玄德受领】刘备暂领徐州事，朝廷降旨追认！",
            "【追认】陶谦病故，糜竺、陈登迎刘备入主徐州。天子降旨敕封刘备暂领徐州刺史，刘备遣使进贡，与曹操互成对峙之势。",
            ImperialPowerDelta: 5,
            TreasuryGoldDelta: 1000,
            LiuBeiPowerDelta: 25,
            LiuBeiLoyaltyDelta: 20,
            CaoCaoPowerDelta: 10,
            CaoCaoLoyaltyDelta: -5,
            XuzhouGovernorId: "liu_bei");
    }
}
