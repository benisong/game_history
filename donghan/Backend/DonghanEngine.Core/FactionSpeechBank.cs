using System;
using System.Collections.Generic;

namespace DonghanEngine.Core;

// P2-7 朝会台词桶：
//   1. 专属桶 (NpcId, Intent) → 动态生成器（state 用于 Treasury/Health 等运行时数据）
//   2. 派系通用桶 (Faction, Intent) → 静态文案（无运行时分支）
//   3. GetDefault() 兜底
// 选 NPC 命中优先级：专属桶 → 派系通用桶 → 默认桶
// 模板内可含 {Treasury} / {Health} / {PopularSupport} / {Morale} 占位符，由 ResolveTemplates 解析
public record CourtSpeechEntry(string Text, int FavDelta, int PowDelta);

public static class FactionSpeechBank
{
    // (NpcId, Intent) → 动态生成器（state 用于 Treasury/Health 等运行时数据）
    private static readonly Dictionary<(string NpcId, CourtIntent Intent), Func<GameState, CourtSpeechEntry>> _bank = new()
    {
        // 曹操(清流派) — 4 条
        [("cao_cao", CourtIntent.Relief)] = _ => new CourtSpeechEntry(
            "陛下圣明！赈灾乃安民之本，臣愿领旨督办！", 5, 2),
        [("cao_cao", CourtIntent.Execute)] = _ => new CourtSpeechEntry(
            "臣附议！乱臣贼子，人人得而诛之！", 10, 5),
        [("cao_cao", CourtIntent.Reward)] = _ => new CourtSpeechEntry(
            "陛下隆恩浩荡！臣定当鞠躬尽瘁！", 15, 3),
        [("cao_cao", CourtIntent.Talent)] = _ => new CourtSpeechEntry(
            "臣不敢自夸，愿以实绩报陛下知遇。", 5, 2),

        // 何进(外戚派) — 4 条
        [("he_jin", CourtIntent.Reward)] = _ => new CourtSpeechEntry(
            "臣谢陛下隆恩！臣定当整军备战，保大汉无虞！", 15, 5),
        [("he_jin", CourtIntent.Treasury)] = _ => new CourtSpeechEntry(
            "军费关乎社稷，不可尽付中官之手。", -2, 0),
        [("he_jin", CourtIntent.MilitaryBuild)] = _ => new CourtSpeechEntry(
            "黄巾虽乱，朝廷威灵尚在。臣请整北军，明示天下。", 3, 2),
        [("he_jin", CourtIntent.EunuchReform)] = _ => new CourtSpeechEntry(
            "中官干政，朝纲日坏。臣请陛下稍裁其权，以安百官。", 5, 2),

        // 张让(阉党派) — 3 条
        [("zhang_rang", CourtIntent.Relief)] = _ => new CourtSpeechEntry(
            "陛下，国库仅 {Treasury} 万钱……不如由奴才来经办，定能省下不少银两。", -3, 0),
        [("zhang_rang", CourtIntent.Treasury)] = s => s.Treasury < 3000
            ? new CourtSpeechEntry(
                "国库仅 {Treasury} 万钱，奴才愿为陛下查核诸库，纵节衣缩食亦必保军国无误。", 3, 2)
            : new CourtSpeechEntry(
                "国库充盈（{Treasury} 万钱），奴才愿为陛下查核诸库，绝不令军国大计因钱粮误事。", 3, 2),
        [("zhang_rang", CourtIntent.EunuchReform)] = _ => new CourtSpeechEntry(
            "奴才等侍奉禁中，所恃不过陛下一念信任（龙体 {Health}/100）。外臣此言，其心可诛。", -5, 0),

        // 蹇硕(西园亲军) — 2 条
        [("jian_shuo", CourtIntent.MilitaryBuild)] = _ => new CourtSpeechEntry(
            "西园诸校尉本为陛下亲军，愿为天子先驱。", 3, 2),
        [("jian_shuo", CourtIntent.Talent)] = _ => new CourtSpeechEntry(
            "西园诸校尉皆陛下亲擢，正可分外廷之权。", 3, 2),
    };

