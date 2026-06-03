# 2026-06-03 东汉末年汉灵帝：NPC 惰性加载与按需部署登台系统设计规范

## 1. 核心目标
为了减少游戏初期内存占用，并营造出东方历史中“群雄按历史时机逐一登台、被天子登庸召见”的宏大叙事沉浸感。
本系统将 NPC 体系在物理上隔离为 **在朝活跃池 (On-Stage Court)** 与 **在野备用池 (Off-Stage Preset)**，并在需要时通过 **AI 调度师或历史旬更按需部署 (Lazy Deployment)** 上台。

---

## 2. 隔离架构设计

### 2.1 物理双池隔离 (Dual Pool Isolation)

```
  [本地 donghan_preset_npcs.json] / [B轨静态冷备列表]  (在野英贤池 - 惰性待命)
                       │
                       │  (由 AI 调度师/历史剧情按需触发 DeployNpcToCourt)
                       ▼
            [GameState.Npcs 字典]                   (在朝活跃池 - 内存轻量)
```

1. **在朝活跃池 (`GameState.Npcs`)**：
   * 仅存储当前活跃在宣政殿、西园或后宫的官员，数据常驻内存，可供发饷、赈灾、抄家。
   * 开局仅有 4 人（何进、张让、曹操、蹇硕）。
2. **在野备用池 (JSON 文件/冷备静态类)**：
   * 包含未登场的数百个武将文臣。不常驻于 GameState，只有被征召部署时，才会被反序列化为 NpcState 并推入 `GameState.Npcs`。

---

## 3. 生命周期管理器接口扩展 (`INpcLifecycleManager`)

在 `INpcLifecycleManager` 接口中，增加支持 AI 调度师主动调用的**按需部署**方法：

```csharp
public interface INpcLifecycleManager
{
    // 根据 NPC 唯一 ID（如 "dong_zhuo"），从预置池中搜索并部署（登庸）到朝局 Npcs 字典中上场
    void DeployNpcToCourt(string npcId, GameState state);

    Task ProcessLifecycleStepAsync(GameState state, bool useHighTokenAi);
    List<NpcState> GetPresetNpcsFallback();
}
```

### 3.1 部署实现逻辑 (`DeployNpcToCourt`)
当部署方法被调用时：
1. 检查 `state.Npcs` 中是否已经存在该 ID 的官员。如果已在朝，则直接忽略。
2. 从本地 JSON (A轨) 或 硬编码冷备 (B轨) 中匹配找到该 `npcId` 的静态原始状态模板。
3. 如果匹配成功，为其实例化动态数据（健康度 100，好感 50，权势 15），并调用 `INpcRegistry.RegisterNpc` 将其激活并注册到 `state.Npcs` 中上场。
4. 历史实录（Chronicle）动态添加：*“【部署/登台】并州刺史【董卓】按大势征召，正式步入宣政殿，进入朝堂局势。”*。

---

## 4. 交互流时序 (Sequence of Deployment)

1. **历史旬更/AI 意图触发** -> 
   * 当年份到达中平元年，或者玩家输入诏书：*“召董卓进京”*。
2. **AI 调度器在 `OrchestrateGrandCourtAsync` 中拦截意图** ->
   * 调度器在后台直接调用：`NpcManager.DeployNpcToCourt("dong_zhuo", state)`。
3. **NPC 状态升级，完成上场** ->
   * 董卓成功实例化并推入 `state.Npcs` 字典。
   * 接下来，董卓的数据便可以参与大朝会的同步/异步群辩（`CourtSpeech`）、或者在大朝会上打头阵上奏，完成了“按需上场”的完美闭环。

---

## 5. 自我审查 (Self-Review)
* **防内存泄露**：惰性加载避免了初期加载成百上千个名臣带来的 GameState 传输和解析负担。
* **数据高可用**：通过 A轨与B轨 结合，确保只要调用 `DeployNpcToCourt("liu_bei", state)` 甚至错误的 Id，都有高容错硬冷备的 fallback。
