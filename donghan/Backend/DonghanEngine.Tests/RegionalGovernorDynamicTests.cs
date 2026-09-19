using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class RegionalGovernorDynamicTests
{
    [Fact]
    public void Test_Historical_AdoptStatePastorSystem_WhenDefaultState()
    {
        var state = new GameState(); // 默认开局：皇权25 < 60
        var evaluator = new RegionalGovernorEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(RegionalGovernorDecision.AdoptStatePastorSystem, result.Decision);
        Assert.Equal(3, result.AppointedGovernorIds.Count);
        Assert.Equal("liu_yan", result.ProvinceGovernorMappings["yizhou"]);
        Assert.Equal("liu_yu", result.ProvinceGovernorMappings["youzhou"]);
        Assert.Equal("liu_biao", result.ProvinceGovernorMappings["jingzhou"]);
        Assert.Equal(-10, result.ImperialPowerDelta);
        Assert.Equal(10, result.PopularSupportDelta);
    }

    [Fact]
    public void Test_RejectAndKeepCentralized_WhenImperialPowerAndSupportHigh()
    {
        var state = new GameState { ImperialPower = 65, PopularSupport = 60 };
        var evaluator = new RegionalGovernorEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(RegionalGovernorDecision.RejectAndKeepCentralized, result.Decision);
        Assert.Empty(result.AppointedGovernorIds);
        Assert.Equal(5, result.ImperialPowerDelta);
        Assert.Contains("驳回", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_DeploysPastorsAndImprovesRemoteProvinces()
    {
        var state = new GameState();
        int initialYizhouSupport = state.Provinces["yizhou"].LocalSupport;

        var evaluator = new RegionalGovernorEvaluator();
        var executor = new RegionalGovernorExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        // 益州、幽州、荆州任命州牧且民心提升
        Assert.Equal("liu_yan", state.Provinces["yizhou"].GovernorId);
        Assert.Equal(initialYizhouSupport + 20, state.Provinces["yizhou"].LocalSupport);
        Assert.Equal("liu_yu", state.Provinces["youzhou"].GovernorId);
        Assert.Equal("liu_biao", state.Provinces["jingzhou"].GovernorId);

        // 刘焉、刘虞、刘表登庸且权势增加
        Assert.True(state.Npcs.ContainsKey("liu_yan"));
        Assert.True(state.Npcs.ContainsKey("liu_yu"));
        Assert.True(state.Npcs.ContainsKey("liu_biao"));
        Assert.True(state.Npcs["liu_yan"].Power >= 40);
        Assert.Contains(state.Chronicle, c => c.Contains("刘焉") || c.Contains("州牧"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo188_3_1_TriggersRegionalGovernor()
    {
        var state = new GameState { Year = 188, Month = 2, Xun = 3 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 188/3/1

        Assert.Contains(state.Chronicle, c => c.Contains("州牧") || c.Contains("刘焉"));
        Assert.True(state.Npcs.ContainsKey("liu_yan"), "188年3月刘焉应登台就任州牧");
    }
}
