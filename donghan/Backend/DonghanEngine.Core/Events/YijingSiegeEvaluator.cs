using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 199 年 3 月公孙瓒易京覆灭与袁绍一统河北四州因果（单一职责）
/// </summary>
public sealed class YijingSiegeEvaluator : IYijingSiegeEvaluator
{
    public YijingSiegeResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool moderateCourt = state.ImperialPower >= 35;

        // 1. 天子皇权强盛 (>=50)：恩威并济，封袁绍为大将军兼冀州牧，同时拔擢曹操为司空兼兖州牧，促成二强互制，朝廷收两千万谢恩贡金
        if (strongCourt)
        {
            return new YijingSiegeResult(
                YijingSiegeOutcome.RatifyYuanShaoAndElevateCaoCao,
                "【河北一统 · 帝策制衡】袁绍平易京尽并四州！天子封大将军以制司空曹操！",
                "【分陕】建安四年春，袁绍筑长围攻破易京，公孙瓒自焚殉城，袁绍尽并幽、冀、青、并四州之地，拥带甲数十万。袁绍遣逢纪入洛阳表奏请封大将军。天子顺势诏加袁绍为大将军兼领冀州牧，收纳河北谢恩助军贡金两千万；同时拜曹操为司空，使南北二强各安分陕互为牵制，皇权赫奕！",
                ImperialPowerDelta: 5,
                TreasuryGoldDelta: 2000,
                YuanShaoPowerDelta: 30,
                YuanShaoLoyaltyDelta: 15,
                CaoCaoPowerDelta: 20,
                CaoCaoLoyaltyDelta: 25,
                WestGardenTroopBonus: 0,
                YouzhouGovernorId: "yuan_shao");
        }

        // 2. 朝廷中平 (35-49)：收拢幽州溃散义从精锐入西园禁军
        if (moderateCourt)
        {
            return new YijingSiegeResult(
                YijingSiegeOutcome.ImperialSecretAllianceGongsun,
                "【易京烽火 · 义从归汉】袁本初席卷河北！朝廷收幽州白马义从充禁军！",
                "【破城】易京城破，幽州大乱。天子遣使收抚幽州溃兵，赵云等旧部率幽州义从骑兵两千人南下归顺朝廷，充入西园禁军！天子追认袁绍领冀州，河北局势骤紧。",
                ImperialPowerDelta: 2,
                TreasuryGoldDelta: 1000,
                YuanShaoPowerDelta: 35,
                YuanShaoLoyaltyDelta: 5,
                CaoCaoPowerDelta: 15,
                CaoCaoLoyaltyDelta: 15,
                WestGardenTroopBonus: 2000,
                YouzhouGovernorId: "yuan_shao");
        }

        // 3. 皇权微弱 (<35)：袁绍一统河北自尊自大，朝廷震恐
        return new YijingSiegeResult(
            YijingSiegeOutcome.YuanShaoDominatesNorthUnchecked,
            "【北方鼎革 · 四州皆袁】袁绍带甲数十万雄视天下！朝廷震恐！",
            "【割据】袁绍尽收河北沃野强兵，自领大将军，不复尊奉朝廷正朔，关东震恐，天下大势尽归北方，朝廷威严大损！",
            ImperialPowerDelta: -10,
            TreasuryGoldDelta: 0,
            YuanShaoPowerDelta: 45,
            YuanShaoLoyaltyDelta: -15,
            CaoCaoPowerDelta: 10,
            CaoCaoLoyaltyDelta: 5,
            WestGardenTroopBonus: 0,
            YouzhouGovernorId: "yuan_shao");
    }
}
