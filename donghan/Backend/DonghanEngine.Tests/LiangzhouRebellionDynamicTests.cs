using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class LiangzhouRebellionDynamicTests
{
    [Fact]
    public void Test_HistoricalUprising_WhenDefaultOpening()
    {
        var state = new GameState(); // 默认开局：凉州民心25，守军4000
        var evaluator = new LiangzhouRebellionEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(LiangzhouRebellionOutcome.HistoricalUprising, result.Outcome);
        Assert.True(result.LiangzhouRebelling);
        Assert.True(result.DeployDongZhuo);
        Assert.True(result.DeployMaTeng);
        Assert.Equal(-5, result.ImperialPowerDelta);
    }

    [Fact]
    public void Test_GarrisonSurrendered_WhenTreasuryDepleted()
    {
        var state = new GameState { Treasury = 1000 };
        state.Provinces["liangzhou"].AdjustLocalSupport(-10); // 25 - 10 = 15 < 20

        var evaluator = new LiangzhouRebellionEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(LiangzhouRebellionOutcome.GarrisonSurrendered, result.Outcome);
        Assert.True(result.LiangzhouRebelling);
        Assert.Equal(-10, result.ImperialPowerDelta);
        Assert.Contains("兵变", result.NarrativeTitle);
    }

    [Fact]
    public void Test_FrontierContained_WhenWellReinforced()
    {
        var state = new GameState();
        // 重点布防：民心 >= 40 且 守军 >= 5000
        state.Provinces["liangzhou"].AdjustLocalSupport(20); // 25 + 20 = 45 >= 40
        state.Provinces["liangzhou"].AdjustGarrison(2000);   // 4000 + 2000 = 6000 >= 5000

        var evaluator = new LiangzhouRebellionEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(LiangzhouRebellionOutcome.FrontierContained, result.Outcome);
        Assert.False(result.LiangzhouRebelling);
        Assert.False(result.DeployDongZhuo); // 董卓未被紧急起用
        Assert.True(result.DeployMaTeng);   // 马腾作为守关边将登场
        Assert.Equal(5, result.ImperialPowerDelta);
    }

    [Fact]
    public void Test_Executor_DeploysGeneralsAndSetsRebellion()
    {
        var state = new GameState();
        Assert.False(state.Npcs.ContainsKey("dong_zhuo"));
        Assert.False(state.Npcs.ContainsKey("ma_teng"));

        var evaluator = new LiangzhouRebellionEvaluator();
        var executor = new LiangzhouRebellionExecutor();

        var rebellion = evaluator.Evaluate(state);
        executor.Execute(state, rebellion);

        Assert.True(state.Provinces["liangzhou"].IsRebelling);
        Assert.True(state.Npcs.ContainsKey("dong_zhuo"), "董卓应登庸入局");
        Assert.True(state.Npcs.ContainsKey("ma_teng"), "马腾应登庸入局");
        Assert.Contains(state.Chronicle, c => c.Contains("凉州") || c.Contains("韩遂"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo184_11_2_TriggersLiangzhouRebellion()
    {
        var state = new GameState { Year = 184, Month = 11, Xun = 1 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 184/11/2

        Assert.True(state.Provinces["liangzhou"].IsRebelling, "进入 184/11/2 时凉州叛乱应触发");
        Assert.True(state.Npcs.ContainsKey("dong_zhuo"), "董卓应在 184/11/2 边乱时登庸");
    }
}
