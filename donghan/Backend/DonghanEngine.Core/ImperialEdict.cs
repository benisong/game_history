using System;
using System.Collections.Generic;

namespace DonghanEngine.Core;

public enum EdictType
{
    Proposal,      // 建议类 (开仓、修防)
    Remonstrance,  // 劝诫类 (直言犯颜)
    Impeachment,   // 弹劾类 (派系党争)
    Merit,         // 邀功类 (请赏银或官职)
    UrgentCrisis   // 急报类 (天灾、暴动)
}

public class EdictOption
{
    public string Description { get; set; } = string.Empty;
    public string ConsequencePreview { get; set; } = string.Empty;
    
    public int ImperialPowerDelta { get; set; } = 0;
    public int TreasuryDelta { get; set; } = 0;
    public int PrivateTreasuryDelta { get; set; } = 0;
    public int PopularSupportDelta { get; set; } = 0;
    public int HealthDelta { get; set; } = 0; // 追加：对皇帝龙体健康的增损
    public int TargetNpcPowerDelta { get; set; } = 0;
    public int TargetNpcFavorabilityDelta { get; set; } = 0;
    
    public int GrantedTitleTierDelta { get; set; } = 0; 
}

public class ImperialEdict
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public EdictType Type { get; set; }
    public string SubmittingNpcId { get; set; } = string.Empty; 
    public string TargetNpcId { get; set; } = string.Empty; // 受益/受罚的主要对象
    public string NarrativeContent { get; set; } = string.Empty;
    public int ExpiryXun { get; set; } = 3; // 剩余保质期
    public List<EdictOption> Options { get; set; } = new();
}