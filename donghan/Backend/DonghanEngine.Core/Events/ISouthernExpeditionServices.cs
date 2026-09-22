namespace DonghanEngine.Core.Events;

public interface ISouthernExpeditionEvaluator
{
    SouthernExpeditionResult Evaluate(GameState state);
}

public interface ISouthernExpeditionExecutor
{
    TurnResult Execute(GameState state, SouthernExpeditionResult result);
}
