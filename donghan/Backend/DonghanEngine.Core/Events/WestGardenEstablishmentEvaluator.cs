using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 188 年 8 月西园八校尉建军与平乐观大阅兵因果（单一职责）
/// </summary>
public sealed class WestGardenEstablishmentEvaluator : IWestGardenEstablishmentEvaluator
{
    public WestGardenEstablishmentResult Evaluate(GameState state)
    {
        bool heJinOverpowered = state.Npcs.TryGetValue("he_jin", out var hj) && hj.IsActive && hj.Power > 80;
        bool hasEnoughFunds = state.PrivateTreasury >= 800;

        // 1. 私库不足：无力支撑八校营房军械开支，建军受阻
        if (!hasEnoughFunds)
        {
            return new WestGardenEstablishmentResult(
                WestGardenEstablishmentOutcome.FundShortageAborted,
                "【军务受阻 · 府库匮乏】内帑不足，西园八校尉建军暂缓！",
                "【军政】天子欲置西园新军以分大将军兵权，然内帑空虚无力置办军械，西园建军之事暂寝。",
                ImperialPowerDelta: -5,
                HeJinPowerDelta: 5,
                JianShuoPowerDelta: -10,
                CaoCaoPowerDelta: 0,
                YuanShaoPowerDelta: 0,
                ArmySizeBonus: 0,
                ArmyMoraleBonus: -10,
                ArmyLoyaltyBonus: -10,
                PrivateTreasuryCost: 0);
        }

        // 2. 何进权势滔天 (>80)：大将军府反客为主，渗透并架空西园军
        if (heJinOverpowered)
        {
            return new WestGardenEstablishmentResult(
                WestGardenEstablishmentOutcome.InfiltratedByHeJin,
                "【西园建军 · 外戚掣肘】西园八校尉成军，然大部受大将军府节制！",
                "【军变】天子置西园八校尉，自称无上将军，令蹇硕统之。然大将军何进势大，袁绍、曹操多遵何进号令，天子分权受掣！",
                ImperialPowerDelta: 5,
                HeJinPowerDelta: 10,
                JianShuoPowerDelta: 5,
                CaoCaoPowerDelta: 10,
                YuanShaoPowerDelta: 15,
                ArmySizeBonus: 2000,
                ArmyMoraleBonus: 10,
                ArmyLoyaltyBonus: -5,
                PrivateTreasuryCost: 600);
        }

        // 3. 史实成功建军：天子自称无上将军，蹇硕统领全军，曹操袁绍就任校尉，平乐观盛大阅兵
        return new WestGardenEstablishmentResult(
            WestGardenEstablishmentOutcome.ImperialTriumph,
            "【平乐观大阅 · 威震洛阳】西园八校尉新军成立！天子亲御六军！",
            "【盛典】天子于平乐观大阅诸军，筑大坛建九层华盖，自号“无上将军”，以蹇硕统八校尉新军，曹操、袁绍分任典军、中军校尉，皇权大振！",
            ImperialPowerDelta: 15,
            HeJinPowerDelta: -15,
            JianShuoPowerDelta: 20,
            CaoCaoPowerDelta: 15,
            YuanShaoPowerDelta: 15,
            ArmySizeBonus: 4000,
            ArmyMoraleBonus: 25,
            ArmyLoyaltyBonus: 25,
            PrivateTreasuryCost: 800);
    }
}
