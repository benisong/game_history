namespace DonghanEngine.Core.Events;

public interface IYuanFamilyRiftEvaluator
{
    YuanFamilyRiftResult Evaluate(GameState state);
}

public interface IYuanFamilyRiftExecutor
{
    TurnResult Execute(GameState state, YuanFamilyRiftResult result);
}
