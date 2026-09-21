using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class ChibiBattleDynamicTests
{
    [Fact]
    public void Test_ImperialTruceAndTributeBalance_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 55 }; // 55 >= 50
        var evaluator = new ChibiBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(ChibiBattleOutcome.ImperialTruceAndTributeBalance, result.Outcome);
        Assert.Equal(15, result.ImperialPowerDelta);
        Assert.Equal(4000, result.TreasuryGoldDelta);
        Assert.Equal(20, result.CaoCaoLoyaltyDelta);
        Assert.Equal(25, result.LiuBeiLoyaltyDelta);
        Assert.Equal(25, result.SunQuanLoyaltyDelta);
        Assert.Contains("赤壁持节", result.NarrativeTitle);
    }

    [Fact]
    public void Test_SunLiuChibiFireTriumph_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 40 }; // 35 <= 40 < 50
        var evaluator = new ChibiBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(ChibiBattleOutcome.SunLiuChibiFireTriumph, result.Outcome);
        Assert.Equal(1500, result.TreasuryGoldDelta);
        Assert.Equal(30, result.LiuBeiPowerDelta);
        Assert.Equal(-30, result.CaoCaoPowerDelta);
        Assert.Contains("火烧赤壁", result.NarrativeTitle);
    }

    [Fact]
    public void Test_CaoCaoCrossesYangtzeHegemony_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 35
        var evaluator = new ChibiBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(ChibiBattleOutcome.CaoCaoCrossesYangtzeHegemony, result.Outcome);
        Assert.Equal(-15, result.ImperialPowerDelta);
        Assert.Equal(45, result.CaoCaoPowerDelta);
        Assert.Contains("席卷东南", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_DeploysSunQuanAndUpdatesTreasury()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55 };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Power = 85, Favorability = 60 };
        var liuBei = new NpcState { Id = "liu_bei", Name = "刘备", IsActive = true, Power = 60, Favorability = 70 };

        state.RegisterNpc(caoCao);
        state.RegisterNpc(liuBei);

        var evaluator = new ChibiBattleEvaluator();
        var executor = new ChibiBattleExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.True(state.Npcs.ContainsKey("sun_quan"), "孙权应当被登庸入朝野谱系");
        Assert.Equal(6000, state.Treasury); // 2000 + 4000
        Assert.Equal(70, state.ImperialPower); // 55 + 15
        Assert.Equal(85, state.Npcs["sun_quan"].Favorability); // 60 + 25
        Assert.Equal(80, caoCao.Favorability); // 60 + 20
        Assert.Equal(95, liuBei.Favorability); // 70 + 25
        Assert.Contains(state.Chronicle, c => c.Contains("赤壁") && c.Contains("罢战"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo208_11_2_TriggersChibiBattle()
    {
        var state = new GameState { Year = 208, Month = 11, Xun = 1, ImperialPower = 55 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 208/11/2

        Assert.Contains(state.Chronicle, c => c.Contains("赤壁") && (c.Contains("曹操") || c.Contains("孙权") || c.Contains("刘备") || c.Contains("持节")));
    }
}
