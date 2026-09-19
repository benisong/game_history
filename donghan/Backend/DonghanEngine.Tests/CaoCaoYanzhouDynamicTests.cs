using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class CaoCaoYanzhouDynamicTests
{
    [Fact]
    public void Test_RatifyAndDraftQingzhouTroops_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 60 }; // 60 >= 55
        var evaluator = new CaoCaoYanzhouEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(CaoCaoYanzhouOutcome.RatifyAndDraftQingzhouTroops, result.Outcome);
        Assert.Equal(8, result.ImperialPowerDelta);
        Assert.Equal(1500, result.TreasuryGoldDelta);
        Assert.Equal(3000, result.WestGardenArmyTroopBonus);
        Assert.Equal(15, result.WestGardenMoraleBonus);
        Assert.Contains("恩威兼济", result.NarrativeTitle);
    }

    [Fact]
    public void Test_RatifyAndAppease_WhenCaoCaoLoyal_AndCourtModerate()
    {
        var state = new GameState { ImperialPower = 45 }; // 45 < 55
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Favorability = 70, Power = 60 };
        state.RegisterNpc(caoCao);

        var evaluator = new CaoCaoYanzhouEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(CaoCaoYanzhouOutcome.RatifyAndAppease, result.Outcome);
        Assert.Equal(2000, result.TreasuryGoldDelta);
        Assert.Equal(25, result.CaoCaoLoyaltyDelta);
        Assert.Contains("顺水推舟", result.NarrativeTitle);
    }

    [Fact]
    public void Test_SendImperialInspector_WhenCourtWeakAndLoyaltyLow()
    {
        var state = new GameState { ImperialPower = 30 };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Favorability = 30, Power = 60 };
        state.RegisterNpc(caoCao);

        var evaluator = new CaoCaoYanzhouEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(CaoCaoYanzhouOutcome.SendImperialInspector, result.Outcome);
        Assert.Equal(-5, result.ImperialPowerDelta);
        Assert.Equal(40, result.CaoCaoPowerDelta);
        Assert.Contains("中原分陕", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_ExpandsWestGardenArmyAndUpdatesYanzhou()
    {
        var state = new GameState { Treasury = 3000, ImperialPower = 55 };
        int initialArmySize = state.WestGardenArmy.Size;
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Favorability = 50, Power = 50 };
        state.RegisterNpc(caoCao);

        var evaluator = new CaoCaoYanzhouEvaluator();
        var executor = new CaoCaoYanzhouExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.Equal(4500, state.Treasury); // 3000 + 1500
        Assert.Equal(63, state.ImperialPower); // 55 + 8
        Assert.Equal(initialArmySize + 3000, state.WestGardenArmy.Size);
        Assert.Equal("cao_cao", state.Provinces["yanzhou"].GovernorId);
        Assert.Contains(state.Chronicle, c => c.Contains("曹操") && c.Contains("兖州"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo192_4_1_TriggersCaoCaoYanzhou()
    {
        var state = new GameState { Year = 192, Month = 3, Xun = 3, ImperialPower = 60 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 192/4/1

        Assert.Contains(state.Chronicle, c => c.Contains("曹操") && (c.Contains("兖州") || c.Contains("黄巾")));
    }
}
