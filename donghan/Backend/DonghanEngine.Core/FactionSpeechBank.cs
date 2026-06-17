using System;
using System.Collections.Generic;

namespace DonghanEngine.Core;

// P2-7 朝会台词桶：(NpcId, Intent) → 台词模板函数 + fav/pow 变化量
// 模板内可含 {Treasury} / {Health} / {PopularSupport} / {Morale} 占位符，由 GetResolvedText 在生成台词时解析
// 选 NPC 选出来后从本桶查台词；桶未命中则用 GetDefault() 兜底
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

    // 桶未命中时的默认台词（兜底：例如新增 NPC 走派系通用桶但还没写专属文案）
    public static CourtSpeechEntry GetDefault() => new("臣等谨遵圣谕。", 1, 0);

    // 命中桶则返回 entry，未命中返回 null（由调用者决定是否回退到 default）
    public static CourtSpeechEntry? TryGetSpeech(string npcId, CourtIntent intent, GameState state)
    {
        if (_bank.TryGetValue((npcId, intent), out var fn))
        {
            return fn(state);
        }
        return null;
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
