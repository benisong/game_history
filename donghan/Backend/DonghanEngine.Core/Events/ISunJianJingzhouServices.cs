namespace DonghanEngine.Core.Events;

public interface ISunJianJingzhouEvaluator
{
    SunJianJingzhouResult Evaluate(GameState state);
}

public interface ISunJianJingzhouExecutor
{
    TurnResult Execute(GameState state, SunJianJingzhouResult result);
}
