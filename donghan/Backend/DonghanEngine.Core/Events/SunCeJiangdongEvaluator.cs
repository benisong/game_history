using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 195 年 2 月孙策平定江东与玉玺真伪（孙策讨封江东）因果（单一职责）
/// </summary>
public sealed class SunCeJiangdongEvaluator : ISunCeJiangdongEvaluator
{
    public SunCeJiangdongResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool moderateCourt = state.ImperialPower >= 35;

        // 1. 天子皇权强盛 (>=50)：天子手握真传国玉玺，顺水推舟敕封孙策为折冲校尉、领会稽太守，收纳江东巨额盐铁岁赋
        if (strongCourt)
        {
            return new SunCeJiangdongResult(
                SunCeJiangdongOutcome.RatifyAndCollectSaltIronTax,
                "【小霸王定江东 · 顺旨受封】孙策席卷江东六郡！天子加封折冲校尉收盐铁巨税！",
                "【拓土】兴平二年春，孙策率周瑜等渡江荡平刘繇、严白虎、王朗，尽并吴会六郡之地。因真传国玉玺稳在洛阳天子之手，孙策未受袁术僭号蛊惑，遣张纮奉表入洛阳称臣纳贡。天子顺势敕封孙策为折冲校尉、领会稽太守，命其按岁进奉江东盐铁税赋两千万，孙策感恩奉诏，江东归附！",
                ImperialPowerDelta: 8,
                TreasuryGoldDelta: 2000,
                SunCePowerDelta: 30,
                SunCeLoyaltyDelta: 25,
                YangzhouGovernorId: "sun_ce");
        }

        // 2. 朝廷中平 (35-49)：天子敕封官职，但保留刘繇等宗室名分牵制
        if (moderateCourt)
        {
            return new SunCeJiangdongResult(
                SunCeJiangdongOutcome.StrictImperialCommission,
                "【江东底定 · 朝廷羁縻】天子颁旨封孙策讨逆将军，保留宗室节制！",
                "【羁縻】孙策克定江东，上表请封。天子敕封孙策为讨逆将军，诏命江东按期进奉土贡一千万钱，孙策领命，江东局势粗定。",
                ImperialPowerDelta: 3,
                TreasuryGoldDelta: 1000,
                SunCePowerDelta: 25,
                SunCeLoyaltyDelta: 15,
                YangzhouGovernorId: "sun_ce");
        }

        // 3. 皇权微弱 (<35)：孙策割据江东，称霸东南，朝廷无力收税
        return new SunCeJiangdongResult(
            SunCeJiangdongOutcome.JiangdongAutonomousDefiance,
            "【东南鼎立 · 孙氏自雄】孙策据长江天险自雄！朝廷封赏难行！",
            "【割据】孙策据江东险阻，带甲数万，东南士民归附。朝廷虽加追认，然江东漕运税赋难通关西，孙氏割据已成气候！",
            ImperialPowerDelta: -5,
            TreasuryGoldDelta: 300,
            SunCePowerDelta: 35,
            SunCeLoyaltyDelta: -10,
            YangzhouGovernorId: "sun_ce");
    }
}
