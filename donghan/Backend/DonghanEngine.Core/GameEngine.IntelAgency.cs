using System;
using System.Collections.Generic;

namespace DonghanEngine.Core;

/// <summary>
/// GameEngine 情报机构(黄门暗探)分部:月度供养结算、拨款档位设置(皇帝确认)、查探 NPC。
/// 详见 README §5.4 与 IntelAgency.cs 的准确率公式。
/// </summary>
public partial class GameEngine
{
    /// <summary>
    /// 皇帝确认设置情报机构供养档位。立即按新档位尝试支付【本月】月供:
    /// 私库足够则扣费、生效；不足则视同不给(降为 None，热情0)。
    /// 返回是否成功按所选档位支付。
    /// </summary>
    public bool SetIntelFunding(IntelFundingTier tier)
    {
        int cost = IntelAgency.MonthlyCost(tier);
        if (cost > 0 && _state.PrivateTreasury >= cost)
        {
            _state.PrivateTreasury -= cost;
            _state.IntelFunding = tier;
            _state.AddToChronicle($"【黄门暗探】天子拨私库 {cost} 万钱{IntelAgency.TierLabel(tier)}，暗探闻赏而动。");
            return true;
        }

        if (cost == 0)
        {
            _state.IntelFunding = IntelFundingTier.None;
            _state.AddToChronicle("【黄门暗探】天子停发暗探用度，黄门耳目渐次涣散。");
            return true;
        }

        // 想给但私库不足 → 视同不给
        _state.IntelFunding = IntelFundingTier.None;
        _state.AddToChronicle($"【黄门暗探】私库告罄(仅 {_state.PrivateTreasury} 万钱)，无力供养暗探，耳目离散。");
        return false;
    }

    /// <summary>
    /// 每月(上旬)自动续付当前供养档位。私库不足以支付则自动降为 None(暗探涣散)。
    /// 由 NextXunAsync 在月初调用。
    /// </summary>
    private void SettleIntelFundingMonthly()
    {
        var tier = _state.IntelFunding;
        int cost = IntelAgency.MonthlyCost(tier);
        if (cost == 0) return; // 本就未供养，无事

        if (_state.PrivateTreasury >= cost)
        {
            _state.PrivateTreasury -= cost;
            _state.AddToChronicle($"【黄门暗探】月供 {cost} 万钱已发({IntelAgency.TierLabel(tier)})。");
        }
        else
        {
            _state.IntelFunding = IntelFundingTier.None;
            _state.AddToChronicle($"【黄门暗探】私库不继，本月停发暗探用度，耳目离散，情报渐不可信。");
        }
    }

    /// <summary>
    /// 查探一名 NPC，返回逐维度评价(武/统/政/魅/廉)。准确率取决于当前供养档位
    /// 激发的暗探工作热情 —— 供养越厚情报越准，克扣或断供则常被误导。
    /// </summary>
    public Dictionary<TraitDimension, DerivedTrait>? InvestigateNpc(string npcId)
    {
        if (!_state.Npcs.TryGetValue(npcId, out var npc)) return null;
        int accuracy = IntelAgency.CurrentAccuracy(_state.IntelFunding);
        return TraitDeriver.BuildIntelAssessment(npc, accuracy, _rng);
    }

    /// <summary>当前情报查探准确率(0-100)，供 UI 显示"情报可信度"。</summary>
    public int CurrentIntelAccuracy() => IntelAgency.CurrentAccuracy(_state.IntelFunding);
}
