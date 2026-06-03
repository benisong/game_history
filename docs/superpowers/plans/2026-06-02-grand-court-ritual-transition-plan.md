# 大朝仪朝会转场情境步进系统实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 实现在玩家点击朝会时，展示三段式大朝会筹备仪式过渡（起驾换装 -> 百官趋步 -> 静鞭迎驾），从而在氛围上烘托威严，并完美分担和隐藏后台 AI 生成群辩的通信延迟。

**Architecture:** C# 后端 GameEngine 同步提供 `RitualStageInfo` 数组，前端 Godot 可通过调用获取这套过渡剧本，以遮罩文字渐显形式引导玩家步进点击，并在后台运行 `TriggerCourtDebateAsync`。

**Tech Stack:** C# (.NET 8.0), xUnit

---

## 拟创建/修改的文件映射 (File Structure)

- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameState.cs` (修改)：增加 `RitualStageInfo` 数据模型类定义。
- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameEngine.cs` (修改)：添加同步获取三阶段朝仪剧本的 C# 方法 `GetGrandCourtRitualStages()`。
- `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Tests\EngineTests.cs` (修改/追加)：编写测试断言，验证 `GetGrandCourtRitualStages` 输出的准确性，确保没有任何逻辑 Regression。

---

## 具体实施任务 (Bite-Sized Tasks)

### Task 1: 升级 `GameState.cs` 追加朝仪过渡剧本实体类

**Files:**
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameState.cs`

- [ ] **Step 1: 在文件尾部追加 `RitualStageInfo` 类的定义**

```csharp
public class RitualStageInfo
{
    public int StageIndex { get; set; } // 1, 2, 3
    public string Title { get; set; } = string.Empty;
    public string Narrative { get; set; } = string.Empty;
}
```

---

### Task 2: 升级 `GameEngine.cs` 实现朝仪剧本同步接口

**Files:**
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Core\GameEngine.cs`

- [ ] **Step 1: 在 `GameEngine` 类中实现 `GetGrandCourtRitualStages` 方法**

在类中合适位置添加如下方法：
```csharp
    // 同步获取大朝会开幕前三阶段大朝仪情境文案，用作转场遮罩展示
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

### Task 3: 升级 `EngineTests.cs` 单元测试与验证全编译

**Files:**
- Modify: `C:\Users\beni3\opencode\donghan\Backend\DonghanEngine.Tests\EngineTests.cs`

- [ ] **Step 1: 在测试中追加大朝仪剧本获取的验证**

在 `EngineTests` 类中添加 `Test_GrandCourt_RitualStages_Retrieval` 单元测试：
```csharp
    [Fact]
    public void Test_GrandCourt_RitualStages_Retrieval()
    {
        // Arrange
        var state = new GameState();
        var engine = new GameEngine(state, new MockScheduler(), new MockOracle(), new MockMinisterAgent(), new MockNarrator());

        // Act
        var stages = engine.GetGrandCourtRitualStages();

        // Assert
        Assert.Equal(3, stages.Count);
        Assert.Equal(1, stages[0].StageIndex);
        Assert.Contains("起驾换装", stages[0].Title);
        Assert.Contains("百官趋步", stages[1].Title);
        Assert.Contains("静鞭鸣磬", stages[2].Title);
    }
```

- [ ] **Step 2: 在当前终端运行 `dotnet test` 确保 12/12 测试项 100% 绿色通过**

Run: `dotnet test`
Expected: 12 tests passed successfully.
