using System;
using DonghanEngine.Core.Geopolitics.Contracts;
using DonghanEngine.Core.Geopolitics.MathEngine;
using DonghanEngine.Core.Geopolitics.Models;

namespace DonghanEngine.Core.Geopolitics.MathEngine;

/// <summary>
/// 纯数学单步战役步进与结算器：计算攻防伤亡、城防损耗与领地易手（单一职责）
/// </summary>
public sealed class CampaignStepResolver : ICampaignStepResolver
{
    public CampaignStepResult ResolveStep(
        MilitaryCampaign campaign,
        WarlordFaction attacker,
        WarlordFaction defender,
        Province targetProvince)
    {
        if (campaign == null) throw new ArgumentNullException(nameof(campaign));
        if (targetProvince == null) throw new ArgumentNullException(nameof(targetProvince));

        // 1. 战力推演
        int attPower = CombatPowerCalculator.CalculateAttackerPower(campaign.CommittedTroops, generalLeadership: 80, generalMartial: 75);
        int defPower = CombatPowerCalculator.CalculateDefenderPower(targetProvince.Garrison, targetProvince.DefenseLevel, generalLeadership: 60);

        var (attLoss, defLoss, attackerTriumph) = CombatPowerCalculator.ResolveClash(
            attPower,
            defPower,
            campaign.CommittedTroops,
            targetProvince.Garrison);

        // 2. 扣除在途部队伤亡与守军伤亡
        campaign.ApplyAttrition(attLoss);
        targetProvince.AdjustGarrison(-defLoss);

        // 3. 推进周期并判定胜负
        campaign.AdvanceTurn();

        if (attackerTriumph || targetProvince.Garrison <= 300)
        {
            campaign.UpdateStatus(CampaignStatus.Victory);
            attacker?.AnnexProvince(targetProvince.Id);
            defender?.CedeProvince(targetProvince.Id);

            return new CampaignStepResult(
                campaign.CampaignId,
                CampaignStatus.Victory,
                campaign.AttackerFactionId,
                campaign.DefenderFactionId,
                targetProvince.Id,
                attLoss,
                defLoss,
                ProvinceConquered: true,
                $"【战报】攻方大破{targetProvince.Name}守军，克城拔寨，尽占其地！");
        }
        else if (campaign.CommittedTroops <= 500)
        {
            campaign.UpdateStatus(CampaignStatus.Defeated);
            return new CampaignStepResult(
                campaign.CampaignId,
                CampaignStatus.Defeated,
                campaign.AttackerFactionId,
                campaign.DefenderFactionId,
                targetProvince.Id,
                attLoss,
                defLoss,
                ProvinceConquered: false,
                $"【战报】攻方受挫于{targetProvince.Name}坚城之下，死伤过半，狼狈溃退！");
        }

        campaign.UpdateStatus(CampaignStatus.Sieging);
        return new CampaignStepResult(
            campaign.CampaignId,
            CampaignStatus.Sieging,
            campaign.AttackerFactionId,
            campaign.DefenderFactionId,
            targetProvince.Id,
            attLoss,
            defLoss,
            ProvinceConquered: false,
            $"【战报】双方于{targetProvince.Name}城外激战对峙，战事陷入胶着。");
    }
}
