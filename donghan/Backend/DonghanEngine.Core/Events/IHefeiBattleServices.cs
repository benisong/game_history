namespace DonghanEngine.Core.Events;

public interface IHefeiBattleEvaluator
{
    HefeiBattleResult Evaluate(GameState state);
}

public interface IHefeiBattleExecutor
{
    TurnResult Execute(GameState state, HefeiBattleResult result);
}
