using System.Collections.Generic;
using Godot;

namespace DonghanFrontend;

public partial class MainScene : Control
{
    private sealed class CourtTopicViewModel
    {
        public string Id { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Summary { get; init; } = string.Empty;
        public List<CourtSpeechViewModel> Speeches { get; init; } = new();
        public List<CourtDecisionViewModel> Decisions { get; init; } = new();
    }

    private sealed class CourtSpeechViewModel
    {
        public string MinisterId { get; init; } = string.Empty;
        public string MinisterName { get; init; } = string.Empty;
        public string Faction { get; init; } = string.Empty;
        public string Speech { get; init; } = string.Empty;
        public string Attitude { get; init; } = string.Empty;
    }

    private sealed class CourtDecisionViewModel
    {
        public string Id { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;
        public string Hint { get; init; } = string.Empty;
    }


    private List<CourtTopicViewModel> CreateDefaultCourtTopics()
    {
        return new List<CourtTopicViewModel>
        {
            new CourtTopicViewModel
            {
                Id = "military_readiness",
                Category = "常议",
                Title = "整军备寇",
                Summary = "北军、西园军久未整备，黄巾乱势未平，朝廷武备不可再弛。",
                Speeches = new List<CourtSpeechViewModel>
                {
                    new() { MinisterId = "he_jin", MinisterName = "何进", Faction = "外戚武臣", Speech = "黄巾虽乱，朝廷威灵尚在。臣请整北军，明示天下。", Attitude = "请战 / 扩权" },
                    new() { MinisterId = "zhang_rang", MinisterName = "张让", Faction = "中官近侍", Speech = "大将军动兵，所费国帑不可不察。奴才以为，应先核军费。", Attitude = "制衡外戚 / 掌财" },
                    new() { MinisterId = "cao_cao", MinisterName = "曹操", Faction = "西园武臣", Speech = "兵贵神速。若迟疑数旬，贼势恐连动并州。", Attitude = "请任 / 速战" },
                    new() { MinisterId = "jian_shuo", MinisterName = "蹇硕", Faction = "西园武臣", Speech = "西园诸校尉本为陛下亲军，愿为天子先驱。", Attitude = "主张动用西园军" }
                },
                Decisions = new List<CourtDecisionViewModel>
                {
                    new() { Id = "重赏何进，命其整军备寇", Label = "准何进整北军", Hint = "走现有朝会输入流程，增强外戚军务存在感。" },
                    new() { Id = "travel_garden", Label = "命曹操整西园军", Hint = "转往西园，后续可阅兵或募兵。" },
                    new() { Id = "命张让核查军费", Label = "令张让核军费", Hint = "走现有朝会输入流程，让中官介入财计。" },
                    new() { Id = "intel", Label = "转黄门密札详查", Hint = "打开情报面板查看州郡与叛乱详情。" }
                }
            },
            new CourtTopicViewModel
            {
                Id = "treasury",
                Category = "常议",
                Title = "国帑筹措",
                Summary = "国库支出渐重，军费、赈济与宫禁开销皆需筹划。",
                Speeches = new List<CourtSpeechViewModel>
                {
                    new() { MinisterId = "zhang_rang", MinisterName = "张让", Faction = "中官近侍", Speech = "奴才愿为陛下查核诸库，绝不令军国大计因钱粮误事。", Attitude = "掌财 / 固宠" },
                    new() { MinisterId = "he_jin", MinisterName = "何进", Faction = "外戚武臣", Speech = "军费关乎社稷，不可尽付中官之手。", Attitude = "防宦 / 护军" },
                    new() { MinisterId = "cao_cao", MinisterName = "曹操", Faction = "西园武臣", Speech = "兵无粮则散，财无制则乱。臣以为当先定军费名目。", Attitude = "务实 / 求制" }
                },
                Decisions = new List<CourtDecisionViewModel>
                {
                    new() { Id = "命张让筹措国帑", Label = "令张让筹措内帑", Hint = "走现有朝会输入流程。" },
                    new() { Id = "travel_garden", Label = "前往西园筹资", Hint = "转往西园处理私库、卖官与军务。" },
                    new() { Id = "back_topics", Label = "暂缓筹措", Hint = "回到今日可议。" }
                }
            },
            new CourtTopicViewModel
            {
                Id = "eunuchs",
                Category = "常议",
                Title = "整饬宦官",
                Summary = "外廷屡言中官弄权，中官则称内廷忠心，朝局暗潮汹涌。",
                Speeches = new List<CourtSpeechViewModel>
                {
                    new() { MinisterId = "he_jin", MinisterName = "何进", Faction = "外戚武臣", Speech = "中官干政，朝纲日坏。臣请陛下稍裁其权，以安百官。", Attitude = "裁抑中官" },
                    new() { MinisterId = "zhang_rang", MinisterName = "张让", Faction = "中官近侍", Speech = "奴才等侍奉禁中，所恃不过陛下一念信任。外臣此言，其心可诛。", Attitude = "自保 / 反击" },
                    new() { MinisterId = "cao_cao", MinisterName = "曹操", Faction = "西园武臣", Speech = "外戚、中官相争，最忌骤激。陛下宜以制衡为先。", Attitude = "缓治 / 制衡" }
                },
                Decisions = new List<CourtDecisionViewModel>
                {
                    new() { Id = "训诫张让", Label = "训诫张让", Hint = "现有 Mock 已支持该关键词。" },
                    new() { Id = "重赏张让", Label = "安抚张让", Hint = "以圣眷压下外廷攻讦。" },
                    new() { Id = "back_topics", Label = "暂不激化", Hint = "回到今日可议。" }
                }
            },
            new CourtTopicViewModel
            {
                Id = "talent",
                Category = "常议",
                Title = "举荐将才",
                Summary = "乱世将起，朝廷需择可用之才，或委以军务，或派往地方。",
                Speeches = new List<CourtSpeechViewModel>
                {
                    new() { MinisterId = "he_jin", MinisterName = "何进", Faction = "外戚武臣", Speech = "军国用人，当以资望为先，不可轻任少年。", Attitude = "护外戚军权" },
                    new() { MinisterId = "cao_cao", MinisterName = "曹操", Faction = "西园武臣", Speech = "臣不敢自夸，愿以实绩报陛下知遇。", Attitude = "自请任事" },
                    new() { MinisterId = "jian_shuo", MinisterName = "蹇硕", Faction = "西园武臣", Speech = "西园诸校尉皆陛下亲擢，正可分外廷之权。", Attitude = "扶植西园" }
                },
                Decisions = new List<CourtDecisionViewModel>
                {
                    new() { Id = "show_cao", Label = "召见曹操", Hint = "打开曹操详情。" },
                    new() { Id = "show_jian", Label = "召见蹇硕", Hint = "打开蹇硕详情。" },
                    new() { Id = "intel", Label = "转黄门密札任官", Hint = "打开情报面板处理地方任免。" }
                }
            },
            new CourtTopicViewModel
            {
                Id = "free",
                Category = "自由",
                Title = "亲拟圣旨",
                Summary = "陛下可直接口召百官，乾纲独断。",
                Speeches = new List<CourtSpeechViewModel>
                {
                    new() { MinisterId = "zhang_rang", MinisterName = "张让", Faction = "中官近侍", Speech = "陛下亲裁，奴才即刻传旨；只是圣意若涉军国，还请明示轻重。", Attitude = "奉旨 / 观望" },
                    new() { MinisterId = "he_jin", MinisterName = "何进", Faction = "外戚武臣", Speech = "臣等静候圣断。若需动兵，北军可听陛下调遣。", Attitude = "候旨 / 请任" }
                },
                Decisions = new List<CourtDecisionViewModel>
                {
                    new() { Id = "free_edict", Label = "铺开黄绢亲拟", Hint = "展开自由输入框。" },
                    new() { Id = "back_topics", Label = "返回议题", Hint = "回到今日可议。" }
                }
            }
        };
    }

}
