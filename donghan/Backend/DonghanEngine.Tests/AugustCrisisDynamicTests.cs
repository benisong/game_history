using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class AugustCrisisDynamicTests
{
    [Fact]
    public void Test_ImperialSuppressed_WhenEmperorAliveAndFirm()
    {
        var state = new GameState { Health = 50, ImperialPower = 40 };
        var evaluator = new AugustCrisisEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(AugustCrisisOutcome.ImperialSuppressed, result.Outcome);
        Assert.False(result.HeJinDied);
        Assert.False(result.DongZhuoAllowedEntry);
        Assert.Equal(10, result.ImperialPowerDelta);
        Assert.Contains("消弭于无形", result.NarrativeTitle);
    }

    [Fact]
    public void Test_HistoricalBloodshed_WhenEmperorCollapsed()
    {
        var state = new GameState { Health = 0, Outcome = GameOutcome.Collapse };
        var evaluator = new AugustCrisisEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(AugustCrisisOutcome.HistoricalBloodshed, result.Outcome);
        Assert.True(result.HeJinDied);
        Assert.True(result.DongZhuoAllowedEntry);
        Assert.Equal(-15, result.ImperialPowerDelta);
        Assert.Contains("何进遇害", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_PreservesHeJin_WhenImperialSuppressed()
    {
        var state = new GameState { Health = 50, ImperialPower = 40 };
        var evaluator = new AugustCrisisEvaluator();
        var executor = new AugustCrisisExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.True(state.Npcs["he_jin"].IsActive, "天子坐镇时何进应当免死健在");
        Assert.Equal(50, state.ImperialPower);
        Assert.Contains(state.Chronicle, c => c.Contains("史实宫变不复发生"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo189_8_3_EmperorAlive_SuppressesChaos()
    {
        var state = new GameState { Year = 189, Month = 8, Xun = 2, Health = 50, ImperialPower = 45 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 189/8/3

        Assert.True(state.Npcs["he_jin"].IsActive, "天子健在时何进在 189/8/3 不会死");
        Assert.Contains(state.Chronicle, c => c.Contains("各守职分") || c.Contains("史实宫变不复发生"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo189_9_1_EmperorAlive_BlocksDongZhuo()
    {
        var state = new GameState { Year = 189, Month = 8, Xun = 3, Health = 50, ImperialPower = 45 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 189/9/1

        Assert.Contains(state.Chronicle, c => c.Contains("董卓军团受阻") || c.Contains("严禁外兵入京"));
    }
}
