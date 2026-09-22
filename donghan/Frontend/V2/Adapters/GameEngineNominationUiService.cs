using System;
using System.Collections.Generic;
using DonghanEngine.Core;
using DonghanEngine.Core.Politics;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

public sealed class GameEngineNominationUiService : INominationUiService
{
    private readonly ITalentNominationDomainService _nominationDomain;

    public GameEngineNominationUiService(ITalentNominationDomainService nominationDomain)
    {
        _nominationDomain = nominationDomain;
    }

    public IReadOnlyList<NominationCandidate> GetPendingNominations() =>
        _nominationDomain.GetPendingNominations();

    public ActionResult Appoint(NominationCandidate candidate, string officeTitle)
    {
        try
        {
            var res = _nominationDomain.AppointNominationCandidate(candidate, officeTitle);
            return new ActionResult(
                true,
                res.NarrativeTitle,
                res.ChronicleText,
                ReportKind.Information,
                Array.Empty<StateChange>());
        }
        catch (Exception ex)
        {
            return ActionResult.Failure("御批除官失败", ex.Message, ReportKind.Warning, ex.GetType().Name);
        }
    }

    public ActionResult Reject(NominationCandidate candidate)
    {
        try
        {
            var res = _nominationDomain.RejectNominationCandidate(candidate);
            return new ActionResult(
                true,
                res.NarrativeTitle,
                res.ChronicleText,
                ReportKind.Information,
                Array.Empty<StateChange>());
        }
        catch (Exception ex)
        {
            return ActionResult.Failure("驳回察举失败", ex.Message, ReportKind.Warning, ex.GetType().Name);
        }
    }
}
