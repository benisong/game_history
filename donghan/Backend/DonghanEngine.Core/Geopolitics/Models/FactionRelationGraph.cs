using System;
using System.Collections.Generic;

namespace DonghanEngine.Core.Geopolitics.Models;

/// <summary>
/// 诸侯多边关系矩阵：维护各势力之间的敌对度、忌惮度与朝廷调停休战令（防篡改封装）
/// </summary>
public sealed class FactionRelationGraph
{
    private readonly Dictionary<(string, string), int> _hostilityMap = new();
    private readonly HashSet<(string, string)> _trucePairs = new();

    public int GetHostility(string fromFaction, string toFaction)
    {
        if (fromFaction == toFaction) return 0;
        return _hostilityMap.TryGetValue((fromFaction, toFaction), out var h) ? h : 0;
    }

    public void SetHostility(string fromFaction, string toFaction, int hostility)
    {
        if (fromFaction == toFaction) return;
        _hostilityMap[(fromFaction, toFaction)] = Math.Clamp(hostility, 0, 100);
    }

    public void AdjustHostility(string fromFaction, string toFaction, int delta)
    {
        if (fromFaction == toFaction) return;
        int current = GetHostility(fromFaction, toFaction);
        _hostilityMap[(fromFaction, toFaction)] = Math.Clamp(current + delta, 0, 100);
    }

    public bool HasTruce(string factionA, string factionB)
    {
        return _trucePairs.Contains((factionA, factionB)) || _trucePairs.Contains((factionB, factionA));
    }

    public void EstablishTruce(string factionA, string factionB)
    {
        _trucePairs.Add((factionA, factionB));
        _trucePairs.Add((factionB, factionA));
    }

    public void BreakTruce(string factionA, string factionB)
    {
        _trucePairs.Remove((factionA, factionB));
        _trucePairs.Remove((factionB, factionA));
    }
}
