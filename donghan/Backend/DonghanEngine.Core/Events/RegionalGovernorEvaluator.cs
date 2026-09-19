using System.Collections.Generic;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 188 年 3 月刘焉废史立牧倡议因果（单一职责）
/// </summary>
public sealed class RegionalGovernorEvaluator : IRegionalGovernorEvaluator
{
    public RegionalGovernorResult Evaluate(GameState state)
    {
        // 1. 若天子皇权强盛 (ImperialPower >= 60) 且 中央民心较高 (PopularSupport >= 50)：
        // 灵帝自信朝廷可控天下，严词驳回刘焉分镇之议，坚持中央监察流官制度
        if (state.ImperialPower >= 60 && state.PopularSupport >= 50)
        {
            return new RegionalGovernorResult(
                RegionalGovernorDecision.RejectAndKeepCentralized,
                "【太常建言 · 驳回封牧】天子明察宗室之谋，严拒州牧分陕！",
                "【政纲】太常刘焉以盗贼未平请改置州牧，天子虑诸侯尾大不掉，严诏驳回，敕令依旧派刺史监察，天下政令一遵洛阳！",
                ImperialPowerDelta: 5,
                PopularSupportDelta: 5,
                AppointedGovernorIds: new List<string>(),
                ProvinceGovernorMappings: new Dictionary<string, string>(),
                RemoteProvinceSupportBonus: 0);
        }

        // 2. 史实路线：边陲骚动未息，灵帝采纳刘焉之议，废刺史改立州牧，宗室重臣分镇天下
        var mappings = new Dictionary<string, string>
        {
            ["yizhou"] = "liu_yan",   // 益州牧 刘焉
            ["youzhou"] = "liu_yu",   // 幽州牧 刘虞
            ["jingzhou"] = "liu_biao" // 荆州牧 刘表
        };

        return new RegionalGovernorResult(
            RegionalGovernorDecision.AdoptStatePastorSystem,
            "【朝廷改制 · 废史立牧】宗室出镇四方，大汉诸侯分陕割据初成！",
            "【制令】天子采太常刘焉之议，改刺史为州牧，以刘焉督益州、刘虞督幽州、刘表督荆州，重臣出陕，四海安辑，然汉威稍损！",
            ImperialPowerDelta: -10,
            PopularSupportDelta: 10,
            AppointedGovernorIds: new List<string> { "liu_yan", "liu_yu", "liu_biao" },
            ProvinceGovernorMappings: mappings,
            RemoteProvinceSupportBonus: 20);
    }
}
