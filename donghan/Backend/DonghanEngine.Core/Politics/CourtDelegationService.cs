using System;
using System.Collections.Generic;
using System.Linq;
using DonghanEngine.Core;
using DonghanEngine.Core.Balance;
using DonghanEngine.Core.Health;

namespace DonghanEngine.Core.Politics;

/// <summary>
/// 纯领域服务：朝堂廷议分权代办、大臣属性扭曲与精力平衡引擎（单一职责）
/// 支持 IInitializableBalance&lt;DelegationConfig&gt; 接口，供超级控制工具动态调参
/// </summary>
public sealed class CourtDelegationService : ICourtDelegationService, IInitializableBalance<IDelegationBalanceProvider>
{
    private IDelegationBalanceProvider _config;
    private readonly IImperialHealthService _healthService;

    public CourtDelegationService(
        IDelegationBalanceProvider? config = null,
        IImperialHealthService? healthService = null)
    {
        _config = config ?? new DelegationConfig();
        _healthService = healthService ?? new ImperialHealthService();
    }

    public void InitializeConfig(IDelegationBalanceProvider config) => UpdateConfig(config);

    public void UpdateConfig(IDelegationBalanceProvider config) => _config = config ?? throw new ArgumentNullException(nameof(config));
    public IDelegationBalanceProvider GetConfig() => _config;

    public IReadOnlyList<DelegationAffairItem> GetPendingAffairs(GameState state)
    {
        // 动态生成当旬/当月待办政务池（亦可根据历史年份与局势扩展）
        return new List<DelegationAffairItem>
        {
            new("affair_irrigation_yuzhou", "修缮豫州河堤水利", "豫州淮泗河渠年久失修，司徒府呈请修葺以增官田承载。", CourtDelegationHubKind.ThreeExcellencies, BaseTreasuryCost: 500, BasePopularSupportDelta: 3, BaseStateLandDelta: 1200, BaseArmyMoraleDelta: 0, BaseDirectEnergyCost: 4),
            new("affair_west_garden_drill", "整肃西园禁军行阵", "大将军府请拨内库修缮校场、操练新募步骑士卒。", CourtDelegationHubKind.GrandGeneral, BaseTreasuryCost: 800, BasePopularSupportDelta: 0, BaseStateLandDelta: 0, BaseArmyMoraleDelta: 8, BaseDirectEnergyCost: 6),
            new("affair_tribute_audit", "清核中原各郡岁贡", "十常侍中常侍府呈请点验关东各郡入洛岁赋贡物入内帑。", CourtDelegationHubKind.PalaceAttendants, BaseTreasuryCost: -600, BasePopularSupportDelta: -2, BaseStateLandDelta: 0, BaseArmyMoraleDelta: 0, BaseDirectEnergyCost: 4),
            new("affair_governor_inspection", "台阁核验各州长吏考课", "尚书台汇总十三州刺史治绩，拟定褒贬黜陟条陈。", CourtDelegationHubKind.Secretariat, BaseTreasuryCost: 200, BasePopularSupportDelta: 2, BaseStateLandDelta: 500, BaseArmyMoraleDelta: 0, BaseDirectEnergyCost: 5)
        }.AsReadOnly();
    }

    public AffairExecutionReport ExecuteDirectAffair(GameState state, string affairId)
    {
        var affair = GetPendingAffairs(state).FirstOrDefault(a => a.AffairId == affairId)
            ?? throw new ArgumentException($"未找到待办政务：{affairId}", nameof(affairId));

        // 天子亲裁：消耗全额精力
        _healthService.ConsumeEnergyForAffairs(state, affair.BaseDirectEnergyCost, $"天子亲裁：{affair.Title}");

        // 亲裁执行结果 100% 遵从天子意志，零贪腐私吞
        ApplyAffairImpact(state, affair.BaseTreasuryCost, affair.BasePopularSupportDelta, affair.BaseStateLandDelta, affair.BaseArmyMoraleDelta);

        string narrative = $"【天子亲裁】天子御笔亲批【{affair.Title}】。乾纲独断，令行禁止，四海承风。";
        state.AddToChronicle(narrative);

        return new AffairExecutionReport(
            AffairId: affair.AffairId,
            Title: affair.Title,
            HandlerNpcId: "emperor",
            HandlerName: "天子御批",
            HubKind: affair.PreferredHub,
            ActualTreasuryCost: affair.BaseTreasuryCost,
            EmbezzledAmount: 0,
            ActualPopularSupportDelta: affair.BasePopularSupportDelta,
            ActualStateLandDelta: affair.BaseStateLandDelta,
            ActualArmyMoraleDelta: affair.BaseArmyMoraleDelta,
            HandlerPowerGained: 0,
            ExecutionNarrative: narrative);
    }

