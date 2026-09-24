using System;
using DonghanEngine.Core;
using DonghanEngine.Core.Politics;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

public sealed class GameEngineGovernorAppraisalUiService : IGovernorAppraisalUiService
{
    private readonly IGovernorAppraisalDomainService _domainService;

    public GameEngineGovernorAppraisalUiService(IGovernorAppraisalDomainService domainService)
    {
        _domainService = domainService;
    }

    public AnnualAppraisalReport GetAnnualAppraisal()
    {
        return _domainService.ExecuteAnnualAppraisal();
    }

    public ActionResult PromoteGovernorToCourt(string governorId, string targetCourtTitle)
    {
        try
        {
            var res = _domainService.ExecutePromoteGovernorToCourt(governorId, targetCourtTitle);
            if (!res.Success)
            {
                return ActionResult.Failure(res.NarrativeTitle, res.ChronicleText, ReportKind.Warning, res.ErrorCode ?? "AppraisalPromotionFailed");
            }

            return new ActionResult(
                true,
                res.NarrativeTitle,
                res.ChronicleText,
                ReportKind.Information,
                Array.Empty<StateChange>());
        }
        catch (Exception ex)
        {
            return ActionResult.Failure("征拜内调失败", ex.Message, ReportKind.Warning, ex.GetType().Name);
        }
    }
}
