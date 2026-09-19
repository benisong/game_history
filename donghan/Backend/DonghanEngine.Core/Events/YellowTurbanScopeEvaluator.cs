using System.Collections.Generic;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 184 年 4 月黄巾起事的烈度与波及州郡（单一职责）
/// </summary>
public sealed class YellowTurbanScopeEvaluator : IYellowTurbanScopeEvaluator
{
    public YellowTurbanScopeResult EvaluateScope(GameState state)
    {
        var affected = new List<string>();

        // 1. 全局民心极度衰颓 (< 20)：全境大溃决 (Catastrophic)
        if (state.PopularSupport < 20)
        {
            affected.AddRange(new[] { "jizhou", "yuzhou", "yanzhou", "qingzhou", "xuzhou", "bingzhou" });
            return new YellowTurbanScopeResult(
                YellowTurbanIntensity.Catastrophic,
                affected,
                "【特大国难】黄巾全境大溃决！天下六州同反，烽火直逼洛阳！",
                "【国难】黄巾大起义全面爆发！冀、豫、兖、青、徐、并六州同时沦陷，张角号称百万信众，洛阳震恐！",
                PopularSupportDrop: 25);
        }

        // 2. 冀州为张角老巢，必然起事
        affected.Add("jizhou");

        // 3. 评估豫州与兖州是否被太守/民心稳住
        // 开局默认状态下：豫州卢植为55分。
        // 史实中原大起义设定：豫州需要达到 58 分以上（即天子施恩或开仓）才能保住豫州不反。
        // 故 threshold 设定为 58（55 < 58 -> 起事）
        bool yuzhouRebels = ShouldProvinceRebel(state, "yuzhou", supportThreshold: 58);
        bool yanzhouRebels = ShouldProvinceRebel(state, "yanzhou", supportThreshold: 45);

        if (yuzhouRebels) affected.Add("yuzhou");
        if (yanzhouRebels) affected.Add("yanzhou");

        // 4. 判定形态
        if (affected.Count >= 3)
        {
            // 史实三州起义 (Historical)
            return new YellowTurbanScopeResult(
                YellowTurbanIntensity.Historical,
                affected,
                "【天下大变】太平道张角起事！中原三州黄巾蜂起！",
                "【大变】太平道妖道张角自称“天公将军”，唐周事泄提前起兵，冀州、豫州、兖州三州大乱，四方响应！",
                PopularSupportDrop: 15);
        }
        else if (affected.Count >= 2)
        {
            // 局势受控 (Contained)
            return new YellowTurbanScopeResult(
                YellowTurbanIntensity.Contained,
                affected,
                "【黄巾之乱】张角起事冀州，中原部分州郡受乱军波及！",
                $"【边报】张角于冀州起兵作乱，波及中原，但朝廷预先安抚使部分郡县守军稳守，未全线溃败！",
                PopularSupportDrop: 10);
        }
        else
        {
            // 极度精细化治理，仅冀州孤立起事，豫/兖均被良吏稳住 (Contained / Localized)
            return new YellowTurbanScopeResult(
                YellowTurbanIntensity.Contained,
                affected,
                "【地方剧变】张角冀州叛乱，得益于天子政务良治，中原腹地未被波及！",
                "【战报】张角于冀州仓促发难，然豫州、兖州得良吏安抚民心，黄巾贼党无法形成中原合流之势！",
                PopularSupportDrop: 5);
        }
    }

    private static bool ShouldProvinceRebel(GameState state, string provinceId, int supportThreshold)
    {
        if (!state.Provinces.TryGetValue(provinceId, out var p)) return true;
        
        // 若太守在任、不处于敌对状态，且民心达到防御阈值，则太守保境安民成功，免于全面陷落
        if (p.GovernorId != null &&
            state.Npcs.TryGetValue(p.GovernorId, out var gov) &&
            gov.IsActive && !gov.IsHostile &&
            p.LocalSupport >= supportThreshold)
        {
            return false;
        }

        // 无太守或民心低于阈值，黄巾贼帅趁虚而起
        return true;
    }
}
