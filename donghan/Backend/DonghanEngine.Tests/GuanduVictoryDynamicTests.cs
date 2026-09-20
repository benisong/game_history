using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class GuanduVictoryDynamicTests
{
    [Fact]
    public void Test_RatifyVictoryAndRestrainCaoCao_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 55 }; // 55 >= 50
        var evaluator = new GuanduVictoryEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(GuanduVictoryOutcome.RatifyVictoryAndRestrainCaoCao, result.Outcome);
        Assert.Equal(10, result.ImperialPowerDelta);
        Assert.Equal(3000, result.TreasuryGoldDelta);
        Assert.Equal(30, result.CaoCaoPowerDelta);
        Assert.Equal(20, result.CaoCaoLoyaltyDelta);
        Assert.Equal(-60, result.YuanShaoPowerDelta);
        Assert.Contains("乌巢夜火", result.NarrativeTitle);
    }

    [Fact]
    public void Test_CaoCaoOverwhelmingHegemony_WhenCaoCaoLoyal_AndCourtModerate()
    {
        var state = new GameState { ImperialPower = 45 }; // 45 < 50
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Favorability = 70, Power = 80 };
        state.RegisterNpc(caoCao);

        var evaluator = new GuanduVictoryEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(GuanduVictoryOutcome.CaoCaoOverwhelmingHegemony, result.Outcome);
        Assert.Equal(1500, result.TreasuryGoldDelta);
        Assert.Equal(35, result.CaoCaoPowerDelta);
        Assert.Contains("雄霸北方", result.NarrativeTitle);
    }

    [Fact]
    public void Test_GuanduStalemateProlonged_WhenCourtWeakAndLoyaltyLow()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 35
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Favorability = 30, Power = 60 };
        state.RegisterNpc(caoCao);

        var evaluator = new GuanduVictoryEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(GuanduVictoryOutcome.GuanduStalemateProlonged, result.Outcome);
        Assert.Equal(-10, result.ImperialPowerDelta);
        Assert.Equal(0, result.TreasuryGoldDelta);
        Assert.Contains("胜负未分", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_UpdatesTreasuryAndPower()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55 };
        var yuanShao = new NpcState { Id = "yuan_shao", Name = "袁绍", IsActive = true, Power = 85, Favorability = 50 };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Power = 70, Favorability = 60 };

        state.RegisterNpc(yuanShao);
        state.RegisterNpc(caoCao);

        var evaluator = new GuanduVictoryEvaluator();
        var executor = new GuanduVictoryExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.Equal(5000, state.Treasury); // 2000 + 3000
        Assert.Equal(65, state.ImperialPower); // 55 + 10
        Assert.Equal(100, caoCao.Power); // 70 + 30
        Assert.Equal(25, yuanShao.Power); // 85 - 60
        Assert.Contains(state.Chronicle, c => c.Contains("乌巢") && c.Contains("曹操"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo200_10_2_TriggersGuanduVictory()
    {
        var state = new GameState { Year = 200, Month = 10, Xun = 1, ImperialPower = 55 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 200/10/2

        Assert.Contains(state.Chronicle, c => c.Contains("乌巢") && (c.Contains("袁绍") || c.Contains("曹操") || c.Contains("张郃")));
    }
}
