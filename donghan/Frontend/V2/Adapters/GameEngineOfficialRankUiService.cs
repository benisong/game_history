using System;
using System.Collections.Generic;
using DonghanEngine.Core;
using DonghanEngine.Core.Politics;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

public sealed class GameEngineOfficialRankUiService : IOfficialRankUiService
{
    private readonly IOfficialRankDomainService _rankDomain;

    public GameEngineOfficialRankUiService(IOfficialRankDomainService rankDomain)
    {
        _rankDomain = rankDomain;
    }

    public IReadOnlyList<OfficialPosition> GetAllPositions() =>
        _rankDomain.GetAllOfficialPositions();

    public ActionResult Promote(string npcId, string targetTitle)
    {
        try
        {
            var res = _rankDomain.ExecutePromoteOfficial(npcId, targetTitle);
            if (!res.Success)
            {
                return ActionResult.Failure(res.NarrativeTitle, res.ChronicleText, ReportKind.Warning, res.ErrorCode ?? "PromotionFailed");
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
            return ActionResult.Failure("除官升迁失败", ex.Message, ReportKind.Warning, ex.GetType().Name);
        }
    }

    public ActionResult SellOffice(string buyerNpcId, string targetTitle)
    {
        try
        {
            var res = _rankDomain.ExecuteSellOfficeToNpc(buyerNpcId, targetTitle);
            if (!res.Success)
            {
                return ActionResult.Failure(res.NarrativeTitle, res.ChronicleText, ReportKind.Warning, res.ErrorCode ?? "OfficeSaleFailed");
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
            return ActionResult.Failure("西园鬻官失败", ex.Message, ReportKind.Warning, ex.GetType().Name);
        }
    }
}
