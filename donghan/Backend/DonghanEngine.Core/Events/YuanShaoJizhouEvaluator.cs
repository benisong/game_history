using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 191 年 4 月袁绍兼并冀州与韩馥让冀州因果（单一职责）
/// </summary>
public sealed class YuanShaoJizhouEvaluator : IYuanShaoJizhouEvaluator
{
    public YuanShaoJizhouResult Evaluate(GameState state)
    {
        bool strongImperialAuthority = state.ImperialPower >= 55;
        bool moderateImperialAuthority = state.ImperialPower >= 35;

        // 1. 天子皇权强盛 (>=55)：天子底气充足，严旨斥责袁绍擅夺符节，并密诏公孙瓒自北向南击其背腹（驱虎吞狼）
        if (strongImperialAuthority)
        {
            return new YuanShaoJizhouResult(
                YuanShaoJizhouOutcome.DenounceAndProvokeGongsun,
                "【天子明斥 · 驱虎吞狼】天子严责袁绍逼夺冀州！密诏公孙瓒引兵南下！",
                "【暗流】初平二年四月，袁绍胁迫韩馥让出冀州牧印绶，上表求封。天子大怒，痛斥其擅黜封疆大吏，密诏幽州公孙瓒发骑兵南下讨逆，河北局势骤变！",
                ImperialPowerDelta: 8,
                TreasuryGoldDelta: 0,
                YuanShaoPowerDelta: 10,
                YuanShaoLoyaltyDelta: -25,
                GongsunZanHostilityDelta: 40,
                HanFuPreserved: false);
        }

        // 2. 天子皇权中平 (35-54)：顺水推舟追认既成事实，加封冀州牧并收取 2000 万谢恩助军金
        if (moderateImperialAuthority)
        {
            return new YuanShaoJizhouResult(
                YuanShaoJizhouOutcome.RatifyAndCollectGold,
                "【既成事实 · 官爵追认】天子准奏封袁绍领冀州牧，纳谢恩金两千万！",
                "【妥协】袁绍既据冀州，陈兵进贡求封。天子顺水推舟加封袁绍为冀州牧，收纳谢恩助军钱两千万，袁绍上表谢恩，暂时臣服。",
                ImperialPowerDelta: -3,
                TreasuryGoldDelta: 2000,
                YuanShaoPowerDelta: 25,
                YuanShaoLoyaltyDelta: 15,
                GongsunZanHostilityDelta: 10,
                HanFuPreserved: false);
        }

        // 3. 天子皇权微弱 (<35)：朝廷无力制衡，袁绍彻底吞并冀州，韩馥出奔投张邈
        return new YuanShaoJizhouResult(
            YuanShaoJizhouOutcome.RatifyAndCollectGold,
            "【河北易主 · 强藩坐大】袁绍尽吞冀州沃野！朝廷无奈追认！",
            "【割据】袁绍入主邺城，尽收韩馥部曲与粮秣，河北四战之地尽归袁氏，朝廷诏命难出关东，唯有追认其位！",
            ImperialPowerDelta: -8,
            TreasuryGoldDelta: 500,
            YuanShaoPowerDelta: 35,
            YuanShaoLoyaltyDelta: -10,
            GongsunZanHostilityDelta: 20,
            HanFuPreserved: false);
    }
}
