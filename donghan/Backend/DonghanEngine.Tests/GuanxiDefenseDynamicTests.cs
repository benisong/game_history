using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class GuanxiDefenseDynamicTests
{
    [Fact]
    public void Test_FirmBorderDeterrence_WhenHuangfuSongLoyalAndStrong()
    {
        var state = new GameState { Treasury = 2000 };
        var huangfu = new NpcState { Id = "huangfu_song", Name = "皇甫嵩", IsActive = true, Favorability = 80, Power = 70 };
        state.RegisterNpc(huangfu);

        var evaluator = new GuanxiDefenseEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(GuanxiDefenseOutcome.FirmBorderDeterrence, result.Outcome);
        Assert.Equal(10, result.ImperialPowerDelta);
        Assert.Equal(500, result.TreasuryCost);
        Assert.Equal(-10, result.DongZhuoPowerDelta);
        Assert.Contains("关西锁钥", result.NarrativeTitle);
    }

    [Fact]
    public void Test_BribeAndAppease_WhenHuangfuSongWeak_ButTreasuryAbundant()
    {
        var state = new GameState { Treasury = 1500 }; // 1500 >= 1000
        // 不注册或弱化皇甫嵩

        var evaluator = new GuanxiDefenseEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(GuanxiDefenseOutcome.BribeAndAppease, result.Outcome);
        Assert.Equal(1000, result.TreasuryCost);
        Assert.Equal(20, result.DongZhuoFavorDelta);
        Assert.Contains("金帛羁縻", result.NarrativeTitle);
    }

    [Fact]
    public void Test_BorderBorderSkirmish_WhenBothWeakAndTreasuryLow()
    {
        var state = new GameState { Treasury = 200 }; // 200 < 1000

        var evaluator = new GuanxiDefenseEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(GuanxiDefenseOutcome.BorderBorderSkirmish, result.Outcome);
        Assert.Equal(-10, result.ImperialPowerDelta);
        Assert.Equal(-10, result.PopularSupportDelta);
        Assert.Contains("西凉犯境", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_UpdatesNpcStatsAndTreasury()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 50, PopularSupport = 50 };
        var huangfu = new NpcState { Id = "huangfu_song", Name = "皇甫嵩", IsActive = true, Favorability = 80, Power = 70 };
        var dong = new NpcState { Id = "dong_zhuo", Name = "董卓", IsActive = true, Favorability = 20, Power = 80 };
        state.RegisterNpc(huangfu);
        state.RegisterNpc(dong);

        var evaluator = new GuanxiDefenseEvaluator();
        var executor = new GuanxiDefenseExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.Equal(1500, state.Treasury); // 2000 - 500
        Assert.Equal(60, state.ImperialPower); // 50 + 10
        Assert.Equal(80, huangfu.Power); // 70 + 10
        Assert.Equal(70, dong.Power); // 80 - 10
        Assert.Contains(state.Chronicle, c => c.Contains("皇甫嵩") && c.Contains("郿坞"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo190_6_2_TriggersGuanxiDefense()
    {
        var state = new GameState { Year = 190, Month = 6, Xun = 1, Treasury = 2000 };
        var huangfu = new NpcState { Id = "huangfu_song", Name = "皇甫嵩", IsActive = true, Favorability = 80, Power = 70 };
        state.RegisterNpc(huangfu);

        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 190/6/2

        Assert.Contains(state.Chronicle, c => c.Contains("皇甫嵩") || c.Contains("关西"));
    }
}
