namespace DonghanEngine.Core.Events;

public interface IGuanduVictoryEvaluator
{
    GuanduVictoryResult Evaluate(GameState state);
}

public interface IGuanduVictoryExecutor
{
    TurnResult Execute(GameState state, GuanduVictoryResult result);
}
