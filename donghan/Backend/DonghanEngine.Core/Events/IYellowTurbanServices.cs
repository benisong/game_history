namespace DonghanEngine.Core.Events;

public interface IYellowTurbanScopeEvaluator
{
    YellowTurbanScopeResult EvaluateScope(GameState state);
}

public interface IYellowTurbanOutbreakExecutor
{
    TurnResult ExecuteOutbreak(GameState state, YellowTurbanScopeResult scope);
}
