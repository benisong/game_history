namespace DonghanEngine.Core.Events;

public interface IEmperorSuccessionCrisisEvaluator
{
    EmperorSuccessionCrisisResult Evaluate(GameState state);
}

public interface IEmperorSuccessionCrisisExecutor
{
    TurnResult Execute(GameState state, EmperorSuccessionCrisisResult result);
}
