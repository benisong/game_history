using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class YijingSiegeDynamicTests
{
    [Fact]
    public void Test_RatifyYuanShaoAndElevateCaoCao_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 55 }; // 55 >= 50
        var evaluator = new YijingSiegeEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(YijingSiegeOutcome.RatifyYuanShaoAndElevateCaoCao, result.Outcome);
        Assert.Equal(5, result.ImperialPowerDelta);
        Assert.Equal(2000, result.TreasuryGoldDelta);
        Assert.Equal(30, result.YuanShaoPowerDelta);
        Assert.Equal(20, result.CaoCaoPowerDelta);
        Assert.Equal("yuan_shao", result.YouzhouGovernorId);
        Assert.Contains("帝策制衡", result.NarrativeTitle);
    }

    [Fact]
    public void Test_ImperialSecretAllianceGongsun_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 40 }; // 35 <= 40 < 50
        var evaluator = new YijingSiegeEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(YijingSiegeOutcome.ImperialSecretAllianceGongsun, result.Outcome);
        Assert.Equal(1000, result.TreasuryGoldDelta);
        Assert.Equal(2000, result.WestGardenTroopBonus);
        Assert.Contains("义从归汉", result.NarrativeTitle);
    }

    [Fact]
    public void Test_YuanShaoDominatesNorthUnchecked_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 35
        var evaluator = new YijingSiegeEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(YijingSiegeOutcome.YuanShaoDominatesNorthUnchecked, result.Outcome);
        Assert.Equal(-10, result.ImperialPowerDelta);
        Assert.Equal(45, result.YuanShaoPowerDelta);
        Assert.Contains("四州皆袁", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_UpdatesTitlesAndYouzhou()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55 };
        var yuanShao = new NpcState { Id = "yuan_shao", Name = "袁绍", IsActive = true, Power = 70, Favorability = 50 };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Power = 70, Favorability = 60 };

        state.RegisterNpc(yuanShao);
        state.RegisterNpc(caoCao);

        var evaluator = new YijingSiegeEvaluator();
        var executor = new YijingSiegeExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.Equal("大将军", yuanShao.Title);
        Assert.Equal("司空", caoCao.Title);
        Assert.Equal(4000, state.Treasury); // 2000 + 2000
        Assert.Equal(60, state.ImperialPower); // 55 + 5
        Assert.Equal("yuan_shao", state.Provinces["youzhou"].GovernorId);
        Assert.Contains(state.Chronicle, c => c.Contains("易京") && c.Contains("袁绍"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo199_3_1_TriggersYijingSiege()
    {
        var state = new GameState { Year = 199, Month = 2, Xun = 3, ImperialPower = 55 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 199/3/1

        Assert.Contains(state.Chronicle, c => c.Contains("易京") && (c.Contains("袁绍") || c.Contains("公孙瓒") || c.Contains("大将军")));
    }
}
