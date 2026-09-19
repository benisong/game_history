using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 184 年 11 月凉州韩遂边章之叛与西凉边防因果（单一职责）
/// </summary>
public sealed class LiangzhouRebellionEvaluator : ILiangzhouRebellionEvaluator
{
    public LiangzhouRebellionResult Evaluate(GameState state)
    {
        if (!state.Provinces.TryGetValue("liangzhou", out var liangzhou))
        {
            return new LiangzhouRebellionResult(
                LiangzhouRebellionOutcome.HistoricalUprising,
                "【西北烽烟】凉州羌胡与韩遂起兵！",
                "【边乱】凉州羌胡与贼帅韩遂起事，西北震动！",
                ImperialPowerDelta: -5,
                PopularSupportDelta: -5,
                LiangzhouRebelling: true,
                DeployDongZhuo: true,
                DeployMaTeng: true);
        }

        // 1. 若国库极度空虚 (< 2000) 且 凉州民心极低 (< 20)：守军哗变与羌胡合流
        if (state.Treasury < 2000 && liangzhou.LocalSupport < 20)
        {
            return new LiangzhouRebellionResult(
                LiangzhouRebellionOutcome.GarrisonSurrendered,
                "【大乱】凉州兵变！边军哗变与羌胡合流！",
                "【兵变】朝廷积欠西凉军饷，凉州守军与韩遂、北宫伯玉合流，尽杀长吏，兵逼三辅长安！",
                ImperialPowerDelta: -10,
                PopularSupportDelta: -10,
                LiangzhouRebelling: true,
                DeployDongZhuo: true,
                DeployMaTeng: true);
        }

        // 2. 若玩家提前对凉州重点布防 (民心 >= 40 且 守军 >= 5000)：边防稳固，叛乱被阻于关外
        if (liangzhou.LocalSupport >= 40 && liangzhou.Garrison >= 5000)
        {
            return new LiangzhouRebellionResult(
                LiangzhouRebellionOutcome.FrontierContained,
                "【边报】羌胡骚动犯金城，西凉守军据险击退！",
                "【克捷】北宫伯玉、韩遂勾结羌胡入寇，得益于天子早饬边备，凉州守军严阵以待，将敌阻于塞外！",
                ImperialPowerDelta: 5,
                PopularSupportDelta: 5,
                LiangzhouRebelling: false,
                DeployDongZhuo: false,
                DeployMaTeng: true); // 马腾作为忠义边将登台
        }

        // 3. 史实路线：韩遂边章攻陷金城，凉州全境叛乱，董卓马腾作为边军将领登庸参战
        return new LiangzhouRebellionResult(
            LiangzhouRebellionOutcome.HistoricalUprising,
            "【西北烽烟】北宫伯玉与韩遂破金城！凉州大乱！",
            "【边警】湟中义从胡反叛，韩遂、边章入寇杀护羌校尉冷征，凉州全境沦陷，天下侧目！",
            ImperialPowerDelta: -5,
            PopularSupportDelta: -5,
            LiangzhouRebelling: true,
            DeployDongZhuo: true,
            DeployMaTeng: true);
    }
}
