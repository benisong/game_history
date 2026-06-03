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

    public List<NpcState> GetPresetNpcsFallback()
    {
        string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "donghan_preset_npcs.json");
        try
        {
            if (File.Exists(jsonPath))
            {
                string jsonContent = File.ReadAllText(jsonPath);
                return JsonSerializer.Deserialize<List<NpcState>>(jsonContent) ?? GetHardcodedFallbackList();
            }
        }
        catch
        {
            // A轨文件读取异常，静默采用 B轨 内置冷备静态武将列表
        }

        return GetHardcodedFallbackList();
    }

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

    public async Task ProcessLifecycleStepAsync(GameState state, bool useHighTokenAi)
    {
        if (useHighTokenAi)
        {
            // 多 Token 纯 AI 版：此接口留给 AI 调度员在接入 LLM 后，根据朝局自由生发事件
            // 目前轻量实现由 Scheduler 编排传递指令，此处可作为一个扩展点
            await Task.Delay(10); // 模拟 AI 耗时
        }

        // 统一启发式衰老机制：
        // 每过一年（1月·上旬），所有大臣年龄增长 1 岁
        if (state.Month == 1 && state.Xun == 1)
        {
            int currentTimestamp = state.Year * 1000 + state.Month * 10 + state.Xun;
            // 如果在同一旬内已经处理过结算，直接拦截退出，防止频繁调度导致官员瞬间变老或老死
            if (currentTimestamp <= state.LastNpcProcessedTimestamp)
            {
                return;
            }
            state.LastNpcProcessedTimestamp = currentTimestamp;

            var npcsToRemove = new List<(string Id, string Reason)>();
            foreach (var pair in state.Npcs)
            {
                var npc = pair.Value;
                int age = state.Year - npc.BirthYear;

                // 寿数与健康判定：
                if (age > npc.BaseLongevity)
                {
                    // 超过期望寿命，每旬 15% 几率自然死亡/寿终致仕
                    if (Random.Shared.Next(0, 100) < 15)
                    {
                        npcsToRemove.Add((pair.Key, $"寿数已尽，在洛阳邸舍中安然就寝致仕（享年 {age} 岁）"));
                        continue;
                    }
                }

                // 随机发病判定
                if (Random.Shared.Next(0, 1000) < 3) // 千分之三概率发病
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
    }

    private static readonly List<NpcState> _hardcodedFallbackList = new()
    {
        new NpcState { Id = "dong_zhuo", Name = "董卓", Title = "并州刺史/河东太守", BirthYear = 139, BaseLongevity = 53, Traits = new() { "孔武有力", "拥兵自重" }, Personality = "残暴", Style = "拥兵自重", Faction = "割据军阀" },
        new NpcState { Id = "yuan_shao", Name = "袁绍", Title = "中军校尉/渤海太守", BirthYear = 154, BaseLongevity = 48, Traits = new() { "老谋深算", "门阀世家", "手下有兵" }, Personality = "外宽内忌", Style = "结党营私", Faction = "清流派" },
        new NpcState { Id = "liu_bei", Name = "刘备", Title = "平原县令", BirthYear = 161, BaseLongevity = 62, Traits = new() { "经天纬地", "爱民如子" }, Personality = "宽厚", Style = "明哲保身", Faction = "清流派" },
        new NpcState { Id = "cao_cao_fallback", Name = "曹操", Title = "议郎/典军校尉", BirthYear = 155, BaseLongevity = 65, Traits = new() { "经天纬地", "老谋深算", "清正廉洁" }, Personality = "深沉", Style = "雷厉风行", Faction = "清流派" },
        new NpcState { Id = "sun_jian", Name = "孙坚", Title = "长沙太守/破虏将军", BirthYear = 155, BaseLongevity = 37, Traits = new() { "孔武有力", "治军严整" }, Personality = "勇烈", Style = "刚直不阿", Faction = "割据军阀" },
        new NpcState { Id = "gongsun_zan", Name = "公孙瓒", Title = "奋武将军/蓟侯", BirthYear = 153, BaseLongevity = 46, Traits = new() { "孔武有力", "懂点兵法" }, Personality = "刚烈", Style = "保皇尽忠", Faction = "割据军阀" },
        new NpcState { Id = "wang_yun", Name = "王允", Title = "司徒/尚书令", BirthYear = 137, BaseLongevity = 55, Traits = new() { "刚直不阿", "经天纬地" }, Personality = "刚直", Style = "雷厉风行", Faction = "清流派" },
        new NpcState { Id = "li_ru", Name = "李儒", Title = "郎中令/董卓谋主", BirthYear = 150, BaseLongevity = 45, Traits = new() { "老谋深算", "有些心计" }, Personality = "阴险", Style = "结党营私", Faction = "割据军阀" },
        new NpcState { Id = "he_jin_fallback", Name = "何进", Title = "大将军", BirthYear = 135, BaseLongevity = 44, Traits = new() { "拥兵自重", "手下有兵" }, Personality = "平庸", Style = "优柔寡断", Faction = "外戚派" },
        new NpcState { Id = "zhang_rang_fallback", Name = "张让", Title = "十常侍之首", BirthYear = 130, BaseLongevity = 60, Traits = new() { "贪得无厌", "谄媚专权" }, Personality = "阴险", Style = "谄媚专权", Faction = "阉党派" },
        new NpcState { Id = "huangfu_song", Name = "皇甫嵩", Title = "左车骑将军/槐里侯", BirthYear = 130, BaseLongevity = 60, Traits = new() { "治军严整", "爱兵如子", "刚直不阿" }, Personality = "忠勇", Style = "雷厉风行", Faction = "清流派" },
        new NpcState { Id = "xun_yu", Name = "荀彧", Title = "尚书令/侍中", BirthYear = 163, BaseLongevity = 50, Traits = new() { "经天纬地", "擅长民政", "清正廉洁" }, Personality = "睿智", Style = "明哲保身", Faction = "清流派" },
        new NpcState { Id = "lu_zhi", Name = "卢植", Title = "北中郎将/尚书", BirthYear = 139, BaseLongevity = 53, Traits = new() { "治军严整", "擅长民政", "刚直不阿" }, Personality = "刚正", Style = "刚直不阿", Faction = "清流派" },
        new NpcState { Id = "guo_si", Name = "郭汜", Title = "校尉/董卓部将", BirthYear = 147, BaseLongevity = 42, Traits = new() { "有些力气", "有些手脏" }, Personality = "粗暴", Style = "拥兵自重", Faction = "割据军阀" },
        new NpcState { Id = "hua_tuo", Name = "华佗", Title = "太医令/方士", BirthYear = 145, BaseLongevity = 63, Traits = new() { "医术高明", "清正廉洁" }, Personality = "仁厚", Style = "明哲保身", Faction = "清流派" },
    };

    private List<NpcState> GetHardcodedFallbackList()
    {
        return _hardcodedFallbackList;
    }
}
