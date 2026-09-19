using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class YellowTurbanDynamicScopeTests
{
    [Fact]
    public void Test_Catastrophic_WhenPopularSupportBelow20()
    {
        var state = new GameState { PopularSupport = 15 };
        var evaluator = new YellowTurbanScopeEvaluator();
        var result = evaluator.EvaluateScope(state);

        Assert.Equal(YellowTurbanIntensity.Catastrophic, result.Intensity);
        Assert.Equal(6, result.AffectedProvinceIds.Count);
        Assert.Contains("jizhou", result.AffectedProvinceIds);
        Assert.Contains("yuzhou", result.AffectedProvinceIds);
        Assert.Contains("yanzhou", result.AffectedProvinceIds);
        Assert.Contains("qingzhou", result.AffectedProvinceIds);
        Assert.Contains("xuzhou", result.AffectedProvinceIds);
        Assert.Contains("bingzhou", result.AffectedProvinceIds);
        Assert.Equal(25, result.PopularSupportDrop);
    }

    [Fact]
    public void Test_Historical_WhenDefaultOpeningState()
    {
        var state = new GameState(); // 默认开局：兖州无太守且民心35 (<40)
        var evaluator = new YellowTurbanScopeEvaluator();
        var result = evaluator.EvaluateScope(state);

        // 冀州(必然起事) + 兖州(无太守且民心35触发)
        Assert.Contains("jizhou", result.AffectedProvinceIds);
        Assert.Contains("yanzhou", result.AffectedProvinceIds);
    }

    [Fact]
    public void Test_Contained_WhenYuzhouAndYanzhouWellGoverned()
    {
        var state = new GameState();
        
        // 豫州初始55分，通过赈灾或奖励提升至 65 >= 60 -> 不反
        state.Provinces["yuzhou"].AdjustLocalSupport(10);
        
        // 给兖州任命曹操，且民心提升至 55 >= 50 -> 不反
        state.Provinces["yanzhou"].AppointGovernor("cao_cao", supportBonus: 20); // 35 + 20 = 55 >= 50

        var evaluator = new YellowTurbanScopeEvaluator();
        var result = evaluator.EvaluateScope(state);

        Assert.Equal(YellowTurbanIntensity.Contained, result.Intensity);
        Assert.Single(result.AffectedProvinceIds);
        Assert.Equal("jizhou", result.AffectedProvinceIds[0]);
        Assert.Equal(5, result.PopularSupportDrop);
    }

    [Fact]
    public void Test_Executor_CorrectlySetsRebellionStateAndRevokesGovernor()
    {
        var state = new GameState();
        var evaluator = new YellowTurbanScopeEvaluator();
        var executor = new YellowTurbanOutbreakExecutor();

        var scope = evaluator.EvaluateScope(state);
        executor.ExecuteOutbreak(state, scope);

        // 冀州必然反
        var jizhou = state.Provinces["jizhou"];
        Assert.True(jizhou.IsRebelling);
        Assert.Equal("黄巾军", jizhou.RebelFaction);
        Assert.Null(state.Npcs["qiao_xuan"].GovernedProvinceId); // 桥玄还朝
        Assert.Contains(state.Chronicle, c => c.Contains("张角"));
    }
}
