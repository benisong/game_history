using System.Collections.Generic;
using System.Threading.Tasks;

namespace DonghanFrontend.V2.Contracts;

// ==========================================
// 状态读取接口（按功能单一隔离）
// ==========================================

public interface IGameStateSnapshotReader
{
    GameStateSnapshot GetSnapshot();
}

public interface IProvinceSnapshotReader
{
    ProvinceSnapshot? GetProvince(string provinceId);
    IReadOnlyList<ProvinceSnapshot> GetAllProvinces();
    IReadOnlyList<WarlordFactionSnapshot> GetAllFactions();
}

public interface IMinisterSnapshotReader
{
    IReadOnlyList<MinisterSnapshot> GetMinisters();
}

/// <summary>
/// 状态读取服务：由单方法接口组合
/// </summary>
public interface IGameStateReader : IGameStateSnapshotReader, IProvinceSnapshotReader, IMinisterSnapshotReader
{
}

// ==========================================
// 起驾巡幸服务（单方法接口）
// ==========================================

public interface ITravelService
{
    ActionResult Travel(TravelCommand command);
}

// ==========================================
// 西园军务服务（单方法接口隔离）
// ==========================================

public interface IPayArmyService
{
    ActionResult PayArmy(ArmyPayCommand command);
}

public interface IDrillArmyService
{
    ActionResult DrillArmy(ArmyDrillCommand command);
}

public interface IRecruitArmyService
{
    ActionResult RecruitArmy(RecruitArmyCommand command);
}

public interface IWestGardenService : IPayArmyService, IDrillArmyService, IRecruitArmyService
{
}

// ==========================================
// 黄门密札与州郡情报服务（单方法接口隔离）
// ==========================================

public interface IInspectProvinceService
{
    ProvinceIntelResult InspectProvince(InspectProvinceCommand command);
}

public interface IExecuteProvinceActionService
{
    ActionResult ExecuteProvinceAction(ProvinceActionCommand command);
}

public interface IIntelService : IInspectProvinceService, IExecuteProvinceActionService
{
}

// ==========================================
// 宣政殿朝会服务（单方法接口隔离）
// ==========================================

public interface IStartCourtSessionService
{
    Task<string> StartSessionAsync();
}

public interface IExecuteCourtDecisionService
{
    Task<ActionResult> ExecuteDecisionAsync(CourtDecisionCommand command);
}

public interface IExecuteFreeEdictService
{
    Task<ActionResult> ExecuteFreeEdictAsync(FreeEdictCommand command);
}

public interface ICourtService : IStartCourtSessionService, IExecuteCourtDecisionService, IExecuteFreeEdictService
{
}

// ==========================================
// 时序演进服务（单方法接口隔离）
// ==========================================

public interface IAdvanceXunService
{
    Task<TurnAdvanceResult> AdvanceXunAsync();
}

public interface IFastForwardService
{
    Task<FastForwardResult> FastForwardAsync(FastForwardCommand command);
}

public interface ITurnService : IAdvanceXunService, IFastForwardService
{
}

// ==========================================
// 尚书台折匣服务（单方法接口隔离）
// ==========================================

public interface IGetPendingEdictsService
{
    IReadOnlyList<EdictSnapshot> GetPendingEdicts();
}

public interface IResolveEdictService
{
    ActionResult Resolve(ResolveEdictCommand command);
}

public interface INominationUiService
{
    IReadOnlyList<DonghanEngine.Core.Politics.NominationCandidate> GetPendingNominations();
    ActionResult Appoint(DonghanEngine.Core.Politics.NominationCandidate candidate, string officeTitle);
    ActionResult Reject(DonghanEngine.Core.Politics.NominationCandidate candidate);
}

public interface IOfficialRankUiService
{
    IReadOnlyList<DonghanEngine.Core.Politics.OfficialPosition> GetAllPositions();
    ActionResult Promote(string npcId, string targetTitle);
    ActionResult SellOffice(string buyerNpcId, string targetTitle);
}

public interface IAgriculturalPolicyUiService
{
    ActionResult SurveyLand(string provinceId, DonghanEngine.Core.Economy.CadastralSurveyIntensity intensity);
    ActionResult BuildIrrigation(string provinceId);
    ActionResult RepurchaseGentryLand(string provinceId, int purchaseAmount);
}

public interface IGovernorAppraisalUiService
{
    DonghanEngine.Core.Politics.AnnualAppraisalReport GetAnnualAppraisal();
    ActionResult PromoteGovernorToCourt(string governorId, string targetCourtTitle);
}

public interface IImperialHealthUiService
{
    DonghanEngine.Core.Health.ImperialHealthDiagnosisReport GetPhysicianDiagnosis();
    ActionResult RestAtWendePalace();
    ActionResult IndulgeInHarem();
}

public interface IEdictService : IGetPendingEdictsService, IResolveEdictService
{
}

// ==========================================
// 特殊政务服务（单方法接口）
// ==========================================

public interface ISpecialActionService
{
    ActionResult Execute(SpecialActionCommand command);
}

// ==========================================
// 表现与导航接口
// ==========================================

public interface IReportPresenter
{
    void Show(ActionResult result);
}

public interface ISceneNavigator
{
    void OpenCourt();
    void OpenIntel();
    void OpenWestGarden();
    void OpenTravel();
    void CloseCurrent();
}
