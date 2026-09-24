using System;
using DonghanEngine.Core;

namespace DonghanEngine.Core.Health;

/// <summary>
/// 纯领域服务：天子隐藏精气神、非线性恢复、阳气折损与不可逆损耗寿命引擎（单一职责）
/// </summary>
public sealed class ImperialHealthService : IImperialHealthService
{
    public ImperialHealthDiagnosisReport AdvanceXunHealthSettlement(GameState state)
    {
        // 规则 1：阳气随着时间缓慢恢复（每旬自然恢复 +3 点，上限 100）
        // 名医（如董奉/张仲景）在朝额外增益 15%
        int yangRecover = 3;
        if (state.ChiefPhysicianId != null)
        {
            yangRecover = (int)(yangRecover * 1.15);
        }
        state.HiddenYangVitality = Math.Clamp(state.HiddenYangVitality + yangRecover, 0, 100);

        // 规则 3：结算时当前精力低于40则永久减少一点最大值，低于20永久减少2点
        int maxEnergyLoss = 0;
        if (state.HiddenCurrentEnergy < 20)
        {
            maxEnergyLoss = 2;
        }
        else if (state.HiddenCurrentEnergy < 40)
        {
            maxEnergyLoss = 1;
        }

        if (maxEnergyLoss > 0)
        {
            state.HiddenMaxEnergy = Math.Max(10, state.HiddenMaxEnergy - maxEnergyLoss);
            state.HiddenCurrentEnergy = Math.Min(state.HiddenCurrentEnergy, state.HiddenMaxEnergy);
            state.ConsecutiveLowEnergyXun++;
            state.AddToChronicle($"【内省虚耗】天子案牍劳神、精力连续亏蚀，龙体气血根本受损（精力上限永久损耗{maxEnergyLoss}点）！");
        }
        else
        {
            state.ConsecutiveLowEnergyXun = 0;
        }

        // 规则 5：精力最大值低于80，开始生病，每少10点生命力少10年（以70岁基准）
        return GetPhysicianDiagnosis(state);
    }

    public HealthActionResolutionResult RestAtWendePalace(GameState state)
    {
        // 规则 2：每次节点恢复精力的时候，恢复的是当前剩余精力的40%
        // 规则 4：阳气低于50恢复效率变成一半即20%，低于20回复率变成10%
        double recoveryRate = 0.40;
        if (state.HiddenYangVitality < 20)
        {
            recoveryRate = 0.10;
        }
        else if (state.HiddenYangVitality < 50)
        {
            recoveryRate = 0.20;
        }

        // 名医羁绊（华佗等在朝提升 15% 恢复效率）
        if (state.ChiefPhysicianId == "hua_tuo" || state.ChiefPhysicianId == "zhang_zhongjing")
        {
            recoveryRate *= 1.15;
        }

        int energyToAdd = (int)Math.Round(state.HiddenCurrentEnergy * recoveryRate);
        energyToAdd = Math.Max(1, energyToAdd); // 至少恢复 1 点

        int oldEnergy = state.HiddenCurrentEnergy;
        state.HiddenCurrentEnergy = Math.Clamp(state.HiddenCurrentEnergy + energyToAdd, 0, state.HiddenMaxEnergy);
        int actualGained = state.HiddenCurrentEnergy - oldEnergy;

        var report = GetPhysicianDiagnosis(state);
        string title = "【温德殿静养】天子息政调元，脉象回和";
        string chronicle = $"【静养】天子罢朝一日，于温德殿焚香调神。气血渐复，神思稍适。";
        state.AddToChronicle(chronicle);

        return new HealthActionResolutionResult(
            Success: true,
            ActionName: "温德殿静养",
            EnergyDelta: actualGained,
            YangDelta: 0,
            NewAura: report.Aura,
            NarrativeTitle: title,
            ChronicleText: chronicle);
    }

