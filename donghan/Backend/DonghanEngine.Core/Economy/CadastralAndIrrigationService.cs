using System;
using DonghanEngine.Core;
using DonghanEngine.Core.Balance;

namespace DonghanEngine.Core.Economy;

/// <summary>
/// 纯领域服务：度田令丈量土地、清查隐匿田产与水利官修工程（单一职责）
/// 支持 IInitializableBalance&lt;CadastralAndIrrigationBalanceConfig&gt; 接口，供超级控制工具动态调参
/// </summary>
public sealed class CadastralAndIrrigationService : ICadastralSurveyService, IIrrigationService, IInitializableBalance<CadastralAndIrrigationBalanceConfig>
{
    private CadastralAndIrrigationBalanceConfig _config;

    public CadastralAndIrrigationService(CadastralAndIrrigationBalanceConfig? config = null)
    {
        _config = config ?? new CadastralAndIrrigationBalanceConfig();
    }

    public void InitializeConfig(CadastralAndIrrigationBalanceConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public CadastralAndIrrigationBalanceConfig GetConfig() => _config;

    public CadastralSurveyResult ExecuteCadastralSurvey(GameState state, string provinceId, CadastralSurveyIntensity intensity)
    {
        if (!state.Provinces.TryGetValue(provinceId, out var province))
        {
            return new CadastralSurveyResult(
                Success: false,
                ProvinceId: provinceId,
                ProvinceName: "未知州郡",
                Intensity: intensity,
                LandRatioReduction: 0,
                ReclaimedArableLand: 0,
                TaxGoldReclaimed: 0,
                AristocratLoyaltyPenalty: 0,
                PopularMoraleBoost: 0,
                NarrativeTitle: "【度田受阻】查无此州郡！",
                ChronicleText: "查无此州郡。");
        }

        double reduction;
        int capacityBoost;
        int taxReclaimed;
        int loyaltyPenalty;
        int moraleBoost;
        string intensityName;

        switch (intensity)
        {
            case CadastralSurveyIntensity.Mild:
                intensityName = "宽和核验";
                reduction = _config.MildReduction;
                capacityBoost = _config.MildCapacityBoost;
                taxReclaimed = _config.MildTaxReclaimed;
                loyaltyPenalty = _config.MildLoyaltyPenalty;
                moraleBoost = _config.MildMoraleBoost;
                break;
            case CadastralSurveyIntensity.Standard:
                intensityName = "严明度田";
                reduction = _config.StandardReduction;
                capacityBoost = _config.StandardCapacityBoost;
                taxReclaimed = _config.StandardTaxReclaimed;
                loyaltyPenalty = _config.StandardLoyaltyPenalty;
                moraleBoost = _config.StandardMoraleBoost;
                break;
            case CadastralSurveyIntensity.Thorough:
            default:
                intensityName = "铁腕丈量";
                reduction = _config.ThoroughReduction;
                capacityBoost = _config.ThoroughCapacityBoost;
                taxReclaimed = _config.ThoroughTaxReclaimed;
                loyaltyPenalty = _config.ThoroughLoyaltyPenalty;
                moraleBoost = _config.ThoroughMoraleBoost;
                break;
        }

        // 1. 提升土地承载力上限
        province.LandCarryingCapacity += capacityBoost;

        // 2. 补缴税金充入国库
        state.Treasury = Math.Clamp(state.Treasury + taxReclaimed, 0, 999999);

        // 3. 提振天下底层自耕农与民心
        state.PopularSupport = Math.Clamp(state.PopularSupport + moraleBoost, 0, 100);

        // 4. 世家豪族官员忠诚受损
        foreach (var (_, npc) in state.Npcs)
        {
            if (npc.IsActive && (npc.Faction == "豪强派" || npc.Faction == "清流派"))
            {
                npc.AdjustFavorability(loyaltyPenalty);
            }
        }

        string title = $"【朝议度田 · 丈量土地】天子下诏于{province.Name}推行【{intensityName}】！收归编户齐民，清缴隐田！";
        string chronicle = $"【度田】天子明诏使御史巡按{province.Name}，以【{intensityName}】清丈田亩。查出豪强隐匿垦田数万顷，有效土地承载提升{capacityBoost}，追缴隐税{taxReclaimed}万贯入太仓！升斗小民称颂仁德，然世家豪族心怀怨怼。";

        state.AddToChronicle(chronicle);

        return new CadastralSurveyResult(
            Success: true,
            ProvinceId: provinceId,
            ProvinceName: province.Name,
            Intensity: intensity,
            LandRatioReduction: reduction,
            ReclaimedArableLand: capacityBoost,
            TaxGoldReclaimed: taxReclaimed,
            AristocratLoyaltyPenalty: loyaltyPenalty,
            PopularMoraleBoost: moraleBoost,
            NarrativeTitle: title,
            ChronicleText: chronicle);
    }

    public IrrigationProjectResult ConstructIrrigation(GameState state, string provinceId)
    {
        if (!state.Provinces.TryGetValue(provinceId, out var province))
        {
            return new IrrigationProjectResult(
                Success: false,
                ProvinceId: provinceId,
                ProvinceName: "未知州郡",
                ProjectName: "无",
                GoldCost: 0,
                CapacityIncrease: 0,
                PopularMoraleBoost: 0,
                NarrativeTitle: "【水利受阻】查无此州郡！",
                ChronicleText: "查无此州郡。");
        }

        int goldCost = _config.IrrigationGoldCost;
        if (state.Treasury < goldCost)
        {
            return new IrrigationProjectResult(
                Success: false,
                ProvinceId: provinceId,
                ProvinceName: province.Name,
                ProjectName: "水利大兴",
                GoldCost: goldCost,
                CapacityIncrease: 0,
                PopularMoraleBoost: 0,
                NarrativeTitle: $"【国库匮乏】修筑水利需国库金{goldCost}万，现钱不足！",
                ChronicleText: "国库资金不足，水利工程搁浅。");
        }

        string projectName = provinceId switch
        {
            "sili" => "引洛灌溉工程",
            "yuzhou" => "修浚白渠水网",
            "jizhou" => "漳水十二渠岁修",
            "yizhou" => "都江堰拓浚工程",
            "yangzhou" => "芍陂蓄水大兴",
            "qingzhou" => "淄水排涝灌溉渠",
            _ => "官修大型水利农田水网"
        };

        // 1. 扣减国库
        state.Treasury -= goldCost;

        // 2. 永久增加土地承载力上限
        int capacityIncrease = _config.IrrigationCapacityIncrease;
        province.LandCarryingCapacity += capacityIncrease;

        // 3. 提振民心
        int moraleBoost = _config.IrrigationMoraleBoost;
        state.PopularSupport = Math.Clamp(state.PopularSupport + moraleBoost, 0, 100);

        string title = $"【大兴水利 · 沃野千里】天子发太仓金千百，于{province.Name}兴筑【{projectName}】！";
        string chronicle = $"【水利】天子下诏发太仓帑银{goldCost}万钱，命水衡都尉督领工役，于{province.Name}大兴【{projectName}】。引水灌田，旱涝保收，土地人口承载力永久提升{capacityIncrease}！四方百姓颂圣主隆恩。";

        state.AddToChronicle(chronicle);

        return new IrrigationProjectResult(
            Success: true,
            ProvinceId: provinceId,
            ProvinceName: province.Name,
            ProjectName: projectName,
            GoldCost: goldCost,
            CapacityIncrease: capacityIncrease,
            PopularMoraleBoost: moraleBoost,
            NarrativeTitle: title,
            ChronicleText: chronicle);
    }
}
