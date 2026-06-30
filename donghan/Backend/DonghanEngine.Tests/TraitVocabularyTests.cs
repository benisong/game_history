using System;
using Xunit;
using DonghanEngine.Core;

namespace DonghanEngine.Tests;

/// <summary>
/// "藏锋于词" 数值驱动词组系统专项测试。
/// 覆盖:品阶映射、确定映射主词组、风评稳定性(id种子)、情报查探准确度。
/// </summary>
public class TraitVocabularyTests
{
    [Theory]
    [InlineData(95, TraitTier.Red)]
    [InlineData(90, TraitTier.Red)]
    [InlineData(89, TraitTier.Gold)]
    [InlineData(75, TraitTier.Gold)]
    [InlineData(74, TraitTier.Purple)]
    [InlineData(55, TraitTier.Purple)]
    [InlineData(54, TraitTier.Blue)]
    [InlineData(35, TraitTier.Blue)]
    [InlineData(34, TraitTier.Gray)]
    [InlineData(0, TraitTier.Gray)]
    public void TierOf_MapsValueToCorrectTier(int value, TraitTier expected)
    {
        Assert.Equal(expected, TraitVocabulary.TierOf(value));
    }

    [Theory]
    [InlineData(TraitTier.Red, 1.5)]
    [InlineData(TraitTier.Gold, 1.2)]
    [InlineData(TraitTier.Purple, 1.0)]
    [InlineData(TraitTier.Blue, 0.8)]
    [InlineData(TraitTier.Gray, 0.5)]
    public void MultiplierOf_MatchesDesignTable(TraitTier tier, double expected)
    {
        Assert.Equal(expected, TraitVocabulary.MultiplierOf(tier), 4);
    }

    [Fact]
    public void WordOf_DeterministicMapping()
    {
        // 政治红档 → 经天纬地；灰档 → 不学无术(确定映射)
        Assert.Equal("经天纬地", TraitVocabulary.WordOf(TraitDimension.Politics, 95));
        Assert.Equal("不学无术", TraitVocabulary.WordOf(TraitDimension.Politics, 20));
        // 武力红档 → 万人之敌
        Assert.Equal("万人之敌", TraitVocabulary.WordOf(TraitDimension.Martial, 98));
        // 廉洁灰档(贪) → 贪得无厌；红档 → 两袖清风
        Assert.Equal("贪得无厌", TraitVocabulary.WordOf(TraitDimension.Integrity, 10));
        Assert.Equal("两袖清风", TraitVocabulary.WordOf(TraitDimension.Integrity, 95));
    }

    [Fact]
    public void GetPrimaryTrait_PicksHighestAbilityDimension()
    {
        // 统帅最高 → 主词组取统帅
        var general = new NpcState { Id = "g", Martial = 70, Leadership = 92, Politics = 40, Charisma = 50 };
        var primary = TraitDeriver.GetPrimaryTrait(general);
        Assert.Equal(TraitDimension.Leadership, primary.Dimension);
        Assert.Equal(TraitTier.Red, primary.Tier);
        Assert.Equal("韩白之才", primary.Word);

        // 政治最高 → 主词组取政治
        var sage = new NpcState { Id = "s", Martial = 10, Leadership = 35, Politics = 96, Charisma = 88 };
        Assert.Equal(TraitDimension.Politics, TraitDeriver.GetPrimaryTrait(sage).Dimension);
    }

    [Fact]
    public void GetPrimaryTrait_IgnoresAmbitionAndIntegrity()
    {
        // 野心/廉洁极高也不抢主词组(只从武统政魅取)
        var npc = new NpcState { Id = "n", Martial = 60, Leadership = 50, Politics = 45, Charisma = 40, Ambition = 99, Integrity = 99 };
        var primary = TraitDeriver.GetPrimaryTrait(npc);
        Assert.Equal(TraitDimension.Martial, primary.Dimension); // 武60 是四能力维最高
    }

    [Fact]
    public void GetGlimpse_ReputeIsStableAcrossCalls()
    {
        // 同一 NPC 多次取 glimpse，风评必须一致(id 种子固定)，否则玩家可刷新拼属性
        var npc = new NpcState { Id = "cao_cao", Martial = 72, Leadership = 90, Politics = 85, Charisma = 80 };
        var g1 = TraitDeriver.GetGlimpse(npc);
        var g2 = TraitDeriver.GetGlimpse(npc);
        var g3 = TraitDeriver.GetGlimpse(npc);
        Assert.Equal(g1.Repute, g2.Repute);
        Assert.Equal(g2.Repute, g3.Repute);
        Assert.False(string.IsNullOrEmpty(g1.Repute));
    }

    [Fact]
    public void BuildIntelAssessment_HighAccuracy_ReportsTruth()
    {
        // 情报头目能力满(accuracy=100) → 评价完全贴合真实档位，零失真
        var npc = new NpcState { Id = "x", Martial = 95, Leadership = 30, Politics = 60, Charisma = 80, Integrity = 20 };
        var rng = new Random(1);
        var assess = TraitDeriver.BuildIntelAssessment(npc, accuracy: 100, rng);
        Assert.Equal("万人之敌", assess[TraitDimension.Martial].Word);   // 武95红
        Assert.Equal("贪得无厌", assess[TraitDimension.Integrity].Word); // 廉20灰
    }

    [Fact]
    public void BuildIntelAssessment_LowAccuracy_CanMislead()
    {
        // 情报头目无能(accuracy=0) → 必失真。用中档(60紫)NPC,偏移易跨档。
        // 真实全为紫档(60)，失真后应有维度偏离紫档。多种子取证(只要存在失真种子即可)。
        var npc = new NpcState { Id = "y", Martial = 60, Leadership = 60, Politics = 60, Charisma = 60, Integrity = 60 };
        bool everMisleading = false;
        for (int s = 0; s < 20 && !everMisleading; s++)
        {
            var assess = TraitDeriver.BuildIntelAssessment(npc, accuracy: 0, new Random(s));
            foreach (var kv in assess)
            {
                if (kv.Value.Tier != TraitTier.Purple) { everMisleading = true; break; }
            }
        }
        Assert.True(everMisleading, "accuracy=0 时情报应当产生误导(偏离真实档位)");
    }
}
