namespace DonghanEngine.Core.Events;

public interface IYuanShaoJizhouEvaluator
{
    YuanShaoJizhouResult Evaluate(GameState state);
}

public interface IYuanShaoJizhouExecutor
{
    TurnResult Execute(GameState state, YuanShaoJizhouResult result);
}
