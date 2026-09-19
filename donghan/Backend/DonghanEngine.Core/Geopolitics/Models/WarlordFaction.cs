using System;
using System.Collections.Generic;

namespace DonghanEngine.Core.Geopolitics.Models;

/// <summary>
/// 诸侯势力充血实体：管理麾下领地集合、兵力、军粮、对天子忠诚与扩张野心（防篡改封装）
/// </summary>
public sealed class WarlordFaction
{
    public string FactionId { get; }
    public string LeaderNpcId { get; private set; }
    public string FactionName { get; }
    public WarlordPosture Posture { get; private set; }

    private readonly HashSet<string> _controlledProvinces = new();
    public IReadOnlyCollection<string> ControlledProvinces => _controlledProvinces;

    public int TotalTroops { get; private set; }
    public int Provisions { get; private set; }
    public int ImperialLoyalty { get; private set; } // 0 - 100
    public int ExpansionDesire { get; private set; }  // 0 - 100

    public WarlordFaction(
        string factionId,
        string leaderNpcId,
        string factionName,
        WarlordPosture posture,
        IEnumerable<string> initialProvinces,
        int initialTroops,
        int initialProvisions,
        int initialLoyalty,
        int initialAmbition)
    {
        FactionId = string.IsNullOrWhiteSpace(factionId) ? throw new ArgumentNullException(nameof(factionId)) : factionId;
        LeaderNpcId = string.IsNullOrWhiteSpace(leaderNpcId) ? throw new ArgumentNullException(nameof(leaderNpcId)) : leaderNpcId;
        FactionName = string.IsNullOrWhiteSpace(factionName) ? "地方部曲" : factionName;
        Posture = posture;

        if (initialProvinces != null)
        {
            foreach (var p in initialProvinces)
            {
                if (!string.IsNullOrWhiteSpace(p)) _controlledProvinces.Add(p);
            }
        }

        TotalTroops = Math.Clamp(initialTroops, 0, 500000);
        Provisions = Math.Clamp(initialProvisions, 0, 999999);
        ImperialLoyalty = Math.Clamp(initialLoyalty, 0, 100);
        ExpansionDesire = Math.Clamp(initialAmbition, 0, 100);
    }

    public void AnnexProvince(string provinceId)
    {
        if (string.IsNullOrWhiteSpace(provinceId)) return;
        _controlledProvinces.Add(provinceId);
    }

    public void CedeProvince(string provinceId)
    {
        if (string.IsNullOrWhiteSpace(provinceId)) return;
        _controlledProvinces.Remove(provinceId);
    }

    public void MobilizeTroops(int troopDelta)
    {
        TotalTroops = Math.Clamp(TotalTroops + troopDelta, 0, 500000);
    }

    public void AdjustProvisions(int provisionDelta)
    {
        Provisions = Math.Clamp(Provisions + provisionDelta, 0, 999999);
    }

    public void AdjustLoyalty(int delta)
    {
        ImperialLoyalty = Math.Clamp(ImperialLoyalty + delta, 0, 100);
    }

    public void AdjustAmbition(int delta)
    {
        ExpansionDesire = Math.Clamp(ExpansionDesire + delta, 0, 100);
    }

    public void ChangePosture(WarlordPosture newPosture)
    {
        Posture = newPosture;
    }
}
