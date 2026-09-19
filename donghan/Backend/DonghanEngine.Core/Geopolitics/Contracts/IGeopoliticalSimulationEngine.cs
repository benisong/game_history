using System.Collections.Generic;
using DonghanEngine.Core.Geopolitics.Contracts;
using DonghanEngine.Core.Geopolitics.Models;

namespace DonghanEngine.Core.Geopolitics.Contracts;

public sealed record GeopoliticalTickResult(
    IReadOnlyList<WarlordDecision> DecisionsMade,
    IReadOnlyList<CampaignStepResult> CampaignResults,
    IReadOnlyList<GeopoliticalMemorial> GeneratedMemorials,
    int TotalTributeGoldCollected,
    string NarrativeSummary);

public interface IGeopoliticalSimulationEngine
{
    IReadOnlyDictionary<string, WarlordFaction> Factions { get; }
    IReadOnlyList<MilitaryCampaign> ActiveCampaigns { get; }
    FactionRelationGraph Relations { get; }

    GeopoliticalTickResult TickTurn(GameState gameState);
    void RegisterFaction(WarlordFaction faction);
}
