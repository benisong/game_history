using System.Collections.Generic;
using System.Threading.Tasks;

namespace DonghanFrontend.V2.Contracts;

public interface IGameStateReader
{
    GameStateSnapshot GetSnapshot();
    ProvinceSnapshot? GetProvince(string provinceId);
    IReadOnlyList<MinisterSnapshot> GetMinisters();
}

public interface ITravelService
{
    ActionResult Travel(TravelCommand command);
}

public interface IWestGardenService
{
    ActionResult PayArmy(ArmyPayCommand command);
    ActionResult DrillArmy(ArmyDrillCommand command);
    ActionResult RecruitArmy(RecruitArmyCommand command);
}

public interface IIntelService
{
    ProvinceIntelResult InspectProvince(InspectProvinceCommand command);
    ActionResult ExecuteProvinceAction(ProvinceActionCommand command);
}

public interface ICourtService
{
    Task<string> StartSessionAsync();
    Task<ActionResult> ExecuteDecisionAsync(CourtDecisionCommand command);
}

public interface ITurnService
{
    Task<TurnAdvanceResult> AdvanceXunAsync();
    Task<FastForwardResult> FastForwardAsync(FastForwardCommand command);
}

public interface IEdictService
{
    IReadOnlyList<EdictSnapshot> GetPendingEdicts();
    ActionResult Resolve(ResolveEdictCommand command);
}

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
