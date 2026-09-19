namespace DonghanEngine.Core.Events;

public interface IXuzhouSuccessionEvaluator
{
    XuzhouSuccessionResult Evaluate(GameState state);
}

public interface IXuzhouSuccessionExecutor
{
    TurnResult Execute(GameState state, XuzhouSuccessionResult result);
}
