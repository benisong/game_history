namespace DonghanEngine.Core.Events;

public interface IXiangfanBattleEvaluator
{
    XiangfanBattleResult Evaluate(GameState state);
}

public interface IXiangfanBattleExecutor
{
    TurnResult Execute(GameState state, XiangfanBattleResult result);
}
