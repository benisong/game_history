using System;
using System.Collections.Generic;
using System.Linq;
using DonghanEngine.Core;
using DonghanEngine.Core.Balance;

namespace DonghanEngine.Core.Politics;

/// <summary>
/// 纯领域服务：文武官阶九品梯队、正道阶梯升迁（限超擢3级）与西园通天卖官系统（单一职责）
/// 支持 IInitializableBalance&lt;OfficialRankBalanceConfig&gt; 接口，供超级控制工具动态调参
/// </summary>
public sealed class OfficialRankService : IOfficialRankService, IInitializableBalance<OfficialRankBalanceConfig>
{
    private OfficialRankBalanceConfig _config;

    public OfficialRankService(OfficialRankBalanceConfig? config = null)
    {
        _config = config ?? new OfficialRankBalanceConfig();
    }

    public void InitializeConfig(OfficialRankBalanceConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public OfficialRankBalanceConfig GetConfig() => _config;

    private static readonly List<OfficialPosition> Positions = new()
    {
        // 9品
        new(9, "郎中", OfficialBranch.Civilian, 100, 30, "初举孝廉宿卫郎署，百石之基。"),
        new(9, "军候", OfficialBranch.Military, 100, 30, "基层军官，统领部曲。"),

        // 8品
        new(8, "县丞", OfficialBranch.Civilian, 200, 40, "辅佐县令，掌文书刑狱。"),
        new(8, "部司马", OfficialBranch.Military, 200, 40, "部营领军，副将佐才。"),

        // 7品
        new(7, "尚书郎", OfficialBranch.Civilian, 400, 50, "尚书台起草公文，天子枢要。"),
        new(7, "别部司马", OfficialBranch.Military, 400, 50, "统领别营，冲锋陷阵。"),

        // 6品
        new(6, "议郎", OfficialBranch.Civilian, 800, 60, "谏诤议论朝政，顾问天子。"),
        new(6, "骑都尉", OfficialBranch.Military, 800, 60, "掌骑兵宿卫，统精锐部曲。"),

        // 5品
        new(5, "尚书丞", OfficialBranch.Civilian, 1500, 70, "尚书台副长官，总揽机要。"),
        new(5, "西园校尉", OfficialBranch.Military, 1500, 70, "天子西园亲军校尉，掌精锐宿卫。"),

        // 4品
        new(4, "郡太守", OfficialBranch.Civilian, 3000, 75, "一郡封疆父母，掌治民领兵。"),
        new(4, "偏将军", OfficialBranch.Military, 3000, 75, "领兵偏师，战阵大将。"),

        // 3品
        new(3, "尚书令", OfficialBranch.Civilian, 5000, 85, "尚书台长官，天下政令机枢。"),
        new(3, "九卿", OfficialBranch.Civilian, 5000, 80, "廷尉/太常/大司农，分掌国政。"),
        new(3, "前将军", OfficialBranch.Military, 5000, 85, "名号大将，坐镇一方。"),

        // 2品
        new(2, "州牧", OfficialBranch.Civilian, 8000, 90, "总领一州军政民赋，封疆大吏。"),
        new(2, "镇东将军", OfficialBranch.Military, 8000, 90, "四镇将军，方面统帅。"),

        // 1品 (极品三公)
        new(1, "太尉", OfficialBranch.Civilian, 10000, 95, "三公之首，总领天下兵马礼法。"),
        new(1, "司徒", OfficialBranch.Civilian, 10000, 95, "三公之一，掌天下户口教化。"),
        new(1, "司空", OfficialBranch.Civilian, 10000, 95, "三公之一，掌水利土木宗庙。"),
        new(1, "大将军", OfficialBranch.Military, 10000, 95, "三军最高统帅，辅政元首。")
    };

    public IReadOnlyList<OfficialPosition> GetAllPositions() => Positions.AsReadOnly();

    public OfficialPosition? GetPositionByTitle(string title) =>
        Positions.FirstOrDefault(p => p.Title == title);

    public PromotionResult PromoteOfficial(GameState state, string npcId, string targetTitle)
    {
        if (!state.Npcs.TryGetValue(npcId, out var npc) || !npc.IsActive)
        {
            return new PromotionResult(
                Success: false,
                NpcId: npcId,
                NpcName: "未知官员",
                OldRankTier: 9,
                NewRankTier: 9,
                OldTitle: "",
                NewTitle: targetTitle,
                IsExtraordinary: false,
                LoyaltyDelta: 0,
                ScholarLoyaltyPenalty: 0,
                NarrativeTitle: "【除官受阻】查无此臣！",
                ChronicleText: "查无此官员。",
                ErrorCode: "NpcNotFound");
        }

        var targetPos = GetPositionByTitle(targetTitle);
        if (targetPos == null)
        {
            return new PromotionResult(
                Success: false,
                NpcId: npcId,
                NpcName: npc.Name,
                OldRankTier: npc.TitleTier,
                NewRankTier: npc.TitleTier,
                OldTitle: npc.Title,
                NewTitle: targetTitle,
                IsExtraordinary: false,
                LoyaltyDelta: 0,
                ScholarLoyaltyPenalty: 0,
                NarrativeTitle: "【除官受阻】无此官职品阶！",
                ChronicleText: "官阶名册无此职位。",
                ErrorCode: "InvalidPosition");
        }

        int currentTier = npc.TitleTier > 0 ? npc.TitleTier : 9;
        int targetTier = targetPos.RankTier;

        // 品阶越小官越大 (1品 > 9品)
        int promotionSteps = currentTier - targetTier;

        // 1. 不能降职或平调走常规升迁
        if (promotionSteps <= 0)
        {
            return new PromotionResult(
                Success: false,
                NpcId: npcId,
                NpcName: npc.Name,
                OldRankTier: currentTier,
                NewRankTier: targetTier,
                OldTitle: npc.Title,
                NewTitle: targetTitle,
                IsExtraordinary: false,
                LoyaltyDelta: 0,
                ScholarLoyaltyPenalty: 0,
                NarrativeTitle: "【升迁无效】目标官职未高过现职！",
                ChronicleText: "未达升迁标准。",
                ErrorCode: "NotPromotion");
        }

        // 2. 正规升迁铁律：一次最多不得超擢超过配置级数（默认3级）
        if (promotionSteps > _config.MaxPromotionStepAllowance)
        {
            string blockedChronicle = $"【封驳】天子欲直拔【{npc.Name}】为【{targetTitle}】（超擢{promotionSteps}级）。尚书台与三公执奏封驳：“名位僭越，逾越祖宗三级考课成宪，伏请收回成命！”";
            state.AddToChronicle(blockedChronicle);

            return new PromotionResult(
                Success: false,
                NpcId: npcId,
                NpcName: npc.Name,
                OldRankTier: currentTier,
                NewRankTier: targetTier,
                OldTitle: npc.Title,
                NewTitle: targetTitle,
                IsExtraordinary: true,
                LoyaltyDelta: 0,
                ScholarLoyaltyPenalty: 0,
                NarrativeTitle: "【尚书台封驳】超擢逾制！跨级超过三品！",
                ChronicleText: blockedChronicle,
                ErrorCode: "ExceedsMaxPromotionSteps");
        }

        // 3. 执行升迁结算
        bool isExtraordinary = promotionSteps >= _config.ExtraordinaryPromotionStepThreshold;
        int loyaltyDelta = isExtraordinary ? _config.ExtraordinaryPromotionLoyaltyGain : _config.StandardPromotionLoyaltyGain;
        int scholarPenalty = isExtraordinary ? _config.ExtraordinaryScholarLoyaltyPenalty : 0;

        string oldTitle = npc.Title;
        npc.Title = targetTitle;
        npc.TitleTier = targetTier;
        npc.AdjustFavorability(loyaltyDelta);

        if (scholarPenalty < 0)
        {
            foreach (var (_, otherNpc) in state.Npcs)
            {
                if (otherNpc.Id != npcId && otherNpc.IsActive && otherNpc.Faction == "清流派")
                {
                    otherNpc.AdjustFavorability(scholarPenalty);
                }
            }
        }

        string title = isExtraordinary
            ? $"【特旨超擢 · 破格拔擢】天子破格擢拜{npc.Name}为{targetTitle}！"
            : $"【考课迁转 · 顺迁除官】天子诏拜{npc.Name}为{targetTitle}。";

        string chronicle = isExtraordinary
            ? $"【超擢】天子特下中旨，不拘常格，直拔【{npc.Name}】由【{oldTitle}】超擢三品拜为【{targetTitle}】！{npc.Name}伏阙叩首誓死报效，然朝中清流微有微词。"
            : $"【拜官】有司考课绩优，天子诏除【{npc.Name}】为【{targetTitle}】。百官肃然，合乎朝仪。";

        state.AddToChronicle(chronicle);

        return new PromotionResult(
            Success: true,
            NpcId: npcId,
            NpcName: npc.Name,
            OldRankTier: currentTier,
            NewRankTier: targetTier,
            OldTitle: oldTitle,
            NewTitle: targetTitle,
            IsExtraordinary: isExtraordinary,
            LoyaltyDelta: loyaltyDelta,
            ScholarLoyaltyPenalty: scholarPenalty,
            NarrativeTitle: title,
            ChronicleText: chronicle);
    }

    public OfficeSaleResult SellOfficeToNpc(GameState state, string buyerNpcId, string targetTitle)
    {
        if (!state.Npcs.TryGetValue(buyerNpcId, out var buyer) || !buyer.IsActive)
        {
            return new OfficeSaleResult(
                Success: false,
                BuyerNpcId: buyerNpcId,
                BuyerName: "未知豪族",
                RankTier: 9,
                TargetTitle: targetTitle,
                GoldPaidToPrivateVault: 0,
                PublicMoralePenalty: 0,
                ScholarLoyaltyPenalty: 0,
                NarrativeTitle: "【西园鬻官受阻】买主无从查核！",
                ChronicleText: "查无此人。",
                ErrorCode: "NpcNotFound");
        }

        var targetPos = GetPositionByTitle(targetTitle);
        if (targetPos == null)
        {
            return new OfficeSaleResult(
                Success: false,
                BuyerNpcId: buyerNpcId,
                BuyerName: buyer.Name,
                RankTier: 9,
                TargetTitle: targetTitle,
                GoldPaidToPrivateVault: 0,
                PublicMoralePenalty: 0,
                ScholarLoyaltyPenalty: 0,
                NarrativeTitle: "【西园鬻官受阻】无此官职标价！",
                ChronicleText: "西园未挂牌此职位。",
                ErrorCode: "InvalidPosition");
        }

        int price = targetPos.PriceInWan;

        // 1. 西园卖官：无视品阶！哪怕是白丁，直接空降一品三公！
        buyer.Title = targetTitle;
        buyer.TitleTier = targetPos.RankTier;
        buyer.AdjustFavorability(_config.OfficeSaleBuyerFavorabilityGain);

        // 2. 资金进入天子私库 (PrivateTreasury)
        state.PrivateTreasury = Math.Clamp(state.PrivateTreasury + price, 0, 999999);

        // 3. 剧烈的天下反噬
        int moralePenalty = targetPos.RankTier switch
        {
            1 => _config.OfficeSaleTier1MoralePenalty, // 卖三公：民心暴跌
            2 => _config.OfficeSaleTier2MoralePenalty, // 卖州牧
            3 => _config.OfficeSaleTier3MoralePenalty, // 卖九卿
            _ => _config.OfficeSaleDefaultMoralePenalty
        };
        state.PopularSupport = Math.Clamp(state.PopularSupport + moralePenalty, 0, 100);

        // 4. 清流名士与老臣耻与为伍，忠诚暴跌
        int scholarPenalty = targetPos.RankTier <= 3 ? _config.OfficeSaleHighRankScholarPenalty : _config.OfficeSaleLowRankScholarPenalty;
        foreach (var (_, otherNpc) in state.Npcs)
        {
            if (otherNpc.Id != buyerNpcId && otherNpc.IsActive && otherNpc.Faction == "清流派")
            {
                otherNpc.AdjustFavorability(scholarPenalty);
            }
        }

        string title = targetPos.RankTier == 1
            ? $"【西园万金堂 · 豪掷万金】天子在西园出卖【{targetTitle}】给{buyer.Name}！得钱{price}万贯！"
            : $"【西园鬻官 · 明码标价】{buyer.Name}纳钱{price}万贯，买拜为【{targetTitle}】。";

        string chronicle = targetPos.RankTier == 1
            ? $"【鬻官】天子开西园万金堂，{buyer.Name}输纳巨金{price}万钱入天子私库，天子当朝除拜其为一品【{targetTitle}】！清流名士无不捶胸顿足，天下士林讥讽为“铜臭三公”，民心大沮！"
            : $"【卖官】西园拍卖要职，{buyer.Name}输钱{price}万贯入内帑，得拜【{targetTitle}】。";

        state.AddToChronicle(chronicle);

        return new OfficeSaleResult(
            Success: true,
            BuyerNpcId: buyerNpcId,
            BuyerName: buyer.Name,
            RankTier: targetPos.RankTier,
            TargetTitle: targetTitle,
            GoldPaidToPrivateVault: price,
            PublicMoralePenalty: moralePenalty,
            ScholarLoyaltyPenalty: scholarPenalty,
            NarrativeTitle: title,
            ChronicleText: chronicle);
    }
}
