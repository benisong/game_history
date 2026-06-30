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
    /// 皇帝确认设置情报机构供养档位的结果。供前端弹窗 + 过渡动画使用。
    /// </summary>
    public class IntelFundingResult
    {
        public bool Success { get; set; }              // 是否按所选档位成功支付
        public IntelFundingTier PreviousTier { get; set; }
        public IntelFundingTier NewTier { get; set; }
        public int Cost { get; set; }                  // 实际支付的私库金额
        public int Accuracy { get; set; }              // 调整后的情报准确率
        public string StoryText { get; set; } = string.Empty; // 朝堂演出文本(含升降档对比)
    }

    /// <summary>
    /// 皇帝确认设置情报机构供养档位。立即按新档位尝试支付【本月】月供:
    /// 私库足够则扣费、生效；不足则视同不给(降为 None，热情0)。
    /// 返回结构化结果(含供前端弹窗/过渡动画的提示文本，体现情报组长卖力程度变化)。
    /// </summary>
    public IntelFundingResult SetIntelFundingDetailed(IntelFundingTier tier)
    {
        var prev = _state.IntelFunding;
        int cost = IntelAgency.MonthlyCost(tier);
        bool success;
        IntelFundingTier applied;

        if (cost > 0 && _state.PrivateTreasury >= cost)
        {
            _state.PrivateTreasury -= cost;
            _state.IntelFunding = tier;
            applied = tier;
            success = true;
            _state.AddToChronicle($"【黄门暗探】天子拨私库 {cost} 万钱{IntelAgency.TierLabel(tier)}，暗探闻赏而动。");
        }
        else if (cost == 0)
        {
            _state.IntelFunding = IntelFundingTier.None;
            applied = IntelFundingTier.None;
            success = true;
            _state.AddToChronicle("【黄门暗探】天子停发暗探用度，黄门耳目渐次涣散。");
        }
        else
        {
            // 想给但私库不足 → 视同不给
            _state.IntelFunding = IntelFundingTier.None;
            applied = IntelFundingTier.None;
            success = false;
            _state.AddToChronicle($"【黄门暗探】私库告罄(仅 {_state.PrivateTreasury} 万钱)，无力供养暗探，耳目离散。");
        }

        return new IntelFundingResult
        {
            Success = success,
            PreviousTier = prev,
            NewTier = applied,
            Cost = success && cost > 0 ? cost : 0,
            Accuracy = IntelAgency.CurrentAccuracy(applied),
            StoryText = BuildIntelFundingStory(prev, applied, success, cost)
        };
    }

    /// <summary>
    /// 生成拨款演出文本:体现情报组长(黄门令)对供养变化的反应与卖力程度升降对比。
    /// </summary>
    private static string BuildIntelFundingStory(IntelFundingTier prev, IntelFundingTier now, bool success, int desiredCost)
    {
        // 私库不足的失败分支
        if (!success)
        {
            return "【黄门暗探 · 内帑告罄】\n\n黄门令垂手而立，半晌不语：「陛下……私库已空，弟兄们的赏钱实在凑不出了。」\n" +
                   "暗探闻讯，三三两两散去，洛阳城中的耳目自此稀疏。情报愈发不可凭信。";
        }

        // 当前档位的基础反应
        string reaction = now switch
        {
            IntelFundingTier.Lavish =>
                "黄门令喜形于色，叩首谢恩：「陛下厚恩！臣等纵粉身碎骨，也必为陛下查个水落石出！」\n暗探倾巢而出，耳目遍布洛阳街巷宫闱。",
            IntelFundingTier.Normal =>
                "黄门令躬身领赏：「臣等谨守本分。」暗探如常当值，密札往来不绝，情报大抵可信。",
            IntelFundingTier.Half =>
                "黄门令面有难色：「用度紧巴，弟兄们……怕是要懈怠几分了。」暗探敷衍塞责，密报多有疏漏。",
            _ =>
                "天子不发用度。黄门令默然告退，暗探作鸟兽散，宫闱内外耳目尽失。",
        };

        // 升/降档对比(卖力程度变化)
        int diff = (int)now - (int)prev;
        string compare = diff > 0
            ? "\n\n[color=green]● 较之先前，赏赐加厚，暗探明显更为卖力，情报准头见涨。[/color]"
            : diff < 0
                ? "\n\n[color=yellow]● 较之先前，用度缩减，组长怨气渐生，暗探办事愈发敷衍，情报恐生疏漏。[/color]"
                : "\n\n● 供养如旧，暗探办事一如往常。";

        string accLine = $"\n[color=gray]（当前情报准确率：约 {IntelAgency.CurrentAccuracy(now)}%）[/color]";

        return $"【黄门暗探 · 拨付内帑】\n\n{reaction}{compare}{accLine}";
    }

    /// <summary>
    /// 皇帝确认设置情报机构供养档位(简版，仅返回是否成功支付)。
    /// 内部委托 SetIntelFundingDetailed。
    /// </summary>
    public bool SetIntelFunding(IntelFundingTier tier) => SetIntelFundingDetailed(tier).Success;

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
