using System.Collections.Generic;

namespace DonghanEngine.Core.Politics;

public interface ITalentNominationService
{
    IReadOnlyList<AristocratFamily> GetRegisteredFamilies();
    IReadOnlyList<NominationCandidate> GenerateAnnualNominations(GameState state);
    NominationResolutionResult AppointCandidate(GameState state, NominationCandidate candidate, string officeTitle);
    NominationResolutionResult RejectCandidate(GameState state, NominationCandidate candidate);
}
