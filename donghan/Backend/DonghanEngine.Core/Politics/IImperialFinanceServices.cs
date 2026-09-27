namespace DonghanEngine.Core.Politics;

public enum ConfiscationDestination
{
    NationalTreasury, // 归入太仓国库
    PrivateTreasury   // 归入西园天子私库
}

public interface IConfiscationService
{
    ConfiscationExecutionResult ConfiscateTarget(GameState state, string targetNpcId, ConfiscationDestination destination = ConfiscationDestination.NationalTreasury);
}

public interface IMilitaryPayrollService
{
    MilitaryPayrollResult ProcessPayroll(GameState state, bool grantExtraBonus = false);
}
