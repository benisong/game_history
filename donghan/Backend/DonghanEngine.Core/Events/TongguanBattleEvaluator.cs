using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 211 年 3 月曹操西征与潼关之战（关中平定，天子直辖三辅）因果（单一职责）
/// </summary>
public sealed class TongguanBattleEvaluator : ITongguanBattleEvaluator
{
    public TongguanBattleResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool moderateCourt = state.ImperialPower >= 35;

        // 1. 天子皇权强盛 (>=50)：曹操破关中联军，天子明诏收复长安与三辅归京畿直辖，封马超偏将军羁縻凉州，收战利金三千万
        if (strongCourt)
        {
            return new TongguanBattleResult(
                TongguanBattleOutcome.ImperialReclaimsGuanzhongDirect,
                "【潼关荡寇 · 三辅归京】曹操抹书破韩遂！天子收复长安三辅直辖京畿！",
                "【关陇】建安十六年春三月，马超、韩遂等关中十部起兵十万拒守潼关。司空曹操引军西征，运奇谋渡蒲坂、抹书间韩遂，大破西凉联军。天子乘势降旨：长安、冯翊、扶风三辅防务悉数收归洛阳京畿直辖，敕封马超为偏将军以抚凉州。曹操受诏班师，上缴关中战利金三千万钱，关陇底定，皇权赫奕！",
                ImperialPowerDelta: 12,
                TreasuryGoldDelta: 3000,
                CaoCaoPowerDelta: 20,
                CaoCaoLoyaltyDelta: 15,
                MaChaoLoyaltyDelta: 20,
                LiangzhouGovernorId: "ma_chao");
        }

        // 2. 朝廷中平 (35-49)：曹操尽并关中陇右，向朝廷献贡一千五百万
        if (moderateCourt)
        {
            return new TongguanBattleResult(
                TongguanBattleOutcome.CaoCaoSubduesGuanzhongHegemony,
                "【虎步关右 · 尽收秦川】曹操大破西凉诸部！进献关中贡赋！",
                "【平秦】曹操西征大捷，斩成宜、李堪，马超溃奔汉中。曹操留夏侯渊督凉州，尽定关中陇右。曹操遣使入洛阳献降表贡金一千五百万，北方与关右连成一片。",
                ImperialPowerDelta: 5,
                TreasuryGoldDelta: 1500,
                CaoCaoPowerDelta: 35,
                CaoCaoLoyaltyDelta: 10,
                MaChaoLoyaltyDelta: -20,
                LiangzhouGovernorId: "cao_cao");
        }

        // 3. 皇权微弱 (<35)：马超韩遂反攻得逞据守长安
        return new TongguanBattleResult(
            TongguanBattleOutcome.MaSuperDominatesChangAn,
            "【西凉犯境 · 关中沦陷】马超韩遂反攻长安！朝廷西门受阻！",
            "【沦陷】潼关战局逆转，西凉铁骑悍勇破关进逼长安。朝廷西陲门户洞开，三辅动荡，天子震恐！",
            ImperialPowerDelta: -10,
            TreasuryGoldDelta: 0,
            CaoCaoPowerDelta: -15,
            CaoCaoLoyaltyDelta: -5,
            MaChaoLoyaltyDelta: -30,
            LiangzhouGovernorId: "ma_chao");
    }
}
