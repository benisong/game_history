namespace DonghanEngine.Core.Events;

public interface IBaimenlouEvaluator
{
    BaimenlouResult Evaluate(GameState state);
}

public interface IBaimenlouExecutor
{
    TurnResult Execute(GameState state, BaimenlouResult result);
}
