using System;
using System.Collections.Generic;
using DonghanEngine.Core;
using Xunit;

namespace DonghanEngine.Tests;

public class DomainEncapsulationTests
{
    [Fact]
    public void Test_NpcState_AttributesClamping()
    {
        var npc = new NpcState
        {
            Martial = 150,
            Leadership = -20,
            Politics = 999,
            Charisma = -1,
            Ambition = 200,
            Favorability = 120,
            Power = -50,
            Corruption = 500,
            Health = 999,
            TitleTier = 10,
            StashedWealth = -500
        };

        Assert.Equal(100, npc.Martial);
        Assert.Equal(0, npc.Leadership);
        Assert.Equal(100, npc.Politics);
        Assert.Equal(0, npc.Charisma);
        Assert.Equal(100, npc.Ambition);
        Assert.Equal(100, npc.Favorability);
        Assert.Equal(0, npc.Power);
        Assert.Equal(100, npc.Corruption);
        Assert.Equal(100, npc.Health);
        Assert.Equal(9, npc.TitleTier);
        Assert.Equal(0, npc.StashedWealth);
    }

    [Fact]
    public void Test_ArmyState_Clamping()
    {
        var army = new ArmyState
        {
            Size = -100,
            BasePayPerTurn = -50,
            Morale = 250,
            Loyalty = -30
        };

        Assert.Equal(0, army.Size);
        Assert.Equal(0, army.BasePayPerTurn);
        Assert.Equal(100, army.Morale);
        Assert.Equal(0, army.Loyalty);
    }

    [Fact]
    public void Test_GameState_Clamping()
    {
        var state = new GameState
        {
            ImperialPower = 150,
            Treasury = -1000,
            PrivateTreasury = -200,
            PopularSupport = -50,
            Health = 200,
            Month = 15,
            Xun = 5
        };

        Assert.Equal(100, state.ImperialPower);
        Assert.Equal(0, state.Treasury);
        Assert.Equal(0, state.PrivateTreasury);
        Assert.Equal(0, state.PopularSupport);
        Assert.Equal(100, state.Health);
        Assert.Equal(12, state.Month);
        Assert.Equal(3, state.Xun);
    }

    [Fact]
    public void Test_Province_Clamping()
    {
        var province = new Province
        {
            LocalSupport = -50,
            DefenseLevel = 150,
            Garrison = -500,
            Wealth = -200,
            Distance = -5,
            RebellionMonths = -10,
            LowSupportStreakMonths = -3
        };

        Assert.Equal(0, province.LocalSupport);
        Assert.Equal(100, province.DefenseLevel);
        Assert.Equal(0, province.Garrison);
        Assert.Equal(0, province.Wealth);
        Assert.Equal(0, province.Distance);
        Assert.Equal(0, province.RebellionMonths);
        Assert.Equal(0, province.LowSupportStreakMonths);
    }

    [Fact]
    public void Test_NpcRelation_And_ImperialEdict_Clamping()
    {
        var relation = new NpcRelation { Strength = 150 };
        Assert.Equal(100, relation.Strength);

        relation.Strength = -10;
        Assert.Equal(0, relation.Strength);

        var edict = new ImperialEdict { ExpiryXun = -5 };
        Assert.Equal(0, edict.ExpiryXun);
    }
}
