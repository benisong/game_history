using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Politics;

namespace DonghanEngine.Tests;

public class OfficialRankAndSaleTests
{
    private readonly OfficialRankService _rankService = new();

    [Fact]
    public void Test_GetAllPositions_CoversNineTiersCivilianAndMilitary()
    {
        var positions = _rankService.GetAllPositions();
        Assert.NotEmpty(positions);

        Assert.Contains(positions, p => p.Title == "太尉" && p.RankTier == 1 && p.Branch == OfficialBranch.Civilian);
        Assert.Contains(positions, p => p.Title == "大将军" && p.RankTier == 1 && p.Branch == OfficialBranch.Military);
        Assert.Contains(positions, p => p.Title == "西园校尉" && p.RankTier == 5 && p.Branch == OfficialBranch.Military);
        Assert.Contains(positions, p => p.Title == "郎中" && p.RankTier == 9 && p.Branch == OfficialBranch.Civilian);
    }

    [Fact]
    public void Test_PromoteOfficial_StepByStep_Success()
    {
        var state = new GameState();
        var officer = new NpcState
        {
            Id = "xun_yu",
            Name = "荀彧",
            Title = "尚书郎", // 7品
            TitleTier = 7,
            Favorability = 70,
            IsActive = true
        };
        state.RegisterNpc(officer);

        // 升迁 1 级到 6品 议郎
        var result = _rankService.PromoteOfficial(state, "xun_yu", "议郎");

        Assert.True(result.Success);
        Assert.Equal(6, officer.TitleTier);
        Assert.Equal("议郎", officer.Title);
        Assert.False(result.IsExtraordinary);
        Assert.Equal(85, officer.Favorability); // 70 + 15
    }

    [Fact]
    public void Test_PromoteOfficial_ExtraordinaryWithin3Steps_SuccessWithPenalty()
    {
        var state = new GameState();
        var officer = new NpcState
        {
            Id = "zhuge_liang",
            Name = "诸葛亮",
            Title = "郎中", // 9品
            TitleTier = 9,
            Favorability = 60,
            IsActive = true
        };
        var scholar = new NpcState
        {
            Id = "old_scholar",
            Name = "宿儒老臣",
            Faction = "清流派",
            Favorability = 80,
            IsActive = true
        };
        state.RegisterNpc(officer);
        state.RegisterNpc(scholar);

        // 特旨超擢 3 级 (9品 -> 6品 议郎)
        var result = _rankService.PromoteOfficial(state, "zhuge_liang", "议郎");

        Assert.True(result.Success);
        Assert.Equal(6, officer.TitleTier);
        Assert.True(result.IsExtraordinary);
        Assert.Equal(95, officer.Favorability); // 60 + 35 (获拔擢者死忠)
        Assert.Equal(70, scholar.Favorability); // 80 - 10 (老臣微词)
    }

    [Fact]
    public void Test_PromoteOfficial_Exceeds3Steps_BlockedByCourt()
    {
        var state = new GameState();
        var officer = new NpcState
        {
            Id = "rookie_officer",
            Name = "白丁新官",
            Title = "郎中", // 9品
            TitleTier = 9,
            IsActive = true
        };
        state.RegisterNpc(officer);

        // 企图直接从 9品 超擢为 3品 尚书令 (跨6级 -> 超过上限3级)
        var result = _rankService.PromoteOfficial(state, "rookie_officer", "尚书令");

        Assert.False(result.Success);
        Assert.Equal("ExceedsMaxPromotionSteps", result.ErrorCode);
        Assert.Equal(9, officer.TitleTier); // 维持原职不变
        Assert.Contains(state.Chronicle, c => c.Contains("封驳") && c.Contains("僭越"));
    }

    [Fact]
    public void Test_SellOfficeToNpc_BypassesAllSteps_EntersPrivateVault_DropsMorale()
    {
        var state = new GameState
        {
            PrivateTreasury = 500,
            PopularSupport = 70
        };

        var richMerchant = new NpcState
        {
            Id = "cui_lie",
            Name = "崔烈",
            Title = "布衣",
            TitleTier = 9,
            Faction = "豪强派",
            IsActive = true
        };
        var scholar = new NpcState
        {
            Id = "scholar_purist",
            Name = "清流名士",
            Faction = "清流派",
            Favorability = 80,
            IsActive = true
        };

        state.RegisterNpc(richMerchant);
        state.RegisterNpc(scholar);

        // 西园买官：哪怕是9品布衣，掏10000万直接买1品司徒！
        var result = _rankService.SellOfficeToNpc(state, "cui_lie", "司徒");

        Assert.True(result.Success);
        Assert.Equal("司徒", richMerchant.Title);
        Assert.Equal(1, richMerchant.TitleTier); // 直接登顶一品三公！
        Assert.Equal(10500, state.PrivateTreasury); // 500 + 10000 (万钱进入天子私库)
        Assert.Equal(45, state.PopularSupport); // 70 - 25 (民心暴跌)
        Assert.Equal(60, scholar.Favorability); // 80 - 20 (清流耻与为伍)
        Assert.Contains(state.Chronicle, c => c.Contains("铜臭") || c.Contains("万金堂"));
    }
}
