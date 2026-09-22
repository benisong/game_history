using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Politics;

namespace DonghanEngine.Tests;

public class ImperialPrestigeDynamicTests
{
    private readonly ImperialPrestigeEvaluator _evaluator = new();

    [Fact]
    public void Test_Prestige_PuppetVulnerable_WhenUnder25()
    {
        var result = _evaluator.Evaluate(18);

        Assert.Equal(PrestigeState.PuppetVulnerable, result.State);
        Assert.True(result.IsPuppetRiskTriggered, "威望过低应触发挟天子风险");
        Assert.True(result.EdictExecutionEfficiency < 0.5, "政令执行效率大打折扣");
        Assert.Equal(0.0, result.OppressionTransferRate);
        Assert.Contains("傀儡", result.StatusDescription);
    }

    [Fact]
    public void Test_Prestige_DisrespectedWeak_When26To45()
    {
        var result = _evaluator.Evaluate(38);

        Assert.Equal(PrestigeState.DisrespectedWeak, result.State);
        Assert.False(result.IsPuppetRiskTriggered);
        Assert.Equal(0.65, result.EdictExecutionEfficiency);
        Assert.Contains("阳奉阴违", result.StatusDescription);
    }

    [Fact]
    public void Test_Prestige_GoldenBalance_WhenCompact46To60()
    {
        var result = _evaluator.Evaluate(52); // 处于 46~60 黄金中庸区

        Assert.Equal(PrestigeState.GoldenBalance, result.State);
        Assert.Equal(1.0, result.EdictExecutionEfficiency);
        Assert.Equal(0.0, result.OppressionTransferRate);
        Assert.True(result.RebellionChanceModifier < 0, "黄金平衡区暴乱几率应下降");
        Assert.Contains("垂拱而治", result.StatusDescription);
    }

    [Fact]
    public void Test_Prestige_OppressiveDread_When61To80()
    {
        var result = _evaluator.Evaluate(72);

        Assert.Equal(PrestigeState.OppressiveDread, result.State);
        Assert.True(result.EdictExecutionEfficiency > 1.0, "高威望政令执行效率提高");
        Assert.True(result.OppressionTransferRate > 0.0, "官员开始向下转嫁压力");
        Assert.Contains("伴君如虎", result.StatusDescription);
    }

    [Fact]
    public void Test_Prestige_TyrannicalTerror_When81To100_TriggersSevereRebellionModifier()
    {
        var result = _evaluator.Evaluate(95);

        Assert.Equal(PrestigeState.TyrannicalTerror, result.State);
        Assert.Equal(1.25, result.EdictExecutionEfficiency);
        Assert.Equal(0.80, result.OppressionTransferRate);
        Assert.True(result.RebellionChanceModifier >= 0.50, "极端暴政应大幅激化民间起义暴乱");
        Assert.Contains("敢怒不敢言", result.StatusDescription);
    }

    [Fact]
    public void Test_EvaluateOppressionTransferOfficials_FiltersCorruptAndAmbitiousTraits()
    {
        var state = new GameState();

        var corruptOfficer = new NpcState
        {
            Id = "corrupt_governor",
            Name = "贪腐太守",
            Corruption = 65,
            Personality = "贪婪",
            IsActive = true
        };

        var honestOfficer = new NpcState
        {
            Id = "honest_scholar",
            Name = "清廉名士",
            Corruption = 0,
            Personality = "正直",
            IsActive = true
        };

        state.RegisterNpc(corruptOfficer);
        state.RegisterNpc(honestOfficer);

        // 高威望情境
        var highPrestigeResult = _evaluator.Evaluate(88);
        var oppressors = _evaluator.EvaluateOppressionTransferOfficials(state, highPrestigeResult);

        Assert.Contains("corrupt_governor", oppressors);
        Assert.DoesNotContain("honest_scholar", oppressors);

        // 黄金平衡区情境
        var goldenResult = _evaluator.Evaluate(50);
        var noOppressors = _evaluator.EvaluateOppressionTransferOfficials(state, goldenResult);
        Assert.Empty(noOppressors);
    }
}
