namespace DonghanEngine.Core.Events;

public interface ICaoCaoSuccessionEvaluator
{
    CaoCaoSuccessionResult Evaluate(GameState state);
}

public interface ICaoCaoSuccessionExecutor
{
    TurnResult Execute(GameState state, CaoCaoSuccessionResult result);
}
