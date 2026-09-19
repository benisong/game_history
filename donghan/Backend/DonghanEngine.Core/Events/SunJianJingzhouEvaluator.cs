using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 191 年 10 月孙坚征荆州与跨江襄阳之战因果（单一职责）
/// </summary>
public sealed class SunJianJingzhouEvaluator : ISunJianJingzhouEvaluator
{
    public SunJianJingzhouResult Evaluate(GameState state)
    {
        bool strongCourtMediation = state.ImperialPower >= 50;
        bool sunJianLoyal = state.Npcs.TryGetValue("sun_jian", out var sj) && sj.IsActive && sj.Favorability >= 60;

        // 1. 天子皇权强盛 (>=50) 或 孙坚对朝廷忠诚 (>=60)：天子持节调停，制止同僚相残，双方各输纳贡金
        if (strongCourtMediation || sunJianLoyal)
        {
            return new SunJianJingzhouResult(
                SunJianJingzhouOutcome.ImperialMediationTruce,
                "【天子持节 · 罢兵息民】朝廷使节敕止跨江之战！孙坚退兵江东，荆襄归安！",
                "【调停】初平二年冬，破虏将军孙坚引兵跨江击荆州，天子闻讯遣使持节调停，严旨申诫同僚不得擅相攻伐。孙坚奉诏还军江东，刘表上表谢恩，两家各纳罢战贡金一千五百万充实国库！",
                ImperialPowerDelta: 8,
                TreasuryGoldDelta: 3000,
                SunJianPowerDelta: 5,
                SunJianLoyaltyDelta: 15,
                LiuBiaoPowerDelta: 5,
                LiuBiaoLoyaltyDelta: 20,
                SunJianDied: false);
        }

        // 2. 朝廷微弱且孙坚好感低：史实走向，孙坚轻进在岘山中暗箭身亡
        return new SunJianJingzhouResult(
            SunJianJingzhouOutcome.SunJianFallsAtXianshan,
            "【猛虎陨落 · 岘山折翼】孙坚追击黄祖中伏！岘山身中暗箭陨命！",
            "【战殇】孙坚围樊城击破黄祖，单骑追敌至岘山竹林，被黄祖部曲乱箭射中身亡。长子孙策年十七，收拢父部退保江东，荆襄大局暂定！",
            ImperialPowerDelta: -5,
            TreasuryGoldDelta: 0,
            SunJianPowerDelta: -50,
            SunJianLoyaltyDelta: 0,
            LiuBiaoPowerDelta: 15,
            LiuBiaoLoyaltyDelta: 10,
            SunJianDied: true);
    }
}
