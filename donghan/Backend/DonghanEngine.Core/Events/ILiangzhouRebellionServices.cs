namespace DonghanEngine.Core.Events;

public interface ILiangzhouRebellionEvaluator
{
    LiangzhouRebellionResult Evaluate(GameState state);
}

public interface ILiangzhouRebellionExecutor
{
    TurnResult Execute(GameState state, LiangzhouRebellionResult result);
}
