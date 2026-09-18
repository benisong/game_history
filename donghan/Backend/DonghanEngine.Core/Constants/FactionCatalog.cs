namespace DonghanEngine.Core;

// P2-4 朝会派系 / 政治阵营词汇表
// 集中所有硬编码的派系字符串，避免 NpcState.Faction / 派系过滤 / 派系基准三处使用不同词汇
public static class FactionCatalog
{
    // NpcState.Faction 政治阵营（清流派 / 外戚派 / 阉党派 / 西园亲军 / 割据军阀 / 反叛势力）
    public const string PureStream = "清流派";
    public const string ImperialClan = "外戚派";
    public const string EunuchFaction = "阉党派";
    public const string WesternGarden = "西园亲军";
    public const string Warlord = "割据军阀";
    public const string Rebel = "反叛势力";

    // 所有已定义阵营（用于遍历 / 验证）
    public static readonly string[] All = { PureStream, ImperialClan, EunuchFaction, WesternGarden, Warlord, Rebel };
}
