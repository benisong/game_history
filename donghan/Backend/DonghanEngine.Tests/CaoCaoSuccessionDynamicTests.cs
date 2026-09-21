using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class CaoCaoSuccessionDynamicTests
{
    [Fact]
    public void Test_ImperialGrantsPosthumousAndRestrainsPi_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 55 }; // 55 >= 50
        var evaluator = new CaoCaoSuccessionEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(CaoCaoSuccessionOutcome.ImperialGrantsPosthumousAndRestrainsPi, result.Outcome);
        Assert.True(result.CaoCaoDied);
        Assert.Equal(15, result.ImperialPowerDelta);
        Assert.Equal(3000, result.TreasuryGoldDelta);
        Assert.Equal(25, result.CaoPiLoyaltyDelta);
        Assert.Contains("魏武星沉", result.NarrativeTitle);
    }

    [Fact]
    public void Test_CaoPiInheritsAndExpandsPower_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 40 }; // 35 <= 40 < 50
        var evaluator = new CaoCaoSuccessionEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(CaoCaoSuccessionOutcome.CaoPiInheritsAndExpandsPower, result.Outcome);
        Assert.True(result.CaoCaoDied);
        Assert.Equal(1500, result.TreasuryGoldDelta);
        Assert.Equal(30, result.CaoPiPowerDelta);
        Assert.Contains("权倾北疆", result.NarrativeTitle);
    }

    [Fact]
    public void Test_CaoPiDefiesImperialAuthority_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 35
        var evaluator = new CaoCaoSuccessionEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(CaoCaoSuccessionOutcome.CaoPiDefiesImperialAuthority, result.Outcome);
        Assert.True(result.CaoCaoDied);
        Assert.Equal(-10, result.ImperialPowerDelta);
        Assert.Equal(40, result.CaoPiPowerDelta);
        Assert.Contains("嗣子跋扈", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_KillsCaoCaoAndDeploysCaoPi()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55 };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Power = 90, Favorability = 70 };
        state.RegisterNpc(caoCao);

        var evaluator = new CaoCaoSuccessionEvaluator();
        var executor = new CaoCaoSuccessionExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.False(caoCao.IsActive, "曹操应当病逝注销");
        Assert.True(state.Npcs.ContainsKey("cao_pi"), "曹丕应当登庸入朝野谱系");
        Assert.Equal(5000, state.Treasury); // 2000 + 3000
        Assert.Equal(70, state.ImperialPower); // 55 + 15
        Assert.Equal(85, state.Npcs["cao_pi"].Favorability); // 60 + 25
        Assert.Contains(state.Chronicle, c => c.Contains("曹操") && c.Contains("曹丕"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo220_1_1_TriggersCaoCaoSuccession()
    {
        var state = new GameState { Year = 219, Month = 12, Xun = 3, ImperialPower = 55 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 220/1/1

        Assert.Contains(state.Chronicle, c => c.Contains("曹操") && (c.Contains("曹丕") || c.Contains("魏武") || c.Contains("病逝")));
    }
}
