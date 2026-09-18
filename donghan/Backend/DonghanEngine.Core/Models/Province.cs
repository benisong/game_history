using System;
using System.Collections.Generic;

namespace DonghanEngine.Core;

public class Province
{
    private int _localSupport = 30;
    private int _wealth = 2000;
    private int _garrison = 2000;
    private int _defenseLevel = 30;
    private int _distance = 2;
    private int _rebellionMonths = 0;
    private int _lowSupportStreakMonths = 0;
    private List<string> _neighbors = new();

    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? GovernorId { get; set; } = null;
    
    public int Distance
    {
        get => _distance;
        set => _distance = Math.Max(0, value);
    }
    
    public IReadOnlyList<string> Neighbors
    {
        get => _neighbors;
        init => _neighbors = value != null ? new List<string>(value) : new List<string>();
    }

    public void AddNeighbor(string neighborId)
    {
        if (!string.IsNullOrWhiteSpace(neighborId) && !_neighbors.Contains(neighborId))
            _neighbors.Add(neighborId);
    }

    public bool IsRebelling { get; set; } = false;
    
    public int RebellionMonths
    {
        get => _rebellionMonths;
        set => _rebellionMonths = Math.Max(0, value);
    }
    
    public string RebelFaction { get; set; } = string.Empty;

    public int LocalSupport
    {
        get => _localSupport;
        set => _localSupport = Math.Clamp(value, 0, 100);
    }
    
    public int Wealth
    {
        get => _wealth;
        set => _wealth = Math.Max(0, value);
    }
    
    public int Garrison
    {
        get => _garrison;
        set => _garrison = Math.Max(0, value);
    }
    
    public int DefenseLevel
    {
        get => _defenseLevel;
        set => _defenseLevel = Math.Clamp(value, 0, 100);
    }

    public int LowSupportStreakMonths
    {
        get => _lowSupportStreakMonths;
        set => _lowSupportStreakMonths = Math.Max(0, value);
    }

    // === 充血业务方法 ===
    public void AdjustLocalSupport(int delta) => LocalSupport += delta;
    public void AdjustWealth(int delta) => Wealth = Math.Max(0, Wealth + delta);
    public void AdjustGarrison(int delta) => Garrison = Math.Max(0, Garrison + delta);
    public void AdjustDefenseLevel(int delta) => DefenseLevel += delta;

    public void StartRebellion(string faction, int initialLocalSupport = 5, int garrisonMultiplier = 2)
    {
        IsRebelling = true;
        RebelFaction = faction;
        RebellionMonths = 0;
        LocalSupport = initialLocalSupport;
        Garrison = Math.Clamp(Garrison * garrisonMultiplier, 0, 20000);
    }

    public void SuppressRebellion(int supportRecovery = 15, int newGarrison = 1000)
    {
        IsRebelling = false;
        RebelFaction = string.Empty;
        RebellionMonths = 0;
        LocalSupport += supportRecovery;
        Garrison = Math.Clamp(newGarrison, 500, 20000);
    }

    public void PacifyRebellion(int supportRecovery = 20)
    {
        IsRebelling = false;
        RebelFaction = string.Empty;
        RebellionMonths = 0;
        LocalSupport += supportRecovery;
    }

    public void AppointGovernor(string governorId, int supportBonus = 10)
    {
        GovernorId = governorId;
        LocalSupport += supportBonus;
    }

    public void RecallGovernor()
    {
        GovernorId = null;
    }

    public void IncrementRebellionMonth() => RebellionMonths++;
    public void IncrementLowSupportStreak() => LowSupportStreakMonths++;
    public void ResetLowSupportStreak() => LowSupportStreakMonths = 0;
}
