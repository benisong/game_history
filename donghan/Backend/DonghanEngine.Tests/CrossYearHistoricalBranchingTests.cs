using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class CrossYearHistoricalBranchingTests
{
    [Fact]
    public void Test_XiangfanSavesGuanYu_Then_YilingBattleSkipsConflict_DirectAlliance()
    {
        var state = new GameState { ImperialPower = 60 };
        var xiangfanEvaluator = new XiangfanBattleEvaluator();
        var xiangfanExecutor = new XiangfanBattleExecutor();

        // 1. 219 年 10 月襄樊之战：天子皇权强盛 (>=50)，持节保全关羽
        var xfResult = xiangfanEvaluator.Evaluate(state);
        Assert.True(xfResult.GuanYuSaved);
        xiangfanExecutor.Execute(state, xfResult);

        Assert.True(state.IsGuanYuSavedAtXiangfan, "全局状态应标记关羽已被天子保全");

        // 2. 222 年 6 月夷陵之战：即使此时由于其他原因皇权跌落到弱势 (<35)，夷陵之战也应当跳过混战，维持孙刘和睦
        state.ImperialPower = 20; // 即使皇权很低，由于关羽没死，刘备也不会伐吴
        var yilingEvaluator = new YilingBattleEvaluator();
        var yilingExecutor = new YilingBattleExecutor();

        var ylResult = yilingEvaluator.Evaluate(state);

        Assert.Equal(YilingBattleOutcome.ImperialMediationSunLiuAlliance, ylResult.Outcome);
        Assert.Contains("关羽永固", ylResult.NarrativeTitle);
        Assert.Contains("夷陵无战事", ylResult.NarrativeTitle);

        var turnRes = yilingExecutor.Execute(state, ylResult);
        Assert.Contains("夷陵烽烟消弭无形", turnRes.StoryText);
    }

    [Fact]
    public void Test_XiangfanGuanYuKilled_Then_YilingBattleTriggersFireAttackOrConflict()
    {
        var state = new GameState { ImperialPower = 40 }; // 弱势朝廷
        var xiangfanEvaluator = new XiangfanBattleEvaluator();
        var xiangfanExecutor = new XiangfanBattleExecutor();

        // 1. 219 年襄樊之战：朝廷中平/微弱，关羽走麦城被害
        var xfResult = xiangfanEvaluator.Evaluate(state);
        Assert.False(xfResult.GuanYuSaved);
        xiangfanExecutor.Execute(state, xfResult);

        Assert.False(state.IsGuanYuSavedAtXiangfan, "关羽战死");

        // 2. 222 年夷陵之战：按史实触发陆逊火烧连营
        var yilingEvaluator = new YilingBattleEvaluator();
        var ylResult = yilingEvaluator.Evaluate(state);

        Assert.Equal(YilingBattleOutcome.LuXunFireAttackTriumph, ylResult.Outcome);
        Assert.Contains("火烧连营", ylResult.NarrativeTitle);
    }
}
