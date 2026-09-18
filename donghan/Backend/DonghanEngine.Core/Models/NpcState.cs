using System;
using System.Collections.Generic;

namespace DonghanEngine.Core;

public class NpcState
{
    private int _favorability = 50;
    private int _power = 15;
    private int _corruption = 20;
    private int _health = 100;
    private int _martial = 40;
    private int _leadership = 40;
    private int _politics = 40;
    private int _charisma = 40;
    private int _ambition = 40;
    private int _stashedWealth = 50;
    private int _titleTier = 0;

    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;    // 初始官职 (如：大将军、十常侍、议郎)
    
    public int TitleTier
    {
        get => _titleTier;
        set => _titleTier = Math.Clamp(value, 0, 4);
    }
    
    public int BirthYear { get; set; } = 150;            // 出生年份
    
    public int StashedWealth
    {
        get => _stashedWealth;
        set => _stashedWealth = Math.Max(0, value);
    }
    
    public int Favorability
    {
        get => _favorability;
        set => _favorability = Math.Clamp(value, 0, 100);
    }
    
    public int Power
    {
        get => _power;
        set => _power = Math.Clamp(value, 0, 100);
    }
    
    public int Corruption
    {
        get => _corruption;
        set => _corruption = Math.Clamp(value, 0, 100);
    }

    // “藏锋于词” 专属文学词汇特征
    private List<string> _traits = new();
    public IReadOnlyList<string> Traits
    {
        get => _traits;
        init => _traits = value != null ? new List<string>(value) : new List<string>();
    }

    public void AddTrait(string trait)
    {
        if (!string.IsNullOrWhiteSpace(trait) && !_traits.Contains(trait))
            _traits.Add(trait);
    }

    public void RemoveTrait(string trait) => _traits.Remove(trait);
    public void ClearTraits() => _traits.Clear();

    // === 充血业务行为方法 ===
    public void AdjustFavorability(int delta) => Favorability += delta;
    public void AdjustPower(int delta) => Power += delta;
    public void AdjustCorruption(int delta) => Corruption += delta;
    public void AdjustHealth(int delta) => Health += delta;
    public void AdjustStashedWealth(int delta) => StashedWealth = Math.Max(0, StashedWealth + delta);
    public void AssignGovernor(string provinceId) => GovernedProvinceId = provinceId;
    public void RevokeGovernor() => GovernedProvinceId = null;
    public void MarkDeceased(string reason)
    {
        IsActive = false;
        DeathReason = reason;
        GovernedProvinceId = null;
    }

    public string Personality { get; set; } = "中庸";     // 性格简述 (如：阴险、刚直、谄媚)
    public string Style { get; set; } = "明哲保身";       // 处事风格 (如：结党营私、雷厉风行、拥兵自重)
    public string Faction { get; set; } = FactionCatalog.PureStream;       // 派系归属 (清流派/外戚派/阉党派/西园亲军/割据军阀/反叛势力)

    // 生存与生命周期控制
    public int Health
    {
        get => _health;
        set => _health = Math.Clamp(value, 0, 100);
    }
    
    public int BaseLongevity { get; set; } = 65;         // 期望寿命上限
    public bool IsActive { get; set; } = true;            // 是否活跃于朝堂
    public string DeathReason { get; set; } = string.Empty; // 死亡/退场因由

    // === 五维基本属性 (0-100) ===
    public int Martial
    {
        get => _martial;
        set => _martial = Math.Clamp(value, 0, 100);
    }
    
    public int Leadership
    {
        get => _leadership;
        set => _leadership = Math.Clamp(value, 0, 100);
    }
    
    public int Politics
    {
        get => _politics;
        set => _politics = Math.Clamp(value, 0, 100);
    }
    
    public int Charisma
    {
        get => _charisma;
        set => _charisma = Math.Clamp(value, 0, 100);
    }
    
    public int Ambition
    {
        get => _ambition;
        set => _ambition = Math.Clamp(value, 0, 100);
    }

    // === 地方治理 ===
    public string? GovernedProvinceId { get; set; } = null; // 正在治理的郡 ID

    // === 历史预设与登场控制 ===
    public string InitialLocation { get; set; } = "洛阳朝堂";  // 洛阳朝堂/地方州郡/在野/边军/敌对势力
    public string EntryCondition { get; set; } = "开局";       // 开局/事件触发/年月触发/冷备
    public string HistoricalRole { get; set; } = string.Empty;  // 简短史料定位
    public bool IsHostile { get; set; } = false;                // 敌对首领不进入任官/平叛/招安候选
    public int? HistoricalDeathYear { get; set; } = null;       // 史实/传统说法卒年，仅作参考，不强制死亡
    public string SourceNote { get; set; } = string.Empty;      // 生卒年与传记来源说明；生年不详者标明游戏估算
}
