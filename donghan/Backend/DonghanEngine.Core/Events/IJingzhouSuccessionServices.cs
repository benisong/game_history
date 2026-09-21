namespace DonghanEngine.Core.Events;

public interface IJingzhouSuccessionEvaluator
{
    JingzhouSuccessionResult Evaluate(GameState state);
}

public interface IJingzhouSuccessionExecutor
{
    TurnResult Execute(GameState state, JingzhouSuccessionResult result);
}
