namespace DonghanEngine.Core.Events;

public interface IPalaceFireEvaluator
{
    PalaceFireResult Evaluate(GameState state);
}

public interface IPalaceFireExecutor
{
    TurnResult Execute(GameState state, PalaceFireResult result);
}