    public BatchDelegationResult ExecuteBatchDelegation(GameState state, IReadOnlyList<string>? affairIds = null)
    {
        var allAffairs = GetPendingAffairs(state);
        var targetAffairs = affairIds == null || affairIds.Count == 0
            ? allAffairs
            : allAffairs.Where(a => affairIds.Contains(a.AffairId)).ToList();

        if (targetAffairs.Count == 0)
        {
            return new BatchDelegationResult(false, 0, Array.Empty<AffairExecutionReport>(), 0, 0, "暂无待办政务可供交办。");
        }

        // 核心力学：一揽子交办只消耗一次固定精力 (默认 2 点，可配置)
        _healthService.ConsumeEnergyForAffairs(state, _config.BatchDelegationEnergyCost, "廷议分权交办群僚");

        var reports = new List<AffairExecutionReport>();
        int totalEmbezzled = 0;
        int totalTreasury = 0;

        foreach (var affair in targetAffairs)
        {
            var (handler, hubName) = ResolveHubHandler(state, affair.PreferredHub);

            // 属性影响与扭曲计算（全部采用可配置参数）
            // 1. 贪腐度截留私吞
            int corruption = handler?.Corruption ?? 0;
            int embezzled = (int)Math.Max(0, Math.Abs(affair.BaseTreasuryCost) * (corruption * _config.CorruptionEmbezzleRatio));
            totalEmbezzled += embezzled;

            if (handler != null && embezzled > 0)
            {
                handler.AdjustStashedWealth(embezzled); // 转入贪官隐匿财产，日后抄家可全额收回
            }

            // 2. 官员能力修正成效
            int ability = handler?.Power ?? 50; // 权势/能力基准
            double efficiency = Math.Clamp(0.5 + (ability / 100.0) * _config.AbilityEfficiencyWeight, 0.5, 2.0);

            // 3. 民心与产出修正
            int popDelta = (int)(affair.BasePopularSupportDelta * efficiency - (corruption * _config.CorruptionPopularityPenaltyRatio));
            int landDelta = (int)(affair.BaseStateLandDelta * efficiency);
            int moraleDelta = (int)(affair.BaseArmyMoraleDelta * efficiency);
            int cost = affair.BaseTreasuryCost + embezzled;
            totalTreasury += cost;

            // 应用国库与国力变动
            ApplyAffairImpact(state, cost, popDelta, landDelta, moraleDelta);

            // 4. 经办人权势提升 (朋党做大)
            if (handler != null)
            {
                handler.AdjustPower(_config.BaseHandlerPowerGain);
                handler.AdjustFavorability(2);
            }

            string handlerName = handler?.Name ?? hubName;
            string narrative = embezzled > 0
                ? $"【群僚分理】{hubName}【{handlerName}】代办【{affair.Title}】。政事告竣，然其中暗中截留侵吞{embezzled}万钱入其私邸！"
                : $"【群僚分理】{hubName}【{handlerName}】恪尽职守经办【{affair.Title}】，公帑修葺，政通人和。";

            state.AddToChronicle(narrative);

            reports.Add(new AffairExecutionReport(
                AffairId: affair.AffairId,
                Title: affair.Title,
                HandlerNpcId: handler?.Id ?? "court_hub",
                HandlerName: handlerName,
                HubKind: affair.PreferredHub,
                ActualTreasuryCost: cost,
                EmbezzledAmount: embezzled,
                ActualPopularSupportDelta: popDelta,
                ActualStateLandDelta: landDelta,
                ActualArmyMoraleDelta: moraleDelta,
                HandlerPowerGained: _config.BaseHandlerPowerGain,
                ExecutionNarrative: narrative));
        }

        string summary = $"【廷议一揽子交办】天子垂拱而治，大笔一挥将{targetAffairs.Count}项政务委派公卿诸曹经办。天子仅耗精力{_config.BatchDelegationEnergyCost}点，群僚受命分理。";
        state.AddToChronicle(summary);

        return new BatchDelegationResult(
            Success: true,
            TotalEnergySpent: _config.BatchDelegationEnergyCost,
            ExecutedReports: reports.AsReadOnly(),
            TotalEmbezzled: totalEmbezzled,
            TotalTreasurySpent: totalTreasury,
            SummaryNarrative: summary);
    }

    private static (NpcState? Handler, string HubName) ResolveHubHandler(GameState state, CourtDelegationHubKind hub)
    {
        return hub switch
        {
            CourtDelegationHubKind.ThreeExcellencies => (
                state.Npcs.Values.FirstOrDefault(n => n.Title == "司徒" || n.Title == "司空" || n.Faction == "清流世族" && n.IsActive),
                "司徒府/公卿诸曹"),
            CourtDelegationHubKind.GrandGeneral => (
                state.Npcs.Values.FirstOrDefault(n => n.Id == "he_jin" || n.Title == "大将军" || n.Faction == "外戚派" && n.IsActive),
                "大将军府"),
            CourtDelegationHubKind.PalaceAttendants => (
                state.Npcs.Values.FirstOrDefault(n => n.Id == "zhang_rang" || n.Faction == "宦官派" && n.IsActive),
                "中常侍/内侍省"),
            _ => (
                state.Npcs.Values.FirstOrDefault(n => n.Id == "cao_cao" || n.Title == "尚书令" && n.IsActive),
                "尚书台/中枢")
        };
    }

    private static void ApplyAffairImpact(GameState state, int cost, int popDelta, int landDelta, int moraleDelta)
    {
        state.Treasury = Math.Max(0, state.Treasury - cost);
        state.PopularSupport = Math.Clamp(state.PopularSupport + popDelta, 0, 100);
        state.WestGardenArmy.AdjustMorale(moraleDelta);

        if (landDelta > 0 && state.Provinces.TryGetValue("yuzhou", out var prov))
        {
            prov.StateControlledLand += landDelta;
        }
    }
}
