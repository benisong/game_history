namespace DonghanEngine.Core.Events;

public interface IBailangMountainEvaluator
{
    BailangMountainResult Evaluate(GameState state);
}

public interface IBailangMountainExecutor
{
    TurnResult Execute(GameState state, BailangMountainResult result);
}
