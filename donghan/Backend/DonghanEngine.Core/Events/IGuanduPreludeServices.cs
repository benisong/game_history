namespace DonghanEngine.Core.Events;

public interface IGuanduPreludeEvaluator
{
    GuanduPreludeResult Evaluate(GameState state);
}

public interface IGuanduPreludeExecutor
{
    TurnResult Execute(GameState state, GuanduPreludeResult result);
}
