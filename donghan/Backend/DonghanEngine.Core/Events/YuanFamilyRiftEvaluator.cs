using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 202 年 5 月袁绍病亡与袁氏诸子争立（天子大义分化二袁内斗）因果（单一职责）
/// </summary>
public sealed class YuanFamilyRiftEvaluator : IYuanFamilyRiftEvaluator
{
    public YuanFamilyRiftResult Evaluate(GameState state)
    {
        bool strongCourt = state.ImperialPower >= 50;
        bool moderateCourt = state.ImperialPower >= 35;

        // 1. 天子皇权强盛 (>=50)：天子分别册封袁谭、袁尚，以朝廷大义名分撺掇二袁内斗自残，河北精锐消耗殆尽，双方各纳贡金入洛阳
        if (strongCourt)
        {
            return new YuanFamilyRiftResult(
                YuanFamilyRiftOutcome.ImperialDivideAndConquer,
                "【二袁相残 · 帝策分化】大将军袁绍忧愤发病亡！天子分别册封二子以内耗河北！",
                "【分裂】建安七年夏五月，大将军袁绍在邺城忧愤发病呕血而亡。审配等立少子袁尚，长子袁谭不服引兵相攻。天子降明诏大展帝王心术：加封袁谭为青州刺史、袁尚为冀州刺史，命两子各领本部。二袁为争正统竞相遣使入洛阳输纳贡金两千万！河北骨肉相残，实力大损，皇权大振！",
                ImperialPowerDelta: 10,
                TreasuryGoldDelta: 2000,
                CaoCaoPowerDelta: 15,
                CaoCaoLoyaltyDelta: 20,
                YuanShaoDied: true);
        }

        // 2. 朝廷中平 (35-49)：曹操借机长驱直入平定河北
        if (moderateCourt)
        {
            return new YuanFamilyRiftResult(
                YuanFamilyRiftOutcome.CaoCaoSubduesHebeiDirect,
                "【河北崩溃 · 曹操克邺】袁绍病亡诸子内讧！曹操乘势北伐扫荡河北！",
                "【北伐】袁绍病死，诸子同室操戈。司空曹操奉诏北伐，连拔黎阳、邺城，二袁奔逃辽东。曹操收降河北降卒十万，上表献贡一千五百万，北方底定。",
                ImperialPowerDelta: 5,
                TreasuryGoldDelta: 1500,
                CaoCaoPowerDelta: 30,
                CaoCaoLoyaltyDelta: 15,
                YuanShaoDied: true);
        }

        // 3. 皇权微弱 (<35)：二袁虽隙但暂时休战
        return new YuanFamilyRiftResult(
            YuanFamilyRiftOutcome.YuanFamilyReconciles,
            "【河北虽乱 · 犹存余威】袁绍病故诸子分据！朝廷号令难加干涉！",
            "【相持】袁绍病殁，二袁分据青冀二州，朝廷诏命难行，北方局势依然动荡。",
            ImperialPowerDelta: -5,
            TreasuryGoldDelta: 0,
            CaoCaoPowerDelta: 10,
            CaoCaoLoyaltyDelta: -5,
            YuanShaoDied: true);
    }
}
