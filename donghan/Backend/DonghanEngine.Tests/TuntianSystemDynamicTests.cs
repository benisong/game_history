using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class TuntianSystemDynamicTests
{
    [Fact]
    public void Test_NationalTuntianProsperity_WhenCourtStrongAndTreasuryAbundant()
    {
        var state = new GameState { ImperialPower = 60, Treasury = 2000, PopularSupport = 50 };
        var evaluator = new TuntianSystemEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(TuntianSystemOutcome.NationalTuntianProsperity, result.Outcome);
        Assert.Equal(10, result.ImperialPowerDelta);
        Assert.Equal(20, result.PopularSupportDelta);
        Assert.Equal(4000, result.TreasuryGoldDelta);
        Assert.Equal(20, result.WestGardenArmyMoraleBonus);
        Assert.Contains("太仓丰稔", result.NarrativeTitle);
    }

    [Fact]
    public void Test_LocalTuntianAppeasement_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 40, Treasury = 500 }; // 35 <= 40 < 50
        var evaluator = new TuntianSystemEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(TuntianSystemOutcome.LocalTuntianAppeasement, result.Outcome);
        Assert.Equal(2000, result.TreasuryGoldDelta);
        Assert.Equal(10, result.PopularSupportDelta);
        Assert.Contains("诸侯劝农", result.NarrativeTitle);
    }

    [Fact]
    public void Test_TuntianStagnation_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 30, Treasury = 200 };
        var evaluator = new TuntianSystemEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(TuntianSystemOutcome.TuntianStagnation, result.Outcome);
        Assert.Equal(-5, result.ImperialPowerDelta);
        Assert.Equal(0, result.TreasuryGoldDelta);
        Assert.Contains("农政滞碍", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_UpdatesTreasuryAndSiliProvince()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55, PopularSupport = 50 };
        int initialMorale = state.WestGardenArmy.Morale;
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Favorability = 50, Power = 60 };
        state.RegisterNpc(caoCao);

        var evaluator = new TuntianSystemEvaluator();
        var executor = new TuntianSystemExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.Equal(6000, state.Treasury); // 2000 + 4000
        Assert.Equal(65, state.ImperialPower); // 55 + 10
        Assert.Equal(70, state.PopularSupport); // 50 + 20
        Assert.Equal(initialMorale + 20, state.WestGardenArmy.Morale);
        Assert.True(state.Provinces["sili"].Wealth > 2000);
        Assert.Contains(state.Chronicle, c => c.Contains("屯田") && c.Contains("太仓"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo196_8_1_TriggersTuntianSystem()
    {
        var state = new GameState { Year = 196, Month = 7, Xun = 3, ImperialPower = 55, Treasury = 2000 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 196/8/1

        Assert.Contains(state.Chronicle, c => c.Contains("屯田") || c.Contains("农桑") || c.Contains("典农"));
    }
}
