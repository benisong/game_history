using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class GrandJubileeDynamicTests
{
    [Fact]
    public void Test_GreatRestorationGrandJubilee_WhenCourtSuperStrong()
    {
        var state = new GameState { ImperialPower = 65 }; // 65 >= 50
        state.WestGardenArmy.Size = 10000; // 10000 >= 8000

        var evaluator = new GrandJubileeEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(GrandJubileeOutcome.GreatRestorationGrandJubilee, result.Outcome);
        Assert.True(result.GrandRestorationAchieved);
        Assert.Equal(30, result.ImperialPowerDelta);
        Assert.Equal(5000, result.TreasuryGoldDelta);
        Assert.Equal(30, result.CaoPiLoyaltyDelta);
        Assert.Equal(30, result.ZhugeLiangLoyaltyDelta);
        Assert.Equal(30, result.SunQuanLoyaltyDelta);
        Assert.Equal(30, result.WestGardenMoraleBonus);
        Assert.Contains("七旬天子万年", result.NarrativeTitle);
    }

    [Fact]
    public void Test_ProsperousAutonomousTributes_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 40 }; // 35 <= 40 < 50
        state.WestGardenArmy.Size = 5000;

        var evaluator = new GrandJubileeEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(GrandJubileeOutcome.ProsperousAutonomousTributes, result.Outcome);
        Assert.False(result.GrandRestorationAchieved);
        Assert.Equal(2000, result.TreasuryGoldDelta);
        Assert.Equal(15, result.ImperialPowerDelta);
        Assert.Equal(15, result.CaoPiLoyaltyDelta);
        Assert.Contains("诸侯归心", result.NarrativeTitle);
    }

    [Fact]
    public void Test_FactionalFissuresAtJubilee_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 35
        state.WestGardenArmy.Size = 3000;

        var evaluator = new GrandJubileeEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(GrandJubileeOutcome.FactionalFissuresAtJubilee, result.Outcome);
        Assert.False(result.GrandRestorationAchieved);
        Assert.Equal(0, result.ImperialPowerDelta);
        Assert.Equal(0, result.TreasuryGoldDelta);
        Assert.Contains("藩镇异心", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_LocksMaxImperialPowerAndMaximizesLoyalty()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 70 };
        state.WestGardenArmy.Size = 12000;
        state.WestGardenArmy.Morale = 70;

        var caoPi = new NpcState { Id = "cao_pi", Name = "曹丕", IsActive = true, Power = 85, Favorability = 70 };
        var zhugeLiang = new NpcState { Id = "zhuge_liang", Name = "诸葛亮", IsActive = true, Power = 85, Favorability = 80 };
        var sunQuan = new NpcState { Id = "sun_quan", Name = "孙权", IsActive = true, Power = 80, Favorability = 70 };

        state.RegisterNpc(caoPi);
        state.RegisterNpc(zhugeLiang);
        state.RegisterNpc(sunQuan);

        var evaluator = new GrandJubileeEvaluator();
        var executor = new GrandJubileeExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.Equal(7000, state.Treasury); // 2000 + 5000
        Assert.Equal(100, state.ImperialPower); // 大中兴达成锁定 100 满值
        Assert.Equal(100, state.WestGardenArmy.Morale); // 70 + 30 达到 100
        Assert.Equal(100, caoPi.Favorability); // 70 + 30
        Assert.Equal(100, zhugeLiang.Favorability); // 80 + 30 clamp 100
        Assert.Equal(100, sunQuan.Favorability); // 70 + 30
        Assert.Contains(state.Chronicle, c => c.Contains("七旬") && c.Contains("大汉中兴"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo226_8_2_TriggersGrandJubilee()
    {
        var state = new GameState { Year = 226, Month = 8, Xun = 1, ImperialPower = 60 };
        state.WestGardenArmy.Size = 10000;
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 226/8/2

        Assert.Contains(state.Chronicle, c => c.Contains("七旬") || c.Contains("大寿") || c.Contains("九宾"));
    }
}
