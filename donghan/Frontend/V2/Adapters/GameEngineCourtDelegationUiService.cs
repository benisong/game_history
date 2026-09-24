using System;
using System.Collections.Generic;
using DonghanEngine.Core;
using DonghanEngine.Core.Politics;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

public sealed class GameEngineCourtDelegationUiService : ICourtDelegationUiService
{
    private readonly ICourtDelegationDomainService _domainService;

    public GameEngineCourtDelegationUiService(ICourtDelegationDomainService domainService)
    {
        _domainService = domainService;
    }

    public IReadOnlyList<DelegationAffairItem> GetPendingAffairs()
    {
        return _domainService.GetPendingAffairs();
    }

    public ActionResult ExecuteDirectAffair(string affairId)
    {
        try
        {
            var rep = _domainService.ExecuteDirectAffair(affairId);
            return new ActionResult(
                true,
                "【天子亲裁毕】",
                rep.ExecutionNarrative,
                ReportKind.Information,
                Array.Empty<StateChange>());
        }
        catch (Exception ex)
        {
            return ActionResult.Failure("天子亲裁失败", ex.Message, ReportKind.Warning, ex.GetType().Name);
        }
    }

    public ActionResult ExecuteBatchDelegation(IReadOnlyList<string>? affairIds = null)
    {
        try
        {
            var res = _domainService.ExecuteBatchDelegation(affairIds);
            string narrative = $"{res.SummaryNarrative}\n\n" +
                               string.Join("\n", res.ExecutedReports.Select(r => $"• {r.ExecutionNarrative}"));

            return new ActionResult(
                res.Success,
                "【廷议分权交办告竣】",
                narrative,
                ReportKind.Information,
                Array.Empty<StateChange>());
        }
        catch (Exception ex)
        {
            return ActionResult.Failure("廷议分权交办失败", ex.Message, ReportKind.Warning, ex.GetType().Name);
        }
    }
}
