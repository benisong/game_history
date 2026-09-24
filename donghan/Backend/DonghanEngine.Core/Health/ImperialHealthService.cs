using System;
using DonghanEngine.Core;

namespace DonghanEngine.Core.Health;

/// <summary>
/// 纯领域服务：天子隐藏精气神、月度比例结算、临幸阳气换精力与寿命损耗引擎（单一职责）
/// </summary>
public sealed class ImperialHealthService : IImperialHealthService
{
    // 规则 3：阳气每一旬自动恢复 1-2 点 (默认 2 点基准，可配置)
    public const int DefaultYangRecoveryPerXun = 2;

    // 规则 3：临幸后宫按照一点阳气换 3-5 点精力 (默认 4 点基准，可配置)
    public const int DefaultEnergyPerYangPoint = 4;

    public ImperialHealthDiagnosisReport AdvanceXunHealthSettlement(GameState state)
    {
        // 1. 阳气随着时间缓慢恢复（每一旬自然恢复 1-2 点，上限 100）
        int yangRecover = DefaultYangRecoveryPerXun;
        if (state.ChiefPhysicianId != null)
        {
            yangRecover = (int)Math.Round(yangRecover * 1.15); // 名医在朝增益 15%
        }
        state.HiddenYangVitality = Math.Clamp(state.HiddenYangVitality + yangRecover, 0, 100);

        return GetPhysicianDiagnosis(state);
    }

    public ImperialHealthDiagnosisReport AdvanceMonthlyEnergySettlement(GameState state)
    {
        // 规则 1：精力按月进行结算恢复
        // 规则 2：每次节点恢复精力的时候，恢复的是当前剩余精力的 40%
        // 规则 4：阳气低于 50 恢复效率变成一半即 20%，低于 20 回复率变成 10%
        double recoveryRate = 0.40;
        if (state.HiddenYangVitality < 20)
        {
            recoveryRate = 0.10;
        }
        else if (state.HiddenYangVitality < 50)
        {
            recoveryRate = 0.20;
        }

        // 名医（华佗/张仲景）辅助恢复提升 15%
        if (state.ChiefPhysicianId == "hua_tuo" || state.ChiefPhysicianId == "zhang_zhongjing")
        {
            recoveryRate *= 1.15;
        }

        int energyRecovered = (int)Math.Round(state.HiddenCurrentEnergy * recoveryRate);
        energyRecovered = Math.Max(1, energyRecovered); // 只要精力未归零，至少恢复 1 点

        state.HiddenCurrentEnergy = Math.Clamp(state.HiddenCurrentEnergy + energyRecovered, 0, state.HiddenMaxEnergy);

        // 规则 3：结算时当前精力低于 40 则永久减少一点最大值，低于 20 永久减少 2 点
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
            state.AddToChronicle($"【月末月候虚耗】天子月内案牍操劳、精元亏蚀，龙体根本受损（精力上限永久损耗{maxEnergyLoss}点）！");
        }
        else
        {
            state.ConsecutiveLowEnergyXun = 0;
            state.AddToChronicle($"【月末月候调元】经月轮转，天子按息调理气血，精神渐复。");
        }

