namespace DonghanEngine.Core.Events;

public interface IGuandongTributeEvaluator
{
    GuandongTributeResult Evaluate(GameState state);
}

public interface IGuandongTributeExecutor
{
    TurnResult Execute(GameState state, GuandongTributeResult result);
}
