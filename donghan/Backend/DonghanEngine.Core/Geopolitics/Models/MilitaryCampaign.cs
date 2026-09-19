using System;

namespace DonghanEngine.Core.Geopolitics.Models;

/// <summary>
/// 在途地缘战役充血实体：记录进攻方、防守方、目标州郡、投入兵力与战役周期状态（防篡改封装）
/// </summary>
public sealed class MilitaryCampaign
{
    public string CampaignId { get; }
    public string AttackerFactionId { get; }
    public string DefenderFactionId { get; }
    public string TargetProvinceId { get; }
    public int CommittedTroops { get; private set; }
    public int TurnsRemaining { get; private set; }
    public CampaignStatus Status { get; private set; }

    public MilitaryCampaign(
        string campaignId,
        string attackerFactionId,
        string defenderFactionId,
        string targetProvinceId,
        int committedTroops,
        int durationTurns)
    {
        CampaignId = string.IsNullOrWhiteSpace(campaignId) ? Guid.NewGuid().ToString("N") : campaignId;
        AttackerFactionId = attackerFactionId ?? throw new ArgumentNullException(nameof(attackerFactionId));
        DefenderFactionId = defenderFactionId ?? string.Empty;
        TargetProvinceId = targetProvinceId ?? throw new ArgumentNullException(nameof(targetProvinceId));
        CommittedTroops = Math.Max(0, committedTroops);
        TurnsRemaining = Math.Max(1, durationTurns);
        Status = CampaignStatus.Mobilizing;
    }

    public void AdvanceTurn()
    {
        if (TurnsRemaining > 0)
        {
            TurnsRemaining--;
        }
    }

    public void ApplyAttrition(int casualtyCount)
    {
        CommittedTroops = Math.Max(0, CommittedTroops - casualtyCount);
    }

    public void UpdateStatus(CampaignStatus newStatus)
    {
        Status = newStatus;
    }
}
