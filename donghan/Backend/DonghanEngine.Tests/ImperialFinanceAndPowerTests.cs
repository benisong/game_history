using System.Collections.Generic;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Politics;

namespace DonghanEngine.Tests;

public class ImperialFinanceAndPowerTests
{
    [Fact]
    public void Test_Confiscation_SeizesWealth_BoostsImperialPowerAndMorale_PenalizesAffiliatedFaction()
    {
        var state = new GameState { Treasury = 1000, ImperialPower = 50 };

        // 注册目标贪官（宦官派）
        var corruptEunuch = new NpcState
        {
            Id = "zhang_rang",
            Name = "张让",
            Faction = "宦官派",
            Corruption = 80,
            StashedWealth = 5000,
            Power = 70,
            Favorability = 90,
            IsActive = true
        };

        // 注册同党（宦官派）
        var fellowEunuch = new NpcState
        {
            Id = "zhao_zhong",
            Name = "赵忠",
            Faction = "宦官派",
            Corruption = 70,
            StashedWealth = 4000,
            Power = 60,
            Favorability = 80,
            IsActive = true
        };

        // 注册异党（士大夫派）
        var scholarOfficial = new NpcState
        {
            Id = "lu_zhi",
            Name = "卢植",
            Faction = "清流派",
            Corruption = 0,
            StashedWealth = 200,
            Power = 50,
            Favorability = 70,
            IsActive = true
        };

        state.RegisterNpc(corruptEunuch);
        state.RegisterNpc(fellowEunuch);
        state.RegisterNpc(scholarOfficial);

        var service = new ConfiscationService();
        var result = service.ConfiscateTarget(state, "zhang_rang");

        Assert.True(result.Success);
        Assert.False(corruptEunuch.IsActive, "被抄家官员应当注销下狱");
        Assert.True(result.GoldSeized > 5000, "起获金帛应当包含基础隐匿与贪腐权势加成");
        Assert.True(state.Treasury > 6000, "国库应当瞬间注入巨款");
        Assert.Equal(58, state.ImperialPower); // 50 + 8

        // 同党官员忠诚应当暴跌 25
        Assert.Equal(55, fellowEunuch.Favorability); // 80 - 25
        // 异党官员忠诚微跌 5
        Assert.Equal(65, scholarOfficial.Favorability); // 70 - 5

        Assert.Contains(state.Chronicle, c => c.Contains("籍没") && c.Contains("张让"));
    }

    [Fact]
    public void Test_MilitaryPayroll_GenerousReward_BoostsMoraleAndImperialPower()
    {
        var state = new GameState { Treasury = 10000, ImperialPower = 50 };
        state.WestGardenArmy.Size = 10000;
        state.WestGardenArmy.Morale = 60;

        var payrollService = new MilitaryPayrollService();
        var result = payrollService.ProcessPayroll(state, grantExtraBonus: true);

        Assert.Equal(MilitaryPayrollStatus.GenerouslyRewarded, result.Status);
        Assert.Equal(56, state.ImperialPower); // 50 + 6 (重赏立威提升皇权)
        Assert.Equal(75, state.WestGardenArmy.Morale); // 60 + 15
        Assert.True(result.GoldSpent > 0);
        Assert.Contains(state.Chronicle, c => c.Contains("犒赏") && (c.Contains("内库") || c.Contains("万岁") || c.Contains("效死")));
    }

    [Fact]
    public void Test_MilitaryPayroll_Shortage_DropsMorale_AndTriggersMutinyWhenMoraleTooLow()
    {
        var state = new GameState { Treasury = 100, ImperialPower = 50 }; // 国库仅剩100贯，严重缺饷
        state.WestGardenArmy.Size = 8000;
        state.WestGardenArmy.Morale = 35; // 处于危险边缘

        var payrollService = new MilitaryPayrollService();
        var result = payrollService.ProcessPayroll(state, grantExtraBonus: false);

        // 第一次缺饷：士气 35 - 20 = 15 (< 30) -> 触发哗变
        Assert.Equal(MilitaryPayrollStatus.MutinyTriggered, result.Status);
        Assert.Equal(35, state.ImperialPower); // 50 - 15 (兵变重创皇权)
        Assert.True(result.DeserterCount >= 1000, "兵变应导致士兵溃散逃逸");
        Assert.Equal(6000, state.WestGardenArmy.Size); // 8000 - 2000
        Assert.Contains(state.Chronicle, c => c.Contains("兵变") || c.Contains("哗变"));
    }
}
