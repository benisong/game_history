namespace DonghanEngine.Core.Events;

public interface IAugustCrisisEvaluator
{
    AugustCrisisResult Evaluate(GameState state);
}

public interface IAugustCrisisExecutor
{
    TurnResult Execute(GameState state, AugustCrisisResult result);
}
