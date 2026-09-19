# 《东汉末年汉灵帝》地缘政治仿真引擎（Geopolitical Engine）架构设计与面向对象规范

**文档版本**：v1.0  
**架构标准**：DDD 领域驱动设计 / SOLID 五大设计原则 / 严格面向对象（OOP） / 接口隔离（ISP）  
**工程目标**：支撑汉灵帝延寿推演期间（184年—226年）天下诸侯“自主攻伐、野心扩张、战后讨封、忠臣听调、朝廷挑拨与暗中制衡”的宏观地缘沙盘。

---

## 一、 核心架构分层与双层驱动模型

地缘政治仿真引擎与内廷微观引擎解耦并存，形成**“宏观地缘沙盘 + 微观帝王御批”双层驱动架构**：

```
┌────────────────────────────────────────────────────────────────────────┐
│                   【宏观地缘推演层 (Geopolitical Engine)】               │
│                                                                        │
│   1. 诸侯 AI 决策策略集 (IWarlordDecisionEvaluator - Strategy Pattern)  │
│      • 称霸枭雄策略 (AmbitiousWarlordStrategy)：兼并邻郡、战后讨封     │
│      • 忠贞藩屏策略 (LoyalistBannerStrategy)：听调平叛、恪守纳贡       │
│      • 自守宗室策略 (CautiousAutonomistStrategy)：保境安民、遇袭求援   │
│                                                                        │
│   2. 战役步进与结算管道 (ICampaignStepResolver)                        │
│      • 基于兵力、将领四维、文学 Traits、地形的纯数学攻防推演           │
│      • 产出：领地割让/兼并、战损消耗、战俘与流亡                       │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │ 产生表奏与地缘事件
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│                   【微观帝王决断层 (Imperial Court Engine)】             │
│                                                                        │
│   1. 尚书台地缘奏折 (IGeopoliticalMemorialFactory)                     │
│      • 【讨封折】：战胜强藩请求加封刺史/州牧/将军号                    │
│      • 【求救血书】：忠良/弱小宗室孤城被围请求朝廷调解/救兵            │
│      • 【暗流密报】：密探呈报地方诸侯军事调动情报                     │
│                                                                        │
│   2. 帝王心术四大杠杆 (IImperialEdictExecutor)                          │
│      • 【密诏背刺】：挑动第三者趁虚而入（驱虎吞狼）                     │
│      • 【官爵追认】：准奏受封并索取巨额谢恩钱（利益交换）               │
│      • 【敕旨罢兵】：遣使持节调停罢兵（居中收贡）                       │
│      • 【奉旨征调】：诏令忠义诸侯出兵平定边患（忠良讨逆）               │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 二、 领域充血模型（Rich Domain Model）

### 1. 诸侯势力实体 (`WarlordFaction`)
- **受管属性**：`FactionId`, `LeaderNpcId`, `Stance`, `TotalTroops`, `Provisions`, `ImperialLoyalty`, `ExpansionDesire`。
- **只读集合暴露**：`IReadOnlyCollection<string> ControlledProvinces`。
- **充血业务方法**：`AnnexProvince()`, `CedeProvince()`, `ConsumeProvisions()`, `MobilizeTroops()`, `AdjustLoyalty()`, `AdjustAmbition()`。

### 2. 在途战役实体 (`MilitaryCampaign`)
- **受管属性**：`CampaignId`, `AttackerFactionId`, `DefenderFactionId`, `TargetProvinceId`, `CommittedTroops`, `TurnsRemaining`, `Status`。
- **充血业务方法**：`AdvanceTurn()`, `ApplyAttrition()`, `Resolve()`。

---

## 三、 单一职责服务接口（ISP 契约体系）

1. **`IWarlordDecisionEvaluator`**：输入单个诸侯势力与地缘上下文，输出当旬决策行动（出征/纳贡/屯田/求救）。
2. **`ICampaignStepResolver`**：输入单场战役与双方实体，单步推进攻防伤亡计算。
3. **`IGeopoliticalMemorialFactory`**：根据战役结果与地缘变动，组装发往朝廷尚书台的奏折。
4. **`IImperialEdictExecutor`**：接收天子御批指令，反向作用于地缘实体的领地、忠诚度与金钱。

---

## 四、 架构演进与交付路径

1. **Phase 1（地缘数据契约与充血实体）**：落定 `DonghanEngine.Core.Geopolitics` 命名空间与实体。
2. **Phase 2（AI 策略与战役求解器）**：实现三套诸侯行为策略与战役数学结算。
3. **Phase 3（尚书台奏折工厂与天子诏令执行器）**：打通宏观地缘与微观朝堂的御批双向通信。
4. **Phase 4（自动化测试与全景验证）**：构建 100% 绿色单测套件，验证多诸侯自主兼并与天子挑拨闭环。
