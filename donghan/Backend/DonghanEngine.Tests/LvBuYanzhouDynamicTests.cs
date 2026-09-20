using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class LvBuYanzhouDynamicTests
{
    [Fact]
    public void Test_ReliefAndMediateYanzhou_WhenCourtStrongAndTreasuryAbundant()
    {
        var state = new GameState { ImperialPower = 55, Treasury = 2000, PopularSupport = 50 };
        var evaluator = new LvBuYanzhouEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(LvBuYanzhouOutcome.ReliefAndMediateYanzhou, result.Outcome);
        Assert.Equal(10, result.ImperialPowerDelta);
        Assert.Equal(15, result.PopularSupportDelta);
        Assert.Equal(1200, result.TreasuryFoodCost);
        Assert.Equal(20, result.CaoCaoLoyaltyDelta);
        Assert.Equal(25, result.LvBuLoyaltyDelta);
        Assert.Contains("开仓赈饥", result.NarrativeTitle);
    }

    [Fact]
    public void Test_CaoCaoRecoversYanzhou_WhenTreasuryModerate()
    {
        var state = new GameState { ImperialPower = 40, Treasury = 800 }; // 500 <= 800 < 1200
        var evaluator = new LvBuYanzhouEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(LvBuYanzhouOutcome.CaoCaoRecoversYanzhou, result.Outcome);
        Assert.Equal(500, result.TreasuryFoodCost);
        Assert.Equal(2, result.ImperialPowerDelta);
        Assert.Contains("曹操逐吕", result.NarrativeTitle);
    }

    [Fact]
    public void Test_ZhongyuanFamineCollapse_WhenTreasuryLow()
    {
        var state = new GameState { ImperialPower = 30, Treasury = 200 };
        var evaluator = new LvBuYanzhouEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(LvBuYanzhouOutcome.ZhongyuanFamineCollapse, result.Outcome);
        Assert.Equal(-10, result.ImperialPowerDelta);
        Assert.Equal(-20, result.PopularSupportDelta);
        Assert.Contains("赤地千里", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_DeploysLvBuAndDeductsTreasury()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55, PopularSupport = 50 };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Power = 60, Favorability = 50 };
        state.RegisterNpc(caoCao);

        var evaluator = new LvBuYanzhouEvaluator();
        var executor = new LvBuYanzhouExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.True(state.Npcs.ContainsKey("lv_bu"), "吕布应当被登庸入朝野谱系");
        Assert.Equal(800, state.Treasury); // 2000 - 1200
        Assert.Equal(65, state.ImperialPower); // 55 + 10
        Assert.Equal(65, state.PopularSupport); // 50 + 15
        Assert.Contains(state.Chronicle, c => c.Contains("吕布") && (c.Contains("常平仓") || c.Contains("赈济")));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo194_5_1_TriggersLvBuYanzhou()
    {
        var state = new GameState { Year = 194, Month = 4, Xun = 3, ImperialPower = 55, Treasury = 2000 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 194/5/1

        Assert.Contains(state.Chronicle, c => c.Contains("吕布") || c.Contains("大蝗"));
    }
}
