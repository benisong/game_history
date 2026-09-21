using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class BailangMountainDynamicTests
{
    [Fact]
    public void Test_ImperialTriumphAndNorthernUnification_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 60 }; // 60 >= 50
        var evaluator = new BailangMountainEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(BailangMountainOutcome.ImperialTriumphAndNorthernUnification, result.Outcome);
        Assert.Equal(10, result.ImperialPowerDelta);
        Assert.Equal(3000, result.TreasuryGoldDelta);
        Assert.Equal(25, result.CaoCaoPowerDelta);
        Assert.Equal(20, result.CaoCaoLoyaltyDelta);
        Assert.Equal(3000, result.WestGardenTroopBonus);
        Assert.Contains("白狼斩虏", result.NarrativeTitle);
    }

    [Fact]
    public void Test_CaoCaoIndependentConquest_WhenCaoCaoLoyal_AndCourtModerate()
    {
        var state = new GameState { ImperialPower = 40 }; // 35 <= 40 < 50
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Favorability = 70, Power = 80 };
        state.RegisterNpc(caoCao);

        var evaluator = new BailangMountainEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(BailangMountainOutcome.CaoCaoIndependentConquest, result.Outcome);
        Assert.Equal(1500, result.TreasuryGoldDelta);
        Assert.Equal(5, result.ImperialPowerDelta);
        Assert.Equal(1000, result.WestGardenTroopBonus);
        Assert.Contains("北伐荡寇", result.NarrativeTitle);
    }

    [Fact]
    public void Test_NorthernFrontierInstability_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 35
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Favorability = 30, Power = 60 };
        state.RegisterNpc(caoCao);

        var evaluator = new BailangMountainEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(BailangMountainOutcome.NorthernFrontierInstability, result.Outcome);
        Assert.Equal(-5, result.ImperialPowerDelta);
        Assert.Equal(0, result.TreasuryGoldDelta);
        Assert.Contains("塞北风雪", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_UpdatesTreasuryAndDefenseLevels()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55 };
        state.WestGardenArmy.Size = 5000;
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Power = 80, Favorability = 60 };
        state.RegisterNpc(caoCao);

        int initialBingDefense = state.Provinces["bingzhou"].DefenseLevel;
        int initialYouDefense = state.Provinces["youzhou"].DefenseLevel;

        var evaluator = new BailangMountainEvaluator();
        var executor = new BailangMountainExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.Equal(5000, state.Treasury); // 2000 + 3000
        Assert.Equal(65, state.ImperialPower); // 55 + 10
        Assert.Equal(8000, state.WestGardenArmy.Size); // 5000 + 3000
        Assert.Equal(initialBingDefense + 10, state.Provinces["bingzhou"].DefenseLevel);
        Assert.Equal(initialYouDefense + 10, state.Provinces["youzhou"].DefenseLevel);
        Assert.Contains(state.Chronicle, c => c.Contains("白狼山") && c.Contains("公孙康"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo207_8_2_TriggersBailangMountain()
    {
        var state = new GameState { Year = 207, Month = 8, Xun = 1, ImperialPower = 55 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 207/8/2

        Assert.Contains(state.Chronicle, c => c.Contains("白狼山") && (c.Contains("乌桓") || c.Contains("蹋顿") || c.Contains("公孙康")));
    }
}
