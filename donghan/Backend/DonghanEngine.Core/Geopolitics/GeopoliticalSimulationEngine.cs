using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DonghanEngine.Core.Geopolitics.Contracts;
using DonghanEngine.Core.Geopolitics.MathEngine;
using DonghanEngine.Core.Geopolitics.Memorials;
using DonghanEngine.Core.Geopolitics.Models;
using DonghanEngine.Core.Geopolitics.Strategies;

namespace DonghanEngine.Core.Geopolitics;

/// <summary>
/// 纯领域地缘政治仿真引擎调度器：管理天下诸侯、在途战役、多边关系、每旬推演与奏折生成（单一职责）
/// </summary>
public sealed class GeopoliticalSimulationEngine : IGeopoliticalSimulationEngine
{
    private readonly Dictionary<string, WarlordFaction> _factions = new();
    private readonly List<MilitaryCampaign> _activeCampaigns = new();
    private readonly FactionRelationGraph _relations = new();

    private readonly IRandomProvider _randomProvider;
    private readonly ICampaignStepResolver _campaignResolver;
    private readonly IGeopoliticalMemorialFactory _memorialFactory;

    private readonly Dictionary<WarlordPosture, IWarlordDecisionEvaluator> _strategyMap;

    public IReadOnlyDictionary<string, WarlordFaction> Factions => _factions;
    public IReadOnlyList<MilitaryCampaign> ActiveCampaigns => _activeCampaigns;
    public FactionRelationGraph Relations => _relations;

    public GeopoliticalSimulationEngine(
        IRandomProvider? randomProvider = null,
        ICampaignStepResolver? campaignResolver = null,
        IGeopoliticalMemorialFactory? memorialFactory = null)
    {
        _randomProvider = randomProvider ?? new DefaultRandomProvider();
        _campaignResolver = campaignResolver ?? new CampaignStepResolver();
        _memorialFactory = memorialFactory ?? new GeopoliticalMemorialFactory();

        // 策略路由映射（策略模式）
        _strategyMap = new Dictionary<WarlordPosture, IWarlordDecisionEvaluator>
        {
            [WarlordPosture.AmbitiousWarlord] = new AmbitiousWarlordStrategy(_randomProvider),
            [WarlordPosture.LoyalistBanner] = new LoyalistBannerStrategy(),
            [WarlordPosture.CautiousAutonomist] = new CautiousAutonomistStrategy()
        };

        // 载入预置诸侯
        foreach (var faction in HistoricalFactionPresets.CreateInitialFactions())
        {
            RegisterFaction(faction);
        }
    }

    public void RegisterFaction(WarlordFaction faction)
    {
        if (faction == null) return;
        _factions[faction.FactionId] = faction;
    }

    public GeopoliticalTickResult TickTurn(GameState gameState)
    {
        if (gameState == null) throw new ArgumentNullException(nameof(gameState));

        var decisions = new List<WarlordDecision>();
        var campaignResults = new List<CampaignStepResult>();
        var memorials = new List<GeopoliticalMemorial>();
        int tributeCollected = 0;
        var summarySb = new StringBuilder();

        var context = new GeopoliticalContext(
            EmperorImperialPower: gameState.ImperialPower,
            WestGardenArmySize: gameState.WestGardenArmy?.Size ?? 0,
            WestGardenMorale: gameState.WestGardenArmy?.Morale ?? 0,
            AllFactions: _factions,
            AllProvinces: gameState.Provinces,
            Relations: _relations);

        // 1. 各诸侯 AI 战略决策评估
        foreach (var faction in _factions.Values)
        {
            if (!_strategyMap.TryGetValue(faction.Posture, out var strategy)) continue;

            var decision = strategy.EvaluateDecision(faction, context);
            decisions.Add(decision);

            switch (decision.ActionType)
            {
                // 纳贡处理
                case WarlordActionType.Tribute:
                {
                    tributeCollected += decision.TributeAmount;
                    gameState.Treasury = Math.Clamp(gameState.Treasury + decision.TributeAmount, 0, 999999);
                    faction.AdjustLoyalty(10);
                    var memorial = _memorialFactory.CreateTributeMemorial(faction, decision.TributeAmount);
                    memorials.Add(memorial);
                    summarySb.AppendLine($"【地缘岁贡】{faction.FactionName}进奉黄金{decision.TributeAmount}万钱。");
                    break;
                }

                // 出兵攻伐
                case WarlordActionType.LaunchCampaign:
                {
                    var newCamp = new MilitaryCampaign(
                        Guid.NewGuid().ToString("N"),
                        faction.FactionId,
                        decision.TargetFactionId,
                        decision.TargetProvinceId,
                        decision.CommittedTroops,
                        durationTurns: 1);
                    _activeCampaigns.Add(newCamp);
                    summarySb.AppendLine($"【地缘烽火】{faction.FactionName}发兵{decision.CommittedTroops}出征{decision.TargetProvinceId}！");
                    break;
                }
            }
        }

        // 2. 在途战役步进推演
        var finishedCampaigns = new List<MilitaryCampaign>();

        foreach (var campaign in _activeCampaigns)
        {
            _factions.TryGetValue(campaign.AttackerFactionId, out var attacker);
            _factions.TryGetValue(campaign.DefenderFactionId, out var defender);
            gameState.Provinces.TryGetValue(campaign.TargetProvinceId, out var targetProv);

            if (targetProv == null)
            {
                finishedCampaigns.Add(campaign);
                continue;
            }

            string attId = string.IsNullOrWhiteSpace(campaign.AttackerFactionId) ? "attacker_dummy" : campaign.AttackerFactionId;
            string defId = string.IsNullOrWhiteSpace(campaign.DefenderFactionId) ? "defender_dummy" : campaign.DefenderFactionId;

            WarlordFaction validAttacker = attacker ?? new WarlordFaction(attId, "unknown", "未知诸侯", WarlordPosture.AmbitiousWarlord, Array.Empty<string>(), 0, 0, 0, 0);
            WarlordFaction validDefender = defender ?? new WarlordFaction(defId, "unknown", "守军诸侯", WarlordPosture.CautiousAutonomist, Array.Empty<string>(), 0, 0, 0, 0);

            var stepResult = _campaignResolver.ResolveStep(campaign, validAttacker, validDefender, targetProv);
            campaignResults.Add(stepResult);

            if (stepResult.Status == CampaignStatus.Victory)
            {
                finishedCampaigns.Add(campaign);
                if (attacker != null)
                {
                    var petition = _memorialFactory.CreatePetitionMemorial(attacker, campaign.TargetProvinceId, targetProv);
                    memorials.Add(petition);
                }
                summarySb.AppendLine(stepResult.SummaryText);
            }
            else if (stepResult.Status == CampaignStatus.Defeated)
            {
                finishedCampaigns.Add(campaign);
                summarySb.AppendLine(stepResult.SummaryText);
            }
            else if (stepResult.Status == CampaignStatus.Sieging && defender != null)
            {
                var appeal = _memorialFactory.CreateAppealMemorial(defender, attacker, campaign.TargetProvinceId, targetProv);
                memorials.Add(appeal);
            }
        }

        foreach (var finished in finishedCampaigns)
        {
            _activeCampaigns.Remove(finished);
        }

        return new GeopoliticalTickResult(
            decisions,
            campaignResults,
            memorials,
            tributeCollected,
            summarySb.ToString());
    }
}
