using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 189 年 8 月中平政变因果（天子坐镇动乱不发生 vs 史实血案）（单一职责）
/// </summary>
public sealed class AugustCrisisEvaluator : IAugustCrisisEvaluator
{
    public AugustCrisisResult Evaluate(GameState state)
    {
        bool emperorAliveAndHealthy = state.Health > 0 && state.Outcome == GameOutcome.Playing;
        bool imperialPowerFirm = state.ImperialPower >= 35;
        bool westGardenDisciplined = state.WestGardenArmy != null && state.WestGardenArmy.Loyalty >= 40;

        // 1. 天子健在、坐镇朝堂、皇权与西园军可控：
        // 宫廷动乱不发生！天子威严震慑，何进不敢召外兵，十常侍不敢伏杀大将军，天下免于战火！
        if (emperorAliveAndHealthy && (imperialPowerFirm || westGardenDisciplined))
        {
            return new AugustCrisisResult(
                AugustCrisisOutcome.ImperialSuppressed,
                "【天子临朝 · 震慑内外】天子坐镇紫宸殿，外戚中官各安其位，宫廷动乱消弭于无形！",
                "【治平】中平六年八月，天子龙体康泰，诏谕大将军何进、中常侍张让各守职分，严禁部曲甲士持械入省，洛阳宫禁肃然，史实宫变不复发生！",
                ImperialPowerDelta: 10,
                PopularSupportDelta: 10,
                HeJinDied: false,
                EunuchsDied: false,
                DongZhuoAllowedEntry: false);
        }

        // 2. 史实路线：天子已崩殂（或皇权衰竭 < 35 且禁军涣散），触发何进遇害与十常侍之乱
        return new AugustCrisisResult(
            AugustCrisisOutcome.HistoricalBloodshed,
            "【宫变大乱 · 何进伏诛】嘉德殿伏兵四起，大将军何进遇害！",
            "【血案】中平六年八月，大将军何进谋诛中官，反被张让等伏甲士杀于嘉德殿前，袁绍等引兵入宫尽诛无须者，洛阳大乱！",
            ImperialPowerDelta: -15,
            PopularSupportDelta: -10,
            HeJinDied: true,
            EunuchsDied: true,
            DongZhuoAllowedEntry: true);
    }
}
