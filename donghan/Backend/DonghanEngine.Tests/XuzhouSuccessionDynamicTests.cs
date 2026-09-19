using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class XuzhouSuccessionDynamicTests
{
    [Fact]
    public void Test_ImperialRatifiesLiuBei_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 55 }; // 55 >= 50
        var evaluator = new XuzhouSuccessionEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(XuzhouSuccessionOutcome.ImperialRatifiesLiuBei, result.Outcome);
        Assert.Equal("liu_bei", result.XuzhouGovernorId);
        Assert.Equal(10, result.ImperialPowerDelta);
        Assert.Equal(1500, result.TreasuryGoldDelta);
        Assert.Equal(30, result.LiuBeiLoyaltyDelta);
        Assert.Contains("皇叔领徐", result.NarrativeTitle);
    }

    [Fact]
    public void Test_CaoCaoSubduesXuzhou_WhenCaoCaoOverpoweredAndCourtWeak()
    {
        var state = new GameState { ImperialPower = 40 }; // 40 < 50
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Power = 90, Favorability = 40 };
        state.RegisterNpc(caoCao);

        var evaluator = new XuzhouSuccessionEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(XuzhouSuccessionOutcome.CaoCaoSubduesXuzhou, result.Outcome);
        Assert.Equal("cao_cao", result.XuzhouGovernorId);
        Assert.Equal(35, result.CaoCaoPowerDelta);
        Assert.Contains("强藩兼并", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_DeploysLiuBeiAndUpdatesXuzhou()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55 };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Power = 60, Favorability = 50 };
        state.RegisterNpc(caoCao);

        var evaluator = new XuzhouSuccessionEvaluator();
        var executor = new XuzhouSuccessionExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.True(state.Npcs.ContainsKey("liu_bei"), "刘备应当被正式登庸进入朝野谱系");
        Assert.Equal("liu_bei", state.Provinces["xuzhou"].GovernorId);
        Assert.Equal(3500, state.Treasury); // 2000 + 1500
        Assert.Equal(65, state.ImperialPower); // 55 + 10
        Assert.Contains(state.Chronicle, c => c.Contains("刘备") && c.Contains("徐州"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo193_9_2_TriggersXuzhouSuccession()
    {
        var state = new GameState { Year = 193, Month = 9, Xun = 1, ImperialPower = 55 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 193/9/2

        Assert.Contains(state.Chronicle, c => c.Contains("徐州") && (c.Contains("刘备") || c.Contains("陶谦")));
    }
}
