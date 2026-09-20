using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Events;

namespace DonghanEngine.Tests;

public class BaimenlouDynamicTests
{
    [Fact]
    public void Test_ExecuteLvBuConsolidateOrder_WhenCourtStrongAndWestGardenSufficient()
    {
        var state = new GameState { ImperialPower = 60 }; // 60 >= 50
        state.WestGardenArmy.AdjustSize(8500); // 8500 >= 8000

        var evaluator = new BaimenlouEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(BaimenlouOutcome.ExecuteLvBuConsolidateOrder, result.Outcome);
        Assert.True(result.LvBuExecuted);
        Assert.Equal(10, result.ImperialPowerDelta);
        Assert.Equal(1500, result.TreasuryGoldDelta);
        Assert.Equal(20, result.CaoCaoLoyaltyDelta);
        Assert.Equal(25, result.LiuBeiLoyaltyDelta);
        Assert.Contains("白门伏诛", result.NarrativeTitle);
    }

    [Fact]
    public void Test_PardonLvBuDraftToWestGarden_WhenWestGardenNeedsTroops()
    {
        var state = new GameState { ImperialPower = 55 }; // 55 >= 40
        state.WestGardenArmy.Size = 4000; // 4000 <= 8000

        var evaluator = new BaimenlouEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(BaimenlouOutcome.PardonLvBuDraftToWestGarden, result.Outcome);
        Assert.False(result.LvBuExecuted);
        Assert.Equal(2000, result.WestGardenTroopBonus);
        Assert.Equal(25, result.WestGardenMoraleBonus);
        Assert.Contains("飞将归汉", result.NarrativeTitle);
    }

    [Fact]
    public void Test_LvBuFleesToHebei_WhenCourtWeak()
    {
        var state = new GameState { ImperialPower = 30 }; // 30 < 40
        var evaluator = new BaimenlouEvaluator();
        var result = evaluator.Evaluate(state);

        Assert.Equal(BaimenlouOutcome.LvBuFleesToHebei, result.Outcome);
        Assert.False(result.LvBuExecuted);
        Assert.Equal(-10, result.ImperialPowerDelta);
        Assert.Contains("飞将北奔", result.NarrativeTitle);
    }

    [Fact]
    public void Test_Executor_KillsLvBuAndDistributesGold()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 60 };
        state.WestGardenArmy.AdjustSize(9000);
        var lvBu = new NpcState { Id = "lv_bu", Name = "吕布", IsActive = true, Power = 80, Favorability = 30 };
        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", IsActive = true, Power = 70, Favorability = 60 };
        var liuBei = new NpcState { Id = "liu_bei", Name = "刘备", IsActive = true, Power = 60, Favorability = 70 };

        state.RegisterNpc(lvBu);
        state.RegisterNpc(caoCao);
        state.RegisterNpc(liuBei);

        var evaluator = new BaimenlouEvaluator();
        var executor = new BaimenlouExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.False(lvBu.IsActive, "吕布应当被处死");
        Assert.Equal(3500, state.Treasury); // 2000 + 1500
        Assert.Equal(70, state.ImperialPower); // 60 + 10
        Assert.Equal(80, caoCao.Favorability); // 60 + 20
        Assert.Equal(95, liuBei.Favorability); // 70 + 25
        Assert.Contains(state.Chronicle, c => c.Contains("吕布") && c.Contains("白门楼"));
    }

    [Fact]
    public void Test_Executor_PardonsLvBuAndIntegratesIntoWestGarden()
    {
        var state = new GameState { Treasury = 2000, ImperialPower = 45 };
        state.WestGardenArmy.Size = 3000;
        var lvBu = new NpcState { Id = "lv_bu", Name = "吕布", IsActive = true, Power = 80, Favorability = 30 };
        state.RegisterNpc(lvBu);

        var evaluator = new BaimenlouEvaluator();
        var executor = new BaimenlouExecutor();

        var result = evaluator.Evaluate(state);
        executor.Execute(state, result);

        Assert.True(lvBu.IsActive, "吕布应当被赦免");
        Assert.Equal("西园前军校尉", lvBu.Title);
        Assert.Equal("帝党派", lvBu.Faction);
        Assert.Equal(5000, state.WestGardenArmy.Size); // 3000 + 2000
        Assert.Equal(90, lvBu.Favorability);
    }

    [Fact]
    public async Task Test_Engine_AdvancesTo198_10_2_TriggersBaimenlou()
    {
        var state = new GameState { Year = 198, Month = 10, Xun = 1, ImperialPower = 60 };
        state.WestGardenArmy.AdjustSize(9000);
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        await engine.NextXunAsync(); // 进入 198/10/2

        Assert.Contains(state.Chronicle, c => c.Contains("吕布") && (c.Contains("白门") || c.Contains("下邳") || c.Contains("赦免")));
    }
}
