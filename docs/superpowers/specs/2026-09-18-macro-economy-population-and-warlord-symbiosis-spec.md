# 宏观经济-人口-土地兼并-流寇反哺地缘核心系统规划 (Future Core System Spec)

> **设计背景**：在东汉末年灵帝延寿 70 岁全景推演中，地缘政治与历史大事件不能是孤立的脚本，而应由底层的**生产力瓶颈、士族土地兼并、马尔萨斯人口陷阱与流寇经济学**驱动。

---

## 一、 系统核心哲学与动态闭环模型

### 1. 核心闭环链条
```
[高人口基数 / 生产力瓶颈]
          ↓
[世家大族兼并土地 / 隐匿户口]
          ↓
[自耕农破产 / 粮食人均占有率跌破生存线]
          ↓
[民心暴跌 / 黄巾与流寇(黑山、白波、青州)死灰复燃]
          ↓
[地方官府无力平贼 / 诸侯借“讨贼保境”合法大肆募兵]
          ↓
[战乱爆发 / 人口锐减 / 产生大量无主荒地]
          ↓
[诸侯推行屯田 / 收编流民为部曲(如青州兵) / 诸侯势力实质坐大]
          ↓
[幸存人口人均土地增加 / 暂时恢复平衡 / 酝酿下一轮兼并危机]
```

---

## 二、 核心子系统模块设计

### 1. 土地与生产力承载力模型 (`LandCarryingCapacityEngine`)
- **州郡土地承载上限 ($L_{\max}$)**：各州根据水利、地理设定基础耕地面积与承载人口上限。
- **世家兼并度系数 ($\alpha \in [0.0, 1.0]$)**：
  - 兼并度越高，产粮中归入世家私家坞堡的比例越高，流向朝廷赋税和自耕农口粮的比例越低；
  - 抑制兼并手段：考课清查田亩、推行官屯/民屯、抑兼并法令（会引发世家忠诚度下降与朝堂阻力）。

### 2. 人口-税收-动乱悖论模型 (`MalthusianPopulationParadox`)
- **正向收益**：人口 $N \uparrow \implies$ 赋税劳役与兵源潜力 $\uparrow$。
- **反向风险**：当 $N > L_{\text{carrying}}$ 且遇旱蝗天灾时：
  - 人均粮食低于警戒线 $\implies$ 触发饥荒、易子而食；
  - 民心断崖式下跌，流民自发转职为“黄巾/流寇”实体。

### 3. 流寇反哺地缘政治机制 (`BanditWarlordSymbiosisEngine`)
- **降而复反机制**：地方剿贼若不解决粮食分田问题，流寇“聚则成贼，散则为民，降而复反”。
- **诸侯收编部曲机制**：
  - 地方动乱度达到阈值 $\implies$ 当地军阀（曹操、袁绍、公孙瓒、陶谦等）触发“平贼自保”募兵事件；
  - 军阀收编流寇精壮为特殊精锐兵种（如曹操之“青州兵”、公孙瓒之“白马义从”），诸侯军阀实力直接与流民规模挂钩。

### 4. 破局手段：国家级经济调控政策
- **常平仓机制**：平抑丰荒年粮价，防止荒年自耕农卖田破产。
- **国家级屯田制**：以无主荒地招募流民，由典农中郎将官屯民屯，粮食五五分账，跳过世家直接充实太仓与禁军军粮。
- **水利修缮与度田令**：提高土地承载力上限，严查隐匿人口。

---

## 三、 与现有系统的集成接口定义 (草案)

```csharp
namespace DonghanEngine.Core.Economy;

public interface IAgriculturalCarryingEngine
{
    ProvinceCarryingReport EvaluateProvince(Province province, int weatherSeverity, double aristocracyLandRatio);
}

public interface IBanditRecruitmentEngine
{
    BanditSpilloverResult CalculateSpilloverToWarlords(string provinceId, int unrestLevel, int displacedPopulation);
}
```

---

*状态：已列入重点架构规划清单，待历史大事件谱系校定完成后作为核心底层引擎实装。*
