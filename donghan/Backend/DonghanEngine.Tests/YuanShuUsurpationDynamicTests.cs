using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class YuanShuUsurpationDynamicTests
{
    [Fact]
    public void Test_ImperialCoalitionCrushesYuanShu_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 55 }; // 55 >= 50
        var evaluator = new YuanShuUsurpationEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(YuanShuUsurpationOutcome.ImperialCoalitionCrushesYuanShu, result.Outcome);
        Assert.Equal(15, result.ImperialPowerDelta);
        Assert.Equal(3000, result.TreasuryGoldDelta);
        Assert.Equal(20, result.CaoCaoLoyaltyDelta);
        Assert.Equal(25, result.LiuBeiLoyaltyDelta);
        Assert.Equal(25, result.SunCeLoyaltyDelta);
        Assert.Equal(-90, result.YuanShuPowerDelta);
        Assert.Contains("奉旨讨逆", result.NarrativeTitle);
    }

    [Fact]
    public void Test_CaoCaoIndependentCrush_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 40 }; // 35 <= 40 < 50
        var evaluator = new YuanShuUsurpationEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(YuanShuUsurpationOutcome.CaoCaoIndependentCrush, result.Outcome);
        Assert.Equal(1500, result.TreasuryGoldDelta);
        Assert.Equal(8, result.ImperialPowerDelta);
        Assert.Contains("克寿春", result.NarrativeTitle);
    }

    [Fact]
    public void Test_YuanShuHoldsHuainan_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 35
        var evaluator = new YuanShuUsurpationEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(YuanShuUsurpationOutcome.YuanShuHoldsHuainan, result.Outcome);
        Assert.Equal(-15, result.ImperialPowerDelta);
        Assert.Equal(0, result.TreasuryGoldDelta);
        Assert.Contains("伪帝猖狂", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_KillsYuanShuAndDistributesGold()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55 };
        var yuanShu = new NpcState { Id = "yuan_shu", Name = "袁术", IsActive = true, Power = 80, Favorability = 10 };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Power = 70, Favorability = 60 };
        var liuBei = new NpcState { Id = "liu_bei", Name = "刘备", IsActive = true, Power = 60, Favorability = 70 };
        var sunCe = new NpcState { Id = "sun_ce", Name = "孙策", IsActive = true, Power = 70, Favorability = 60 };

        state.RegisterNpc(yuanShu);
        state.RegisterNpc(caoCao);
        state.RegisterNpc(liuBei);
        state.RegisterNpc(sunCe);

        var evaluator = new YuanShuUsurpationEvaluator();
        var executor = new YuanShuUsurpationExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.False(yuanShu.IsActive, "逆贼袁术应当被诛灭");
        Assert.Equal(5000, state.Treasury); // 2000 + 3000
        Assert.Equal(70, state.ImperialPower); // 55 + 15
        Assert.Equal(80, caoCao.Favorability); // 60 + 20
        Assert.Equal(95, liuBei.Favorability); // 70 + 25
        Assert.Equal(85, sunCe.Favorability); // 60 + 25
        Assert.Contains(state.Chronicle, c => c.Contains("袁术") && c.Contains("寿春"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo197_1_1_TriggersYuanShuUsurpation()
    {
        var state = new GameState { Year = 196, Month = 12, Xun = 3, ImperialPower = 55 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 197/1/1

        Assert.Contains(state.Chronicle, c => c.Contains("袁术") && (c.Contains("称帝") || c.Contains("寿春") || c.Contains("僭号")));
    }
}
