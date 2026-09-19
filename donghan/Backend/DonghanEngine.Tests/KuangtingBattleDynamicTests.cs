using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class KuangtingBattleDynamicTests
{
    [Fact]
    public void Test_ImperialAuthorizesCaoCao_WhenCourtStrongAndCaoCaoLoyal()
    {
        var state = new GameState { ImperialPower = 55 };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Favorability = 60, Power = 60 };
        state.RegisterNpc(caoCao);

        var evaluator = new KuangtingBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(KuangtingBattleOutcome.ImperialAuthorizesCaoCao, result.Outcome);
        Assert.Equal(8, result.ImperialPowerDelta);
        Assert.Equal(1500, result.TreasuryGoldDelta);
        Assert.Equal(25, result.CaoCaoPowerDelta);
        Assert.Equal(20, result.CaoCaoLoyaltyDelta);
        Assert.Contains("奉诏讨逆", result.NarrativeTitle);
    }

    [Fact]
    public void Test_CaoCaoIndependentVictory_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 40 }; // 40 < 50
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Favorability = 55, Power = 60 };
        state.RegisterNpc(caoCao);

        var evaluator = new KuangtingBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(KuangtingBattleOutcome.CaoCaoIndependentVictory, result.Outcome);
        Assert.Equal(1000, result.TreasuryGoldDelta);
        Assert.Equal(2, result.ImperialPowerDelta);
        Assert.Contains("曹操破贼", result.NarrativeTitle);
    }

    [Fact]
    public void Test_YuanShuDominatesZhongyuan_WhenCourtWeakAndLoyaltyLow()
    {
        var state = new GameState { ImperialPower = 30 };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Favorability = 30, Power = 50 };
        state.RegisterNpc(caoCao);

        var evaluator = new KuangtingBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(KuangtingBattleOutcome.YuanShuDominatesZhongyuan, result.Outcome);
        Assert.Equal(-10, result.ImperialPowerDelta);
        Assert.Equal(-10, result.CaoCaoPowerDelta);
        Assert.Contains("袁术僭越", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_UpdatesTreasuryAndChronicle()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55 };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Favorability = 60, Power = 60 };
        state.RegisterNpc(caoCao);

        var evaluator = new KuangtingBattleEvaluator();
        var executor = new KuangtingBattleExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.Equal(3500, state.Treasury); // 2000 + 1500
        Assert.Equal(63, state.ImperialPower); // 55 + 8
        Assert.Equal(85, caoCao.Power); // 60 + 25
        Assert.Contains(state.Chronicle, c => c.Contains("匡亭") && c.Contains("袁术"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo193_3_1_TriggersKuangtingBattle()
    {
        var state = new GameState { Year = 193, Month = 2, Xun = 3, ImperialPower = 55 };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Favorability = 60, Power = 60 };
        state.RegisterNpc(caoCao);

        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 193/3/1

        Assert.Contains(state.Chronicle, c => c.Contains("匡亭") || c.Contains("袁术"));
    }
}