        return GetPhysicianDiagnosis(state);
    }

    public HealthActionResolutionResult IndulgeInHarem(GameState state, int yangToSpend = 4)
    {
        // 规则 3：临幸后宫按照一点阳气换 3-5 点精力
        int actualYangSpent = Math.Min(yangToSpend, state.HiddenYangVitality);
        if (actualYangSpent <= 0)
        {
            return new HealthActionResolutionResult(
                Success: false,
                ActionName: "临幸后宫",
                EnergyDelta: 0,
                YangDelta: 0,
                NewMentalState: GetPhysicianDiagnosis(state).MentalState,
                NarrativeTitle: "【阳气衰竭 · 难近声色】",
                ChronicleText: "天子真阳耗竭、四肢畏寒，已无力临幸后宫，切须清心养元！");
        }

        state.HiddenYangVitality = Math.Max(0, state.HiddenYangVitality - actualYangSpent);

        // 兑换比例：1 点阳气兑换 3~5 点精力 (默认 4)
        int rate = DefaultEnergyPerYangPoint;
        if (state.HiddenYangVitality < 20)
        {
            rate = 3; // 阳气极度枯竭时转化折损
        }
        else if (state.HiddenYangVitality >= 60)
        {
            rate = 5; // 阳气充盈时转化效率高
        }

        int energyGained = actualYangSpent * rate;
        int oldEnergy = state.HiddenCurrentEnergy;
        state.HiddenCurrentEnergy = Math.Clamp(state.HiddenCurrentEnergy + energyGained, 0, state.HiddenMaxEnergy);
        int realGained = state.HiddenCurrentEnergy - oldEnergy;

        var diag = GetPhysicianDiagnosis(state);
        string warning = state.HiddenYangVitality < 50 ? "（太医令密奏：真阳已跌破半数，畏寒面青，月末恢复效率将折半！）" : "";
        string chronicle = $"【临幸后宫】天子驻跸后宫温德欢宴。阳气折损{actualYangSpent}点，精神提振{warning}。";
        state.AddToChronicle(chronicle);

        return new HealthActionResolutionResult(
            Success: true,
            ActionName: "临幸后宫",
            EnergyDelta: realGained,
            YangDelta: -actualYangSpent,
            NewMentalState: diag.MentalState,
            NarrativeTitle: "【后宫临幸 · 借阳化精】",
            ChronicleText: chronicle);
    }

    public HealthActionResolutionResult RestAtWendePalace(GameState state)
    {
        // 温德殿静养：本旬息政，额外提前获得一次当月剩余精力 40% 的休整恢复
        double recoveryRate = 0.40;
        if (state.HiddenYangVitality < 20)
        {
            recoveryRate = 0.10;
        }
        else if (state.HiddenYangVitality < 50)
        {
            recoveryRate = 0.20;
        }

        if (state.ChiefPhysicianId == "hua_tuo" || state.ChiefPhysicianId == "zhang_zhongjing")
        {
            recoveryRate *= 1.15;
        }

        int energyToAdd = (int)Math.Round(state.HiddenCurrentEnergy * recoveryRate);
        energyToAdd = Math.Max(1, energyToAdd);

        int oldEnergy = state.HiddenCurrentEnergy;
        state.HiddenCurrentEnergy = Math.Clamp(state.HiddenCurrentEnergy + energyToAdd, 0, state.HiddenMaxEnergy);
        int actualGained = state.HiddenCurrentEnergy - oldEnergy;

        var diag = GetPhysicianDiagnosis(state);
        string title = "【温德殿静养】天子息政调元，神闲气定";
        string chronicle = $"【静养】天子罢朝一日，于温德殿焚香调神。气血渐复，神思稍适。";
        state.AddToChronicle(chronicle);

        return new HealthActionResolutionResult(
            Success: true,
            ActionName: "温德殿静养",
            EnergyDelta: actualGained,
            YangDelta: 0,
            NewMentalState: diag.MentalState,
            NarrativeTitle: title,
            ChronicleText: chronicle);
    }

    public void ConsumeEnergyForAffairs(GameState state, int cost, string affairName)
    {
        // 政务消耗：名医在朝减免 15% 损耗
        int actualCost = cost;
        if (state.ChiefPhysicianId == "zhang_zhongjing")
        {
            actualCost = (int)Math.Max(1, cost * 0.85);
        }

        state.HiddenCurrentEnergy = Math.Max(0, state.HiddenCurrentEnergy - actualCost);
    }

    public ImperialHealthDiagnosisReport GetPhysicianDiagnosis(GameState state)
    {
        // 规则 5：精力最大值低于 80，开始生病，每少 10 点生命力少 10 年寿数（以 70 岁大寿为基准）
        bool isDiseased = state.HiddenMaxEnergy < 80;
        int yearsLost = 0;
        if (isDiseased)
        {
            int gap = 80 - state.HiddenMaxEnergy;
            yearsLost = (int)Math.Ceiling(gap / 10.0) * 10;
        }

        // 规则 2：精力不显示，以古典精神状态来暗示
        ImperialMentalState mentalState;
        string stateDesc;
        string pulse;
        string advice;

        if (state.HiddenCurrentEnergy <= 10 || state.HiddenMaxEnergy <= 40)
        {
            mentalState = ImperialMentalState.CriticalCollapse;
            stateDesc = "💀 气若游丝 · 卧榻难起";
            pulse = "脉微欲绝，沉细无根，真阳欲脱！";
            advice = "太医院急奏：陛下龙体危笃，已动摇天年，切不可再临外朝半步！";
        }
        else if (state.HiddenCurrentEnergy < 35)
        {
            mentalState = ImperialMentalState.DeeplyExhausted;
            stateDesc = "🟠 虚耗神伤 · 亏蚀根本";
            pulse = "脉象浮大无力，弦紧兼数，气血两亏。";
            advice = "太医院急奏：精亏血耗，已逼近沉疴之坎，月末结算恐永久损耗寿命元气！";
        }
        else if (state.HiddenYangVitality < 50)
        {
            mentalState = ImperialMentalState.YangDeficient;
            stateDesc = "🟣 虚阳浮越 · 畏寒神怠";
            pulse = "尺脉沉迟，命门火衰，畏寒神怠。";
            advice = "太医令密奏：阳气大亏，月末精力恢复效率折半，切忌再近声色！";
        }
        else if (state.HiddenCurrentEnergy < 50)
        {
            mentalState = ImperialMentalState.SlightlyFatigued;
            stateDesc = "🟡 神思稍倦 · 案牍微劳";
            pulse = "寸关见涩，神思微浮。";
            advice = "太医令奏：案牍劳形，略有倦意，稍事静养即可平复。";
        }
        else if (state.HiddenCurrentEnergy >= 80 && state.HiddenYangVitality >= 60)
        {
            mentalState = ImperialMentalState.Radiant;
            stateDesc = "🔴 龙精虎猛 · 神采奕奕";
            pulse = "六脉调匀，沉潜有力，龙行虎步。";
            advice = "太医院贺：圣躬康泰，神充气足，国运隆昌！";
        }
        else
        {
            mentalState = ImperialMentalState.ClearAndCalm;
            stateDesc = "🟢 神闲气定 · 如常视事";
            pulse = "脉息平和，从容如常。";
            advice = "太医令奏：起居调摄得宜，如常视事无虞。";
        }

        string summary = isDiseased
            ? $"【病骨染疾】精神状态：{stateDesc}（寿数折损{yearsLost}年）。{advice}"
            : $"【圣躬安和】精神状态：{stateDesc}。{advice}";

        return new ImperialHealthDiagnosisReport(
            MentalState: mentalState,
            MentalStateDescription: stateDesc,
            PulseDescription: pulse,
            PhysicianAdvice: advice,
            IsAfflictedWithDisease: isDiseased,
            YearsOfLifeLost: yearsLost,
            NarrativeSummary: summary);
    }
}
