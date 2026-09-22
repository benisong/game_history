using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class YilingBattleDynamicTests
{
    [Fact]
    public void Test_ImperialMediationSunLiuAlliance_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 55 }; // 55 >= 50
        var evaluator = new YilingBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(YilingBattleOutcome.ImperialMediationSunLiuAlliance, result.Outcome);
        Assert.Equal(15, result.ImperialPowerDelta);
        Assert.Equal(3000, result.TreasuryGoldDelta);
        Assert.Equal(25, result.LiuBeiLoyaltyDelta);
        Assert.Equal(25, result.SunQuanLoyaltyDelta);
        Assert.Contains("孙刘修好", result.NarrativeTitle);
    }

    [Fact]
    public void Test_LuXunFireAttackTriumph_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 40 }; // 35 <= 40 < 50
        var evaluator = new YilingBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(YilingBattleOutcome.LuXunFireAttackTriumph, result.Outcome);
        Assert.Equal(1500, result.TreasuryGoldDelta);
        Assert.Equal(-30, result.LiuBeiPowerDelta);
        Assert.Equal(25, result.SunQuanPowerDelta);
        Assert.Contains("火烧连营", result.NarrativeTitle);
    }

    [Fact]
    public void Test_LiuBeiCrushesEasternWu_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 35
        var evaluator = new YilingBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(YilingBattleOutcome.LiuBeiCrushesEasternWu, result.Outcome);
        Assert.Equal(-10, result.ImperialPowerDelta);
        Assert.Equal(35, result.LiuBeiPowerDelta);
        Assert.Equal(-35, result.SunQuanPowerDelta);
        Assert.Contains("蜀军破吴", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_UpdatesTreasuryAndPower()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55 };
        var liuBei = new NpcState { Id = "liu_bei", Name = "刘备", IsActive = true, Power = 80, Favorability = 70 };
        var sunQuan = new NpcState { Id = "sun_quan", Name = "孙权", IsActive = true, Power = 75, Favorability = 60 };

        state.RegisterNpc(liuBei);
        state.RegisterNpc(sunQuan);

        var evaluator = new YilingBattleEvaluator();
        var executor = new YilingBattleExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.Equal(5000, state.Treasury); // 2000 + 3000
        Assert.Equal(70, state.ImperialPower); // 55 + 15
        Assert.Equal(95, liuBei.Favorability); // 70 + 25
        Assert.Equal(85, sunQuan.Favorability); // 60 + 25
        Assert.Equal(90, liuBei.Power); // 80 + 10
        Assert.Equal(85, sunQuan.Power); // 75 + 10
        Assert.Contains(state.Chronicle, c => c.Contains("夷陵") && c.Contains("罢战"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo222_6_2_TriggersYilingBattle()
    {
        var state = new GameState { Year = 222, Month = 6, Xun = 1, ImperialPower = 55 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 222/6/2

        Assert.Contains(state.Chronicle, c => c.Contains("夷陵") && (c.Contains("刘备") || c.Contains("孙权") || c.Contains("修好") || c.Contains("火烧连营")));
    }
}
