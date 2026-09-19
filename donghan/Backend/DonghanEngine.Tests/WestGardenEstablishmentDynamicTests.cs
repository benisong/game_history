using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class WestGardenEstablishmentDynamicTests
{
    [Fact]
    public void Test_ImperialTriumph_WhenPrivateTreasurySufficientAndHeJinBalanced()
    {
        var state = new GameState(); // 默认开局：私库1200 >= 800, 何进权势70 < 80
        var evaluator = new WestGardenEstablishmentEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(WestGardenEstablishmentOutcome.ImperialTriumph, result.Outcome);
        Assert.Equal(15, result.ImperialPowerDelta);
        Assert.Equal(-15, result.HeJinPowerDelta);
        Assert.Equal(20, result.JianShuoPowerDelta);
        Assert.Equal(4000, result.ArmySizeBonus);
        Assert.Equal(800, result.PrivateTreasuryCost);
        Assert.Contains("平乐观", result.NarrativeTitle);
    }

    [Fact]
    public void Test_InfiltratedByHeJin_WhenHeJinOverpowered()
    {
        var state = new GameState();
        state.Npcs["he_jin"].AdjustPower(10); // 80 + 10 = 90 > 80

        var evaluator = new WestGardenEstablishmentEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(WestGardenEstablishmentOutcome.InfiltratedByHeJin, result.Outcome);
        Assert.Equal(10, result.HeJinPowerDelta);
        Assert.Equal(2000, result.ArmySizeBonus);
        Assert.Equal(600, result.PrivateTreasuryCost);
        Assert.Contains("外戚掣肘", result.NarrativeTitle);
    }

    [Fact]
    public void Test_FundShortageAborted_WhenPrivateTreasuryLow()
    {
        var state = new GameState { PrivateTreasury = 500 }; // 500 < 800

        var evaluator = new WestGardenEstablishmentEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(WestGardenEstablishmentOutcome.FundShortageAborted, result.Outcome);
        Assert.Equal(-5, result.ImperialPowerDelta);
        Assert.Equal(0, result.ArmySizeBonus);
        Assert.Equal(0, result.PrivateTreasuryCost);
        Assert.Contains("府库匮乏", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_UpdatesArmyAndDeductsPrivateTreasury()
    {
        var state = new GameState();
        int initialPrivate = state.PrivateTreasury;
        int initialSize = state.WestGardenArmy.Size;
        int initialImperial = state.ImperialPower;

        var evaluator = new WestGardenEstablishmentEvaluator();
        var executor = new WestGardenEstablishmentExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.Equal(initialPrivate - 800, state.PrivateTreasury);
        Assert.Equal(initialSize + 4000, state.WestGardenArmy.Size);
        Assert.Equal(initialImperial + 15, state.ImperialPower);
        Assert.True(state.Npcs["jian_shuo"].Power >= 50);
        Assert.Contains(state.Chronicle, c => c.Contains("无上将军") || c.Contains("平乐观"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo188_8_2_TriggersWestGardenEstablishment()
    {
        var state = new GameState { Year = 188, Month = 8, Xun = 1 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 188/8/2

        Assert.Contains(state.Chronicle, c => c.Contains("平乐观") || c.Contains("八校尉"));
    }
}
