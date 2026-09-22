namespace DonghanEngine.Core.Politics;

public enum PrestigeState
{
    PuppetVulnerable,   // 0~25: 极度虚弱，权臣觊觎，极易触发【挟天子以令诸侯】/废立架空
    DisrespectedWeak,   // 26~45: 威望偏低，百官轻慢，政令大打折扣(50%~70%)，诸侯截留赋税
    GoldenBalance,      // 46~60: 【紧凑黄金平衡区】恩威并施，政令100%执行，无转嫁压迫，天下最稳
    OppressiveDread,    // 61~80: 威压过甚，百官战栗，部分酷吏/贪官向下转嫁压迫，民怨悄生
    TyrannicalTerror    // 81~100: 雷霆暴政，百官敢怒不敢言，全面压迫底层，民变/黄巾暴乱几率暴增
}

public sealed record PrestigeEvaluationResult(
    int Prestige,
    PrestigeState State,
    double EdictExecutionEfficiency, // 政令执行效率 (0.3 ~ 1.25)
    double OppressionTransferRate,   // 官员向下转嫁压迫概率 (0.0 ~ 0.85)
    double RebellionChanceModifier,  // 百姓民变暴乱几率加成 (-0.2 ~ +0.6)
    bool IsPuppetRiskTriggered,       // 是否触发挟天子风险
    string StatusDescription,
    string NarrativeSummary);
