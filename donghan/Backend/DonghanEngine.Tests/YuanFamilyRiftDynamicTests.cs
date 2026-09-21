using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class YuanFamilyRiftDynamicTests
{
    [Fact]
    public void Test_ImperialDivideAndConquer_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 55 }; // 55 >= 50
        var evaluator = new YuanFamilyRiftEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(YuanFamilyRiftOutcome.ImperialDivideAndConquer, result.Outcome);
        Assert.True(result.YuanShaoDied);
        Assert.Equal(10, result.ImperialPowerDelta);
        Assert.Equal(2000, result.TreasuryGoldDelta);
        Assert.Equal(20, result.CaoCaoLoyaltyDelta);
        Assert.Contains("帝策分化", result.NarrativeTitle);
    }

    [Fact]
    public void Test_CaoCaoSubduesHebeiDirect_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 40 }; // 35 <= 40 < 50
        var evaluator = new YuanFamilyRiftEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(YuanFamilyRiftOutcome.CaoCaoSubduesHebeiDirect, result.Outcome);
        Assert.True(result.YuanShaoDied);
        Assert.Equal(1500, result.TreasuryGoldDelta);
        Assert.Equal(30, result.CaoCaoPowerDelta);
        Assert.Contains("曹操克邺", result.NarrativeTitle);
    }

    [Fact]
    public void Test_YuanFamilyReconciles_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 35
        var evaluator = new YuanFamilyRiftEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(YuanFamilyRiftOutcome.YuanFamilyReconciles, result.Outcome);
        Assert.True(result.YuanShaoDied);
        Assert.Equal(-5, result.ImperialPowerDelta);
        Assert.Contains("犹存余威", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_KillsYuanShaoAndUpdatesTreasury()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55 };
        var yuanShao = new NpcState { Id = "yuan_shao", Name = "袁绍", IsActive = true, Power = 30, Favorability = 50 };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Power = 80, Favorability = 60 };

        state.RegisterNpc(yuanShao);
        state.RegisterNpc(caoCao);

        var evaluator = new YuanFamilyRiftEvaluator();
        var executor = new YuanFamilyRiftExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.False(yuanShao.IsActive, "袁绍应当病故注销");
        Assert.Equal(4000, state.Treasury); // 2000 + 2000
        Assert.Equal(65, state.ImperialPower); // 55 + 10
        Assert.Equal(80, caoCao.Favorability); // 60 + 20
        Assert.Contains(state.Chronicle, c => c.Contains("袁绍") && c.Contains("袁谭"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo202_5_1_TriggersYuanFamilyRift()
    {
        var state = new GameState { Year = 202, Month = 4, Xun = 3, ImperialPower = 55 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 202/5/1

        Assert.Contains(state.Chronicle, c => c.Contains("袁绍") && (c.Contains("病") || c.Contains("袁谭") || c.Contains("袁尚")));
    }
}
