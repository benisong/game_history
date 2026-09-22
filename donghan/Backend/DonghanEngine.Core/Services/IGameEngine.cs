using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DonghanEngine.Core;

// ==========================================
// 后端领域服务细粒度接口（单方法/单一职责）
// ==========================================

public interface ITravelDomainService
{
    void TravelToLocation(string newLocation);
}

public interface IDrillArmyDomainService
{
    TurnResult ExecuteDrillArmyActionWithOfficer(int paidAmount, string officerId);
}

public interface IRecruitArmyDomainService
{
    TurnResult ExecuteRaiseWestGardenTroopsAction(int troops);
}

public interface IDisasterReliefDomainService
{
    TurnResult ExecuteDisasterReliefAction(int reliefAmount, string officerId);
}

public interface IConfiscationDomainService
{
    TurnResult ExecuteConfiscationAction(string targetMinisterId, string destination);
    DonghanEngine.Core.Politics.ConfiscationExecutionResult ExecuteConfiscateTarget(string targetNpcId);
}

public interface IMilitaryPayrollDomainService
{
    DonghanEngine.Core.Politics.MilitaryPayrollResult ExecuteGrantMilitaryBonus();
}

public interface ITalentNominationDomainService
{
    IReadOnlyList<DonghanEngine.Core.Politics.NominationCandidate> GetPendingNominations();
    DonghanEngine.Core.Politics.NominationResolutionResult AppointNominationCandidate(DonghanEngine.Core.Politics.NominationCandidate candidate, string officeTitle);
    DonghanEngine.Core.Politics.NominationResolutionResult RejectNominationCandidate(DonghanEngine.Core.Politics.NominationCandidate candidate);
}

public interface IQuickActionDomainService
{
    TurnResult ExecuteQuickAction(string actionId);
}

public interface IResolveEdictDomainService
{
    TurnResult ResolveEdictAction(string edictId, int optionIndex);
}

public interface IGrandCourtDomainService
{
    string ActiveOfficerId { get; set; }
    string StartGrandCourtSync();
    Task TriggerCourtDebateAsync(string playerInput, string activeOfficerId);
    Task<TurnResult> ProcessPlayerTurnAsync(string playerInput);
}

public interface IProvinceGovernanceDomainService
{
    TurnResult SuppressRebellion(string provinceId, string generalId);
    TurnResult SuppressRebellion(string provinceId, string generalId, int troops);
    TurnResult PacifyRebellion(string provinceId, string envoyId, GameEngine.PacifyStrategy strategies, int reliefGold = 0);
    TurnResult AssignGovernor(string provinceId, string npcId);
    TurnResult RecallGovernor(string provinceId);
    string GetProvinceReport();
}

public interface ITurnAdvanceDomainService
{
    Task NextXunAsync();
}

public interface IGameStateProvider
{
    GameState GetState();
}

public interface IGeopoliticalDomainService
{
    DonghanEngine.Core.Geopolitics.Contracts.IGeopoliticalSimulationEngine GeopoliticsEngine { get; }
    DonghanEngine.Core.Geopolitics.Contracts.EdictExecutionResult ResolveGeopoliticalMemorial(string memorialId, string optionId);
}

/// <summary>
/// 聚合核心引擎接口：完全由单职责领域接口组合而成
/// </summary>
public interface IGameEngine :
    IGameStateProvider,
    IGeopoliticalDomainService,
    ITravelDomainService,
    IDrillArmyDomainService,
    IRecruitArmyDomainService,
    IDisasterReliefDomainService,
    IConfiscationDomainService,
    IMilitaryPayrollDomainService,
    ITalentNominationDomainService,
    IQuickActionDomainService,
    IResolveEdictDomainService,
    IGrandCourtDomainService,
    IProvinceGovernanceDomainService,
    ITurnAdvanceDomainService
{
}
