using System.Collections.Generic;

namespace DonghanEngine.Core;

// P2-5 朝会 Stance 派系基准
// 给定 NPC 政治阵营 + 玩家输入意图，返回该 NPC 应表态的 Stance
//   返回 "AGREED" / "OPPOSE" / null
//   null = 该阵营对此 Intent 不主动表态（不发言）
//
// 当前为最小边界：仅覆盖现有硬编码 4 NPC（曹操 / 何进 / 张让 / 蹇硕）× 7 意图
// 未来扩"动态选 NPC" 时，新 NPC 的 Stance 直接查本表即可
public static class FactionStance
{
    // (Faction, Intent) → Stance；矩阵当前条目严格复制原硬编码 EmitFaction 语义
    private static readonly Dictionary<(string Faction, CourtIntent Intent), string> _matrix = new()
    {
        // 赈灾
        [(FactionCatalog.PureStream,    CourtIntent.Relief)]         = "AGREED",
        [(FactionCatalog.EunuchFaction, CourtIntent.Relief)]         = "OPPOSE",
        // 诛杀
        [(FactionCatalog.PureStream,    CourtIntent.Execute)]        = "AGREED",
        // 奖赏
        [(FactionCatalog.PureStream,    CourtIntent.Reward)]         = "AGREED",
        [(FactionCatalog.ImperialClan,  CourtIntent.Reward)]         = "AGREED",
        // 国帑
        [(FactionCatalog.EunuchFaction, CourtIntent.Treasury)]       = "AGREED",
        [(FactionCatalog.ImperialClan,  CourtIntent.Treasury)]       = "OPPOSE",
        // 整军
        [(FactionCatalog.ImperialClan,  CourtIntent.MilitaryBuild)]  = "AGREED",
        [(FactionCatalog.WesternGarden, CourtIntent.MilitaryBuild)]  = "AGREED",
        // 整饬宦官
        [(FactionCatalog.ImperialClan,  CourtIntent.EunuchReform)]   = "AGREED",
        [(FactionCatalog.EunuchFaction, CourtIntent.EunuchReform)]   = "OPPOSE",
        // 举荐
        [(FactionCatalog.PureStream,    CourtIntent.Talent)]         = "AGREED",
        [(FactionCatalog.WesternGarden, CourtIntent.Talent)]         = "AGREED",
    };

    public static string? GetStance(string faction, CourtIntent intent)
    {
        return _matrix.TryGetValue((faction, intent), out var stance) ? stance : null;
    }

    // P2-7：朝会"派系对立"映射，主持人派系 → 与之对立的派系列表
    // 历史逻辑：清流 vs 阉党为根本对立；外戚 vs 阉党 + 西园（皇权 vs 相权 / 私兵）；西园 vs 外戚 + 阉党（天子亲军 vs 后党 / 中官）
    private static readonly Dictionary<string, string[]> _oppositionMatrix = new()
    {
        [FactionCatalog.PureStream]    = new[] { FactionCatalog.EunuchFaction },
        [FactionCatalog.EunuchFaction] = new[] { FactionCatalog.ImperialClan, FactionCatalog.PureStream },
        [FactionCatalog.ImperialClan]  = new[] { FactionCatalog.EunuchFaction, FactionCatalog.WesternGarden },
        [FactionCatalog.WesternGarden] = new[] { FactionCatalog.ImperialClan, FactionCatalog.EunuchFaction },
    };

    public static string[] GetOppositionFactions(string faction)
    {
        return _oppositionMatrix.TryGetValue(faction, out var opp) ? opp : System.Array.Empty<string>();
    }
}
