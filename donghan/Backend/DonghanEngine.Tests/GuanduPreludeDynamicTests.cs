using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class GuanduPreludeDynamicTests
{
    [Fact]
    public void Test_ImperialDualAppeasementAndTribute_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 60 }; // 60 >= 50
        var evaluator = new GuanduPreludeEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(GuanduPreludeOutcome.ImperialDualAppeasementAndTribute, result.Outcome);
        Assert.Equal(10, result.ImperialPowerDelta);
        Assert.Equal(3000, result.TreasuryGoldDelta);
        Assert.Equal(20, result.CaoCaoLoyaltyDelta);
        Assert.Equal(15, result.YuanShaoLoyaltyDelta);
        Assert.Equal(2000, result.WestGardenTroopBonus);
        Assert.Contains("帝策居中", result.NarrativeTitle);
    }

    [Fact]
    public void Test_CaoCaoFavoredSecretSupport_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 40 }; // 35 <= 40 < 50
        var evaluator = new GuanduPreludeEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(GuanduPreludeOutcome.CaoCaoFavoredSecretSupport, result.Outcome);
        Assert.Equal(1000, result.TreasuryGoldDelta);
        Assert.Equal(25, result.CaoCaoLoyaltyDelta);
        Assert.Equal(-20, result.YuanShaoLoyaltyDelta);
        Assert.Contains("密诏扶曹", result.NarrativeTitle);
    }

    [Fact]
    public void Test_HegemonicClashUnchecked_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 35
        var evaluator = new GuanduPreludeEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(GuanduPreludeOutcome.HegemonicClashUnchecked, result.Outcome);
        Assert.Equal(-10, result.ImperialPowerDelta);
        Assert.Equal(0, result.TreasuryGoldDelta);
        Assert.Contains("黄河雷动", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_UpdatesTreasuryAndWestGardenArmy()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55 };
        state.WestGardenArmy.Size = 4000;
        var yuanShao = new NpcState { Id = "yuan_shao", Name = "袁绍", IsActive = true, Power = 85, Favorability = 50 };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Power = 80, Favorability = 60 };

        state.RegisterNpc(yuanShao);
        state.RegisterNpc(caoCao);

        var evaluator = new GuanduPreludeEvaluator();
        var executor = new GuanduPreludeExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.Equal(5000, state.Treasury); // 2000 + 3000
        Assert.Equal(65, state.ImperialPower); // 55 + 10
        Assert.Equal(6000, state.WestGardenArmy.Size); // 4000 + 2000
        Assert.Equal(80, caoCao.Favorability); // 60 + 20
        Assert.Equal(65, yuanShao.Favorability); // 50 + 15
        Assert.Contains(state.Chronicle, c => c.Contains("官渡") && c.Contains("袁绍"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo200_2_1_TriggersGuanduPrelude()
    {
        var state = new GameState { Year = 200, Month = 1, Xun = 3, ImperialPower = 55 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 200/2/1

        Assert.Contains(state.Chronicle, c => c.Contains("官渡") && (c.Contains("袁绍") || c.Contains("黎阳") || c.Contains("曹操")));
    }
}
