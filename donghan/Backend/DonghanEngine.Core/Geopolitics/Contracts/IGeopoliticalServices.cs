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
