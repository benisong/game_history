using System;
using System.Collections.Generic;
using System.Linq;

namespace DonghanEngine.Core;

/// <summary>
/// 单个派生词组(带品阶与颜色)，供前端着色显示。
/// </summary>
public class DerivedTrait
{
    public TraitDimension Dimension { get; set; }
    public string Word { get; set; } = string.Empty;
    public TraitTier Tier { get; set; }
    public string ColorHex { get; set; } = string.Empty;
    public string DimensionLabel { get; set; } = string.Empty;
}

/// <summary>
/// NPC 平时(朝堂/名册)可见的信息卡:1 个主词组 + 1 句模糊风评。
/// 真实数值不在此结构中暴露。
/// </summary>
public class NpcGlimpse
{
    public DerivedTrait Primary { get; set; } = new();  // 主词组(最高能力维，确定映射)
    public string Repute { get; set; } = string.Empty;  // 模糊综合风评(可带误导)
}

/// <summary>
/// 词组派生器：把后台真实五维数值翻译成玩家可见的词组信号。
///
/// 玩家平时只见 NpcGlimpse(主词组 + 风评)；逐维度真相需走情报查探
/// (BuildIntelAssessment，准确度受情报头目能力影响)。
///
/// 稳定性：主词组是确定映射(纯数值函数)；风评用 NPC id 哈希作种子，
/// 保证同一 NPC 每次显示一致，玩家无法靠反复刷新拼出全部属性。
/// </summary>
public static class TraitDeriver
{
    // 参与"主词组"竞争的能力维(野心本期不派生；廉洁是品性，不进主词组)。
    private static readonly TraitDimension[] AbilityDims =
    {
        TraitDimension.Martial,
        TraitDimension.Leadership,
        TraitDimension.Politics,
        TraitDimension.Charisma
    };

    private static int ValueOf(NpcState npc, TraitDimension dim) => dim switch
    {
        TraitDimension.Martial => npc.Martial,
        TraitDimension.Leadership => npc.Leadership,
        TraitDimension.Politics => npc.Politics,
        TraitDimension.Charisma => npc.Charisma,
        TraitDimension.Integrity => npc.Integrity,
        _ => 0
    };

    private static DerivedTrait MakeTrait(NpcState npc, TraitDimension dim)
    {
        int v = ValueOf(npc, dim);
        var tier = TraitVocabulary.TierOf(v);
        return new DerivedTrait
        {
            Dimension = dim,
            Word = TraitVocabulary.WordOf(dim, v),
            Tier = tier,
            ColorHex = TraitVocabulary.ColorHexOf(tier),
            DimensionLabel = TraitVocabulary.DimensionLabel(dim)
        };
    }

    /// <summary>
    /// 主词组：武/统/政/魅中数值最高的那一维(并列时按固定顺序取第一个，保证确定性)。
    /// </summary>
    public static DerivedTrait GetPrimaryTrait(NpcState npc)
    {
        TraitDimension best = AbilityDims[0];
        int bestVal = ValueOf(npc, best);
        foreach (var dim in AbilityDims)
        {
            int v = ValueOf(npc, dim);
            if (v > bestVal) { bestVal = v; best = dim; }
        }
        return MakeTrait(npc, best);
    }

    /// <summary>
    /// 平时可见信息卡：主词组 + 模糊风评。风评由 id 种子稳定选取。
    /// </summary>
    public static NpcGlimpse GetGlimpse(NpcState npc)
    {
        var primary = GetPrimaryTrait(npc);
        // 整体成色 = 四能力维均值，用于粗分风评高/中/低(故意笼统)。
        int avg = (npc.Martial + npc.Leadership + npc.Politics + npc.Charisma) / 4;
        string[] pool = avg >= 70 ? TraitVocabulary.ReputeHigh
                      : avg >= 45 ? TraitVocabulary.ReputeMid
                      : TraitVocabulary.ReputeLow;
        int seed = StableSeed(npc.Id);
        string repute = pool[seed % pool.Length];
        return new NpcGlimpse { Primary = primary, Repute = repute };
    }

    /// <summary>
    /// 情报查探：逐维度评价。供阶段2情报页使用。
    /// accuracy: 情报头目能力(0-100)。越高，给出的词组越贴近真实档位；
    /// 越低，越可能偏移(往高报或往低报)，造成误导。
    /// 返回:每个维度(含廉洁) → 玩家所见的(可能失真的)词组。
    /// </summary>
    public static Dictionary<TraitDimension, DerivedTrait> BuildIntelAssessment(
        NpcState npc, int accuracy, Random rng)
    {
        var result = new Dictionary<TraitDimension, DerivedTrait>();
        var dims = new[]
        {
            TraitDimension.Martial, TraitDimension.Leadership,
            TraitDimension.Politics, TraitDimension.Charisma,
            TraitDimension.Integrity
        };
        // 失真幅度：accuracy 越低，偏移概率与幅度越大。
        // accuracy>=85 几乎不偏；<=40 经常 ±1~2 档。
        foreach (var dim in dims)
        {
            int real = ValueOf(npc, dim);
            int reported = real;
            int miss = 100 - Math.Clamp(accuracy, 0, 100);
            if (rng.Next(0, 100) < miss)
            {
                // 偏移幅度 1~2 档(每档约 20 点)，方向随机。
                int shiftMag = (rng.Next(0, 100) < miss / 2) ? 2 : 1;
                int dir = rng.Next(0, 2) == 0 ? -1 : 1;
                reported = Math.Clamp(real + dir * shiftMag * 20, 0, 100);
            }
            var tier = TraitVocabulary.TierOf(reported);
            result[dim] = new DerivedTrait
            {
                Dimension = dim,
                Word = TraitVocabulary.WordOf(dim, reported),
                Tier = tier,
                ColorHex = TraitVocabulary.ColorHexOf(tier),
                DimensionLabel = TraitVocabulary.DimensionLabel(dim)
            };
        }
        return result;
    }

    // 稳定哈希(非加密)：同一 id 永远得同一种子。避免用 string.GetHashCode()
    // (后者跨进程不稳定)。
    private static int StableSeed(string s)
    {
        unchecked
        {
            int hash = 17;
            foreach (char c in s) hash = hash * 31 + c;
            return hash & 0x7fffffff;
        }
    }
}
