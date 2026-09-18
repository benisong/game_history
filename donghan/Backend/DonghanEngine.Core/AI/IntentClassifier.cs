using System.Linq;
using System.Text.RegularExpressions;

namespace DonghanEngine.Core;

// P2-6 朝会玩家输入意图分类器
// 把一行朱批文本分类为 (Intent, Intensity, TargetNpcId?) 三元组
//   Intensity 1-3: 1=轻/小, 2=中, 3=重/大（从"X 万/千" 数额或强副词推断）
//   TargetNpcId: 玩家在批文中点名的大臣 id（"抄何进" → "he_jin"），null = 未点名
public enum CourtIntent
{
    Unknown,         // 兜底：未匹配任何意图
    Relief,          // 赈/灾/饥/旱/流民
    Execute,         // 抄/诛/捕/杀
    Reward,          // 赏/赐/封/升/嘉
    Treasury,        // 国帑/筹/税/卖官
    MilitaryBuild,   // 整军/练兵/募/北军/西园/校尉
    EunuchReform,    // 中官/宦官/阉/裁抑/训诫/冷落
    Talent,          // 举/召/才/贤/荐
    Decline,         // 驳/不/停/暂缓/罢/免
    Intel,           // 探/查/报/密探/细作
    Travel,          // 驾/起驾/往/赴
    Idle,            // 静候/卿/退朝/无旨
}

public readonly record struct IntentClassification(
    CourtIntent Intent,
    int Intensity,
    string? TargetNpcId);

public static class IntentClassifier
{
    // 常见 NPC 名字 → id 映射（覆盖玩家最可能点名的群臣）
    // 不在表内的名字视作未点名（target = null）
    private static readonly (string Id, string[] Keys)[] KnownOfficers =
    {
        ("he_jin",     new[] { "何进", "大将军" }),
        ("zhang_rang", new[] { "张让", "中常侍", "常侍" }),
        ("cao_cao",    new[] { "曹操", "孟德" }),
        ("jian_shuo",  new[] { "蹇硕", "上军校尉" }),
        ("yuan_shao",  new[] { "袁绍", "本初" }),
        ("yuan_shu",   new[] { "袁术", "公路" }),
        ("dong_zhuo",  new[] { "董卓", "仲颖" }),
        ("lu_zhi",     new[] { "卢植", "子干" }),
        ("huangfu_song", new[] { "皇甫嵩", "义真" }),
        ("zhu_jun",    new[] { "朱儁", "公伟" }),
    };

    public static IntentClassification Classify(string playerInput)
    {
        if (string.IsNullOrWhiteSpace(playerInput))
            return new IntentClassification(CourtIntent.Unknown, 0, null);

        string s = playerInput.Trim();

        // 1) 意图分类（按从强到弱优先级匹配；首个命中即定）
        CourtIntent intent = CourtIntent.Unknown;
        if (HitAny(s, "赈", "灾", "饥", "旱", "流民", "饥民"))
            intent = CourtIntent.Relief;
        else if (HitAny(s, "抄", "诛", "捕", "杀", "斩"))
            intent = CourtIntent.Execute;
        else if (HitAny(s, "赏", "赐", "封", "升", "嘉"))
            intent = CourtIntent.Reward;
        else if (HitAny(s, "国帑", "筹", "税", "卖官", "库银"))
            intent = CourtIntent.Treasury;
        else if (HitAny(s, "整军", "练兵", "募兵", "北军", "西园军", "校尉", "募"))
            intent = CourtIntent.MilitaryBuild;
        else if (HitAny(s, "中官", "宦官", "阉", "裁抑", "训诫", "冷落", "十常侍"))
            intent = CourtIntent.EunuchReform;
        else if (HitAny(s, "举", "召", "才", "贤", "荐", "召见"))
            intent = CourtIntent.Talent;
        else if (HitAny(s, "退朝", "静候", "卿等", "无旨"))
            intent = CourtIntent.Idle;
        else if (HitAny(s, "驳", "罢", "免", "暂缓", "不许", "不准"))
            intent = CourtIntent.Decline;
        else if (HitAny(s, "探", "查", "报", "密探", "细作", "暗探"))
            intent = CourtIntent.Intel;
        else if (HitAny(s, "起驾", "驾临", "前往", "赴"))
            intent = CourtIntent.Travel;

        // 2) 强度推断：抓"X 万/千/百" 数额 + 强副词
        int intensity = QuantifyIntensity(s);

        // 3) 目标 NPC 提取
        string? target = ExtractTargetNpc(s);

        return new IntentClassification(intent, intensity, target);
    }

    private static bool HitAny(string s, params string[] keys) => keys.Any(s.Contains);

    private static int QuantifyIntensity(string s)
    {
        // 强副词
        if (HitAny(s, "重", "大", "速", "急", "严", "尽"))
            return 3;
        if (HitAny(s, "稍", "微", "小", "略"))
            return 1;

        // 数额：抓"X 万" 或"X 千" 或"X 百"
        var m = Regex.Match(s, @"(\d+)\s*[万千百]");
        if (m.Success && int.TryParse(m.Groups[1].Value, out int n))
        {
            // 万：>=2000 算 3，>=500 算 2，否则 1
            if (s.Contains("万"))
                return n >= 2000 ? 3 : (n >= 500 ? 2 : 1);
            // 千：>=50 算 3
            if (s.Contains("千"))
                return n >= 50 ? 3 : (n >= 10 ? 2 : 1);
        }
        return 1;
    }

    private static string? ExtractTargetNpc(string s)
    {
        // 取第一个命中的（顺序：何进 → 张让 → 曹操 → ...）
        // 避免同时多个命中导致歧义：返回第一个
        foreach (var (id, keys) in KnownOfficers)
        {
            if (keys.Any(s.Contains)) return id;
        }
        return null;
    }
}
