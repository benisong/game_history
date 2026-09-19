using System.Collections.Generic;
using DonghanEngine.Core;
using DonghanEngine.Core.Geopolitics.Contracts;
using DonghanEngine.Core.Geopolitics.Memorials;
using DonghanEngine.Core.Geopolitics.Models;
using Xunit;

namespace DonghanEngine.Tests.Geopolitics;

public class GeopoliticalMemorialAndEdictTests
{
    [Fact]
    public void Test_MemorialFactory_CreatesPetitionMemorial_WithValidOptions()
    {
        var victor = new WarlordFaction("cao_cao", "cao_cao", "曹操", WarlordPosture.AmbitiousWarlord, new[] { "yanzhou" }, 15000, 5000, 60, 90);
        var prov = new Province { Id = "yanzhou", Name = "兖州" };
        var factory = new GeopoliticalMemorialFactory();

        var memorial = factory.CreatePetitionMemorial(victor, "yanzhou", prov);

        Assert.Equal(GeopoliticalMemorialType.PetitionRank, memorial.MemorialType);
        Assert.Equal("cao_cao", memorial.SenderFactionId);
        Assert.Equal(2, memorial.Options.Count);
        Assert.Contains("兖州", memorial.Title);
    }

    [Fact]
    public void Test_MemorialFactory_CreatesAppealMemorial_WhenBesieged()
    {
        var besieged = new WarlordFaction("liu_yu", "liu_yu", "刘虞", WarlordPosture.LoyalistBanner, new[] { "youzhou" }, 3000, 2000, 90, 10);
        var attacker = new WarlordFaction("gongsun_zan", "gongsun_zan", "公孙瓒", WarlordPosture.AmbitiousWarlord, new[] { "youzhou" }, 15000, 5000, 40, 85);
        var prov = new Province { Id = "youzhou", Name = "幽州" };
        var factory = new GeopoliticalMemorialFactory();

        var memorial = factory.CreateAppealMemorial(besieged, attacker, "youzhou", prov);

        Assert.Equal(GeopoliticalMemorialType.AppealForHelp, memorial.MemorialType);
        Assert.Equal("liu_yu", memorial.SenderFactionId);
        Assert.Equal(2, memorial.Options.Count);
        Assert.Contains("公孙瓒", memorial.ContentText);
    }

    [Fact]
    public void Test_EdictExecutor_RatifyAndReward_IncreasesTreasuryAndLoyalty()
    {
        var state = new GameState { Treasury = 5000, ImperialPower = 50 };
        var caoCao = new WarlordFaction("cao_cao", "cao_cao", "曹操", WarlordPosture.AmbitiousWarlord, new[] { "yanzhou" }, 15000, 5000, 60, 90);
        var factions = new Dictionary<string, WarlordFaction> { ["cao_cao"] = caoCao };
        var relations = new FactionRelationGraph();

        var edict = new GeopoliticalImperialEdict(GeopoliticalEdictType.RatifyAndReward, "cao_cao", string.Empty, "yanzhou", 2000, "准奏追认");
        var executor = new ImperialEdictExecutor();

        var result = executor.ExecuteEdict(edict, state, factions, relations);

        Assert.True(result.Success);
        Assert.Equal(7000, state.Treasury);
        Assert.Equal(47, state.ImperialPower); // -3 微损威严
        Assert.Equal(75, caoCao.ImperialLoyalty); // 60 + 15 = 75
        Assert.Contains(state.Chronicle, c => c.Contains("官爵追认") && c.Contains("曹操"));
    }

    [Fact]
    public void Test_EdictExecutor_DenounceAndProvoke_BoostsImperialPower_AndAggravatesHostility()
    {
        var state = new GameState { ImperialPower = 50 };
        var yuanShao = new WarlordFaction("yuan_shao", "yuan_shao", "袁绍", WarlordPosture.AmbitiousWarlord, new[] { "jizhou" }, 20000, 8000, 50, 85);
        var gongsunZan = new WarlordFaction("gongsun_zan", "gongsun_zan", "公孙瓒", WarlordPosture.AmbitiousWarlord, new[] { "youzhou" }, 15000, 6000, 60, 75);

        var factions = new Dictionary<string, WarlordFaction>
        {
            ["yuan_shao"] = yuanShao,
            ["gongsun_zan"] = gongsunZan
        };
        var relations = new FactionRelationGraph();

        var edict = new GeopoliticalImperialEdict(GeopoliticalEdictType.DenounceAndProvoke, "yuan_shao", "gongsun_zan", "jizhou", 0, "严斥袁绍");
        var executor = new ImperialEdictExecutor();

        var result = executor.ExecuteEdict(edict, state, factions, relations);

        Assert.True(result.Success);
        Assert.Equal(58, state.ImperialPower); // +8 刚正不阿
        Assert.Equal(20, yuanShao.ImperialLoyalty); // 50 - 30 = 20
        Assert.Equal(40, relations.GetHostility("gongsun_zan", "yuan_shao")); // 挑动公孙瓒敌对度激化
        Assert.Contains(state.Chronicle, c => c.Contains("天子明斥"));
    }

    [Fact]
    public void Test_EdictExecutor_MediateTruce_EstablishesTruce_AndCollectsGold()
    {
        var state = new GameState { Treasury = 4000, ImperialPower = 40 };
        var facA = new WarlordFaction("fac_a", "lead_a", "诸侯A", WarlordPosture.AmbitiousWarlord, new[] { "a" }, 5000, 3000, 50, 50);
        var facB = new WarlordFaction("fac_b", "lead_b", "诸侯B", WarlordPosture.CautiousAutonomist, new[] { "b" }, 4000, 3000, 50, 50);

        var factions = new Dictionary<string, WarlordFaction> { ["fac_a"] = facA, ["fac_b"] = facB };
        var relations = new FactionRelationGraph();

        var edict = new GeopoliticalImperialEdict(GeopoliticalEdictType.MediateTruce, "fac_a", "fac_b", "a", 1000, "敕令罢兵");
        var executor = new ImperialEdictExecutor();

        var result = executor.ExecuteEdict(edict, state, factions, relations);

        Assert.True(result.Success);
        Assert.Equal(6000, state.Treasury); // +2000 (双方各1000)
        Assert.True(relations.HasTruce("fac_a", "fac_b"), "双方应建立朝廷休战令");
        Assert.Contains(state.Chronicle, c => c.Contains("朝廷持节") && c.Contains("罢兵"));
    }
}
