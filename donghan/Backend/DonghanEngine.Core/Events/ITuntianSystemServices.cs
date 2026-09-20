namespace DonghanEngine.Core.Events;

public interface ITuntianSystemEvaluator
{
    TuntianSystemResult Evaluate(GameState state);
}

public interface ITuntianSystemExecutor
{
    TurnResult Execute(GameState state, TuntianSystemResult result);
}
