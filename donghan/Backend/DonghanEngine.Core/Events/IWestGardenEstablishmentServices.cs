namespace DonghanEngine.Core.Events;

public interface IWestGardenEstablishmentEvaluator
{
    WestGardenEstablishmentResult Evaluate(GameState state);
}

public interface IWestGardenEstablishmentExecutor
{
    TurnResult Execute(GameState state, WestGardenEstablishmentResult result);
}
