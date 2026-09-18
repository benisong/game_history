using System.Threading.Tasks;

namespace DonghanEngine.Core;

// 天灾/后宫/健康随机事件数据结构
public class OracleEvent
{
    public string EventName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ImperialPowerChange { get; set; } = 0;
    public int TreasuryChange { get; set; } = 0;
    public int HealthChange { get; set; } = 0;
}

public interface IEventOracle
{
    Task<OracleEvent?> CheckRandomEventAsync(GameState state);
}
