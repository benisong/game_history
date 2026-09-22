using System.Linq;
using System.Threading.Tasks;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Politics;

namespace DonghanEngine.Tests;

public class GameEngineCoreIntegrationTests
{
    [Fact]
    public void Test_ExecuteConfiscateTarget_InCourt_SeizesWealthAndExecutesService()
    {
        var state = new GameState { CurrentLocation = "宣政殿", Treasury = 2000, ImperialPower = 50 };
        var corruptNpc = new NpcState
        {
            Id = "corrupt_minister",
            Name = "弄权奸臣",
            Faction = "宦官派",
            Corruption = 60,
            StashedWealth = 3000,
            Power = 50,
            IsActive = true
        };
        state.RegisterNpc(corruptNpc);

        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        var result = engine.ExecuteConfiscateTarget("corrupt_minister");

        Assert.True(result.Success);
        Assert.False(corruptNpc.IsActive);
        Assert.True(state.Treasury > 5000);
        Assert.True(state.ImperialPower > 50);
        Assert.Contains(state.Chronicle, c => c.Contains("籍没") && c.Contains("弄权奸臣"));
    }

    [Fact]
    public void Test_ExecuteGrantMilitaryBonus_InWestGarden_BoostsMoraleAndImperialPower()
    {
        var state = new GameState { CurrentLocation = "西园", Treasury = 10000, ImperialPower = 50 };
        state.WestGardenArmy.Size = 6000;
        state.WestGardenArmy.Morale = 60;

        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        var result = engine.ExecuteGrantMilitaryBonus();

        Assert.Equal(MilitaryPayrollStatus.GenerouslyRewarded, result.Status);
        Assert.True(state.ImperialPower > 50);
        Assert.Equal(75, state.WestGardenArmy.Morale);
        Assert.True(result.GoldSpent > 0);
    }

    [Fact]
    public void Test_TalentNominationWorkflow_AppointAndReject()
    {
        var state = new GameState { ImperialPower = 50 };
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        var nominations = engine.GetPendingNominations();
        Assert.NotEmpty(nominations);

        var xunYu = nominations.FirstOrDefault(n => n.CandidateId == "xun_yu");
        Assert.NotNull(xunYu);

        // 批准除官
        var appointResult = engine.AppointNominationCandidate(xunYu, "尚书令");
        Assert.True(appointResult.Appointed);
        Assert.True(state.Npcs.ContainsKey("xun_yu"));
        Assert.Equal("尚书令", state.Npcs["xun_yu"].Title);

        var xunChen = nominations.FirstOrDefault(n => n.CandidateId == "xun_chen");
        Assert.NotNull(xunChen);

        // 驳回弃贤
        var rejectResult = engine.RejectNominationCandidate(xunChen);
        Assert.False(rejectResult.Appointed);
        Assert.Equal("yuan_shao", rejectResult.DestinationFactionId);
    }

    [Fact]
    public async Task Test_NextXunAsync_TriggersAutomaticPayroll_AndMacroEconomySpillover()
    {
        var state = new GameState
        {
            Year = 184,
            Month = 3,
            Xun = 2, // 下一步进入 184/3/3 季末结算
            Treasury = 5000,
            ImperialPower = 52
        };
        state.WestGardenArmy.Size = 8000;
        state.WestGardenArmy.Morale = 50;

        // 设置青州严重超载
        state.Provinces["qingzhou"].Population = 200000;
        state.Provinces["qingzhou"].LandCarryingCapacity = 80000;

        var caoCao = new NpcState { Id = "cao_cao", Name = "曹操", Power = 60, IsActive = true };
        state.RegisterNpc(caoCao);

        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        int initialTreasury = state.Treasury;

        await engine.NextXunAsync(); // 进入 184/3/3

        // 1. 验证常规军饷已自动核扣
        Assert.True(state.Treasury < initialTreasury, "每旬推进应自动扣除西园禁军常规军饷");

        // 2. 验证季末宏观经济超载触发流民潮与曹操收编
        Assert.True(caoCao.Power > 60, "青州严重超载应在季末反哺曹操收编流民部曲增长权势");
        Assert.Contains(state.Chronicle, c => c.Contains("青州兵") || c.Contains("发饷") || c.Contains("流民"));
    }
}
