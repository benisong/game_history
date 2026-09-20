using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 200 年 10 月乌巢劫粮与官渡决胜因果（单一职责）
/// </summary>
public sealed class GuanduVictoryEvaluator : IGuanduVictoryEvaluator
{
    public GuanduVictoryResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool caoCaoLoyal = state.Npcs.TryGetValue("cao_cao", out var cc) && cc.IsActive && cc.Favorability >= 60;

        // 1. 天子皇权强盛 (>=50)：天子恩威并施，准曹操献捷表奏，加封进爵同时命宗室监军制衡，收缴乌巢战利金三千万
        if (strongCourt)
        {
            return new GuanduVictoryResult(
                GuanduVictoryOutcome.RatifyVictoryAndRestrainCaoCao,
                "【官渡决胜 · 乌巢夜火】曹操奇袭乌巢焚袁军粮饷！天子受献捷巨贡！",
                "【大捷】建安五年冬十月，司空曹操亲引五千骑夜袭乌巢，尽焚袁绍粮屯，张郃、高览倒戈降曹，袁绍八万大军土崩瓦解，单骑渡黄河北遁。曹操遣使向洛阳告捷，进献乌巢战利金与河北珍宝三千万钱。天子下诏嘉勉曹操破贼殊勋，加封进爵，同时命宗室重臣巡抚黄河沿线以申朝纲，皇权大振！",
                ImperialPowerDelta: 10,
                TreasuryGoldDelta: 3000,
                CaoCaoPowerDelta: 30,
                CaoCaoLoyaltyDelta: 20,
                YuanShaoPowerDelta: -60,
                YuanShaoLoyaltyDelta: -30);
        }

        // 2. 曹操好感度高 (>=60) 且朝廷中平：曹操大捷威震天下，朝廷顺水推舟全力封赏
        if (caoCaoLoyal)
        {
            return new GuanduVictoryResult(
                GuanduVictoryOutcome.CaoCaoOverwhelmingHegemony,
                "【中原底定 · 雄霸北方】曹操官渡大捷席卷河北！朝廷顺势重赏！",
                "【封赏】曹操以弱胜强破袁绍于官渡，天下震动。曹操表奏献贡一千五百万，天子降诏重加封赏，曹操权势倾动中原，袁绍一蹶不振。",
                ImperialPowerDelta: 5,
                TreasuryGoldDelta: 1500,
                CaoCaoPowerDelta: 35,
                CaoCaoLoyaltyDelta: 15,
                YuanShaoPowerDelta: -50,
                YuanShaoLoyaltyDelta: -20);
        }

        // 3. 皇权微弱且将帅离心：袁曹久战相持不下
        return new GuanduVictoryResult(
            GuanduVictoryOutcome.GuanduStalemateProlonged,
            "【黄河鏖兵 · 胜负未分】袁曹两军官渡苦战对峙！北方生灵涂炭！",
            "【相持】官渡战局胶着，两军死伤枕藉，粮饷匮竭。朝廷号令难行，关东民生凋敝，社稷威严大损！",
            ImperialPowerDelta: -10,
            TreasuryGoldDelta: 0,
            CaoCaoPowerDelta: 10,
            CaoCaoLoyaltyDelta: -5,
            YuanShaoPowerDelta: -10,
            YuanShaoLoyaltyDelta: -10);
    }
}
