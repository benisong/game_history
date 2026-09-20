namespace DonghanEngine.Core.Events;

public interface IYuanShuUsurpationEvaluator
{
    YuanShuUsurpationResult Evaluate(GameState state);
}

public interface IYuanShuUsurpationExecutor
{
    TurnResult Execute(GameState state, YuanShuUsurpationResult result);
}
