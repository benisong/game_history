using System.Linq;
using Xunit;
using DonghanEngine.Core;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Tests;

public class V2PrestigeAndMacroFinanceTests
{
    [Fact]
    public void Test_StateReader_IncludesPrestigeAndCarryingCapacity()
    {
        var state = new GameState { ImperialPower = 52 };
        state.Provinces["yuzhou"].Population = 150000;
        state.Provinces["yuzhou"].LandCarryingCapacity = 160000;

        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());
        var runtime = V2RuntimeFactory.Create(engine);

        var snapshot = runtime.State.GetSnapshot();
        Assert.Equal("GoldenBalance", snapshot.PrestigeState);
        Assert.Contains("垂拱而治", snapshot.PrestigeDescription);

        var yuzhou = runtime.State.GetProvince("yuzhou");
        Assert.NotNull(yuzhou);
        Assert.Equal(150000, yuzhou.Population);
        Assert.Equal(160000, yuzhou.LandCarryingCapacity);
    }

    [Fact]
    public void Test_SpecialAction_ConfiscateDirect_ExecutesViaAdapter()
    {
        var state = new GameState { CurrentLocation = "宣政殿", Treasury = 2000, ImperialPower = 50 };
        var corruptNpc = new NpcState
        {
            Id = "corrupt_eunuch",
            Name = "贪腐常侍",
            Faction = "宦官派",
            Corruption = 70,
            StashedWealth = 4000,
            Power = 60,
            IsActive = true
        };
        state.RegisterNpc(corruptNpc);

        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());
        var runtime = V2RuntimeFactory.Create(engine);

        var result = runtime.SpecialActions.Execute(new SpecialActionCommand("confiscate_direct", TargetNpcId: "corrupt_eunuch"));

        Assert.True(result.Success);
        Assert.Contains("籍没", result.Title);
        Assert.False(corruptNpc.IsActive);
        Assert.True(runtime.State.GetSnapshot().Treasury > 6000);
    }

    [Fact]
    public void Test_SpecialAction_GrantMilitaryBonus_InWestGarden_ExecutesViaAdapter()
    {
        var state = new GameState { CurrentLocation = "西园", Treasury = 10000, ImperialPower = 50 };
        state.WestGardenArmy.Size = 6000;
        state.WestGardenArmy.Morale = 60;

        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());
        var runtime = V2RuntimeFactory.Create(engine);

        var result = runtime.SpecialActions.Execute(new SpecialActionCommand("grant_military_bonus"));

        Assert.True(result.Success);
        Assert.Contains("犒赏", result.Title);
        Assert.Equal(75, runtime.State.GetSnapshot().WestGardenMorale);
        Assert.True(runtime.State.GetSnapshot().ImperialPower > 50);
    }
}
