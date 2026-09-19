using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 185 年 2 月洛阳南宫大火与开征修宫钱决策（单一职责）
/// </summary>
public sealed class PalaceFireEvaluator : IPalaceFireEvaluator
{
    public PalaceFireResult Evaluate(GameState state)
    {
        bool zhangRangPowerful = state.Npcs.TryGetValue("zhang_rang", out var zr) && zr.IsActive && zr.Power >= 40;
        bool zhangRangHighFavor = zr != null && zr.Favorability >= 60;

        // 1. 若天子勤政爱民（民心 >= 50 且 阉党权势被压制 < 40 或 张让好感 < 40）：
        // 天子下诏罪己，拒绝加征亩税十钱，厉行节俭
        if (state.PopularSupport >= 50 && (!zhangRangPowerful || !zhangRangHighFavor))
        {
            return new PalaceFireResult(
                PalaceFireDecision.RejectAndAusterity,
                "【南宫大火 · 诏罪己抚民】天子罢修宫之役，下诏减赋！",
                "【政德】南宫大火，中常侍请税天下田亩修宫，天子大怒斥退，下诏罪己减赋，万民欢呼万岁！",
                ImperialPowerDelta: 8,
                PopularSupportDelta: 15,
                TreasuryDelta: 0,
                PrivateTreasuryDelta: 0,
                ZhangRangFavorDelta: -15,
                AllProvinceSupportDelta: 8);
        }

        // 2. 若国库相对充裕 (国库 >= 5000) 但私库紧缺 (私库 < 1000)：
        // 采取折中公款修缮路线，动用国库 1500 万钱，不动民间田赋
        if (state.Treasury >= 5000 && state.PrivateTreasury < 1000)
        {
            return new PalaceFireResult(
                PalaceFireDecision.TreasuryOnlyRebuild,
                "【南宫大火 · 拨帑修葺】朝廷动用国库修葺要紧殿宇，不扰州郡！",
                "【工役】南宫遭祝融之灾，天子敕令度支国库千五百万钱修葺，不加民间田税，朝野粗安。",
                ImperialPowerDelta: 3,
                PopularSupportDelta: 5,
                TreasuryDelta: -1500,
                PrivateTreasuryDelta: 0,
                ZhangRangFavorDelta: -5,
                AllProvinceSupportDelta: 2);
        }

        // 3. 史实路线：张让权势滔天且得宠，推动天下亩税十钱，地方太守进奉修宫钱，私库大充但天下沸腾
        return new PalaceFireResult(
            PalaceFireDecision.LevyTaxAndRebuild,
            "【南宫大火 · 亩税十钱】张让督修南宫，天下加征田亩税！",
            "【苛政】南宫失火，中常侍张让等劝帝税天下田亩修宫，刺史太守皆输助军钱，百姓大怨，盗贼复起！",
            ImperialPowerDelta: -5,
            PopularSupportDelta: -15,
            TreasuryDelta: 0,
            PrivateTreasuryDelta: 3000,
            ZhangRangFavorDelta: 15,
            AllProvinceSupportDelta: -8);
    }
}
