using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class PalaceFireDynamicTests
{
    [Fact]
    public void Test_Historical_LevyTaxAndRebuild_WhenDefaultState()
    {
        var state = new GameState(); // 默认开局：民心28 < 50, 张让权势75 >= 40, 好感65 >= 60
        var evaluator = new PalaceFireEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(PalaceFireDecision.LevyTaxAndRebuild, result.Decision);
        Assert.Equal(3000, result.PrivateTreasuryDelta);
        Assert.Equal(-15, result.PopularSupportDelta);
        Assert.Equal(-8, result.AllProvinceSupportDelta);
        Assert.Equal(15, result.ZhangRangFavorDelta);
    }

    [Fact]
    public void Test_RejectAndAusterity_WhenPopularSupportHighAndEunuchSuppressed()
    {
        var state = new GameState { PopularSupport = 55 };
        state.Npcs["zhang_rang"].AdjustPower(-40); // 75 - 40 = 35 < 40

        var evaluator = new PalaceFireEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(PalaceFireDecision.RejectAndAusterity, result.Decision);
        Assert.Equal(0, result.PrivateTreasuryDelta);
        Assert.Equal(15, result.PopularSupportDelta);
        Assert.Equal(8, result.AllProvinceSupportDelta);
        Assert.Equal(-15, result.ZhangRangFavorDelta);
        Assert.Contains("罪己", result.NarrativeTitle);
    }

    [Fact]
    public void Test_TreasuryOnlyRebuild_WhenTreasurySufficientAndPrivateLow()
    {
        var state = new GameState
        {
            Treasury = 6000,
            PrivateTreasury = 800,
            PopularSupport = 40 // 不触发 RejectAndAusterity
        };
        // 降低张让好感以允许折中
        state.Npcs["zhang_rang"].AdjustFavorability(-10); // 65 - 10 = 55 < 60

        var evaluator = new PalaceFireEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(PalaceFireDecision.TreasuryOnlyRebuild, result.Decision);
        Assert.Equal(-1500, result.TreasuryDelta);
        Assert.Equal(0, result.PrivateTreasuryDelta);
        Assert.Equal(5, result.PopularSupportDelta);
    }

    [Fact]
    public void Test_Executor_UpdatesAllProvincesAndChronicle()
    {
        var state = new GameState();
        int initialSiliSupport = state.Provinces["sili"].LocalSupport;
        int initialPrivate = state.PrivateTreasury;

        var evaluator = new PalaceFireEvaluator();
        var executor = new PalaceFireExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.Equal(initialPrivate + 3000, state.PrivateTreasury);
        Assert.Equal(initialSiliSupport - 8, state.Provinces["sili"].LocalSupport);
        Assert.Contains(state.Chronicle, c => c.Contains("南宫") || c.Contains("十常侍"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo185_2_1_TriggersPalaceFire()
    {
        var state = new GameState { Year = 185, Month = 1, Xun = 3 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 185/2/1

        Assert.Contains(state.Chronicle, c => c.Contains("南宫"));
    }
}
