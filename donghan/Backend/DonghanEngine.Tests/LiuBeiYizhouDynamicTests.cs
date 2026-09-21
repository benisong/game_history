using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class LiuBeiYizhouDynamicTests
{
    [Fact]
    public void Test_ImperialRatifiesYizhouAndCollectsTribute_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 55 }; // 55 >= 50
        var evaluator = new LiuBeiYizhouEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(LiuBeiYizhouOutcome.ImperialRatifiesYizhouAndCollectsTribute, result.Outcome);
        Assert.Equal("liu_bei", result.YizhouGovernorId);
        Assert.Equal(10, result.ImperialPowerDelta);
        Assert.Equal(3000, result.TreasuryGoldDelta);
        Assert.Equal(30, result.LiuBeiLoyaltyDelta);
        Assert.Contains("天府归汉", result.NarrativeTitle);
    }

    [Fact]
    public void Test_CaoCaoInterferesHanzhong_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 40 }; // 35 <= 40 < 50
        var evaluator = new LiuBeiYizhouEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(LiuBeiYizhouOutcome.CaoCaoInterferesHanzhong, result.Outcome);
        Assert.Equal("liu_bei", result.YizhouGovernorId);
        Assert.Equal(1500, result.TreasuryGoldDelta);
        Assert.Equal(20, result.CaoCaoPowerDelta);
        Assert.Contains("曹公窥蜀", result.NarrativeTitle);
    }

    [Fact]
    public void Test_LiuBeiAutonomousDefiance_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 35
        var evaluator = new LiuBeiYizhouEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(LiuBeiYizhouOutcome.LiuBeiAutonomousDefiance, result.Outcome);
        Assert.Equal(-10, result.ImperialPowerDelta);
        Assert.Equal(40, result.LiuBeiPowerDelta);
        Assert.Contains("蜀中自立", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_RelocatesLiuBeiAndUpdatesYizhou()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55 };
        var liuBei = new NpcState { Id = "liu_bei", Name = "刘备", IsActive = true, Power = 60, Favorability = 70, InitialLocation = "荆州公安" };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Power = 80, Favorability = 60 };

        state.RegisterNpc(liuBei);
        state.RegisterNpc(caoCao);

        var evaluator = new LiuBeiYizhouEvaluator();
        var executor = new LiuBeiYizhouExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.Equal(5000, state.Treasury); // 2000 + 3000
        Assert.Equal(65, state.ImperialPower); // 55 + 10
        Assert.Equal(100, liuBei.Favorability); // 70 + 30
        Assert.Equal("益州成都", liuBei.InitialLocation);
        Assert.Equal("liu_bei", state.Provinces["yizhou"].GovernorId);
        Assert.Contains(state.Chronicle, c => c.Contains("刘备") && c.Contains("益州"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo214_5_1_TriggersLiuBeiYizhou()
    {
        var state = new GameState { Year = 214, Month = 4, Xun = 3, ImperialPower = 55 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 214/5/1

        Assert.Contains(state.Chronicle, c => c.Contains("益州") && (c.Contains("刘备") || c.Contains("刘璋") || c.Contains("天府")));
    }
}
