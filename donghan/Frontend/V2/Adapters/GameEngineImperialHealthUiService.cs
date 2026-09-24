using System;
using DonghanEngine.Core;
using DonghanEngine.Core.Health;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

public sealed class GameEngineImperialHealthUiService : IImperialHealthUiService
{
    private readonly IImperialHealthDomainService _domainService;

    public GameEngineImperialHealthUiService(IImperialHealthDomainService domainService)
    {
        _domainService = domainService;
    }

    public ImperialHealthDiagnosisReport GetPhysicianDiagnosis()
    {
        return _domainService.GetPhysicianDiagnosis();
    }

    public ActionResult RestAtWendePalace()
    {
        try
        {
            var res = _domainService.RestAtWendePalace();
            return new ActionResult(
                res.Success,
                res.NarrativeTitle,
                res.ChronicleText,
                ReportKind.Information,
                Array.Empty<StateChange>());
        }
        catch (Exception ex)
        {
            return ActionResult.Failure("温德殿静养失败", ex.Message, ReportKind.Warning, ex.GetType().Name);
        }
    }

    public ActionResult IndulgeInHarem()
    {
        try
        {
            var res = _domainService.IndulgeInHarem();
            return new ActionResult(
                res.Success,
                res.NarrativeTitle,
                res.ChronicleText,
                ReportKind.Information,
                Array.Empty<StateChange>());
        }
        catch (Exception ex)
        {
            return ActionResult.Failure("临幸后宫失败", ex.Message, ReportKind.Warning, ex.GetType().Name);
        }
    }
}
