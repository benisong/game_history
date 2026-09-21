namespace DonghanEngine.Core.Events;

public interface ITongguanBattleEvaluator
{
    TongguanBattleResult Evaluate(GameState state);
}

public interface ITongguanBattleExecutor
{
    TurnResult Execute(GameState state, TongguanBattleResult result);
}
