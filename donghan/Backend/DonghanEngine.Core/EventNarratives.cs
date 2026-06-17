using System.Collections.Generic;

namespace DonghanEngine.Core;

// P3 事件叙事：
//   - 与 trigger 硬逻辑（黄巾/何进之死/董卓进京）解耦，独立维护"叙事层"
//   - 触发条件：(year, month, xun) 精确匹配
//   - 触发效果：AddToChronicle 一行"叙事感"文字（与 trigger 自己的 AddToChronicle 并存，丰富历史感）
//   - 防重复：每个 EventNarrative 只能触发一次
// 为什么不直接合并到 trigger：叙事文案迭代频繁（玩家对"读起来怎么样"敏感），
// 但 trigger 硬逻辑变更代价高（影响游戏平衡）。分层后两者可独立演进。
public record EventNarrative(
    string Id,
    string Title,
    string Description,    // 触发时追加到 Chronicle 的叙事文本
    int TriggerYear,
    int TriggerMonth,
    int TriggerXun,
    string Category        // "黄巾" / "政变" / "驾崩" — 供前端按类目着色
);

public static class EventNarratives
{
    // 静态注册表：3 个事件（按时间排序：184 黄巾 → 189 何进之死 → 189 董卓进京）
    // 注意：实际 trigger 硬逻辑保留在 GameEngine（TriggerHistoricalYellowTurban / TriggerHeJinDeath / TriggerDongZhuoEntry），
    //       本层只追加"叙事感"文字，不重复 trigger 的硬日志。
    private static readonly List<EventNarrative> _all = new()
    {
        new(
            Id: "yellow_turban_184_4_2",
            Title: "黄巾起事",
            Description: "中平元年三月，唐周告密，张角被迫提前起事。太平道数十万信众头裹黄巾，自冀兖豫三州并起，旬月之间天下响应。",
            TriggerYear: 184, TriggerMonth: 4, TriggerXun: 2,
            Category: "黄巾"
        ),
        new(
            Id: "he_jin_death_189_8_3",
            Title: "何进伏诛",
            Description: "中平六年八月，大将军何进谋诛十常侍，反被张让等矫诏伏诛于嘉德殿前。袁绍、袁术等率私兵围宫，外戚与阉宦之祸延及内廷。",
            TriggerYear: 189, TriggerMonth: 8, TriggerXun: 3,
            Category: "政变"
        ),
        new(
            Id: "dong_zhuo_entry_189_9_1",
            Title: "董卓入京",
            Description: "中平六年九月，何进死后京城大乱。凉州军阀董卓闻讯，率三千西凉精兵入京，旋即废少帝立献帝，独揽朝纲。",
            TriggerYear: 189, TriggerMonth: 9, TriggerXun: 1,
            Category: "政变"
        ),
    };

    // 全表
    public static IReadOnlyList<EventNarrative> All => _all;

    // 按 ID 查
    public static EventNarrative? TryGet(string id)
    {
        foreach (var n in _all)
        {
            if (n.Id == id) return n;
        }
        return null;
    }

    // 按时间查所有应触发的事件
    public static IEnumerable<EventNarrative> FindTriggering(int year, int month, int xun)
    {
        foreach (var n in _all)
        {
            if (n.TriggerYear == year && n.TriggerMonth == month && n.TriggerXun == xun)
            {
                yield return n;
            }
        }
    }
}
