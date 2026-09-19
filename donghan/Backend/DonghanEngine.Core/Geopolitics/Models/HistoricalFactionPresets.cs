using System.Collections.Generic;
using DonghanEngine.Core.Geopolitics.Models;

namespace DonghanEngine.Core.Geopolitics.Models;

/// <summary>
/// 历史诸侯势力预置档案库：定义 189 年后天下诸侯的初始领地、兵力、粮秣与性格倾向（静态数据源）
/// </summary>
public static class HistoricalFactionPresets
{
    public static IReadOnlyList<WarlordFaction> CreateInitialFactions()
    {
        return new List<WarlordFaction>
        {
            // 1. 曹操军团（兖州 - 称霸枭雄）
            new(
                factionId: "cao_cao",
                leaderNpcId: "cao_cao",
                factionName: "曹操军团",
                posture: WarlordPosture.AmbitiousWarlord,
                initialProvinces: new[] { "yanzhou" },
                initialTroops: 12000,
                initialProvisions: 6000,
                initialLoyalty: 60,
                initialAmbition: 92),

            // 2. 袁绍军团（冀州 - 称霸枭雄）
            new(
                factionId: "yuan_shao",
                leaderNpcId: "yuan_shao",
                factionName: "袁绍军团",
                posture: WarlordPosture.AmbitiousWarlord,
                initialProvinces: new[] { "jizhou" },
                initialTroops: 20000,
                initialProvisions: 10000,
                initialLoyalty: 45,
                initialAmbition: 88),

            // 3. 孙坚/孙策军团（扬州 - 称霸枭雄）
            new(
                factionId: "sun_jian",
                leaderNpcId: "sun_jian",
                factionName: "孙氏江东军",
                posture: WarlordPosture.AmbitiousWarlord,
                initialProvinces: new[] { "yangzhou" },
                initialTroops: 10000,
                initialProvisions: 5000,
                initialLoyalty: 55,
                initialAmbition: 85),

            // 4. 皇甫嵩军团（并州/关西 - 忠贞藩屏）
            new(
                factionId: "huangfu_song",
                leaderNpcId: "huangfu_song",
                factionName: "皇甫嵩左军",
                posture: WarlordPosture.LoyalistBanner,
                initialProvinces: new[] { "bingzhou" },
                initialTroops: 8000,
                initialProvisions: 4000,
                initialLoyalty: 95,
                initialAmbition: 15),

            // 5. 刘虞军团（幽州 - 忠贞藩屏）
            new(
                factionId: "liu_yu",
                leaderNpcId: "liu_yu",
                factionName: "刘虞幽州军",
                posture: WarlordPosture.LoyalistBanner,
                initialProvinces: new[] { "youzhou" },
                initialTroops: 6000,
                initialProvisions: 5000,
                initialLoyalty: 90,
                initialAmbition: 10),

            // 6. 刘表军团（荆州 - 自守宗室）
            new(
                factionId: "liu_biao",
                leaderNpcId: "liu_biao",
                factionName: "刘表荆襄军",
                posture: WarlordPosture.CautiousAutonomist,
                initialProvinces: new[] { "jingzhou" },
                initialTroops: 15000,
                initialProvisions: 12000,
                initialLoyalty: 65,
                initialAmbition: 30),

            // 7. 刘焉军团（益州 - 自守宗室）
            new(
                factionId: "liu_yan",
                leaderNpcId: "liu_yan",
                factionName: "刘焉巴蜀军",
                posture: WarlordPosture.CautiousAutonomist,
                initialProvinces: new[] { "yizhou" },
                initialTroops: 14000,
                initialProvisions: 15000,
                initialLoyalty: 50,
                initialAmbition: 45)
        };
    }
}
