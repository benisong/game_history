namespace DonghanEngine.Core.Events;

public interface IChibiBattleEvaluator
{
    ChibiBattleResult Evaluate(GameState state);
}

public interface IChibiBattleExecutor
{
    TurnResult Execute(GameState state, ChibiBattleResult result);
}
