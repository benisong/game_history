using Xunit;
using DonghanEngine.Core;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Tests;

public class V2OfficialRankUiTests
{
    [Fact]
    public void Test_RankUi_PromoteStepByStep_AndExtraordinaryWithin3Steps()
    {
        var state = new GameState { CurrentLocation = "宣政殿" };
        var officer = new NpcState
        {
            Id = "xun_yu",
            Name = "荀彧",
            Title = "尚书郎", // 7品
            TitleTier = 7,
            Favorability = 60,
            IsActive = true
        };
        state.RegisterNpc(officer);

        var runtime = V2RuntimeFactory.CreateDefault(state);

        // 1. 获取所有品阶官职
        var positions = runtime.Ranks.GetAllPositions();
        Assert.NotEmpty(positions);
        Assert.Contains(positions, p => p.Title == "太尉");

        // 2. 超擢 3 级 (7品 -> 4品 郡太守)
        var result = runtime.Ranks.Promote("xun_yu", "郡太守");
        Assert.True(result.Success);
        Assert.Contains("荀彧", result.Title);
        Assert.Equal(4, state.Npcs["xun_yu"].TitleTier);
        Assert.Equal("郡太守", state.Npcs["xun_yu"].Title);

        // 3. 超擢超过 3 级 (4品 企图直升 1品 太尉 -> 跨3级以内？4-1=3级允许，但再降到 9品 企图升 1品 跨8级阻断)
        var rookie = new NpcState { Id = "rookie", Name = "新兵", Title = "军候", TitleTier = 9, IsActive = true };
        state.RegisterNpc(rookie);

        var failResult = runtime.Ranks.Promote("rookie", "太尉"); // 9品 -> 1品 跨8级
        Assert.False(failResult.Success);
        Assert.Contains("逾制", failResult.Title);
    }

    [Fact]
    public void Test_RankUi_SellOffice_BypassesSteps_EntersPrivateVault()
    {
        var state = new GameState { CurrentLocation = "西园", PrivateTreasury = 100 };
        var merchant = new NpcState { Id = "rich_man", Name = "豪商", Title = "布衣", TitleTier = 9, IsActive = true };
        state.RegisterNpc(merchant);

        var runtime = V2RuntimeFactory.CreateDefault(state);

        // 西园卖官：直接买一品太尉 (10000万)
        var result = runtime.Ranks.SellOffice("rich_man", "太尉");
        Assert.True(result.Success);
        Assert.Contains("万金堂", result.Title);
        Assert.Equal(1, state.Npcs["rich_man"].TitleTier);
        Assert.Equal("太尉", state.Npcs["rich_man"].Title);
        Assert.True(runtime.State.GetSnapshot().PrivateTreasury > 10000);
    }
}
