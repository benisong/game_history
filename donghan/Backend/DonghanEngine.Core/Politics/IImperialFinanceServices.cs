namespace DonghanEngine.Core.Politics;

public interface IConfiscationService
{
    ConfiscationExecutionResult ConfiscateTarget(GameState state, string targetNpcId);
}

public interface IMilitaryPayrollService
{
    MilitaryPayrollResult ProcessPayroll(GameState state, bool grantExtraBonus = false);
}
