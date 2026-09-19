namespace DonghanEngine.Core.Events;

public interface IGuanxiDefenseEvaluator
{
    GuanxiDefenseResult Evaluate(GameState state);
}

public interface IGuanxiDefenseExecutor
{
    TurnResult Execute(GameState state, GuanxiDefenseResult result);
}
