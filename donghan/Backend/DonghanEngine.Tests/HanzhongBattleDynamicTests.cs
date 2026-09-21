using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class HanzhongBattleDynamicTests
{
    [Fact]
    public void Test_ImperialRatifiesKingOfHanzhong_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 55 }; // 55 >= 50
        var evaluator = new HanzhongBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(HanzhongBattleOutcome.ImperialRatifiesKingOfHanzhong, result.Outcome);
        Assert.Equal("汉中王", result.LiuBeiTitle);
        Assert.Equal(15, result.ImperialPowerDelta);
        Assert.Equal(4000, result.TreasuryGoldDelta);
        Assert.Equal(30, result.LiuBeiLoyaltyDelta);
        Assert.Equal(35, result.LiuBeiPowerDelta);
        Assert.Contains("皇叔进王", result.NarrativeTitle);
    }

    [Fact]
    public void Test_CaoCaoHoldsHanzhongFrontier_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 40 }; // 35 <= 40 < 50
        var evaluator = new HanzhongBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(HanzhongBattleOutcome.CaoCaoHoldsHanzhongFrontier, result.Outcome);
        Assert.Equal(2000, result.TreasuryGoldDelta);
        Assert.Equal(5, result.ImperialPowerDelta);
        Assert.Equal(30, result.LiuBeiPowerDelta);
        Assert.Contains("鼎足鼎立", result.NarrativeTitle);
    }

    [Fact]
    public void Test_LiuBeiBreaksIntoGuanzhong_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 35
        var evaluator = new HanzhongBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(HanzhongBattleOutcome.LiuBeiBreaksIntoGuanzhong, result.Outcome);
        Assert.Equal(-10, result.ImperialPowerDelta);
        Assert.Equal(40, result.LiuBeiPowerDelta);
        Assert.Contains("蜀军出峡", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_UpdatesLiuBeiTitleAndYizhouDefense()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55 };
        var liuBei = new NpcState { Id = "liu_bei", Name = "刘备", IsActive = true, Power = 60, Favorability = 70, Title = "益州牧", TitleTier = 3 };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Power = 85, Favorability = 60 };

        state.RegisterNpc(liuBei);
        state.RegisterNpc(caoCao);

        int initialYizhouDefense = state.Provinces["yizhou"].DefenseLevel;

        var evaluator = new HanzhongBattleEvaluator();
        var executor = new HanzhongBattleExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.Equal(6000, state.Treasury); // 2000 + 4000
        Assert.Equal(70, state.ImperialPower); // 55 + 15
        Assert.Equal("汉中王", liuBei.Title);
        Assert.Equal(1, liuBei.TitleTier);
        Assert.Equal(100, liuBei.Favorability); // 70 + 30
        Assert.Equal(95, liuBei.Power); // 60 + 35
        Assert.Equal(initialYizhouDefense + 15, state.Provinces["yizhou"].DefenseLevel);
        Assert.Contains(state.Chronicle, c => c.Contains("汉中王") && c.Contains("刘备"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo219_5_1_TriggersHanzhongBattle()
    {
        var state = new GameState { Year = 219, Month = 4, Xun = 3, ImperialPower = 55 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 219/5/1

        Assert.Contains(state.Chronicle, c => c.Contains("汉中") && (c.Contains("刘备") || c.Contains("夏侯渊") || c.Contains("汉中王")));
    }
}
