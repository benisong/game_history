using System;
using DonghanEngine.Core;
using DonghanEngine.Core.Economy;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

public sealed class GameEngineAgriculturalPolicyUiService : IAgriculturalPolicyUiService
{
    private readonly ICadastralAndIrrigationDomainService _domainService;
    private readonly ILandOwnershipDomainService _landDomainService;

    public GameEngineAgriculturalPolicyUiService(
        ICadastralAndIrrigationDomainService domainService,
        ILandOwnershipDomainService landDomainService)
    {
        _domainService = domainService;
        _landDomainService = landDomainService;
    }

    public ActionResult SurveyLand(string provinceId, CadastralSurveyIntensity intensity)
    {
        try
        {
            var res = _domainService.ExecuteCadastralSurvey(provinceId, intensity);
            if (!res.Success)
            {
                return ActionResult.Failure(res.NarrativeTitle, res.ChronicleText, ReportKind.Warning, "CadastralFailed");
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
            return ActionResult.Failure("度田清查失败", ex.Message, ReportKind.Warning, ex.GetType().Name);
        }
    }

    public ActionResult BuildIrrigation(string provinceId)
    {
        try
        {
            var res = _domainService.ExecuteConstructIrrigation(provinceId);
            if (!res.Success)
            {
                return ActionResult.Failure(res.NarrativeTitle, res.ChronicleText, ReportKind.Warning, "IrrigationFailed");
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
            return ActionResult.Failure("兴修水利失败", ex.Message, ReportKind.Warning, ex.GetType().Name);
        }
    }

    public ActionResult RepurchaseGentryLand(string provinceId, int purchaseAmount)
    {
        try
        {
            var res = _landDomainService.ExecuteRepurchaseLand(provinceId, purchaseAmount);
            if (!res.Success)
            {
                return ActionResult.Failure(res.NarrativeTitle, res.ChronicleText, ReportKind.Warning, "RepurchaseFailed");
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
            return ActionResult.Failure("世家赎田失败", ex.Message, ReportKind.Warning, ex.GetType().Name);
        }
    }
}
