using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class SunCeJiangdongDynamicTests
{
    [Fact]
    public void Test_RatifyAndCollectSaltIronTax_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 55 }; // 55 >= 50
        var evaluator = new SunCeJiangdongEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(SunCeJiangdongOutcome.RatifyAndCollectSaltIronTax, result.Outcome);
        Assert.Equal("sun_ce", result.YangzhouGovernorId);
        Assert.Equal(8, result.ImperialPowerDelta);
        Assert.Equal(2000, result.TreasuryGoldDelta);
        Assert.Equal(25, result.SunCeLoyaltyDelta);
        Assert.Contains("顺旨受封", result.NarrativeTitle);
    }

    [Fact]
    public void Test_StrictImperialCommission_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 40 }; // 35 <= 40 < 50
        var evaluator = new SunCeJiangdongEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(SunCeJiangdongOutcome.StrictImperialCommission, result.Outcome);
        Assert.Equal(1000, result.TreasuryGoldDelta);
        Assert.Equal(3, result.ImperialPowerDelta);
        Assert.Contains("朝廷羁縻", result.NarrativeTitle);
    }

    [Fact]
    public void Test_JiangdongAutonomousDefiance_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 35
        var evaluator = new SunCeJiangdongEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(SunCeJiangdongOutcome.JiangdongAutonomousDefiance, result.Outcome);
        Assert.Equal(-5, result.ImperialPowerDelta);
        Assert.Equal(300, result.TreasuryGoldDelta);
        Assert.Contains("孙氏自雄", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_DeploysSunCeAndUpdatesYangzhou()
    {
        var state = new GameState { Treasury = 3000, ImperialPower = 55 };
        var evaluator = new SunCeJiangdongEvaluator();
        var executor = new SunCeJiangdongExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.True(state.Npcs.ContainsKey("sun_ce"), "孙策应当被登庸入朝野谱系");
        Assert.Equal("sun_ce", state.Provinces["yangzhou"].GovernorId);
        Assert.Equal(5000, state.Treasury); // 3000 + 2000
        Assert.Equal(63, state.ImperialPower); // 55 + 8
        Assert.Contains(state.Chronicle, c => c.Contains("孙策") && c.Contains("江东"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo195_2_1_TriggersSunCeJiangdong()
    {
        var state = new GameState { Year = 195, Month = 1, Xun = 3, ImperialPower = 55 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 195/2/1

        Assert.Contains(state.Chronicle, c => c.Contains("孙策") && (c.Contains("江东") || c.Contains("扬州") || c.Contains("折冲")));
    }
}
