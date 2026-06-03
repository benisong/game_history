# 2026-06-02 东汉末年汉灵帝：大朝仪朝会转场情境步进系统设计规范

## 1. 核心目标
为了体现东方封建帝制下“临朝听政”的崇高威严，并完美平抑、消除 AI 调度员在后台进行多角色编排的网络通信延迟。
本系统在玩家点击【朝会】时，不再瞬间弹出首发大臣折子，而是引入一个 **“大朝仪朝会步进情境过渡遮罩（Grand Court Ritual Step-by-Step Transition Mask）”**。

---

## 2. 仪式步进逻辑设计

### 2.1 仪式情境三步走 (Three Ritual Stages)

在遮罩弹出时，通过文字和插图/动效顺次播放三个朝会筹备情景，带给玩家极致的庙堂换装与朝拜沉浸感：

| 阶段 | 仪式主题 | 史实文案参考 |
| :--- | :--- | :--- |
| **Stage 1** | **御宿换装 (The Dressing)** | *“陛下龙体复苏，在御宿殿换装。尚衣监呈上玄衣纁裳，佩玉大带，头戴十二旒冕冠，环佩锵鸣，天子起驾宣政……”* |
| **Stage 2** | **群臣趋步 (The Marching)** | *“宣政殿重门訇然大开，晨光破晓。殿前黄门长鸣，大将军、十常侍、朝中百官按品秩低头趋步入殿，两列金甲肃立……”* |
| **Stage 3** | **静鞭鸣磬 (The Quietness)** | *“‘圣上驾到！’ 黄门侍郎高啼，铜磬齐鸣，殿前静鞭三响，回音绕梁。满朝文武屏息整肃，合抱笏板，静候陛下驾临龙椅……”* |

---

## 3. 架构与类库实体扩展

### 3.1 实体模型

我们在 `GameState.cs`（或 `GameEngine.cs` 内部）定义朝仪步骤的文本结构：
```csharp
public class RitualStageInfo
{
    public int StageIndex { get; set; } // 1, 2, 3
    public string Title { get; set; } = string.Empty;
    public string Narrative { get; set; } = string.Empty;
}
```

### 3.2 接口与同步通道扩展 (`GameEngine.cs`)

当玩家进入宣政殿开启朝会：
```csharp
    // 同步获取整套大朝仪情境步骤
    public List<RitualStageInfo> GetGrandCourtRitualStages()
    {
        return new List<RitualStageInfo>
        {
            new RitualStageInfo {
                StageIndex = 1,
                Title = "【第一仪：起驾换装】",
                Narrative = "陛下在温德殿后暖阁换装。尚衣监、尚冠局太监躬身呈上玄衣纁裳，佩玉大带，头戴天子十二旒冕冠，环佩锵鸣。龙舆启行，天子仪仗往宣政殿进发……"
            },
            new RitualStageInfo {
                StageIndex = 2,
                Title = "【第二仪：百官趋步】",
                Narrative = "宣政殿外朱漆重门訇然大开，晨光破晓，洒满京洛。殿前黄门侍郎扯开嗓子长啼，大将军、十常侍、朝中百官执笏板，按官阶品秩低头趋步入殿，两列金甲羽林肃立，庄严肃穆。"
            },
            new RitualStageInfo {
                StageIndex = 3,
                Title = "【第三仪：静鞭鸣磬】",
                Narrative = "“圣上驾到！” 黄门侍郎高呼，殿上铜磬齐鸣。殿前御史高唱“肃静”，静鞭三响，回音绕梁。满朝文武屏息整肃，面向御台朱漆龙椅深揖，静候陛下驾临御极。"
            }
        };
    }
```

---

## 4. 交互流控制 (Flow of Interaction)

1. **玩家点击【进入朝会】** ->
2. **触发同步朝会启动 & 异步 AI 请求**：
   * 引擎调用 `StartGrandCourtSync()` 并启动 `TriggerCourtDebateAsync(...)`。
   * 同时前端立刻弹出 **大朝仪过渡遮罩**。
3. **情境步进展示（消除延迟）**：
   * 前端拉取 `GetGrandCourtRitualStages()` 并展示 Stage 1。
   * 玩家点击“下一仪”（或者系统设定 1.5 秒自动计时切换）切换到 Stage 2，再点击切换到 Stage 3。
   * 这 3 次点击/自动停留至少给后台 AI 预留了 **4.5 - 6 秒** 的宽裕加载时间。
4. **大朝门开，政务宣读**：
   * 当 Stage 3 播放完毕（或玩家点击“大开宫门”），过渡遮罩淡出，界面直接无缝展现首发大臣折子与下方已加载完毕的大臣群辩插播队列。

---

## 5. 自我审查 (Self-Review)
* **无死角遮盖**：朝仪遮罩可以采用完全防点击穿透的 Godot Control UI 层。
* **时序容错**：即使玩家手速极快 1 秒点完 3 个 Stage，如果此时后台 `TriggerCourtDebateAsync` 尚未加载完毕（可能性极小），前端遮罩仍会在 Stage 3 界面显示一个精致的小菊花 loading 动画，确保线程安全。
