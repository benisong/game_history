namespace DonghanEngine.Core.Events;

public interface ILiuBeiYizhouEvaluator
{
    LiuBeiYizhouResult Evaluate(GameState state);
}

public interface ILiuBeiYizhouExecutor
{
    TurnResult Execute(GameState state, LiuBeiYizhouResult result);
}
