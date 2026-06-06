using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DonghanEngine.Core;

public interface INpcLifecycleManager
{
    // 供 AI 调度员(Scheduler)按需主动调度。进行百官的自然衰老、寿终、疾病生老病死退场结算与新臣登录
    Task ProcessLifecycleStepAsync(GameState state, bool useHighTokenAi);

    // 测试注入用：接受显式 Random 以控制随机性
    Task ProcessLifecycleStepAsync(GameState state, bool useHighTokenAi, Random rng);

    // 提供给可视化界面的本地冷备 NPC 预置名单查询
    List<NpcState> GetPresetNpcsFallback();

    // 根据 NPC 唯一 ID（如 "dong_zhuo"），从预置池中搜索并部署（登庸）到朝局 Npcs 字典中上场
    void DeployNpcToCourt(string npcId, GameState state);
}

public class NpcLifecycleManager : INpcLifecycleManager
{
    private readonly INpcRegistry _registry;

    public NpcLifecycleManager(INpcRegistry registry)
    {
        _registry = registry;
    }

    private List<NpcState>? _cachedPresets;

    private List<NpcState> GetCachedPresets()
    {
        if (_cachedPresets != null) return _cachedPresets;

        string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "donghan_preset_npcs.json");
        try
        {
            if (File.Exists(jsonPath))
            {
                string jsonContent = File.ReadAllText(jsonPath);
                var presets = JsonSerializer.Deserialize<List<NpcState>>(jsonContent);
                if (presets != null && presets.Count > 0)
                {
                    _cachedPresets = presets;
                    return _cachedPresets;
                }
            }
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"[NpcLifecycleManager] A轨 JSON 文件读取失败（IO错误），降级至 B轨硬编码冷备：{ex.Message}");
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"[NpcLifecycleManager] A轨 JSON 格式损坏，降级至 B轨硬编码冷备：{ex.Message}");
        }

        _cachedPresets = GetHardcodedFallbackList();
        return _cachedPresets;
    }

    public List<NpcState> GetPresetNpcsFallback() => GetCachedPresets();

    public void DeployNpcToCourt(string npcId, GameState state)
    {
        if (state.Npcs.ContainsKey(npcId))
        {
            return; // 已经部署在朝堂上，无需重复加载
        }

        // 1. 从冷备 A轨/B轨 列表中寻找对应的静态资料模板
        var presets = GetPresetNpcsFallback();
        var template = presets.Find(n => n.Id == npcId);

        if (template != null)
        {
            // 2. 存在模板，反序列化实例化新 NPC 登堂
            var newCourtNpc = new NpcState
            {
                Id = template.Id,
                Name = template.Name,
                Title = template.Title,
                BirthYear = template.BirthYear,
                BaseLongevity = template.BaseLongevity,
                Traits = new List<string>(template.Traits),
                Personality = template.Personality,
                Style = template.Style,
                Faction = template.Faction,
                
                // 赋以初始动态属性
                Health = 100,
                Favorability = 50,
                Power = 15,
                Corruption = template.Id == "dong_zhuo" ? 70 : 20, // 董卓作为割据军阀给予符合历史的高初始贪腐，其余 20
                StashedWealth = template.Id == "dong_zhuo" ? 500 : 50, // 初始资本
                IsActive = true
            };

            // 3. 调用 Registry 统一注册通道使其进入朝堂
            _registry.RegisterNpc(newCourtNpc, state);
            state.AddToChronicle($"【部署】并州刺史【{template.Name}】受到朝局大势感召，奉天子诏命正式踏上宣政殿！");
        }
    }

    public Task ProcessLifecycleStepAsync(GameState state, bool useHighTokenAi)
        => ProcessLifecycleStepAsync(state, useHighTokenAi, Random.Shared);

    public Task ProcessLifecycleStepAsync(GameState state, bool useHighTokenAi, Random rng)
    {
        // AI 调度扩展点：预留 LLM 驱动的自由事件生发接口
        // useHighTokenAi 模式下由外部调度器接管，此处仅执行启发式衰老逻辑

        // 统一启发式衰老机制：
        // 每年仅在一月上处理一次。年龄由 state.Year - npc.BirthYear 动态计算，自然实现一年一岁。
        // 寿终判定和疾病判定均在此每年一次的结算中发生。
        if (state.Month == 1 && state.Xun == 1)
        {
            int currentTimestamp = state.Year * 1000 + state.Month * 10 + state.Xun;
            // 时间戳原子防颠簸锁：同年同xun已处理过则直接退出，
            // 防止 AI 调度员多次调用导致一年内重复衰老/死亡。
            if (currentTimestamp <= state.LastNpcProcessedTimestamp)
            {
                return Task.CompletedTask;
            }
            state.LastNpcProcessedTimestamp = currentTimestamp;

            var npcsToRemove = new List<(string Id, string Reason)>();
            foreach (var pair in state.Npcs)
            {
                var npc = pair.Value;
                int age = state.Year - npc.BirthYear; // 隐式一年一岁

                // 寿终判定：超过期望寿命后，每年有 15% 概率寿终正寝
                if (age > npc.BaseLongevity)
                {
                    if (rng.Next(0, 100) < 15)
                    {
                        npcsToRemove.Add((pair.Key, $"寿数已尽，在洛阳邸舍中安然就寝致仕（享年 {age} 岁）"));
                        continue;
                    }
                }

                // 每年一次随机发病判定：千分之三概率染上洛阳伤寒，健康值暴跌 30
                if (rng.Next(0, 1000) < 3)
                {
                    npc.Health = Math.Clamp(npc.Health - 30, 0, 100);
                    state.AddToChronicle($"【伤寒】大臣【{npc.Name}】近日染上洛阳伤寒温疫，龙体不安，健康度暴跌 30点！");
                }

                if (npc.Health <= 0)
                {
                    npcsToRemove.Add((pair.Key, $"身染恶疾，医治无效，在京病逝"));
                }
            }

            // 物理退场
            foreach (var removeInfo in npcsToRemove)
            {
                _registry.DeregisterNpc(removeInfo.Id, removeInfo.Reason, state);
            }
        }

        return Task.CompletedTask;
    }

    private static readonly List<NpcState> _hardcodedFallbackList = new()
    {
        new NpcState { Id = "dong_zhuo", Name = "董卓", Title = "并州刺史/河东太守", BirthYear = 139, BaseLongevity = 53, Traits = new() { TraitNames.KongWuYouLi, TraitNames.YongBingZiZhong }, Personality = "残暴", Style = TraitNames.YongBingZiZhong, Faction = "割据军阀", Martial = 85, Leadership = 75, Politics = 40, Charisma = 50, Ambition = 95 },
        new NpcState { Id = "yuan_shao", Name = "袁绍", Title = "中军校尉/渤海太守", BirthYear = 154, BaseLongevity = 48, Traits = new() { TraitNames.LaoMouShenSuan, TraitNames.MenFaShiJia, TraitNames.ShouXiaYouBing }, Personality = "外宽内忌", Style = "结党营私", Faction = "清流派", Martial = 55, Leadership = 65, Politics = 60, Charisma = 85, Ambition = 85 },
        new NpcState { Id = "liu_bei", Name = "刘备", Title = "平原县令", BirthYear = 161, BaseLongevity = 62, Traits = new() { TraitNames.JingTianWeiDi, TraitNames.AiMinRuZi }, Personality = "宽厚", Style = "明哲保身", Faction = "清流派", Martial = 60, Leadership = 70, Politics = 65, Charisma = 90, Ambition = 80 },
        new NpcState { Id = "cao_cao", Name = "曹操", Title = "议郎/典军校尉", BirthYear = 155, BaseLongevity = 65, Traits = new() { TraitNames.JingTianWeiDi, TraitNames.LaoMouShenSuan }, Personality = "深沉", Style = "雷厉风行", Faction = "清流派", Martial = 72, Leadership = 90, Politics = 85, Charisma = 80, Ambition = 75 },
        new NpcState { Id = "sun_jian", Name = "孙坚", Title = "长沙太守/破虏将军", BirthYear = 155, BaseLongevity = 37, Traits = new() { TraitNames.KongWuYouLi, TraitNames.ZhiJunYanZheng }, Personality = "勇烈", Style = TraitNames.GangZhiBuE, Faction = "割据军阀", Martial = 90, Leadership = 80, Politics = 35, Charisma = 65, Ambition = 70 },
        new NpcState { Id = "gongsun_zan", Name = "公孙瓒", Title = "奋武将军/蓟侯", BirthYear = 153, BaseLongevity = 46, Traits = new() { TraitNames.KongWuYouLi, TraitNames.DongDianBingFa }, Personality = "刚烈", Style = "保皇尽忠", Faction = "割据军阀", Martial = 80, Leadership = 70, Politics = 30, Charisma = 55, Ambition = 65 },
        new NpcState { Id = "wang_yun", Name = "王允", Title = "司徒/尚书令", BirthYear = 137, BaseLongevity = 55, Traits = new() { TraitNames.GangZhiBuE, TraitNames.JingTianWeiDi }, Personality = "刚直", Style = "雷厉风行", Faction = "清流派", Martial = 20, Leadership = 45, Politics = 80, Charisma = 70, Ambition = 60 },
        new NpcState { Id = "li_ru", Name = "李儒", Title = "郎中令/董卓谋主", BirthYear = 150, BaseLongevity = 45, Traits = new() { TraitNames.LaoMouShenSuan, TraitNames.YouXieXinJi }, Personality = "阴险", Style = "结党营私", Faction = "割据军阀", Martial = 10, Leadership = 35, Politics = 78, Charisma = 40, Ambition = 70 },
        new NpcState { Id = "he_jin", Name = "何进", Title = "大将军", BirthYear = 135, BaseLongevity = 44, Traits = new() { TraitNames.YongBingZiZhong, TraitNames.ShouXiaYouBing }, Personality = "平庸", Style = "优柔寡断", Faction = "外戚派", Martial = 40, Leadership = 35, Politics = 30, Charisma = 40, Ambition = 60 },
        new NpcState { Id = "zhang_rang", Name = "张让", Title = "十常侍之首", BirthYear = 130, BaseLongevity = 60, Traits = new() { TraitNames.TanDeWuYan, TraitNames.ChanMeiZhuanQuan }, Personality = "阴险", Style = TraitNames.ChanMeiZhuanQuan, Faction = "阉党派", Martial = 10, Leadership = 15, Politics = 55, Charisma = 60, Ambition = 85 },
        new NpcState { Id = "huangfu_song", Name = "皇甫嵩", Title = "左车骑将军/槐里侯", BirthYear = 130, BaseLongevity = 60, Traits = new() { TraitNames.ZhiJunYanZheng, TraitNames.AiBingRuZi, TraitNames.GangZhiBuE }, Personality = "忠勇", Style = "雷厉风行", Faction = "清流派", Martial = 75, Leadership = 92, Politics = 55, Charisma = 70, Ambition = 25 },
        new NpcState { Id = "xun_yu", Name = "荀彧", Title = "尚书令/侍中", BirthYear = 163, BaseLongevity = 50, Traits = new() { TraitNames.JingTianWeiDi, TraitNames.ShanChangMinZheng, TraitNames.QingZhengLianJie }, Personality = "睿智", Style = "明哲保身", Faction = "清流派", Martial = 15, Leadership = 30, Politics = 95, Charisma = 85, Ambition = 40 },
        new NpcState { Id = "lu_zhi", Name = "卢植", Title = "北中郎将/尚书", BirthYear = 139, BaseLongevity = 53, Traits = new() { TraitNames.ZhiJunYanZheng, TraitNames.ShanChangMinZheng, TraitNames.GangZhiBuE }, Personality = "刚正", Style = TraitNames.GangZhiBuE, Faction = "清流派", Martial = 55, Leadership = 80, Politics = 80, Charisma = 75, Ambition = 20 },
        new NpcState { Id = "guo_si", Name = "郭汜", Title = "校尉/董卓部将", BirthYear = 147, BaseLongevity = 42, Traits = new() { TraitNames.YouXieLiQi, TraitNames.YouXieShouZang }, Personality = "粗暴", Style = TraitNames.YongBingZiZhong, Faction = "割据军阀", Martial = 70, Leadership = 40, Politics = 15, Charisma = 20, Ambition = 75 },
        new NpcState { Id = "hua_tuo", Name = "华佗", Title = "太医令/方士", BirthYear = 145, BaseLongevity = 63, Traits = new() { TraitNames.YiShuGaoMing, TraitNames.QingZhengLianJie }, Personality = "仁厚", Style = "明哲保身", Faction = "清流派", Martial = 10, Leadership = 5, Politics = 15, Charisma = 60, Ambition = 5 },
    };

    private List<NpcState> GetHardcodedFallbackList()
    {
        return _hardcodedFallbackList;
    }
}
