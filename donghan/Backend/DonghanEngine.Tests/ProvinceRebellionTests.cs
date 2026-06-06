using System;
using Xunit;
using DonghanEngine.Core;

namespace DonghanEngine.Tests;

public class ProvinceRebellionTests
{
    [Fact]
    public void Test_Provinces_Initialized_Correctly()
    {
        var state = new GameState();
        Assert.Equal(6, state.Provinces.Count);
        Assert.True(state.Provinces.ContainsKey("sili"));
        Assert.True(state.Provinces.ContainsKey("jizhou"));
        Assert.Equal(18, state.Provinces["jizhou"].LocalSupport);
        Assert.Equal(3, state.Provinces["jizhou"].Distance);
        Assert.Contains("sili", state.Provinces["jizhou"].Neighbors);
    }

    [Fact]
    public void Test_AssignGovernor_StabilizesProvince()
    {
        var state = new GameState();
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        var result = engine.AssignGovernor("jizhou", "cao_cao");

        Assert.Equal("cao_cao", state.Provinces["jizhou"].GovernorId);
        Assert.Equal("jizhou", state.Npcs["cao_cao"].GovernedProvinceId);
        Assert.True(state.Provinces["jizhou"].LocalSupport > 18); // +10
        Assert.Equal(10, state.Npcs["cao_cao"].Power); // 15 - 5 = 10
    }

    [Fact]
    public void Test_AssignGovernor_AlreadyAssigned_ShouldThrow()
    {
        var state = new GameState();
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());
        engine.AssignGovernor("jizhou", "cao_cao");
        Assert.Throws<InvalidOperationException>(() => engine.AssignGovernor("yanzhou", "cao_cao"));
    }

    [Fact]
    public void Test_RecallGovernor_ReturnsToCapital()
    {
        var state = new GameState();
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());
        engine.AssignGovernor("jizhou", "cao_cao");

        var result = engine.RecallGovernor("jizhou");

        Assert.Null(state.Provinces["jizhou"].GovernorId);
        Assert.Null(state.Npcs["cao_cao"].GovernedProvinceId);
        Assert.Equal(13, state.Npcs["cao_cao"].Power); // 10 + 3
    }

    [Fact]
    public void Test_Npc_HasFiveAttributes()
    {
        var state = new GameState();
        var cao = state.Npcs["cao_cao"];
        Assert.Equal(72, cao.Martial);
        Assert.Equal(90, cao.Leadership);
        Assert.Equal(85, cao.Politics);
        Assert.Equal(80, cao.Charisma);
        Assert.Equal(75, cao.Ambition);
    }

    [Fact]
    public void Test_CombatPower_CalculatesCorrectly()
    {
        var cao = new NpcState { Martial = 72, Leadership = 90, Corruption = 5 };
        cao.Traits.AddRange(new[] { "经天纬地", "老谋深算" }); // These don't affect combat

        double power = NpcTraitEvaluator.GetCombatPower(cao);
        // base = 72*0.4 + 90*0.6 = 28.8 + 54 = 82.8
        // no combat traits, corruption penalty = 5/100*20 = 1
        // 82.8 * 1.0 - 1 = 81.8
        Assert.True(power > 80 && power < 83, $"Expected ~81.8, got {power}");
    }

    [Fact]
    public void Test_CombatPower_WithCombatTraits()
    {
        var huangfu = new NpcState { Martial = 75, Leadership = 92, Corruption = 10 };
        huangfu.Traits.AddRange(new[] { "治军严整", "爱兵如子" });

        double power = NpcTraitEvaluator.GetCombatPower(huangfu);
        // base = 75*0.4 + 92*0.6 = 30 + 55.2 = 85.2
        // "治军严整" = 1.25x multiplier
        // corruption = 2
        // 85.2 * 1.25 - 2 = 104.5
        Assert.True(power > 100, $"C皇嵩 should have >100 combat power, got {power}");
    }

    [Fact]
    public void Test_PoliticalSkill_CalculatesCorrectly()
    {
        var xunyu = new NpcState { Politics = 95, Charisma = 85, Corruption = 5 };
        xunyu.Traits.AddRange(new[] { "经天纬地", "擅长民政" });

        double skill = NpcTraitEvaluator.GetPoliticalSkill(xunyu);
        // base = 95*0.6 + 85*0.4 = 57 + 34 = 91
        // 经天纬地 1.20, 擅长民政 1.08 → 1.20*1.08 = 1.296
        // 91 * 1.296 = 117.9
        Assert.True(skill > 110, $"荀彧 should have >110 political skill, got {skill}");
    }

    [Fact]
    public void Test_EnvoyDeathRisk_LowMartialHighRisk()
    {
        var xunyu = new NpcState { Martial = 15 };
        int risk = NpcTraitEvaluator.GetEnvoyDeathRisk(xunyu, rebellionMonths: 4, distance: 5, usedPunishment: false, imperialPower: 25);
        // base 40 + 15 (low martial) + 20 (4 months) + 15 (distance) = 90, clamped
        Assert.Equal(90, risk);
    }

    [Fact]
    public void Test_EnvoyDeathRisk_HighMartialLowRisk()
    {
        var huangfu = new NpcState { Martial = 75 };
        int risk = NpcTraitEvaluator.GetEnvoyDeathRisk(huangfu, rebellionMonths: 2, distance: 2, usedPunishment: false, imperialPower: 40);
        // base 40 - 20 (high martial) + 10 (2 months) + 6 (distance) = 36
        Assert.Equal(36, risk);
    }

    [Fact]
    public void Test_ProvinceReport_ContainsAllProvinces()
    {
        var state = new GameState();
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        string report = engine.GetProvinceReport();
        Assert.Contains("冀州", report);
        Assert.Contains("司隶", report);
        Assert.Contains("荆州", report);
        Assert.Contains("暂无", report); // No governors assigned yet
    }
}
