using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class JingzhouSuccessionDynamicTests
{
    [Fact]
    public void Test_ImperialMediationAndPartition_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 55 }; // 55 >= 50
        var evaluator = new JingzhouSuccessionEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(JingzhouSuccessionOutcome.ImperialMediationAndPartition, result.Outcome);
        Assert.Equal("liu_bei", result.JingzhouGovernorId);
        Assert.Equal(10, result.ImperialPowerDelta);
        Assert.Equal(2500, result.TreasuryGoldDelta);
        Assert.Equal(25, result.LiuBeiLoyaltyDelta);
        Assert.Contains("帝策分陕", result.NarrativeTitle);
    }

    [Fact]
    public void Test_CaoCaoSubduesJingzhouDirect_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 40 }; // 35 <= 40 < 50
        var evaluator = new JingzhouSuccessionEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(JingzhouSuccessionOutcome.CaoCaoSubduesJingzhouDirect, result.Outcome);
        Assert.Equal("cao_cao", result.JingzhouGovernorId);
        Assert.Equal(1500, result.TreasuryGoldDelta);
        Assert.Equal(35, result.CaoCaoPowerDelta);
        Assert.Contains("席卷荆襄", result.NarrativeTitle);
    }

    [Fact]
    public void Test_LiuBeiConsolidatesJingxiang_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 35
        var evaluator = new JingzhouSuccessionEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(JingzhouSuccessionOutcome.LiuBeiConsolidatesJingxiang, result.Outcome);
        Assert.Equal(-5, result.ImperialPowerDelta);
        Assert.Equal(30, result.LiuBeiPowerDelta);
        Assert.Contains("江汉分立", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_KillsLiuBiaoAndUpdatesJingzhou()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55 };
        var liuBiao = new NpcState { Id = "liu_biao", Name = "刘表", IsActive = true, Power = 60, Favorability = 50 };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Power = 80, Favorability = 60 };
        var liuBei = new NpcState { Id = "liu_bei", Name = "刘备", IsActive = true, Power = 60, Favorability = 70 };

        state.RegisterNpc(liuBiao);
        state.RegisterNpc(caoCao);
        state.RegisterNpc(liuBei);

        var evaluator = new JingzhouSuccessionEvaluator();
        var executor = new JingzhouSuccessionExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.False(liuBiao.IsActive, "刘表应当病故注销");
        Assert.Equal(4500, state.Treasury); // 2000 + 2500
        Assert.Equal(65, state.ImperialPower); // 55 + 10
        Assert.Equal("liu_bei", state.Provinces["jingzhou"].GovernorId);
        Assert.Equal(95, liuBei.Favorability); // 70 + 25
        Assert.Contains(state.Chronicle, c => c.Contains("刘表") && c.Contains("荆州"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo208_7_1_TriggersJingzhouSuccession()
    {
        var state = new GameState { Year = 208, Month = 6, Xun = 3, ImperialPower = 55 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 208/7/1

        Assert.Contains(state.Chronicle, c => c.Contains("荆州") && (c.Contains("刘表") || c.Contains("刘琦") || c.Contains("刘琮") || c.Contains("江夏")));
    }
}
