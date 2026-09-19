namespace DonghanEngine.Core.Events;

public interface IPacificationBattleEvaluator
{
    PacificationBattleResult Evaluate(GameState state);
}

public interface IPacificationBattleExecutor
{
    TurnResult Execute(GameState state, PacificationBattleResult battleResult);
}
