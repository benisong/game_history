using System;
using System.Collections.Generic;

namespace DonghanEngine.Core;

/// <summary>
/// 五色品阶枚举（红最强 → 灰最弱）。
/// 颜色既用于前端词组着色，也对应后台执行率加成档位。
/// </summary>
public enum TraitTier
{
    Gray = 0,    // ⚪ 灰  0-34   平庸/负面   系数 ×0.5
    Blue = 1,    // 🔵 蓝  35-54  普通        系数 ×0.8
    Purple = 2,  // 🟣 紫  55-74  精良(中线)  系数 ×1.0
    Gold = 3,    // 🟡 金  75-89  史诗        系数 ×1.2
    Red = 4      // 🔴 红  90-100 神级        系数 ×1.5
}

/// <summary>
/// 能力维度枚举。野心(Ambition)本期不派生词组(隐藏、可变，后期单独处理)。
/// 廉洁(Integrity)为品性维，不参与"主词组"竞争，仅作侧词组/情报揭示。
/// </summary>
public enum TraitDimension
{
    Martial,     // 武力
    Leadership,  // 统帅
    Politics,    // 政治
    Charisma,    // 魅力
    Integrity    // 廉洁(品性，不进主词组)
}

/// <summary>
/// "藏锋于词" 词组库（玩家可见的唯一信息层）。
///
/// 设计核心：五维真实数值对玩家完全隐藏(后台黑盒)，玩家只能看到由数值
/// 区间【确定映射】派生出的词组，并据此"猜"NPC 的成色 → 信息不完全导致
/// 不同玩家判断不同、产生不同结局。
///
/// - 能力四维(武/统/政/魅)：每维 × 五档(红金紫蓝灰) → 一个词组。
/// - 廉洁(Integrity)：单独五档词组，不进主词组候选，仅侧词组/情报揭示。
/// - 主词组：取武/统/政/魅中【最高】维的档位词组(确定映射，玩家可背)。
/// - 模糊风评：一句不指向具体维度、可带误导性的综合评语，给感觉但不可推算。
/// </summary>
public static class TraitVocabulary
{
    // 区间 → 品阶。中线落在「精良/紫」= ×1.0。
    public static TraitTier TierOf(int value)
    {
        if (value >= 90) return TraitTier.Red;
        if (value >= 75) return TraitTier.Gold;
        if (value >= 55) return TraitTier.Purple;
        if (value >= 35) return TraitTier.Blue;
        return TraitTier.Gray;
    }

    // 品阶 → 执行率加成系数(后台黑盒，玩家不可见)。
    public static double MultiplierOf(TraitTier tier) => tier switch
    {
        TraitTier.Red => 1.5,
        TraitTier.Gold => 1.2,
        TraitTier.Purple => 1.0,
        TraitTier.Blue => 0.8,
        TraitTier.Gray => 0.5,
        _ => 1.0
    };

    // 品阶 → 前端颜色名(供 UI 着色，BBCode/十六进制由前端 Style 决定)。
    public static string ColorNameOf(TraitTier tier) => tier switch
    {
        TraitTier.Red => "红",
        TraitTier.Gold => "金",
        TraitTier.Purple => "紫",
        TraitTier.Blue => "蓝",
        TraitTier.Gray => "灰",
        _ => "紫"
    };

    // 品阶 → 前端十六进制色值(汉风配色)。
    public static string ColorHexOf(TraitTier tier) => tier switch
    {
        TraitTier.Red => "#c0392b",    // 朱红
        TraitTier.Gold => "#d4af37",   // 鎏金
        TraitTier.Purple => "#8e44ad", // 绛紫
        TraitTier.Blue => "#2e6da4",   // 靛蓝
        TraitTier.Gray => "#7f8c8d",   // 灰
        _ => "#8e44ad"
    };

    // === 四能力维 × 五档 词组表 ===
    // 索引: [维度][品阶]。品阶用 TraitTier 的 int 值(0灰..4红)。
    private static readonly Dictionary<TraitDimension, string[]> _vocab = new()
    {
        // 顺序: [Gray, Blue, Purple, Gold, Red]
        [TraitDimension.Martial]    = new[] { "手无缚鸡", "略通拳脚", "孔武有力", "勇冠三军", "万人之敌" },
        [TraitDimension.Leadership] = new[] { "不闲军旅", "粗谙韬略", "知兵善阵", "治军严整", "韩白之才" },
        [TraitDimension.Politics]   = new[] { "不学无术", "粗理庶务", "擅长民政", "王佐之才", "经天纬地" },
        [TraitDimension.Charisma]   = new[] { "声名狼藉", "泛泛之名", "温文尔雅", "德高望重", "众望所归" },
        // 廉洁(品性): 低=贪(负面)，高=清。不进主词组。
        [TraitDimension.Integrity]  = new[] { "贪得无厌", "手脚不净", "持守有度", "清正廉洁", "两袖清风" },
    };

    /// <summary>
    /// 取某维度在某数值下的词组(确定映射)。
    /// </summary>
    public static string WordOf(TraitDimension dim, int value)
        => _vocab[dim][(int)TierOf(value)];

    public static string DimensionLabel(TraitDimension dim) => dim switch
    {
        TraitDimension.Martial => "武",
        TraitDimension.Leadership => "统",
        TraitDimension.Politics => "政",
        TraitDimension.Charisma => "魅",
        TraitDimension.Integrity => "廉",
        _ => "?"
    };

    // === 模糊综合风评库 ===
    // 不指向具体维度、可带误导性。给玩家"感觉"但无法推算具体属性。
    // 按"整体成色"粗分三组(高/中/低)，每组多条，由 id 种子稳定选取。
    // 注意:风评故意笼统，且高/中/低的判定本身有模糊地带，不可当精确信号。
    public static readonly string[] ReputeHigh = new[]
    {
        "朝野俱称其能，士林引为时望。",
        "百官多有称道，然其深浅难测。",
        "声誉隆于一时，褒之者众。",
        "时人许为国器，名重当世。",
    };
    public static readonly string[] ReputeMid = new[]
    {
        "朝野褒贬不一，毁誉参半。",
        "其人莫测，众论纷纭。",
        "或誉或讥，未有定评。",
        "在朝多年，名实之间，外人难辨。",
    };
    public static readonly string[] ReputeLow = new[]
    {
        "市井颇有微词，清议不甚许之。",
        "名声平平，少有称道者。",
        "时人多所轻之，然或有遗才之讥。",
        "风评不佳，然究系实情抑或构陷，未可知也。",
    };
}
