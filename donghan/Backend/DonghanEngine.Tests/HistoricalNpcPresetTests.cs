using System.Linq;
using System;
using Xunit;
using DonghanEngine.Core;

namespace DonghanEngine.Tests;

public class HistoricalNpcPresetTests
{
    [Fact]
    public void Test_OpeningCourt_ExpandsRosterWithoutDeployingColdPool()
    {
        var state = new GameState();

        Assert.Equal(27, state.Npcs.Count);
        Assert.Contains("yuan_shao", state.Npcs.Keys);
        Assert.Contains("huangfu_song", state.Npcs.Keys);
        Assert.Contains("zhao_zhong", state.Npcs.Keys);
        Assert.Contains("yuan_wei", state.Npcs.Keys);
        Assert.Contains("qiao_xuan", state.Npcs.Keys);
        Assert.Contains("xia_yun", state.Npcs.Keys);
        Assert.Contains("han_kui", state.Npcs.Keys);
        Assert.DoesNotContain("dong_zhuo", state.Npcs.Keys);
        Assert.DoesNotContain("liu_bei", state.Npcs.Keys);
        Assert.DoesNotContain("guo_jia", state.Npcs.Keys);
        Assert.DoesNotContain("qiao_mao", state.Npcs.Keys);
        Assert.DoesNotContain("zhang_jue", state.Npcs.Keys);
        Assert.DoesNotContain("zhang_yan", state.Npcs.Keys);
        Assert.All(state.Npcs.Values, npc => Assert.False(npc.IsHostile));
    }

    [Fact]
    public void Test_HistoricalNpcPresets_HaveUniqueIdsAndMetadata()
    {
        var presets = HistoricalNpcPresets.All;
        var ids = presets.Select(n => n.Id).ToList();

        Assert.True(presets.Count >= 70, $"Expected at least 70 historical NPC presets, got {presets.Count}");
        Assert.Equal(ids.Count, ids.Distinct().Count());
        Assert.All(presets, npc =>
        {
            Assert.False(string.IsNullOrWhiteSpace(npc.InitialLocation), $"{npc.Id} missing InitialLocation");
            Assert.False(string.IsNullOrWhiteSpace(npc.EntryCondition), $"{npc.Id} missing EntryCondition");
            Assert.False(string.IsNullOrWhiteSpace(npc.HistoricalRole), $"{npc.Id} missing HistoricalRole");
            Assert.InRange(npc.Martial, 0, 100);
            Assert.InRange(npc.Leadership, 0, 100);
            Assert.InRange(npc.Politics, 0, 100);
            Assert.InRange(npc.Charisma, 0, 100);
            Assert.InRange(npc.Ambition, 0, 100);
        });
    }


    [Fact]
    public void Test_SecondWavePresets_KeepOpeningAndColdPoolSeparated()
    {
        var presets = HistoricalNpcPresets.All.ToDictionary(n => n.Id);

        Assert.Equal("洛阳朝堂", presets["yuan_wei"].InitialLocation);
        Assert.Equal("开局", presets["yuan_wei"].EntryCondition);
        Assert.Contains(TraitNames.MenFaShiJia, presets["yuan_wei"].Traits);
        Assert.Contains(TraitNames.QingZhengLianJie, presets["qiao_xuan"].Traits);
        Assert.Equal("洛阳宫中", presets["xia_yun"].InitialLocation);
        Assert.Equal("事件触发", presets["qiao_mao"].EntryCondition);
        Assert.Equal("年月触发", presets["guo_jia"].EntryCondition);
        Assert.True(presets["zhang_yan"].IsHostile);
        Assert.True(presets["beigong_boyu"].IsHostile);
    }

    [Fact]
    public void Test_LifecycleDeploy_PreservesPresetMetadata()
    {
        var state = new GameState();
        var manager = new NpcLifecycleManager(new NpcRegistry());

        manager.DeployNpcToCourt("dong_zhuo", state);

        var dong = state.Npcs["dong_zhuo"];
        Assert.Equal("董卓", dong.Name);
        Assert.Equal("并州边军", dong.InitialLocation);
        Assert.Equal("事件触发", dong.EntryCondition);
        Assert.Contains("凉州军阀", dong.HistoricalRole);
        Assert.Contains(TraitNames.YongBingZiZhong, dong.Traits);
        Assert.True(dong.Ambition >= 90);
    }

    [Fact]
    public void Test_HostilePreset_CannotBeAppointedOrUsedForRebellionActions()
    {
        var state = new GameState();
        var manager = new NpcLifecycleManager(new NpcRegistry());
        manager.DeployNpcToCourt("zhang_jue", state);
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator(), new Random(42));
        state.Provinces["jizhou"].IsRebelling = true;
        state.Provinces["jizhou"].RebelFaction = "黄巾军";

        Assert.True(state.Npcs["zhang_jue"].IsHostile);
        Assert.Throws<InvalidOperationException>(() => engine.AssignGovernor("jizhou", "zhang_jue"));
        Assert.Throws<InvalidOperationException>(() => engine.SuppressRebellion("jizhou", "zhang_jue"));
        Assert.Throws<InvalidOperationException>(() => engine.PacifyRebellion("jizhou", "zhang_jue", GameEngine.PacifyStrategy.Persuade));
    }
}
