using System;
using Xunit;
using DonghanEngine.Core;

namespace DonghanEngine.Tests;

/// <summary>
/// 情报机构(黄门暗探)供养系统测试:档位→月供→工作热情→准确率,私库扣费/不足降级,查探。
/// </summary>
public class IntelAgencyTests
{
    [Theory]
    [InlineData(IntelFundingTier.None, 0, 0, 35)]
    [InlineData(IntelFundingTier.Half, 100, 50, 55)]
    [InlineData(IntelFundingTier.Normal, 200, 100, 75)]
    [InlineData(IntelFundingTier.Lavish, 300, 150, 95)]
    public void FundingTier_MapsToCostZealAccuracy(IntelFundingTier tier, int cost, int zeal, int accuracy)
    {
        Assert.Equal(cost, IntelAgency.MonthlyCost(tier));
        Assert.Equal(zeal, IntelAgency.Zeal(tier));
        Assert.Equal(accuracy, IntelAgency.CurrentAccuracy(tier));
    }

    [Fact]
    public void AccuracyFormula_UsesBalanceConstant_1_6()
    {
        // 公式 75 + 1.6×(zeal-100)×0.25
        Assert.Equal(1.6, IntelAgency.AccuracyBalanceK, 4);
        Assert.Equal(35, IntelAgency.AccuracyFromZeal(0));
        Assert.Equal(55, IntelAgency.AccuracyFromZeal(50));
        Assert.Equal(75, IntelAgency.AccuracyFromZeal(100));
        Assert.Equal(95, IntelAgency.AccuracyFromZeal(150));
    }

    [Fact]
    public void SetIntelFunding_DeductsFromPrivateTreasury()
    {
        var state = new GameState();
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());
        int before = state.PrivateTreasury; // 初始 1200

        bool ok = engine.SetIntelFunding(IntelFundingTier.Normal); // 200 万

        Assert.True(ok);
        Assert.Equal(IntelFundingTier.Normal, state.IntelFunding);
        Assert.Equal(before - 200, state.PrivateTreasury);
        Assert.Equal(75, engine.CurrentIntelAccuracy());
    }

    [Fact]
    public void SetIntelFunding_InsufficientTreasury_FallsBackToNone()
    {
        var state = new GameState();
        state.PrivateTreasury = 50; // 不足以付任何档(最低半价100)
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        bool ok = engine.SetIntelFunding(IntelFundingTier.Lavish); // 想打赏300，但只有50

        Assert.False(ok);
        Assert.Equal(IntelFundingTier.None, state.IntelFunding);
        Assert.Equal(50, state.PrivateTreasury); // 不扣费
        Assert.Equal(35, engine.CurrentIntelAccuracy()); // 降为未供养
    }

    [Fact]
    public async System.Threading.Tasks.Task MonthlySettlement_DeductsEachMonth_AndDropsWhenBroke()
    {
        var state = new GameState();
        state.DisableHistoricalTriggers = true; // 避免历史硬 trigger 干扰
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        engine.SetIntelFunding(IntelFundingTier.Normal); // 立即付一次 200，私库 1200→1000
        Assert.Equal(1000, state.PrivateTreasury);

        // 推进到下一个月初(184/4/1 → 5/1 需跨 3 旬)。跨月时月初再扣 200。
        await engine.NextXunAsync(); // 4/2
        await engine.NextXunAsync(); // 4/3
        await engine.NextXunAsync(); // 5/1 → 月初扣 200
        Assert.Equal(800, state.PrivateTreasury);
        Assert.Equal(IntelFundingTier.Normal, state.IntelFunding);

        // 把私库榨干，下个月初无力续付 → 自动降 None
        state.PrivateTreasury = 50;
        await engine.NextXunAsync(); // 5/2
        await engine.NextXunAsync(); // 5/3
        await engine.NextXunAsync(); // 6/1 → 无力续付
        Assert.Equal(IntelFundingTier.None, state.IntelFunding);
        Assert.Equal(50, state.PrivateTreasury); // 未扣
    }

    [Fact]
    public void InvestigateNpc_HighFunding_ReturnsAccurateAssessment()
    {
        var state = new GameState();
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator(), rng: new Random(1));
        engine.SetIntelFunding(IntelFundingTier.Lavish); // 95% 准确

        // 曹操:统帅90(红) 政治85(金)
        var assess = engine.InvestigateNpc("cao_cao");
        Assert.NotNull(assess);
        Assert.Equal(5, assess!.Count); // 武统政魅廉 五维
    }

    [Fact]
    public void InvestigateNpc_UnknownId_ReturnsNull()
    {
        var state = new GameState();
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());
        Assert.Null(engine.InvestigateNpc("not_exist"));
    }

    [Fact]
    public void SetIntelFundingDetailed_UpgradeShowsMoreZealStory()
    {
        var state = new GameState();
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        engine.SetIntelFunding(IntelFundingTier.Half);            // 先克扣
        var r = engine.SetIntelFundingDetailed(IntelFundingTier.Lavish); // 升档到打赏

        Assert.True(r.Success);
        Assert.Equal(IntelFundingTier.Half, r.PreviousTier);
        Assert.Equal(IntelFundingTier.Lavish, r.NewTier);
        Assert.Equal(95, r.Accuracy);
        Assert.Contains("更为卖力", r.StoryText);   // 升档对比提示
        Assert.Contains("叩首谢恩", r.StoryText);   // 打赏反应
    }

    [Fact]
    public void SetIntelFundingDetailed_DowngradeShowsSlackStory()
    {
        var state = new GameState();
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        engine.SetIntelFunding(IntelFundingTier.Lavish);          // 先打赏
        var r = engine.SetIntelFundingDetailed(IntelFundingTier.Half); // 降档到克扣

        Assert.True(r.Success);
        Assert.Contains("用度缩减", r.StoryText);   // 降档对比提示
    }

    [Fact]
    public void SetIntelFundingDetailed_Broke_ShowsFailureStory()
    {
        var state = new GameState();
        state.PrivateTreasury = 30;
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        var r = engine.SetIntelFundingDetailed(IntelFundingTier.Normal);

        Assert.False(r.Success);
        Assert.Equal(IntelFundingTier.None, r.NewTier);
        Assert.Contains("私库已空", r.StoryText);
    }
}
