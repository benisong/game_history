using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class EmperorSuccessionCrisisDynamicTests
{
    [Fact]
    public void Test_PeacefulRecovery_WhenHealthSufficientAndFactionsBalanced()
    {
        var state = new GameState { Health = 45 }; // 默认开局：何进好感35 >= 25, 张让好感65 >= 25
        var evaluator = new EmperorSuccessionCrisisEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(EmperorCrisisCause.SafeAndHealthy, result.Cause);
        Assert.Equal(EmperorSurvivalOutcome.PeacefulRecovery, result.Outcome);
        Assert.Equal(15, result.ImperialPowerDelta);
        Assert.Equal(10, result.HealthDelta);
        Assert.Contains("天命延年", result.NarrativeTitle);
    }

    [Fact]
    public void Test_HeJinAssassination_Collapse_WhenHeJinHatedAndHealthLow()
    {
        var state = new GameState { Health = 30 };
        state.Npcs["he_jin"].AdjustFavorability(-20); // 35 - 20 = 15 < 25 (极度憎恨)
        state.Npcs["he_jin"].AdjustPower(10);        // 80 + 10 = 90 >= 70

        var evaluator = new EmperorSuccessionCrisisEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(EmperorCrisisCause.HeJinAssassination, result.Cause);
        Assert.Equal(EmperorSurvivalOutcome.DiedFromPoisonOrIllness, result.Outcome);
        Assert.Equal(-100, result.HealthDelta);
        Assert.Equal("he_jin", result.CulpritNpcId);
        Assert.Contains("大将军何进", result.NarrativeTitle);
    }

    [Fact]
    public void Test_HeJinAssassination_Exposed_WhenEmperorHealthHigh()
    {
        var state = new GameState { Health = 60 }; // 60 > 35 挺过鸩毒
        state.Npcs["he_jin"].AdjustFavorability(-20); // 15 < 25

        var evaluator = new EmperorSuccessionCrisisEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(EmperorCrisisCause.HeJinAssassination, result.Cause);
        Assert.Equal(EmperorSurvivalOutcome.SurvivedAndDiscovered, result.Outcome);
        Assert.True(result.CulpritExposed);
        Assert.Equal(-25, result.HealthDelta);
        Assert.Equal(25, result.ImperialPowerDelta);
        Assert.Equal(-30, result.CulpritPowerDelta);
        Assert.Contains("查实何进弑君", result.NarrativeTitle);
    }

    [Fact]
    public void Test_EunuchAssassination_WhenZhangRangHated()
    {
        var state = new GameState { Health = 30 };
        state.Npcs["zhang_rang"].AdjustFavorability(-50); // 65 - 50 = 15 < 25

        var evaluator = new EmperorSuccessionCrisisEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(EmperorCrisisCause.EunuchAssassination, result.Cause);
        Assert.Equal("zhang_rang", result.CulpritNpcId);
    }

    [Fact]
    public void Test_NaturalIllness_WhenHealthExtremelyLow()
    {
        var state = new GameState { Health = 15 }; // 15 <= 20
        var evaluator = new EmperorSuccessionCrisisEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(EmperorCrisisCause.NaturalIllness, result.Cause);
        Assert.Equal(EmperorSurvivalOutcome.DiedFromPoisonOrIllness, result.Outcome);
    }

    [Fact]
    public void Test_Executor_HandlesExposedCulpritAndCollapse()
    {
        var state = new GameState { Health = 20 };
        var evaluator = new EmperorSuccessionCrisisEvaluator();
        var executor = new EmperorSuccessionCrisisExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.Equal(0, state.Health);
        Assert.Equal(GameOutcome.Collapse, state.Outcome);
        Assert.Contains(state.Chronicle, c => c.Contains("崩殂") || c.Contains("社稷倾覆"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo189_4_1_TriggersSuccessionCrisis()
    {
        var state = new GameState { Year = 189, Month = 3, Xun = 3, Health = 50 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 189/4/1

        Assert.Contains(state.Chronicle, c => c.Contains("天命延年") || c.Contains("亲政"));
    }
}
