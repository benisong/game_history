using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class TongguanBattleDynamicTests
{
    [Fact]
    public void Test_ImperialReclaimsGuanzhongDirect_WhenCourtStrong()
    {
        var state = new GameState { ImperialPower = 55 }; // 55 >= 50
        var evaluator = new TongguanBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(TongguanBattleOutcome.ImperialReclaimsGuanzhongDirect, result.Outcome);
        Assert.Equal("ma_chao", result.LiangzhouGovernorId);
        Assert.Equal(12, result.ImperialPowerDelta);
        Assert.Equal(3000, result.TreasuryGoldDelta);
        Assert.Equal(20, result.MaChaoLoyaltyDelta);
        Assert.Contains("三辅归京", result.NarrativeTitle);
    }

    [Fact]
    public void Test_CaoCaoSubduesGuanzhongHegemony_WhenCourtModerate()
    {
        var state = new GameState { ImperialPower = 40 }; // 35 <= 40 < 50
        var evaluator = new TongguanBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(TongguanBattleOutcome.CaoCaoSubduesGuanzhongHegemony, result.Outcome);
        Assert.Equal("cao_cao", result.LiangzhouGovernorId);
        Assert.Equal(1500, result.TreasuryGoldDelta);
        Assert.Equal(35, result.CaoCaoPowerDelta);
        Assert.Contains("尽收秦川", result.NarrativeTitle);
    }

    [Fact]
    public void Test_MaSuperDominatesChangAn_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 35
        var evaluator = new TongguanBattleEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(TongguanBattleOutcome.MaSuperDominatesChangAn, result.Outcome);
        Assert.Equal(-10, result.ImperialPowerDelta);
        Assert.Equal(-15, result.CaoCaoPowerDelta);
        Assert.Contains("关中沦陷", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_DeploysMaChaoAndUpdatesSili()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 55 };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Power = 80, Favorability = 60 };
        state.RegisterNpc(caoCao);

        int initialSiliDefense = state.Provinces["sili"].DefenseLevel;

        var evaluator = new TongguanBattleEvaluator();
        var executor = new TongguanBattleExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.True(state.Npcs.ContainsKey("ma_chao"), "马超应当被登庸入朝野谱系");
        Assert.Equal(5000, state.Treasury); // 2000 + 3000
        Assert.Equal(67, state.ImperialPower); // 55 + 12
        Assert.Equal(70, state.Npcs["ma_chao"].Favorability); // 50 + 20
        Assert.Equal(initialSiliDefense + 10, state.Provinces["sili"].DefenseLevel);
        Assert.Equal("ma_chao", state.Provinces["liangzhou"].GovernorId);
        Assert.Contains(state.Chronicle, c => c.Contains("潼关") && c.Contains("三辅"));
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo211_3_1_TriggersTongguanBattle()
    {
        var state = new GameState { Year = 211, Month = 2, Xun = 3, ImperialPower = 55 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 211/3/1

        Assert.Contains(state.Chronicle, c => c.Contains("潼关") && (c.Contains("马超") || c.Contains("韩遂") || c.Contains("三辅")));
    }
}
