using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class GuandongTributeDynamicTests
{
    [Fact]
    public void Test_ImperialTributeProsperity_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 60 };
        state.WestGardenArmy.AdjustSize(6000);

        var evaluator = new GuandongTributeEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(GuandongTributeOutcome.ImperialTributeProsperity, result.Outcome);
        Assert.Equal(10, result.ImperialPowerDelta);
        Assert.Equal(3000, result.TreasuryGoldDelta);
        Assert.Equal(15, result.CaoCaoLoyaltyDelta);
        Assert.Contains("万国输贡", result.NarrativeTitle);
    }

    [Fact]
    public void Test_PartialTributeDissent_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 40 }; // 40 >= 35 但禁军未达标
        var evaluator = new GuandongTributeEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(GuandongTributeOutcome.PartialTributeDissent, result.Outcome);
        Assert.Equal(1500, result.TreasuryGoldDelta);
        Assert.Equal(-20, result.YuanShuLoyaltyDelta);
        Assert.Contains("淮南违抗", result.NarrativeTitle);
    }

    [Fact]
    public void Test_FactionalBoycott_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 20 };
        var evaluator = new GuandongTributeEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(GuandongTributeOutcome.FactionalBoycott, result.Outcome);
        Assert.Equal(0, result.TreasuryGoldDelta);
        Assert.Equal(-10, result.ImperialPowerDelta);
        Assert.Contains("贡道受阻", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_UpdatesTreasuryAndChronicle()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 50 };
        state.WestGardenArmy.AdjustSize(5000);

        var evaluator = new GuandongTributeEvaluator();
        var executor = new GuandongTributeExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.Equal(5000, state.Treasury); // 2000 + 3000
        Assert.Equal(60, state.ImperialPower); // 50 + 10
        Assert.Contains(state.Chronicle, c => c.Contains("关东") && c.Contains("朝贺"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo190_1_1_TriggersGuandongTribute()
    {
        var state = new GameState { Year = 189, Month = 12, Xun = 3, ImperialPower = 50 };
        state.WestGardenArmy.AdjustSize(6000);
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 190/1/1

        Assert.Contains(state.Chronicle, c => c.Contains("朝贺") || c.Contains("关东诸侯"));
    }
}
