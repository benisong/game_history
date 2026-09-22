using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class BaidiEntrustmentDynamicTests
{
    [Fact]
    public void Test_ImperialAppointsZhugeLiangLoyal_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 55 }; // 55 >= 50
        var evaluator = new BaidiEntrustmentEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(BaidiEntrustmentOutcome.ImperialAppointsZhugeLiangLoyal, result.Outcome);
        Assert.True(result.LiuBeiDied);
        Assert.Equal(15, result.ImperialPowerDelta);
        Assert.Equal(4000, result.TreasuryGoldDelta);
        Assert.Equal(30, result.ZhugeLiangLoyaltyDelta);
        Assert.Equal(35, result.ZhugeLiangPowerDelta);
        Assert.Contains("诸葛秉政", result.NarrativeTitle);
    }

    [Fact]
    public void Test_ZhugeLiangGovernsAutonomously_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 40 }; // 35 <= 40 < 50
        var evaluator = new BaidiEntrustmentEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(BaidiEntrustmentOutcome.ZhugeLiangGovernsAutonomously, result.Outcome);
        Assert.True(result.LiuBeiDied);
        Assert.Equal(2000, result.TreasuryGoldDelta);
        Assert.Equal(5, result.ImperialPowerDelta);
        Assert.Equal(30, result.ZhugeLiangPowerDelta);
        Assert.Contains("蜀相秉政", result.NarrativeTitle);
    }

    [Fact]
    public void Test_ShuHanFactionalChaos_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 35
        var evaluator = new BaidiEntrustmentEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(BaidiEntrustmentOutcome.ShuHanFactionalChaos, result.Outcome);
        Assert.True(result.LiuBeiDied);
        Assert.Equal(-10, result.ImperialPowerDelta);
        Assert.Equal(0, result.TreasuryGoldDelta);
        Assert.Contains("南中蠢动", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_KillsLiuBeiAndDeploysZhugeLiang()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55 };
        var liuBei = new NpcState { Id = "liu_bei", Name = "刘备", IsActive = true, Power = 80, Favorability = 90 };
        state.RegisterNpc(liuBei);

        int initialYizhouWealth = state.Provinces["yizhou"].Wealth;

        var evaluator = new BaidiEntrustmentEvaluator();
        var executor = new BaidiEntrustmentExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.False(liuBei.IsActive, "刘备应当病逝注销");
        Assert.True(state.Npcs.ContainsKey("zhuge_liang"), "诸葛亮应当登庸入朝野谱系");
        Assert.Equal(6000, state.Treasury); // 2000 + 4000
        Assert.Equal(70, state.ImperialPower); // 55 + 15
        Assert.Equal(100, state.Npcs["zhuge_liang"].Favorability); // 70 + 30
        Assert.Equal("zhuge_liang", state.Provinces["yizhou"].GovernorId);
        Assert.Equal(initialYizhouWealth + 3000, state.Provinces["yizhou"].Wealth);
        Assert.Contains(state.Chronicle, c => c.Contains("刘备") && c.Contains("诸葛亮"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo223_4_1_TriggersBaidiEntrustment()
    {
        var state = new GameState { Year = 223, Month = 3, Xun = 3, ImperialPower = 55 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 223/4/1

        Assert.Contains(state.Chronicle, c => c.Contains("白帝") && (c.Contains("诸葛亮") || c.Contains("刘备") || c.Contains("托孤")));
    }
}
