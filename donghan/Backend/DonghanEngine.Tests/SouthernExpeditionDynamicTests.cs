using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class SouthernExpeditionDynamicTests
{
    [Fact]
    public void Test_ImperialTriumphAndNanzhongTribute_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 55 }; // 55 >= 50
        var evaluator = new SouthernExpeditionEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(SouthernExpeditionOutcome.ImperialTriumphAndNanzhongTribute, result.Outcome);
        Assert.Equal(15, result.ImperialPowerDelta);
        Assert.Equal(3000, result.TreasuryGoldDelta);
        Assert.Equal(30, result.ZhugeLiangLoyaltyDelta);
        Assert.Equal(3000, result.WestGardenTroopBonus);
        Assert.Contains("深入不毛", result.NarrativeTitle);
    }

    [Fact]
    public void Test_ZhugeLiangAutonomousPacification_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 40 }; // 35 <= 40 < 50
        var evaluator = new SouthernExpeditionEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(SouthernExpeditionOutcome.ZhugeLiangAutonomousPacification, result.Outcome);
        Assert.Equal(1500, result.TreasuryGoldDelta);
        Assert.Equal(5, result.ImperialPowerDelta);
        Assert.Equal(1000, result.WestGardenTroopBonus);
        Assert.Contains("蛮夷宾服", result.NarrativeTitle);
    }

    [Fact]
    public void Test_NanzhongRebellionProtracted_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 35
        var evaluator = new SouthernExpeditionEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(SouthernExpeditionOutcome.NanzhongRebellionProtracted, result.Outcome);
        Assert.Equal(-5, result.ImperialPowerDelta);
        Assert.Equal(0, result.TreasuryGoldDelta);
        Assert.Contains("西南苦战", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_UpdatesTreasuryAndWestGardenArmy()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55 };
        state.WestGardenArmy.Size = 5000;
        var zhugeLiang = new NpcState { Id = "zhuge_liang", Name = "诸葛亮", IsActive = true, Power = 80, Favorability = 70 };
        state.RegisterNpc(zhugeLiang);

        int initialYizhouWealth = state.Provinces["yizhou"].Wealth;

        var evaluator = new SouthernExpeditionEvaluator();
        var executor = new SouthernExpeditionExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.Equal(5000, state.Treasury); // 2000 + 3000
        Assert.Equal(70, state.ImperialPower); // 55 + 15
        Assert.Equal(8000, state.WestGardenArmy.Size); // 5000 + 3000
        Assert.Equal(100, zhugeLiang.Favorability); // 70 + 30
        Assert.Equal(initialYizhouWealth + 2000, state.Provinces["yizhou"].Wealth);
        Assert.Contains(state.Chronicle, c => c.Contains("七擒") && c.Contains("孟获"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo225_3_1_TriggersSouthernExpedition()
    {
        var state = new GameState { Year = 225, Month = 2, Xun = 3, ImperialPower = 55 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 225/3/1

        Assert.Contains(state.Chronicle, c => c.Contains("七擒") || c.Contains("孟获") || c.Contains("南中"));
    }
}
