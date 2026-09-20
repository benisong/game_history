namespace DonghanEngine.Core.Events;

public interface ILvBuYanzhouEvaluator
{
    LvBuYanzhouResult Evaluate(GameState state);
}

public interface ILvBuYanzhouExecutor
{
    TurnResult Execute(GameState state, LvBuYanzhouResult result);
}
