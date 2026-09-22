using System;
using DonghanEngine.Core;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Adapters;

public sealed class GameEngineSpecialActionService : ISpecialActionService
{
    private readonly IQuickActionDomainService _quickActionDomain;
    private readonly IDisasterReliefDomainService _reliefDomain;
    private readonly IConfiscationDomainService _confiscationDomain;
    private readonly IMilitaryPayrollDomainService _payrollDomain;
    private readonly IGameStateProvider _stateProvider;

    public GameEngineSpecialActionService(
        IQuickActionDomainService quickActionDomain,
        IDisasterReliefDomainService reliefDomain,
        IConfiscationDomainService confiscationDomain,
        IMilitaryPayrollDomainService payrollDomain,
        IGameStateProvider stateProvider)
    {
        _quickActionDomain = quickActionDomain;
        _reliefDomain = reliefDomain;
        _confiscationDomain = confiscationDomain;
        _payrollDomain = payrollDomain;
        _stateProvider = stateProvider;
    }

    public ActionResult Execute(SpecialActionCommand command)
    {
        try
        {
            var state = _stateProvider.GetState();

            if (command.ActionId == "grant_military_bonus")
            {
                var payrollResult = _payrollDomain.ExecuteGrantMilitaryBonus();
                return new ActionResult(
                    true,
                    payrollResult.NarrativeTitle,
                    payrollResult.ChronicleText,
                    ReportKind.Information,
                    Array.Empty<StateChange>());
            }

            if (command.ActionId == "confiscate_direct")
            {
                var confiscateResult = _confiscationDomain.ExecuteConfiscateTarget(command.TargetNpcId);
                if (!confiscateResult.Success)
                {
                    return ActionResult.Failure("籍没查抄回奏", confiscateResult.ChronicleText, ReportKind.Warning, "ConfiscationFailed");
                }

                return new ActionResult(
                    true,
                    confiscateResult.NarrativeTitle,
                    confiscateResult.ChronicleText,
                    ReportKind.Information,
                    Array.Empty<StateChange>());
            }

            TurnResult result = command.ActionId switch
            {
                "sell_office" => _quickActionDomain.ExecuteQuickAction("sell_office"),
                "harem_rest" => _quickActionDomain.ExecuteQuickAction("harem_rest"),
                "disaster_relief" => _reliefDomain.ExecuteDisasterReliefAction(command.Amount, command.OfficerId),
                "confiscation" => _confiscationDomain.ExecuteConfiscationAction(command.TargetNpcId, command.Destination),
                _ => throw new ArgumentOutOfRangeException(nameof(command.ActionId), command.ActionId, "未知特殊行动")
            };

            if (command.ActionId == "disaster_relief" && command.Amount > state.Treasury)
                return ActionResult.Failure("开仓赈灾", result.StoryText, ReportKind.Warning, "InsufficientTreasury");

            if (command.ActionId == "confiscation"
                && (result.StoryText.Contains("抄家流产", StringComparison.Ordinal)
                    || result.StoryText.Contains("抄家受阻", StringComparison.Ordinal)))
                return ActionResult.Failure("抄家回奏", result.StoryText, ReportKind.Warning, "ConfiscationFailed");

            return new ActionResult(true, ResolveTitle(command.ActionId), result.StoryText, ReportKind.Information, Array.Empty<StateChange>());
        }
        catch (Exception ex)
        {
            return ActionResult.Failure(ResolveTitle(command.ActionId), ex.Message, ReportKind.Warning, ex.GetType().Name);
        }
    }

    private static string ResolveTitle(string actionId) => actionId switch
    {
        "sell_office" => "西园鬻官回奏",
        "harem_rest" => "后宫起居回奏",
        "disaster_relief" => "大朝赈灾回奏",
        "confiscation" => "抄家回奏",
        "grant_military_bonus" => "犒赏三军回奏",
        "confiscate_direct" => "籍没查抄回奏",
        _ => "特殊行动回奏"
    };
}
