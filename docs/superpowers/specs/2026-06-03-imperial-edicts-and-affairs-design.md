# 2026-06-03 东汉末年汉灵帝：政务处理系统与五大奏折机制设计规范

## 1. 核心目标
构建【政务处理】（Imperial Edicts and Affairs）子系统的底层数据模型与处理逻辑。通过引入 **五大奏折类别** 和 **阶梯式官秩升迁（Title Tiers）反噬系统**，将单调的数值审批转化为高度政治化、派系化的朝堂博弈。

---

## 2. 五大奏折核心实体设计

### 2.1 奏折类别枚举 (`EdictType`)
```csharp
public enum EdictType
{
    Proposal,      // 建议类 (地方开仓、筑防、开启新法)
    Remonstrance,  // 劝诫类 (直言犯颜，因天子卖官或健康极低触发)
    Impeachment,   // 弹劾类 (派系党争，弹劾对方贪污或造假)
    Merit,         // 邀功类 (办事得力，请赏银两或官职)
    UrgentCrisis   // 急报类 (随机天灾、黄巾暴动，必须紧急决断)
}
```

### 2.2 奏折数据模型 (`ImperialEdict`)
```csharp
public class EdictOption
{
    public string Description { get; set; } = string.Empty;   // 选项文本 (如: "准奏，拔款五百万", "留中不发")
    public string ConsequencePreview { get; set; } = string.Empty; // AI或系统给予的御前建议/风险预警
    
    // 物理结算效果
    public int ImperialPowerDelta { get; set; }
    public int TreasuryDelta { get; set; }
    public int PrivateTreasuryDelta { get; set; }
    public int PopularSupportDelta { get; set; }
    public int TargetNpcPowerDelta { get; set; }
    public int TargetNpcFavorabilityDelta { get; set; }
    
    // 加官进爵专用
    public int GrantedTitleTierDelta { get; set; } = 0; // 赏赐跃升的官阶级数
}

public class ImperialEdict
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public EdictType Type { get; set; }
    public string SubmittingNpcId { get; set; } = string.Empty; // 上奏人 (可能是活跃朝臣，也可能是外地刺史)
    public string NarrativeContent { get; set; } = string.Empty; // 详细文学叙事
    public int ExpiryXun { get; set; } = 3;                     // 保质期：超过 3 旬未批阅将导致流产反噬
    public List<EdictOption> Options { get; set; } = new();      // 绝大多数折子提供 2-3 个对峙选项
}
```

---

## 3. 阶梯式官秩与跨级跃迁预警系统

### 3.1 官秩层级 (`TitleTier`)
我们在 `NpcState` 中追加字段 `public int TitleTier { get; set; }`：
*   **0级 (白身/布衣)**：无权势，待选名士。
*   **1级 (微臣/郎官/校尉)**：负责跑腿执行，权势上限较低。
*   **2级 (重臣/郡守/刺史)**：地方封疆大吏。
*   **3级 (辅政/九卿/卿士)**：中枢高级决策层。
*   **4级 (柱石/三公/大将军)**：权倾朝野，一人之下。

### 3.2 跃迁与跨级反噬逻辑
当天子在 **邀功类 (Merit)** 或特定政务中，选择提升某 NPC 官职时：
1.  **逐级晋升 ($\Delta \text{Tier} == 1$)**：
    合乎礼制。NPC 权势大幅成长（例如 +15），好感度大涨，且获得对应的新称号（由 AI 或本地词典赋名）。
2.  **跨级骤升 ($\Delta \text{Tier} \ge 2$)**：
    *   **御前预警拦截**：选项上会标记高亮警告 *“一次性拔擢过快，恐引朝野非议与党派连奏弹劾！”*
    *   **物理反噬结算**：
        *   NPC 本人权势与好感剧增。
        *   **皇权 (ImperialPower)** 立即遭受非议冲击，扣除 $5 \times (\Delta \text{Tier} - 1)$ 点。
        *   **异步复仇锁定**：由 `IAIScheduler` 在接下来的 1-2 旬内，强制向 `ActiveEdicts` 中塞入一封由敌对派系发起的 **弹劾类 (Impeachment)** 奏折。逼迫玩家付出更多代价去保住这个幸臣，或屈服于百官将其革职。

---

## 4. 奏折的生成与消亡时序

### 4.1 AI 调度师生成 (Edict Generation)
*   **触发时机**：在旬更方法 `NextXunAsync()` 中，`IAIScheduler.OrchestrateXunUpdateAsync` 会根据当前的国库、民心与天子健康度，结合派系斗争大势：
    *   如民心 < 30，生成紧急开仓建议或暴乱急报。
    *   如玩家健康 < 40，生成清流派劝诫天子保重龙体、远离阉党的劝诫折。
    *   如某武将在前两旬完成了剿匪，生成邀功请赏折。

### 4.2 留中不发与过期流产 (Expiry Backlash)
*   如果折子在 `ActiveEdicts` 里存放超过 `ExpiryXun`（默认 3 旬，即 1 个月）且未被处理，折子自动从队列中移除。
*   急报与建议类流产，会导致对应的灾难恶化（民心大跌）；劝诫类流产，进谏者心寒，好感度大幅下降。
