namespace DonghanEngine.Core.Events;

public interface ICaoCaoYanzhouEvaluator
{
    CaoCaoYanzhouResult Evaluate(GameState state);
}

public interface ICaoCaoYanzhouExecutor
{
    TurnResult Execute(GameState state, CaoCaoYanzhouResult result);
}
