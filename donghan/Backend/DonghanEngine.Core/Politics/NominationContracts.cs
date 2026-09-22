using System.Collections.Generic;

namespace DonghanEngine.Core.Politics;

public sealed record AristocratFamily(
    string FamilyId,
    string FamilyName,
    string NativeProvince,
    int Prestige,   // 门阀声望 0~100
    int Loyalty,    // 对汉室忠诚度 0~100
    int Power,      // 门生故吏在朝权势 0~100
    IReadOnlyList<string> KeyFigureNpcIds);

public sealed record NominationCandidate(
    string CandidateId,
    string Name,
    string SponsoringFamilyId,
    string NativeProvince,
    int Politics,
    int Intelligence,
    int Charisma,
    int Martial,
    string RecommendedOfficeTitle,
    string CandidateBio);

public sealed record NominationResolutionResult(
    bool Appointed,
    string CandidateId,
    string CandidateName,
    string SponsoringFamilyId,
    int SponsoringFamilyLoyaltyDelta,
    int SponsoringFamilyPowerDelta,
    int ImperialPowerDelta,
    string DestinationFactionId, // 若被冷落驳回，流向的诸侯派系 ID (如 "cao_cao", "yuan_shao")
    string NarrativeTitle,
    string ChronicleText);
