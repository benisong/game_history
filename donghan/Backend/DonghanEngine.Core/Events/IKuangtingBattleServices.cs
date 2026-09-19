namespace DonghanEngine.Core.Events;

public interface IKuangtingBattleEvaluator
{
    KuangtingBattleResult Evaluate(GameState state);
}

public interface IKuangtingBattleExecutor
{
    TurnResult Execute(GameState state, KuangtingBattleResult result);
}
