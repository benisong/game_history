namespace DonghanEngine.Core.Events;

public interface IGrandJubileeEvaluator
{
    GrandJubileeResult Evaluate(GameState state);
}

public interface IGrandJubileeExecutor
{
    TurnResult Execute(GameState state, GrandJubileeResult result);
}
