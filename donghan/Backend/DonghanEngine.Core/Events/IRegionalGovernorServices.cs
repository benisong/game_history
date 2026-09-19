namespace DonghanEngine.Core.Events;

public interface IRegionalGovernorEvaluator
{
    RegionalGovernorResult Evaluate(GameState state);
}

public interface IRegionalGovernorExecutor
{
    TurnResult Execute(GameState state, RegionalGovernorResult result);
}
