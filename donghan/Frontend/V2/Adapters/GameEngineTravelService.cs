using System;
using DonghanEngine.Core;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

public sealed class GameEngineTravelService : ITravelService
{
    private readonly GameEngine _engine;
    private readonly IGameStateReader _state;

    public GameEngineTravelService(GameEngine engine, IGameStateReader state)
    {
        _engine = engine;
        _state = state;
    }

    public ActionResult Travel(TravelCommand command)
    {
        try
        {
            string before = _engine.GetState().CurrentLocation;
            _engine.TravelToLocation(command.Destination);
            return new ActionResult(
                true,
                "起驾奏报",
                $"陛下已由【{before}】移驾至【{command.Destination}】。",
                ReportKind.Travel,
                new[] { new StateChange("CurrentLocation", before, command.Destination) });
        }
        catch (Exception ex)
        {
            return ActionResult.Failure("起驾未成", ex.Message, ReportKind.Warning, ex.GetType().Name);
        }
    }
}
