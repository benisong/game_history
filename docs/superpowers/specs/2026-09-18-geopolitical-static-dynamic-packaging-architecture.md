# 《东汉末年汉灵帝》地缘政治引擎分包架构与静态/动态双轨规范

**文档版本**：v1.0  
**核心原则**：分包隔离（Package Separation） / 静态独立（Zero-AI Static Foundation） / 接口可插拔（ISP/DIP） / 严格面向对象（OOP）

---

## 一、 核心架构原则：静态底座与动态增强双轨隔离

地缘推演与朝堂博弈严格遵循**双轨隔离原则**：
1. **静态双轨（Static Core Track - 核心底座）**：
   - 100% 本地离线运行，零网络与 LLM 依赖。
   - 所有地缘决策、战役攻防、诸侯扩张、臣子发言均由**多因子加权概率矩阵（Weighted Multi-Factor Probability）与规则启发式（Heuristics）**驱动。
   - 毫秒级运算，通过 `IRandomProvider` 注入实现 100% 确定性自动化单测。
2. **动态双轨（Dynamic AI Track - 可选增强）**：
   - 仅作为表现层/文学润色/深层对话增强插件。
   - 动态通道完全独立于纯静态引擎，**严禁动态 AI 逻辑侵入静态领域的运算类与领域实体**。
   - 当没有配置 API Key 或断网时，静态引擎无缝独立运行，游戏核心玩法 100% 完整可用。

---

## 二、 命名空间与工程分包目录规范 (DDD Packaging)

所有的计算类、工具类、策略类与实体严格分包归类：

```
Backend/DonghanEngine.Core/
├── Geopolitics/                           # 【地缘政治子领域根目录】
│   ├── Models/                            # 1. 地缘充血实体包 (Entities)
│   │   ├── WarlordFaction.cs              #    诸侯势力充血实体
│   │   ├── MilitaryCampaign.cs            #    在途战役充血实体
│   │   ├── FactionRelationGraph.cs        #    诸侯多边关系矩阵
│   │   └── GeopoliticalEnums.cs           #    阵营倾向/战役状态等枚举
│   │
│   ├── Contracts/                         # 2. 细粒度接口包 (ISP Interfaces)
│   │   ├── IRandomProvider.cs             #    随机数生成器抽象接口
│   │   ├── IWarlordDecisionEvaluator.cs   #    诸侯决策评估单方法接口
│   │   ├── ICampaignStepResolver.cs       #    战役步进与战损结算接口
│   │   ├── IGeopoliticalMemorialFactory.cs#    战后奏折生成工厂接口
│   │   └── IImperialEdictExecutor.cs      #    天子诏令执行器接口
│   │
│   ├── MathEngine/                        # 3. 静态数学与加权概率计算包 (Static Pure Math)
│   │   ├── DefaultRandomProvider.cs       #    生产环境随机数提供者
│   │   ├── SeededRandomProvider.cs        #    测试环境可控种子随机数提供者
│   │   ├── CombatCombatPowerCalculator.cs #    战力多维数学计算工具类
│   │   ├── AttritionCalculator.cs         #    战损与后勤消耗数学工具类
│   │   └── WarlordAggressionFormula.cs    #    诸侯进攻概率多因子公式计算类
│   │
│   ├── Strategies/                        # 4. 静态诸侯 AI 策略包 (Static AI / Heuristics)
│   │   ├── AmbitiousWarlordStrategy.cs    #    称霸枭雄决策策略 (曹操/袁绍/孙策)
│   │   ├── LoyalistBannerStrategy.cs      #    忠贞藩屏决策策略 (皇甫嵩/卢植/刘备)
│   │   └── CautiousAutonomistStrategy.cs  #    自守宗室决策策略 (刘表/刘璋/陶谦)
│   │
│   ├── DynamicAI/                         # 5. 动态大模型接入包 (Dynamic LLM - 独立隔离)
│   │   ├── ILlmGeopoliticalNarrator.cs    #    大模型战报文学润色接口
│   │   ├── LlmMemorialEnricher.cs         #    大模型奏折个性化言论丰富器
│   │   └── OpenAiGeopoliticalAdapter.cs   #    OpenAI / DeepSeek 异步中转适配器
│   │
│   └── Memorials/                         # 6. 地缘奏折与天子诏令包 (Court Handlers)
│       ├── GeopoliticalMemorialFactory.cs #    战后既成事实与讨封奏折组装器
│       └── ImperialEdictExecutor.cs       #    天子密诏/封赏/调停执行器
```

---

## 三、 静态工具类与运算类的面向对象职责划分

### 1. 静态纯运算工具类 (`MathEngine/`)
- **`CombatPowerCalculator`**：输入军队规模、武将统帅、武力、文学 Traits 与城防等级，输出标准化的综合战斗力数值。
- **`WarlordAggressionFormula`**：纯数学公式计算进攻概率 $P(\text{Attack})$，输入进攻方野心、双方兵力比、天子皇权威慑力，输出 `[0, 95]` 的概率值。
- **职责约束**：**无状态（Stateless）、纯函数式（Pure Function）、零外部副作用**，便于进行高并发极速单测。

### 2. 静态决策策略类 (`Strategies/`)
- 实现 `IWarlordDecisionEvaluator` 接口。
- 仅依赖 `MathEngine` 中的纯数学计算工具与 `IRandomProvider`。
- 绝不调用任何网络或大模型组件。

### 3. 动态 AI 适配类 (`DynamicAI/`)
- 仅作为静态推演结果产出后的**“叙事外包装（Narrative Decorator）”**。
- 接收静态引擎计算好的 `CampaignStepResult` 或 `CourtMemorial`，调用 LLM 生成符合该大臣性格与文风的起居注文案；
- 若网络超时或抛出异常，自动 fallback 降级为静态模板字符串，保障游戏永不中断。

---

## 四、 架构总结

1. **运算归运算，策略归策略，实体归实体，AI 归 AI**；
2. 静态游戏是 100% 独立完整的自洽闭环，动态 AI 仅为可插拔的文学表现层；
3. 严格分包杜绝代码相互杂糅，为后续的大规模功能迭代打下坚不可摧的架构底座。
