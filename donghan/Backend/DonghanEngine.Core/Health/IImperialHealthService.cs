using System;
using DonghanEngine.Core;

namespace DonghanEngine.Core.Health;

public interface IImperialHealthService
{
    // 1. 每旬时序演进结算（阳气随时间自然缓慢恢复 + 旬末低于阈值永久扣减最大值 + 判定染疾与寿数折损）
    ImperialHealthDiagnosisReport AdvanceXunHealthSettlement(GameState state);

    // 2. 节点恢复精力（恢复量 = 当前剩余精力的40% * 阳气与名医系数）
    HealthActionResolutionResult RestAtWendePalace(GameState state);

    // 3. 后宫游幸调剂（刺激回精，但扣减阳气）
    HealthActionResolutionResult IndulgeInHarem(GameState state);

    // 4. 消耗精力执行政务（大朝会、巡幸、阅兵、抄家）
    void ConsumeEnergyForAffairs(GameState state, int cost, string affairName);

    // 5. 获取太医令当前请脉诊断
    ImperialHealthDiagnosisReport GetPhysicianDiagnosis(GameState state);
}
