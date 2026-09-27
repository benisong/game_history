using DonghanEngine.Core;
using DonghanEngine.Core.Settlement;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

/// <summary>
/// 纯 UI 适配层：将旬末起居注结算服务包装给前端调用（单一职责）
/// </summary>
public sealed class GameEngineTurnSettlementUiService : ITurnSettlementUiService
{
    private readonly IGameStateProvider _stateProvider;
    private readonly ITurnSettlementSlideService _slideService;

    public GameEngineTurnSettlementUiService(
        IGameStateProvider stateProvider,
        ITurnSettlementSlideService? slideService = null)
    {
        _stateProvider = stateProvider;
        _slideService = slideService ?? new TurnSettlementSlideService();
    }

    public TurnSettlementPackage CompileSettlementPackage()
    {
        return _slideService.CompileTurnSettlementPackage(_stateProvider.GetState());
    }
}