    // (Faction, Intent) → 通用模板；新 NPC 走派系通用桶时直接套用
    // fav/pow 按 Stance 决定：AGREED=(3,1) / OPPOSE=(-2,0)
    private static readonly Dictionary<(string Faction, CourtIntent Intent), string> _genericBank = new()
    {
        // 清流派 — 文人风骨，倡改革
        [(FactionCatalog.PureStream,    CourtIntent.Relief)]        = "国库若丰，当赈济灾民以安天下。",
        [(FactionCatalog.PureStream,    CourtIntent.Execute)]       = "乱臣贼子当伏法，以正朝纲。",
        [(FactionCatalog.PureStream,    CourtIntent.Reward)]        = "陛下隆恩，臣等敢不效力。",
        [(FactionCatalog.PureStream,    CourtIntent.Treasury)]      = "度支当明，不使中官侵蚀。",
        [(FactionCatalog.PureStream,    CourtIntent.MilitaryBuild)] = "兵者国之大事，当慎之。",
        [(FactionCatalog.PureStream,    CourtIntent.EunuchReform)]  = "中官干政，非社稷之福。",
        [(FactionCatalog.PureStream,    CourtIntent.Talent)]        = "举贤任能，乃为政之本。",
        // 外戚派 — 皇亲国戚，重权势
        [(FactionCatalog.ImperialClan,  CourtIntent.Relief)]        = "大将军府亦可助赈济。",
        [(FactionCatalog.ImperialClan,  CourtIntent.Execute)]       = "正法乱党，以固朝纲。",
        [(FactionCatalog.ImperialClan,  CourtIntent.Reward)]        = "臣等谢陛下隆恩。",
        [(FactionCatalog.ImperialClan,  CourtIntent.Treasury)]      = "军费关乎社稷，不当尽付中官之手。",
        [(FactionCatalog.ImperialClan,  CourtIntent.MilitaryBuild)] = "黄巾虽乱，朝廷威灵尚在。",
        [(FactionCatalog.ImperialClan,  CourtIntent.EunuchReform)]  = "中官干政，朝纲日坏。",
        [(FactionCatalog.ImperialClan,  CourtIntent.Talent)]        = "本家门生故吏遍天下，皆可举荐。",
        // 阉党派 — 内廷宠臣，掌财固宠
        [(FactionCatalog.EunuchFaction, CourtIntent.Relief)]        = "奴才可督办赈济，定能省下不少银两。",
        [(FactionCatalog.EunuchFaction, CourtIntent.Execute)]       = "奴才等唯陛下之命是从。",
        [(FactionCatalog.EunuchFaction, CourtIntent.Reward)]        = "奴才等受陛下厚恩，粉身碎骨难报。",
        [(FactionCatalog.EunuchFaction, CourtIntent.Treasury)]      = "奴才愿为陛下查核诸库。",
        [(FactionCatalog.EunuchFaction, CourtIntent.MilitaryBuild)] = "西园军本可分忧，奴才可从中协调。",
        [(FactionCatalog.EunuchFaction, CourtIntent.EunuchReform)]  = "奴才等侍奉禁中，所恃不过陛下一念信任。",
        [(FactionCatalog.EunuchFaction, CourtIntent.Talent)]        = "奴才亦知荐贤之义。",
        // 西园亲军 — 天子亲军，皇权象征
        [(FactionCatalog.WesternGarden, CourtIntent.Relief)]        = "西园亦可出力赈济。",
        [(FactionCatalog.WesternGarden, CourtIntent.Execute)]       = "臣愿为陛下执戈，讨此乱贼。",
        [(FactionCatalog.WesternGarden, CourtIntent.Reward)]        = "陛下之恩，西园将士铭感。",
        [(FactionCatalog.WesternGarden, CourtIntent.Treasury)]      = "西园军饷，请陛下明察。",
        [(FactionCatalog.WesternGarden, CourtIntent.MilitaryBuild)] = "西园诸校尉本为陛下亲军。",
        [(FactionCatalog.WesternGarden, CourtIntent.EunuchReform)]  = "西园军只听陛下号令。",
        [(FactionCatalog.WesternGarden, CourtIntent.Talent)]        = "西园诸校尉皆陛下亲擢。",
    };

    // 桶未命中时的默认台词（兜底：派系通用桶也没定义的组合）
    public static CourtSpeechEntry GetDefault() => new("臣等谨遵圣谕。", 1, 0);

    // 命中专属桶则返回 entry
    public static CourtSpeechEntry? TryGetSpeech(string npcId, CourtIntent intent, GameState state)
    {
        if (_bank.TryGetValue((npcId, intent), out var fn))
        {
            return fn(state);
        }
        return null;
    }

    // 命中派系通用桶则按 stance 返回 entry（fav/pow 由 stance 决定）
    public static CourtSpeechEntry? TryGetGenericEntry(string faction, CourtIntent intent, string stance)
    {
        if (!_genericBank.TryGetValue((faction, intent), out var text)) return null;
        var (fav, pow) = stance == "OPPOSE" ? (-2, 0) : stance == "AGREED" ? (3, 1) : (0, 0);
        return new CourtSpeechEntry(text, fav, pow);
    }

    // P2-1：占位符解析（{Treasury}/{Health}/{PopularSupport}/{Morale}）
    public static string ResolveTemplates(string text, GameState state)
    {
        var morale = state.WestGardenArmy?.Morale.ToString() ?? "—";
        return text
            .Replace("{Treasury}", state.Treasury.ToString())
            .Replace("{Health}", state.Health.ToString())
            .Replace("{PopularSupport}", state.PopularSupport.ToString())
            .Replace("{Morale}", morale);
    }
}
