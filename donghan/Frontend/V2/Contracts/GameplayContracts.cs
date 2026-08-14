using System;
using System.Collections.Generic;

namespace DonghanFrontend.V2.Contracts;

public enum ReportKind
{
    Information,
    Court,
    Intel,
    WestGarden,
    Travel,
    Warning
}

public enum ProvinceActionKind
{
    RecallGovernor,
    AssignGovernor,
    SuppressRebellion,
    PacifyRebellion
}

public sealed record StateChange(string Key, string Before, string After);

public sealed record ActionResult(
    bool Success,
    string Title,
    string StoryText,
    ReportKind Kind,
    IReadOnlyList<StateChange> Changes,
    string? ErrorCode = null)
{
    public static ActionResult Failure(string title, string message, ReportKind kind = ReportKind.Warning, string errorCode = "action_failed") =>
        new(false, title, message, kind, Array.Empty<StateChange>(), errorCode);
}

public sealed record GameStateSnapshot(
    string ReignTitle,
    int ReignYear,
    int Year,
    int Month,
    int Xun,
    string CurrentLocation,
    int ImperialPower,
    int Treasury,
    int PrivateTreasury,
    int PopularSupport,
    int Health,
    int WestGardenArmySize,
    int WestGardenArmyCapacity,
    int WestGardenMorale,
    int WestGardenLoyalty,
    string Outcome,
    IReadOnlyList<string> Chronicle);

public sealed record ProvinceSnapshot(
    string Id,
    string Name,
    bool IsRebelling,
    string? RebelFaction,
    int RebellionMonths,
    int LocalSupport,
    int Garrison,
    int Wealth,
    int DefenseLevel,
    int Distance,
    string? GovernorId,
    string GovernorName);

public sealed record MinisterSnapshot(
    string Id,
    string Name,
    string Title,
    string Faction,
    int Favorability,
    int Power,
    int Corruption,
    bool IsActive,
    bool IsHostile);

public sealed record TravelCommand(string Destination);
public sealed record ArmyPayCommand(int Amount, string OfficerId);
public sealed record ArmyDrillCommand(int Amount, string OfficerId);
public sealed record RecruitArmyCommand(int Troops);
public sealed record InspectProvinceCommand(string ProvinceId);
public sealed record ProvinceActionCommand(
    string ProvinceId,
    ProvinceActionKind Action,
    string? OfficerId = null,
    int Troops = 0,
    int ReliefGold = 0,
    string? Strategy = null);
public sealed record CourtDecisionCommand(string TopicId, string DecisionId, string? ActiveOfficerId = null);
public sealed record FreeEdictCommand(string PlayerInput, string ActiveOfficerId);
public sealed record SpecialActionCommand(string ActionId, int Amount = 0, string OfficerId = "", string TargetNpcId = "", string Destination = "");
public sealed record FastForwardCommand(int XunCount);
public sealed record EdictOptionSnapshot(string Description, string ConsequencePreview);
public sealed record EdictSnapshot(string Id, string Title, string Type, string NarrativeContent, int ExpiryXun, IReadOnlyList<EdictOptionSnapshot> Options);
public sealed record ResolveEdictCommand(string EdictId, int OptionIndex);

public sealed record ProvinceIntelResult(
    bool Success,
    ProvinceSnapshot? Province,
    string? ErrorCode = null,
    string? ErrorMessage = null);

public sealed record TurnAdvanceResult(
    bool Success,
    GameStateSnapshot Snapshot,
    IReadOnlyList<string> Events,
    string? ErrorCode = null,
    string? ErrorMessage = null);

public sealed record FastForwardResult(
    bool Success,
    int RequestedXun,
    int AdvancedXun,
    GameStateSnapshot Snapshot,
    IReadOnlyList<string> Events,
    bool Interrupted,
    string? InterruptReason = null,
    string? ErrorCode = null);
