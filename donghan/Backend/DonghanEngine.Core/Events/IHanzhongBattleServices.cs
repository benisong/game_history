namespace DonghanEngine.Core.Events;

public interface IHanzhongBattleEvaluator
{
    HanzhongBattleResult Evaluate(GameState state);
}

public interface IHanzhongBattleExecutor
{
    TurnResult Execute(GameState state, HanzhongBattleResult result);
}
