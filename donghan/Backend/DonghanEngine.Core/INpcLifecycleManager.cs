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
            // 2. 存在模板，克隆实例化新 NPC 登堂
            var newCourtNpc = HistoricalNpcPresets.Clone(template);
            newCourtNpc.Health = 100;
            newCourtNpc.IsActive = true;
            newCourtNpc.DeathReason = string.Empty;
            newCourtNpc.GovernedProvinceId = null;

            // 3. 调用 Registry 统一注册通道使其进入朝堂
            _registry.RegisterNpc(newCourtNpc, state);
            string entryLabel = newCourtNpc.IsHostile ? "敌势" : newCourtNpc.InitialLocation.Contains("地方") || newCourtNpc.InitialLocation.Contains("州") ? "外镇" : "部署";
            state.AddToChronicle($"【{entryLabel}】{template.Name}因“{template.EntryCondition}”进入大局：{template.HistoricalRole}");
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

    private static readonly List<NpcState> _hardcodedFallbackList = HistoricalNpcPresets.All;

    private List<NpcState> GetHardcodedFallbackList()
    {
        return _hardcodedFallbackList;
    }
}
