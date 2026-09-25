using System;
using System.Collections.Generic;
using DonghanEngine.Core;
using DonghanEngine.Core.Balance;

namespace DonghanEngine.Core.Politics;

/// <summary>
/// 纯领域服务：刺史/州牧年终大考课与内调升迁博弈（单一职责）
/// 支持 IInitializableBalance&lt;GovernorAppraisalBalanceConfig&gt; 接口，供超级控制工具动态调参
/// </summary>
public sealed class GovernorAppraisalService : IGovernorAppraisalService, IInitializableBalance<IGovernorAppraisalBalanceProvider>
{
    private IGovernorAppraisalBalanceProvider _config;

    public GovernorAppraisalService(IGovernorAppraisalBalanceProvider? config = null)
    {
        _config = config ?? new GovernorAppraisalBalanceConfig();
    }

    public void InitializeConfig(IGovernorAppraisalBalanceProvider config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public IGovernorAppraisalBalanceProvider GetConfig() => _config;

    public AnnualAppraisalReport EvaluateAnnualAppraisal(GameState state)
    {
        var records = new List<GovernorAppraisalRecord>();
        int totalAnnualTax = 0;

        foreach (var (pId, prov) in state.Provinces)
        {
            if (string.IsNullOrEmpty(prov.GovernorId))
                continue;

            if (!state.Npcs.TryGetValue(prov.GovernorId, out var governor) || !governor.IsActive)
                continue;

            // 1. 评估考课三要素：民心治安 (LocalSupport)、纳税贡献、户口增殖
            int taxContribution = (prov.StateControlledLand / 1000) * 10 + (int)((prov.GentryControlledLand / 1000) * 6);
            if (prov.IsRebelling) taxContribution = 0;
            totalAnnualTax += taxContribution;

            int popGrowth = prov.Population > prov.LandCarryingCapacity ? -5000 : 3000;
            int support = prov.LocalSupport;

            // 2. 评定考课等级
            AppraisalGrade grade;
            string recommendedAction;
            string evalReport;

            if (prov.IsRebelling || support < _config.InferiorSupportThreshold || (governor.Ambition >= _config.InferiorAmbitionThreshold && governor.Favorability < _config.InferiorFavorabilityThreshold))
            {
                grade = AppraisalGrade.Inferior;
                recommendedAction = "宜下诏申饬，削减部曲，甚至遣御史收捕";
                evalReport = $"【下考】{prov.Name}治下生乱，民心凋敝（{support}点）。刺史野心高企（{governor.Ambition}点），隐匿田赋，有跋扈不臣之势！";
            }
            else if (support >= _config.SuperiorSupportThreshold && taxContribution >= _config.SuperiorTaxContributionThreshold && governor.Favorability >= _config.SuperiorFavorabilityThreshold)
            {
                grade = AppraisalGrade.Superior;
                recommendedAction = "政绩卓绝，宜召拜九卿/执金吾内调中枢辅政";
                evalReport = $"【上考】{prov.Name}治绩昭著，民殷国富（治安{support}点），岁输租赋{taxContribution}万钱，可为天下长吏表率！";
            }
            else
            {
                grade = AppraisalGrade.Standard;
                recommendedAction = "勉修职守，宜赐玺书褒勉并增其禄米";
                evalReport = $"【中考】{prov.Name}治理平平，考课平妥，差强人意。";
            }

            records.Add(new GovernorAppraisalRecord(
                ProvinceId: pId,
                ProvinceName: prov.Name,
                GovernorId: governor.Id,
                GovernorName: governor.Name,
                Grade: grade,
                LocalSupport: support,
                TaxContribution: taxContribution,
                PopulationGrowth: popGrowth,
                Ambition: governor.Ambition,
                Favorability: governor.Favorability,
                RecommendedAction: recommendedAction,
                EvaluationReport: evalReport));
        }

        string summary = $"【尚书台岁终考课】司徒府与尚书台大考十三州刺史太守。全岁共收各州上计租赋{totalAnnualTax}万钱，上考诸州政通人和，下考封疆隐怀异志。";
        state.AddToChronicle(summary);

        return new AnnualAppraisalReport(state.Year, records.AsReadOnly(), totalAnnualTax, summary);
    }

    public GovernorPromotionResolutionResult PromoteGovernorToCourt(GameState state, string governorId, string targetCourtTitle)
    {
        if (!state.Npcs.TryGetValue(governorId, out var governor) || !governor.IsActive)
        {
            return new GovernorPromotionResolutionResult(
                Success: false,
                GovernorId: governorId,
                GovernorName: "未知官员",
                ProvinceId: "",
                AcceptedRecall: false,
                TargetCourtTitle: targetCourtTitle,
                ImperialPowerDelta: 0,
                GentryLoyaltyDelta: 0,
                NarrativeTitle: "【内调受阻】查无此长吏！",
                ChronicleText: "查无此官员。",
                ErrorCode: "GovernorNotFound");
        }

        string provinceId = governor.GovernedProvinceId ?? "";
        string provinceName = state.Provinces.TryGetValue(provinceId, out var prov) ? prov.Name : "外郡";

        // 核心博弈力学：刺史是否遵旨奉召内调？
        bool willAcceptRecall = (state.ImperialPower >= _config.PromotionImperialPowerThreshold || governor.Favorability >= _config.PromotionFavorabilityThreshold) && governor.Ambition < _config.PromotionMaxAmbitionThreshold;

        if (willAcceptRecall)
        {
            // 1. 顺服内调：卸任太守，入朝任高官
            governor.GovernedProvinceId = null;
            if (state.Provinces.TryGetValue(provinceId, out var p))
            {
                p.GovernorId = null;
            }

            governor.Title = targetCourtTitle;
            governor.InitialLocation = "洛阳朝堂";
            governor.AdjustFavorability(_config.PromotionFavorabilityGain);
            governor.AdjustPower(_config.PromotionPowerGain);

            state.ImperialPower = Math.Clamp(state.ImperialPower + _config.PromotionImperialPowerGain, 0, 100);

            string title = $"【征拜九卿 · 顺服还朝】天子征拜【{governor.Name}】为【{targetCourtTitle}】！{provinceName}军政大权顺利收归中央！";
            string chronicle = $"【还朝】天子降明诏征拜{provinceName}太守【{governor.Name}】入朝担任【{targetCourtTitle}】。{governor.Name}恪守纯臣之节，奉诏交卸州印还京入阁，{provinceName}平稳重归朝廷直辖，皇权+{_config.PromotionImperialPowerGain}！";

            state.AddToChronicle(chronicle);

            return new GovernorPromotionResolutionResult(
                Success: true,
                GovernorId: governor.Id,
                GovernorName: governor.Name,
                ProvinceId: provinceId,
                AcceptedRecall: true,
                TargetCourtTitle: targetCourtTitle,
                ImperialPowerDelta: _config.PromotionImperialPowerGain,
                GentryLoyaltyDelta: _config.PromotionGentryLoyaltyGain,
                NarrativeTitle: title,
                ChronicleText: chronicle);
        }
        else
        {
            // 2. 封疆抗命
            governor.AdjustFavorability(_config.RefusalFavorabilityPenalty);
            governor.AdjustPower(_config.RefusalPowerGain);
            state.ImperialPower = Math.Clamp(state.ImperialPower + _config.RefusalImperialPowerPenalty, 0, 100);

            string title = $"【抗旨推诿 · 拥兵自重】{provinceName}太守【{governor.Name}】抗拒内调！上表称病留任！";
            string chronicle = $"【抗命】天子欲征拜{provinceName}长吏【{governor.Name}】为【{targetCourtTitle}】内调回京。{governor.Name}自恃山高皇帝远、拥兵自重，托辞“边陲未靖、抱病难行”上表谢绝入朝，强行留据{provinceName}！朝野侧目，皇权{_config.RefusalImperialPowerPenalty}！";

            state.AddToChronicle(chronicle);

            return new GovernorPromotionResolutionResult(
                Success: false,
                GovernorId: governor.Id,
                GovernorName: governor.Name,
                ProvinceId: provinceId,
                AcceptedRecall: false,
                TargetCourtTitle: targetCourtTitle,
                ImperialPowerDelta: _config.RefusalImperialPowerPenalty,
                GentryLoyaltyDelta: _config.RefusalGentryLoyaltyPenalty,
                NarrativeTitle: title,
                ChronicleText: chronicle,
                ErrorCode: "GovernorRefusedRecall");
        }
    }
}
