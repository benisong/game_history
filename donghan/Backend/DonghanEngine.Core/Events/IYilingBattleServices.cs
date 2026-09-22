namespace DonghanEngine.Core.Events;

public interface IYilingBattleEvaluator
{
    YilingBattleResult Evaluate(GameState state);
}

public interface IYilingBattleExecutor
{
    TurnResult Execute(GameState state, YilingBattleResult result);
}
