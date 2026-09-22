namespace DonghanEngine.Core.Events;

public interface IBaidiEntrustmentEvaluator
{
    BaidiEntrustmentResult Evaluate(GameState state);
}

public interface IBaidiEntrustmentExecutor
{
    TurnResult Execute(GameState state, BaidiEntrustmentResult result);
}
