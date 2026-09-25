using DonghanEngine.Core.Balance;

namespace DonghanEngine.Core.Economy;

public sealed class BanditBalanceConfig : IBanditBalanceProvider
{
    public int AbsorbedTroopsPercent { get; set; } = 35; // 收编精壮比例 (35%)
    public int PowerGainDivisor { get; set; } = 200;      // 权势增长除数
    public int MinPowerGain { get; set; } = 5;            // 最低权势增量
}
