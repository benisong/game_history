namespace DonghanEngine.Core.Events;

public interface ISunCeJiangdongEvaluator
{
    SunCeJiangdongResult Evaluate(GameState state);
}

public interface ISunCeJiangdongExecutor
{
    TurnResult Execute(GameState state, SunCeJiangdongResult result);
}
