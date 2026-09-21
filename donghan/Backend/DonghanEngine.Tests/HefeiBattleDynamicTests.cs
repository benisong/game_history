using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class HefeiBattleDynamicTests
{
    [Fact]
    public void Test_ImperialMediationAndHuaiheTruce_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 55 }; // 55 >= 50
        var evaluator = new HefeiBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(HefeiBattleOutcome.ImperialMediationAndHuaiheTruce, result.Outcome);
        Assert.Equal(10, result.ImperialPowerDelta);
        Assert.Equal(3000, result.TreasuryGoldDelta);
        Assert.Equal(20, result.CaoCaoLoyaltyDelta);
        Assert.Equal(20, result.SunQuanLoyaltyDelta);
        Assert.Contains("威震逍遥", result.NarrativeTitle);
    }

    [Fact]
    public void Test_ZhangLiaoIndependentGlory_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 40 }; // 35 <= 40 < 50
        var evaluator = new HefeiBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(HefeiBattleOutcome.ZhangLiaoIndependentGlory, result.Outcome);
        Assert.Equal(1500, result.TreasuryGoldDelta);
        Assert.Equal(25, result.CaoCaoPowerDelta);
        Assert.Equal(-20, result.SunQuanPowerDelta);
        Assert.Contains("逍遥津捷", result.NarrativeTitle);
    }

    [Fact]
    public void Test_SunQuanBreaksHefei_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 35
        var evaluator = new HefeiBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(HefeiBattleOutcome.SunQuanBreaksHefei, result.Outcome);
        Assert.Equal(-10, result.ImperialPowerDelta);
        Assert.Equal(35, result.SunQuanPowerDelta);
        Assert.Contains("合肥陷落", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_UpdatesTreasuryAndPower()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55 };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Power = 80, Favorability = 60 };
        var sunQuan = new NpcState { Id = "sun_quan", Name = "孙权", IsActive = true, Power = 70, Favorability = 60 };

        state.RegisterNpc(caoCao);
        state.RegisterNpc(sunQuan);

        var evaluator = new HefeiBattleEvaluator();
        var executor = new HefeiBattleExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.Equal(5000, state.Treasury); // 2000 + 3000
        Assert.Equal(65, state.ImperialPower); // 55 + 10
        Assert.Equal(80, caoCao.Favorability); // 60 + 20
        Assert.Equal(80, sunQuan.Favorability); // 60 + 20
        Assert.Equal(100, caoCao.Power); // 80 + 20
        Assert.Equal(55, sunQuan.Power); // 70 - 15
        Assert.Contains(state.Chronicle, c => c.Contains("张辽") && c.Contains("合肥"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo215_11_2_TriggersHefeiBattle()
    {
        var state = new GameState { Year = 215, Month = 11, Xun = 1, ImperialPower = 55 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 215/11/2

        Assert.Contains(state.Chronicle, c => c.Contains("合肥") && (c.Contains("张辽") || c.Contains("孙权") || c.Contains("逍遥津")));
    }
}
