using System.Linq;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Politics;

namespace DonghanEngine.Tests;

public class CourtDelegationDynamicTests
{
    private readonly CourtDelegationService _delegationService = new();

    [Fact]
    public void Test_DirectAffairExecution_ConsumesFullEnergy_WithoutEmbezzlement()
    {
        var state = new GameState
        {
            HiddenCurrentEnergy = 50,
            HiddenMaxEnergy = 100,
            HiddenYangVitality = 80,
            Treasury = 5000,
            PopularSupport = 50
        };

        // 天子亲裁：消耗全额 4 点精力，零私吞
        var rep = _delegationService.ExecuteDirectAffair(state, "affair_irrigation_yuzhou");

        Assert.Equal("emperor", rep.HandlerNpcId);
        Assert.Equal(0, rep.EmbezzledAmount);
        Assert.Equal(46, state.HiddenCurrentEnergy); // 50 - 4
        Assert.Equal(4500, state.Treasury); // 5000 - 500
        Assert.Equal(53, state.PopularSupport); // 50 + 3
    }

    [Fact]
    public void Test_BatchDelegation_ConsumesFixedTwoEnergy_AndIsInfluencedByHandlerCorruption()
    {
        var state = new GameState
        {
            HiddenCurrentEnergy = 50,
            HiddenMaxEnergy = 100,
            HiddenYangVitality = 80,
            Treasury = 5000,
            PopularSupport = 50
        };

        // 注册贪官张让 (贪腐 80)
        var zhangRang = new NpcState
        {
            Id = "zhang_rang",
            Name = "张让",
            Faction = "宦官派",
            Corruption = 80,
            Power = 60,
            StashedWealth = 1000,
            IsActive = true
        };
        state.RegisterNpc(zhangRang);

        // 一键分权交办群僚：仅耗费 2 点精力
        var batchResult = _delegationService.ExecuteBatchDelegation(state);

        Assert.True(batchResult.Success);
        Assert.Equal(2, batchResult.TotalEnergySpent);
        Assert.Equal(48, state.HiddenCurrentEnergy); // 50 - 2 (仅扣 2 点精力)

        // 验证十常侍代办政务产生私吞与财产累加
        var tributeReport = batchResult.ExecutedReports.FirstOrDefault(r => r.AffairId == "affair_tribute_audit");
        Assert.NotNull(tributeReport);
        Assert.True(tributeReport.EmbezzledAmount > 0);
        Assert.True(zhangRang.StashedWealth > 1000); // 贪官私吞入宅邸，留待日后抄家
    }

    [Fact]
    public void Test_DelegationConfig_CanBeUpdatedDynamically_WithoutHardcodedValues()
    {
        var customConfig = new DelegationConfig
        {
            BatchDelegationEnergyCost = 1, // 调整为仅消耗 1 点精力
            CorruptionEmbezzleRatio = 0.01 // 调大贪腐比例
        };

        var service = new CourtDelegationService(customConfig);
        var state = new GameState
        {
            HiddenCurrentEnergy = 50,
            HiddenMaxEnergy = 100
        };

        var res = service.ExecuteBatchDelegation(state);
        Assert.Equal(1, res.TotalEnergySpent);
        Assert.Equal(49, state.HiddenCurrentEnergy); // 50 - 1
    }
}
