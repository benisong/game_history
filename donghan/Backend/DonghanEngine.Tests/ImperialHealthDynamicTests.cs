using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Health;

namespace DonghanEngine.Tests;

public class ImperialHealthDynamicTests
{
    private readonly ImperialHealthService _healthService = new();

    [Fact]
    public void Test_RestAtWendePalace_RecoversFortyPercentOfCurrentEnergy()
    {
        var state = new GameState
        {
            HiddenCurrentEnergy = 50,
            HiddenMaxEnergy = 100,
            HiddenYangVitality = 100
        };

        // 规则 2：恢复当前剩余精力的 40%
        // 50 * 0.40 = 20 点恢复 -> 50 + 20 = 70
        var res = _healthService.RestAtWendePalace(state);

        Assert.True(res.Success);
        Assert.Equal(20, res.EnergyDelta);
        Assert.Equal(70, state.HiddenCurrentEnergy);
    }

    [Fact]
    public void Test_YangVitalityDepletion_ReducesRecoveryRate()
    {
        // 规则 4：阳气低于50恢复效率变成一半即20%
        var stateYang30 = new GameState
        {
            HiddenCurrentEnergy = 50,
            HiddenMaxEnergy = 100,
            HiddenYangVitality = 30 // 阳气 < 50
        };

        // 50 * 0.20 = 10 点恢复 -> 50 + 10 = 60
        var res30 = _healthService.RestAtWendePalace(stateYang30);
        Assert.Equal(10, res30.EnergyDelta);
        Assert.Equal(60, stateYang30.HiddenCurrentEnergy);

        // 阳气低于20恢复效率变成10%
        var stateYang10 = new GameState
        {
            HiddenCurrentEnergy = 50,
            HiddenMaxEnergy = 100,
            HiddenYangVitality = 10 // 阳气 < 20
        };

        // 50 * 0.10 = 5 点恢复 -> 50 + 5 = 55
        var res10 = _healthService.RestAtWendePalace(stateYang10);
        Assert.Equal(5, res10.EnergyDelta);
        Assert.Equal(55, stateYang10.HiddenCurrentEnergy);
    }

    [Fact]
    public void Test_Settlement_EnergyBelowThresholds_PermanentlyReducesMaxEnergy()
    {
        // 规则 3：当前精力低于40扣1点最大值，低于20扣2点
        var state35 = new GameState
        {
            HiddenCurrentEnergy = 35,
            HiddenMaxEnergy = 100,
            HiddenYangVitality = 80
        };

        _healthService.AdvanceXunHealthSettlement(state35);
        Assert.Equal(99, state35.HiddenMaxEnergy); // 100 - 1
        Assert.Equal(83, state35.HiddenYangVitality); // 规则 1：阳气随时间自然回升 +3

        var state15 = new GameState
        {
            HiddenCurrentEnergy = 15,
            HiddenMaxEnergy = 100,
            HiddenYangVitality = 80
        };

        _healthService.AdvanceXunHealthSettlement(state15);
        Assert.Equal(98, state15.HiddenMaxEnergy); // 100 - 2
    }

    [Fact]
    public void Test_MaxEnergyBelowEighty_TriggersAfflictionAndReducesLifespan()
    {
        // 规则 5：精力最大值低于80开始生病，每少10点少10年寿命
        var state = new GameState
        {
            HiddenCurrentEnergy = 75,
            HiddenMaxEnergy = 75 // 80 - 75 = 5 (处于第一个10点区间，扣10年)
        };

        var diagnosis = _healthService.GetPhysicianDiagnosis(state);
        Assert.True(diagnosis.IsAfflictedWithDisease);
        Assert.Equal(10, diagnosis.YearsOfLifeLost);
        Assert.Contains("病骨染疾", diagnosis.NarrativeSummary);

        // 若精力上限跌到 65 (少了15点，处于第二个区间，扣20年)
        state.HiddenMaxEnergy = 65;
        var diagnosis65 = _healthService.GetPhysicianDiagnosis(state);
        Assert.Equal(20, diagnosis65.YearsOfLifeLost);
    }

    [Fact]
    public void Test_HaremIndulgence_DrainsYangVitality()
    {
        var state = new GameState
        {
            HiddenCurrentEnergy = 40,
            HiddenMaxEnergy = 100,
            HiddenYangVitality = 90
        };

        var res = _healthService.IndulgeInHarem(state);
        Assert.True(res.Success);
        Assert.Equal(-12, res.YangDelta);
        Assert.Equal(78, state.HiddenYangVitality); // 90 - 12
        Assert.Equal(55, state.HiddenCurrentEnergy); // 40 + 15
    }
}
