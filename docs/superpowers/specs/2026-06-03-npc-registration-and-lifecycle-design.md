# 2026-06-03 东汉末年汉灵帝：NPC 登录与双轨制生命周期管理系统设计规范

## 1. 核心目标
为了实现东汉末年朝堂百官与地方势力“新旧更替、生老病死”的动态生态，本系统设计了一套统一的 **NPC 登录（注册）接口** 以及一个 **NPC 生命周期管理 AI 助手（NPC Lifecycle Manager）**。
通过**“藏锋于词”**（用文学词汇代替死板数值）和**“双轨并行”**（多 Token 纯 AI 生成版与本地预置 + 兜底高容错版）机制，实现极具史实感和策略深度的动态官僚沙盒。

---

## 2. 核心实体模型与“藏锋于词”属性设计

### 2.1 统一的 `NpcState` 模型
游戏中的所有登场官员、在野名士统一采用 `NpcState` 结构表达。原有的 `MinisterState` 将重构、合并入全新的 `NpcState`：

```csharp
public class NpcState
{
    // 基本历史档案
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;    // 初始官职 (如：议郎、并州刺史、十常侍)
    public int BirthYear { get; set; } = 150;            // 出生年份
    public int StashedWealth { get; set; } = 50;         // 私蓄赃款 (万钱)
    public int Favorability { get; set; } = 50;          // 对天子好感 (0-100)
    public int Power { get; set; } = 15;                 // 朝堂政治权势 (0-100)
    public int Corruption { get; set; } = 20;            // 贪腐度 (0-100)

    // “藏锋于词” 专属文学词汇特征
    public List<string> Traits { get; set; } = new();    // 核心成语特征 (如：经天纬地、孔武有力、老谋深算、贪得无厌)
    public string Personality { get; set; } = "中庸";     // 性格简述 (如：阴险、刚直、谄媚)
    public string Style { get; set; } = "明哲保身";       // 处事风格 (如：结党营私、雷厉风行、拥兵自重)
    public string Faction { get; set; } = "清流派";       // 派系归属 (清流派/外戚派/阉党派/割据军阀)

    // 生存与生命周期控制
    public int Health { get; set; } = 100;               // 健康值 (0-100)，归 0 则病逝
    public int BaseLongevity { get; set; } = 65;         // 期望寿命上限，到岁数后自然隐退/老死概率暴涨
    public bool IsActive { get; set; } = true;            // 是否活跃于朝堂 (false 代表已退隐、贬庶或死亡)
    public string DeathReason { get; set; } = string.Empty; // 死亡/退出因由叙事 (如：“中平六年病逝于洛阳”、“被天子下诏抄家籍没，流放偏远”)
}
```

### 2.2 成语特征与底层机制映射
在 Backend 核心中，NPC 的 `Traits` 将不再是无用的纯文本，而是会**直接参与大政方针的系数加成**：
*   **「经天纬地」**：作为开仓赈灾钦差时，灾区民心回升效率额外提升 **1.2x**。
*   **「老谋深算」**：作为督办抄家近臣时，遭到党羽政治反噬的概率和威力折减 **30%**。
*   **「孔武有力」**：作为西园发饷将领时，犒赏阅兵对新军士气提升效率额外提升 **1.3x**。
*   **「贪得无厌」**：贪腐度下限强制锁定为 **70%**，督办任何事情时，漂没吃水比例大幅暴涨。

---

## 3. 双轨制生命周期管理器 (Dual-Engine Lifecycle)

生命周期管理器 `INpcLifecycleManager` 运行在每旬/每月更迭的尾部，支持两个不同能耗的版本：

```
                             [旬更流逝 (NextXunAsync)]
                                        │
                                        ▼
                           ┌─────────────────────────┐
                           │ INpcLifecycleManager    │
                           └────────────┬────────────┘
                                        │
                 ┌──────────────────────┴──────────────────────┐
                 ▼ (多 Token 纯 AI 版)                         ▼ (低 Token 本地预置版)
┌─────────────────────────────────────────┐         ┌─────────────────────────────────┐
│ 1. 完全通过 LLM 后台推理。              │         │ 1. 加载本地 JSON。              │
│ 2. 自由生老病死、撰写符合中平大势的故事。│         │ 2. 若 JSON 缺失，C# 硬编码兜底。│
│ 3. 随机发病、暗杀，生成全新的虚构 NPC。 │         │ 3. 简单启发式自然衰老与寿终算法。│
└─────────────────────────────────────────┘         └─────────────────────────────────┘
```

### 3.1 预置 NPC 本地载体与兜底安全设计 (Scheme A + Scheme B Fallback)
*   **A轨：本地 `donghan_preset_npcs.json`**：存放在游戏资源或配置目录下。包含三国真实历史文武官员（董卓、袁绍、袁术、刘备、卢植等）的出生、大势登庸年份和性格成语等。
*   **B轨：嵌入式 C# 硬编码备份列表**：在 `NpcPresetDatabase` 类中硬编码一个静态静态数组。
*   **兜底防崩机制**：
    ```csharp
    public static List<NpcState> LoadPresetNpcs()
    {
        try {
            // 尝试读取本地 C:\...\donghan_preset_npcs.json 并反序列化
        }
        catch {
            // 一旦 JSON 损坏/缺失，瞬间无缝采用内置的 C# 硬编码静态武将列表返回！
            // 100% 保证游戏开局或运转永远不崩溃！
        }
    }
    ```

---

## 4. 接口与核心助手定义

### 4.1 统一的登录（注册）接口 (`INpcRegistry`)
```csharp
public interface INpcRegistry
{
    // 主动登录一个 NPC 进游戏大局 (可以由可视化界面输入，也可以由 AI 导入)
    void RegisterNpc(NpcState npc, GameState state);

    // 罢免、下野或物理消除一个 NPC 
    void DeregisterNpc(string npcId, string reason, GameState state);
}
```

### 4.2 生命周期管理器接口 (`INpcLifecycleManager`)
```csharp
public interface INpcLifecycleManager
{
    // 每旬/每月调用，负责管理 NPC 的衰老、生病、寿终就寝或引入新官员
    Task ProcessLifecycleStepAsync(GameState state, bool useHighTokenAi);
}
```

---

## 5. 自我审查 (Self-Review)
*   **类型安全与重构**：此设计会将原有的 `GameState.Ministers` 的数据类型从 `Dictionary<string, MinisterState>` 重构扩展为统一的 `Dictionary<string, NpcState> Npcs`。
*   **向前兼容**：朝会、发饷、赈灾、抄家等功能通过重构，继续无缝读写新的 `Npcs` 字典。
*   **高低 Token 切换**：利用 `useHighTokenAi` 开关在 runtime 控制是触发昂贵的 LLM API（生成文学情境和完全自由的死法）还是执行本地轻量的 C# 算法。
