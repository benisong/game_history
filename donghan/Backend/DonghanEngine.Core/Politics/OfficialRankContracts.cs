using System.Collections.Generic;

namespace DonghanEngine.Core.Politics;

public enum OfficialBranch
{
    Civilian, // 文官体系 (重政治/智谋)
    Military  // 武官体系 (重统帅/武力)
}

public sealed record OfficialPosition(
    int RankTier,             // 品阶 1~9 (1品极品三公，9品基层郎中)
    string Title,             // 官职名称 (如 "太尉", "尚书令", "郡太守", "西园校尉")
    OfficialBranch Branch,    // 文官 / 武官
    int PriceInWan,           // 西园鬻官标价 (万钱)
    int MinStatRequirement,   // 正常任职基础属性门槛
    string Description);

public sealed record PromotionResult(
    bool Success,
    string NpcId,
    string NpcName,
    int OldRankTier,
    int NewRankTier,
    string OldTitle,
    string NewTitle,
    bool IsExtraordinary,     // 是否属于特旨超擢 (跨2~3级)
    int LoyaltyDelta,         // 被提拔官员忠诚变化
    int ScholarLoyaltyPenalty,// 朝堂清流与老臣忠诚惩罚
    string NarrativeTitle,
    string ChronicleText,
    string? ErrorCode = null);

public sealed record OfficeSaleResult(
    bool Success,
    string BuyerNpcId,
    string BuyerName,
    int RankTier,
    string TargetTitle,
    int GoldPaidToPrivateVault, // 进入天子私库金额 (万钱)
    int PublicMoralePenalty,    // 天下民心暴跌
    int ScholarLoyaltyPenalty,  // 清流士林耻与为伍惩罚
    string NarrativeTitle,
    string ChronicleText,
    string? ErrorCode = null);
