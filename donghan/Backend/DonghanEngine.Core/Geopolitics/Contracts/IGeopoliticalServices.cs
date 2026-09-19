using System.Collections.Generic;
using DonghanEngine.Core;
using DonghanEngine.Core.Geopolitics.Models;

namespace DonghanEngine.Core.Geopolitics.Contracts;

public interface IRandomProvider
{
    int NextPercentage(); // 0 - 99
    int NextRange(int min, int max);
}

public interface IWarlordDecisionEvaluator
{
    WarlordDecision EvaluateDecision(WarlordFaction faction, GeopoliticalContext context);
}

public interface ICampaignStepResolver
{
    CampaignStepResult ResolveStep(
        MilitaryCampaign campaign,
        WarlordFaction attacker,
        WarlordFaction defender,
        Province targetProvince);
}

public interface IGeopoliticalMemorialFactory
{
    GeopoliticalMemorial CreatePetitionMemorial(WarlordFaction victor, string provinceId, Province province);
    GeopoliticalMemorial CreateAppealMemorial(WarlordFaction besieged, WarlordFaction attacker, string provinceId, Province province);
    GeopoliticalMemorial CreateTributeMemorial(WarlordFaction loyalist, int tributeAmount);
}

public interface IImperialEdictExecutor
{
    EdictExecutionResult ExecuteEdict(
        GeopoliticalImperialEdict edict,
        GameState gameState,
        IReadOnlyDictionary<string, WarlordFaction> allFactions,
        FactionRelationGraph relations);
}
