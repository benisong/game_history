using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class PacificationBattleDynamicTests
{
    [Fact]
    public void Test_HistoricalFraming_WhenZhangRangPowerfulAndLuZhiFavorLow()
    {
        var state = new GameState(); // 默认开局：张让 Power=60 >= 40, 卢植 Favor=55 < 60
        var evaluator = new PacificationBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(PacificationOutcome.LuZhiFramed, result.Outcome);
        Assert.Equal("huangfu_song", result.GeneralId);
        Assert.True(result.LuZhiImprisoned);
        Assert.True(result.JizhouPacified);
        Assert.Contains("左丰", result.ChronicleText);
    }

    [Fact]
    public void Test_LuZhiTriumph_WhenEmperorTrustsLuZhi()
    {
        var state = new GameState();
        state.Npcs["lu_zhi"].AdjustFavorability(15); // 55 + 15 = 70 >= 60

        var evaluator = new PacificationBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(PacificationOutcome.LuZhiTriumph, result.Outcome);
        Assert.Equal("lu_zhi", result.GeneralId);
        Assert.False(result.LuZhiImprisoned);
        Assert.True(result.JizhouPacified);
        Assert.Equal(10, result.ImperialPowerDelta);
        Assert.Contains("尚书令", result.ChronicleText);
    }

    [Fact]
    public void Test_LuZhiTriumph_WhenEunuchPowerSuppressed()
    {
        var state = new GameState();
        state.Npcs["zhang_rang"].AdjustPower(-40); // 75 - 40 = 35 < 40 中官失势，无法构陷

        var evaluator = new PacificationBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(PacificationOutcome.LuZhiTriumph, result.Outcome);
        Assert.False(result.LuZhiImprisoned);
    }

    [Fact]
    public void Test_Executor_PacifiesJizhouAndUpdatesPower()
    {
        var state = new GameState();
        state.Provinces["jizhou"].StartRebellion("黄巾军", initialLocalSupport: 5, garrisonMultiplier: 2);
        
        var evaluator = new PacificationBattleEvaluator();
        var executor = new PacificationBattleExecutor();

        var battle = evaluator.Evaluate(state);
        executor.Execute(state, battle);

        // 冀州已平定
        Assert.False(state.Provinces["jizhou"].IsRebelling);
        Assert.True(state.Provinces["jizhou"].LocalSupport >= 30);
        // 卢植被下狱，权势下降
        Assert.True(state.Npcs["lu_zhi"].Power < 28);
        // 皇甫嵩功成，权势上升
        Assert.True(state.Npcs["huangfu_song"].Power > 32);
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo184_10_3_TriggersPacification()
    {
        var state = new GameState { Year = 184, Month = 10, Xun = 2 };
        state.Provinces["jizhou"].StartRebellion("黄巾军", 5, 2);

        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());
        await engine.NextXunAsync(); // 进入 184/10/3

        Assert.False(state.Provinces["jizhou"].IsRebelling, "进入 184/10/3 时冀州黄巾应被大捷平定");
        Assert.Contains(state.Chronicle, c => c.Contains("广宗") || c.Contains("曲阳"));
    }
}
