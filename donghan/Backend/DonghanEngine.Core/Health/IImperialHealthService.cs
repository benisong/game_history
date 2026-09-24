using DonghanEngine.Core;

namespace DonghanEngine.Core.Health;

public interface IImperialHealthService
{
    // 1. 每旬结算（阳气每旬自然恢复 1-2 点）
    ImperialHealthDiagnosisReport AdvanceXunHealthSettlement(GameState state);

    // 2. 每月末（下旬/第3旬）统一精力结算与恢复（恢复当前剩余精力的 40%，受阳气折损制约；低谷扣减精力上限；判定生病与寿命）
    ImperialHealthDiagnosisReport AdvanceMonthlyEnergySettlement(GameState state);

    // 3. 临幸后宫（每次减少5点阳气；阳气80以上1点换5点精力，70以上换4点，70以下换3点）
    HealthActionResolutionResult IndulgeInHarem(GameState state);

    // 4. 温德殿静养（本旬息政，额外提前获得一次当月剩余精力 40% 的休整恢复）
    HealthActionResolutionResult RestAtWendePalace(GameState state);

    // 5. 政务消耗精力（大朝会、巡幸、阅兵、抄家等消耗 2~8 点精力）
    void ConsumeEnergyForAffairs(GameState state, int cost, string affairName);

    // 6. 获取太医令当前请脉诊断与精神状态
    ImperialHealthDiagnosisReport GetPhysicianDiagnosis(GameState state);
}
