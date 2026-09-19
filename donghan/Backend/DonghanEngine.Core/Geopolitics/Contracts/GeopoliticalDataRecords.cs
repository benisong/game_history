using System.Collections.Generic;
using DonghanEngine.Core.Geopolitics.Models;

namespace DonghanEngine.Core.Geopolitics.Contracts;

public sealed record WarlordDecision(
    WarlordActionType ActionType,
    string TargetFactionId,
    string TargetProvinceId,
    int CommittedTroops,
    int TributeAmount,
    string NarrativeIntent);

public sealed record GeopoliticalContext(
    int EmperorImperialPower,
    int WestGardenArmySize,
    int WestGardenMorale,
    IReadOnlyDictionary<string, WarlordFaction> AllFactions,
    IReadOnlyDictionary<string, Province> AllProvinces,
    FactionRelationGraph Relations);

public sealed record CampaignStepResult(
    string CampaignId,
    CampaignStatus Status,
    string AttackerFactionId,
    string DefenderFactionId,
    string TargetProvinceId,
    int AttackerCasualties,
    int DefenderCasualties,
    bool ProvinceConquered,
    string SummaryText);
