using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class SunJianJingzhouDynamicTests
{
    [Fact]
    public void Test_ImperialMediationTruce_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 55 }; // 55 >= 50
        var evaluator = new SunJianJingzhouEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(SunJianJingzhouOutcome.ImperialMediationTruce, result.Outcome);
        Assert.False(result.SunJianDied);
        Assert.Equal(3000, result.TreasuryGoldDelta);
        Assert.Equal(8, result.ImperialPowerDelta);
        Assert.Contains("罢兵息民", result.NarrativeTitle);
    }

    [Fact]
    public void Test_SunJianFallsAtXianshan_WhenCourtWeakAndLoyaltyLow()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 50
        var sunJian = new NpcState { Id = "sun_jian", Name = "孙坚", IsActive = true, Favorability = 30, Power = 80 };
        state.RegisterNpc(sunJian);

        var evaluator = new SunJianJingzhouEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(SunJianJingzhouOutcome.SunJianFallsAtXianshan, result.Outcome);
        Assert.True(result.SunJianDied);
        Assert.Equal(-50, result.SunJianPowerDelta);
        Assert.Contains("猛虎陨落", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_PreservesSunJian_WhenMediationSucceeds()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 50 };
        var sunJian = new NpcState { Id = "sun_jian", Name = "孙坚", IsActive = true, Favorability = 60, Power = 80 };
        var liuBiao = new NpcState { Id = "liu_biao", Name = "刘表", IsActive = true, Favorability = 60, Power = 60 };
        state.RegisterNpc(sunJian);
        state.RegisterNpc(liuBiao);

        var evaluator = new SunJianJingzhouEvaluator();
        var executor = new SunJianJingzhouExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.True(sunJian.IsActive, "调停成功后孙坚应当免死保全");
        Assert.Equal(5000, state.Treasury); // 2000 + 3000
        Assert.Equal(58, state.ImperialPower); // 50 + 8
        Assert.Contains(state.Chronicle, c => c.Contains("孙坚") && c.Contains("荆州"));
    }

    [Fact]
    public void Test_Executor_KillsSunJian_WhenHistoricalCollapse()
    {
        var state = new GameState { ImperialPower = 30 };
        var sunJian = new NpcState { Id = "sun_jian", Name = "孙坚", IsActive = true, Favorability = 30, Power = 80 };
        state.RegisterNpc(sunJian);

        var evaluator = new SunJianJingzhouEvaluator();
        var executor = new SunJianJingzhouExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.False(sunJian.IsActive);
        Assert.Contains("岘山", sunJian.DeathReason);
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo191_10_2_TriggersSunJianJingzhou()
    {
        var state = new GameState { Year = 191, Month = 10, Xun = 1, ImperialPower = 55 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 191/10/2

        Assert.Contains(state.Chronicle, c => c.Contains("孙坚") && (c.Contains("江东") || c.Contains("荆州") || c.Contains("岘山")));
    }
}
