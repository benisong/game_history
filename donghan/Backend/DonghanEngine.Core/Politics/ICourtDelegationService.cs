using System.Collections.Generic;
using DonghanEngine.Core;

namespace DonghanEngine.Core.Politics;

public interface ICourtDelegationService
{
    // 获取当前待办政务清单
    IReadOnlyList<DelegationAffairItem> GetPendingAffairs(GameState state);

    // 天子亲裁单项待办（消耗全额精力）
    AffairExecutionReport ExecuteDirectAffair(GameState state, string affairId);

    // 廷议分权代办：将剩余所有待办“一揽子交办给群僚”，仅消耗一次固定精力 (默认 2 点)
    BatchDelegationResult ExecuteBatchDelegation(GameState state, IReadOnlyList<string>? affairIds = null);

    // 允许外部动态调整/加载平衡配置（完全不写死）
    void UpdateConfig(DonghanEngine.Core.Balance.IDelegationBalanceProvider config);
    DonghanEngine.Core.Balance.IDelegationBalanceProvider GetConfig();
}
