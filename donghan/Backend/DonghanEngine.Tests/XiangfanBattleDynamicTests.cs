using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class XiangfanBattleDynamicTests
{
    [Fact]
    public void Test_ImperialMediationSavesGuanYu_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 55 }; // 55 >= 50
        var evaluator = new XiangfanBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(XiangfanBattleOutcome.ImperialMediationSavesGuanYu, result.Outcome);
        Assert.True(result.GuanYuSaved);
        Assert.Equal(15, result.ImperialPowerDelta);
        Assert.Equal(3000, result.TreasuryGoldDelta);
        Assert.Equal(30, result.LiuBeiLoyaltyDelta);
        Assert.Equal(20, result.SunQuanLoyaltyDelta);
        Assert.Contains("帝节罢兵", result.NarrativeTitle);
    }

    [Fact]
    public void Test_HistoricalLvmengCrossesYangtze_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 40 }; // 35 <= 40 < 50
        var evaluator = new XiangfanBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(XiangfanBattleOutcome.HistoricalLvmengCrossesYangtze, result.Outcome);
        Assert.False(result.GuanYuSaved);
        Assert.Equal(1500, result.TreasuryGoldDelta);
        Assert.Equal(-30, result.LiuBeiPowerDelta);
        Assert.Equal(25, result.SunQuanPowerDelta);
        Assert.Contains("白衣渡江", result.NarrativeTitle);
    }

    [Fact]
    public void Test_GuanYuBreaksFanCity_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 35
        var evaluator = new XiangfanBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(XiangfanBattleOutcome.GuanYuBreaksFanCity, result.Outcome);
        Assert.True(result.GuanYuSaved);
        Assert.Equal(-15, result.ImperialPowerDelta);
        Assert.Equal(40, result.LiuBeiPowerDelta);
        Assert.Contains("威逼中原", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_SavesGuanYuAndUpdatesJingzhou()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55 };
        var liuBei = new NpcState { Id = "liu_bei", Name = "刘备", IsActive = true, Power = 80, Favorability = 70 };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Power = 85, Favorability = 60 };
        var sunQuan = new NpcState { Id = "sun_quan", Name = "孙权", IsActive = true, Power = 70, Favorability = 60 };

        state.RegisterNpc(liuBei);
        state.RegisterNpc(caoCao);
        state.RegisterNpc(sunQuan);

        int initialJingzhouDefense = state.Provinces["jingzhou"].DefenseLevel;

        var evaluator = new XiangfanBattleEvaluator();
        var executor = new XiangfanBattleExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.Equal(5000, state.Treasury); // 2000 + 3000
        Assert.Equal(70, state.ImperialPower); // 55 + 15
        Assert.Equal(100, liuBei.Favorability); // 70 + 30
        Assert.Equal(80, sunQuan.Favorability); // 60 + 20
        Assert.Equal("liu_bei", state.Provinces["jingzhou"].GovernorId);
        Assert.Equal(initialJingzhouDefense + 10, state.Provinces["jingzhou"].DefenseLevel);
        Assert.Contains(state.Chronicle, c => c.Contains("关羽") && c.Contains("罢斗"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo219_10_2_TriggersXiangfanBattle()
    {
        var state = new GameState { Year = 219, Month = 10, Xun = 1, ImperialPower = 55 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 219/10/2

        Assert.Contains(state.Chronicle, c => c.Contains("关羽") && (c.Contains("水淹七军") || c.Contains("樊城") || c.Contains("襄樊")));
    }
}