    public HealthActionResolutionResult IndulgeInHarem(GameState state)
    {
        // 后宫游幸：短暂刺激精神，但折损阳气真元 12 点
        int yangDrain = 12;
        state.HiddenYangVitality = Math.Max(0, state.HiddenYangVitality - yangDrain);

        // 刺激回精（基准恢复 15 点，但受阳气亏虚惩罚）
        int boost = 15;
        if (state.HiddenYangVitality < 50)
        {
            boost = (int)(boost * 0.5); // 阳虚时游幸反倒疲惫
        }

        state.HiddenCurrentEnergy = Math.Clamp(state.HiddenCurrentEnergy + boost, 0, state.HiddenMaxEnergy);

        var report = GetPhysicianDiagnosis(state);
        string title = "【后宫游幸】笙歌达旦，损阳耗真";
        string warning = state.HiddenYangVitality < 50 ? "（太医令密奏：阳气已亏，面带青白，恢复效率折半！）" : "";
        string chronicle = $"【后宫】天子巡幸后宫，内廷欢宴。短暂释怀，然阳气亏蚀{yangDrain}点{warning}。";
        state.AddToChronicle(chronicle);

        return new HealthActionResolutionResult(
            Success: true,
            ActionName: "后宫游幸",
            EnergyDelta: boost,
            YangDelta: -yangDrain,
            NewAura: report.Aura,
            NarrativeTitle: title,
            ChronicleText: chronicle);
    }

    public void ConsumeEnergyForAffairs(GameState state, int cost, string affairName)
    {
        // 名医在朝减少 15% 损耗
        int actualCost = cost;
        if (state.ChiefPhysicianId == "zhang_zhongjing")
        {
            actualCost = (int)Math.Max(1, cost * 0.85);
        }

        state.HiddenCurrentEnergy = Math.Max(0, state.HiddenCurrentEnergy - actualCost);
    }

    public ImperialHealthDiagnosisReport GetPhysicianDiagnosis(GameState state)
    {
        // 规则 5：精力最大值低于80，开始生病，每少10点生命力少10年（以70岁延寿基准）
        bool isDiseased = state.HiddenMaxEnergy < 80;
        int yearsLost = 0;
        if (isDiseased)
        {
            // 例如 79-70 少10年，69-60 少20年
            int gap = 80 - state.HiddenMaxEnergy;
            yearsLost = (int)Math.Ceiling(gap / 10.0) * 10;
        }

        // 判定古典气色体征
        ImperialVitalityAura aura;
        string pulse;
        string advice;

        if (state.HiddenCurrentEnergy <= 10 || state.HiddenMaxEnergy <= 40)
        {
            aura = ImperialVitalityAura.BedriddenCritical;
            pulse = "脉微欲绝，沉细无根，真阳欲脱！";
            advice = "太医院严奏：陛下龙体危笃，已动摇元寿，切不可再临外朝半步！";
        }
        else if (state.HiddenCurrentEnergy < 35)
        {
            aura = ImperialVitalityAura.SeverelyExhausted;
            pulse = "脉象浮大无力，弦紧兼数，气血两亏。";
            advice = "太医院急奏：精亏血耗，已逼近沉疴之坎，若再过劳必损根本！";
        }
        else if (state.HiddenYangVitality < 50)
        {
            aura = ImperialVitalityAura.YangDepleted;
            pulse = "尺脉沉迟，命门火衰，畏寒神怠。";
            advice = "太医令奏：阳气大损，进补难受，切须清心寡欲以培真元。";
        }
        else if (state.HiddenCurrentEnergy < 50)
        {
            aura = ImperialVitalityAura.SlightlyWeary;
            pulse = "寸关见涩，神思微浮。";
            advice = "太医令奏：案牍劳形，略有倦意，稍事静养即可平复。";
        }
        else if (state.HiddenCurrentEnergy >= 80 && state.HiddenYangVitality >= 60)
        {
            aura = ImperialVitalityAura.RadiantDragon;
            pulse = "六脉调匀，沉潜有力，龙行虎步。";
            advice = "太医院贺：圣躬康泰，神充气足，国运隆昌！";
        }
        else
        {
            aura = ImperialVitalityAura.StableHarmonious;
            pulse = "脉息平和，从容如常。";
            advice = "太医令奏：起居调摄得宜，如常视事无虞。";
        }

        string summary = isDiseased 
            ? $"【病骨染疾】龙体根本受损（预期寿数折损{yearsLost}年）。气色：{aura}。{advice}"
            : $"【圣躬康宁】气色：{aura}。{advice}";

        return new ImperialHealthDiagnosisReport(
            Aura: aura,
            PulseDescription: pulse,
            ImperialPhysicianAdvice: advice,
            IsAfflictedWithDisease: isDiseased,
            YearsOfLifeLost: yearsLost,
            NarrativeSummary: summary);
    }
}
