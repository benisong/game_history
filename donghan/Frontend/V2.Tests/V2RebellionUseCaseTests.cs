using DonghanEngine.Core;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;
using Xunit;

namespace DonghanFrontend.V2.Tests;

public sealed class V2RebellionUseCaseTests
{
    [Fact]
    public void Suppress_rebellion_success_updates_province_and_returns_success()
    {
        var state = new GameState();
        state.Provinces["jizhou"].IsRebelling = true;
        state.Provinces["jizhou"].RebelFaction = "黄巾军";
        state.Provinces["jizhou"].RebellionMonths = 2;
        var runtime = V2RuntimeFactory.CreateDefault(state, new Random(42));

        var result = runtime.Intel.ExecuteProvinceAction(
            new ProvinceActionCommand("jizhou", ProvinceActionKind.SuppressRebellion, "cao_cao", 3000));

        Assert.True(result.Success, $"{result.ErrorCode}: {result.StoryText}");
        Assert.False(runtime.State.GetProvince("jizhou")!.IsRebelling);
        Assert.Contains("成功", result.StoryText);
    }

    [Fact]
    public void Pacify_rebellion_success_with_persuade_updates_province()
    {
        var state = new GameState();
        state.Provinces["yuzhou"].IsRebelling = true;
        state.Provinces["yuzhou"].RebelFaction = "民变";
        state.Provinces["yuzhou"].RebellionMonths = 1;
        var runtime = V2RuntimeFactory.CreateDefault(state, new Random(42));

        var result = runtime.Intel.ExecuteProvinceAction(
            new ProvinceActionCommand("yuzhou", ProvinceActionKind.PacifyRebellion, "cao_cao", Strategy: "说服"));

        Assert.True(result.Success);
        Assert.False(runtime.State.GetProvince("yuzhou")!.IsRebelling);
        Assert.Contains("成功", result.StoryText);
    }

    [Fact]
    public void Rebellion_action_on_peaceful_province_returns_failure()
    {
        var runtime = V2RuntimeFactory.CreateDefault();

        var result = runtime.Intel.ExecuteProvinceAction(
            new ProvinceActionCommand("jizhou", ProvinceActionKind.SuppressRebellion, "cao_cao"));

        Assert.False(result.Success);
        Assert.Equal(nameof(InvalidOperationException), result.ErrorCode);
    }

    [Fact]
    public void Pacify_without_strategy_returns_failure()
    {
        var state = new GameState();
        state.Provinces["jizhou"].IsRebelling = true;
        var runtime = V2RuntimeFactory.CreateDefault(state);

        var result = runtime.Intel.ExecuteProvinceAction(
            new ProvinceActionCommand("jizhou", ProvinceActionKind.PacifyRebellion, "cao_cao"));

        Assert.False(result.Success);
        Assert.Equal(nameof(ArgumentException), result.ErrorCode);
    }
}
