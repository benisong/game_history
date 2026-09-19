using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class YuanShaoJizhouDynamicTests
{
    [Fact]
    public void Test_DenounceAndProvoke_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 60 }; // 60 >= 55
        var evaluator = new YuanShaoJizhouEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(YuanShaoJizhouOutcome.DenounceAndProvokeGongsun, result.Outcome);
        Assert.Equal(8, result.ImperialPowerDelta);
        Assert.Equal(-25, result.YuanShaoLoyaltyDelta);
        Assert.Equal(40, result.GongsunZanHostilityDelta);
        Assert.Contains("驱虎吞狼", result.NarrativeTitle);
    }

    [Fact]
    public void Test_RatifyAndCollectGold_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 45 }; // 35 <= 45 < 55
        var evaluator = new YuanShaoJizhouEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(YuanShaoJizhouOutcome.RatifyAndCollectGold, result.Outcome);
        Assert.Equal(2000, result.TreasuryGoldDelta);
        Assert.Equal(-3, result.ImperialPowerDelta);
        Assert.Equal(15, result.YuanShaoLoyaltyDelta);
        Assert.Contains("官爵追认", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_UpdatesYuanShaoPowerAndJizhouGarrison()
    {
        var state = new GameState { Treasury = 3000, ImperialPower = 45 };
        var yuanShao = new NpcState { Id = "yuan_shao", Name = "袁绍", IsActive = true, Power = 60, Favorability = 50 };
        state.RegisterNpc(yuanShao);

        var evaluator = new YuanShaoJizhouEvaluator();
        var executor = new YuanShaoJizhouExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.Equal(5000, state.Treasury); // 3000 + 2000
        Assert.Equal(85, yuanShao.Power); // 60 + 25
        Assert.Equal("yuan_shao", state.Provinces["jizhou"].GovernorId);
        Assert.Contains(state.Chronicle, c => c.Contains("袁绍") && c.Contains("冀州"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo191_4_1_TriggersYuanShaoJizhou()
    {
        var state = new GameState { Year = 191, Month = 3, Xun = 3, ImperialPower = 50 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 191/4/1

        Assert.Contains(state.Chronicle, c => c.Contains("袁绍") && (c.Contains("冀州") || c.Contains("韩馥")));
    }
}
