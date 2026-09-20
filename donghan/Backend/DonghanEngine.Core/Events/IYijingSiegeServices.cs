namespace DonghanEngine.Core.Events;

public interface IYijingSiegeEvaluator
{
    YijingSiegeResult Evaluate(GameState state);
}

public interface IYijingSiegeExecutor
{
    TurnResult Execute(GameState state, YijingSiegeResult result);
}
