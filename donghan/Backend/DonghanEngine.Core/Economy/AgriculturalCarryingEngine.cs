using System;
using DonghanEngine.Core;
using DonghanEngine.Core.Balance;

namespace DonghanEngine.Core.Economy;

/// <summary>
/// 纯领域服务：评估州郡土地人口承载力、世家土地兼并与马尔萨斯危机（单一职责）
/// 支持 IInitializableBalance&lt;AgriculturalCarryingBalanceConfig&gt; 接口，供超级控制工具动态调参
/// </summary>
public sealed class AgriculturalCarryingEngine : IAgriculturalCarryingEngine, IInitializableBalance<IAgriculturalCarryingBalanceProvider>
{
    private IAgriculturalCarryingBalanceProvider _config;

    public AgriculturalCarryingEngine(IAgriculturalCarryingBalanceProvider? config = null)
    {
        _config = config ?? new AgriculturalCarryingBalanceConfig();
    }

    public void InitializeConfig(IAgriculturalCarryingBalanceProvider config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public IAgriculturalCarryingBalanceProvider GetConfig() => _config;

    public ProvinceCarryingReport EvaluateProvince(Province province, int weatherSeverity = 0, double aristocracyLandRatio = 0.40)
    {
        double clampedAristocracy = Math.Clamp(aristocracyLandRatio, 0.0, 0.90);
        int population = province.Population;
        int rawCapacity = province.LandCarryingCapacity;

        // 1. 计算灾害对土地实际承载力的折损系数
        double weatherPenalty = Math.Clamp(weatherSeverity * _config.WeatherSeverityScale, 0.0, _config.MaxWeatherPenalty);
        
        // 若处于战乱焦土期（半年不产粮），有效土地承载直接扣除焦土规模
        int productiveLand = Math.Max(1000, rawCapacity - province.ScorchedLand);
        int effectiveCapacity = (int)(productiveLand * (1.0 - weatherPenalty));
        effectiveCapacity = Math.Max(1000, effectiveCapacity);

        // 2. 粮食产出与需求计算 (石)
        int grainRequired = (population * _config.GrainPerThousandPeople) / 1000;

        // 土地产粮：有效承载力对应标准粮食产量，但世家大族兼并比例越高，截留在坞堡私仓的越多，流向民间的口粮越少
        double effectiveYieldRatio = 1.0 - (clampedAristocracy * _config.AristocracyInterceptFactor);
        int totalGrainYield = (int)((effectiveCapacity * _config.GrainPerThousandPeople / 1000) * effectiveYieldRatio);

        int grainDeficit = Math.Max(0, grainRequired - totalGrainYield);
        double deficitRatio = grainRequired > 0 ? (double)grainDeficit / grainRequired : 0.0;

        // 3. 判定危机级别 (ProvinceCrisisLevel)
        ProvinceCrisisLevel crisisLevel;
        int publicMoraleDelta;
        int displacedRefugees = 0;
        string description;

        if (deficitRatio > _config.CollapseDeficitRatioThreshold || (population > effectiveCapacity * _config.CollapseCapacityRatioThreshold))
        {
            // 马尔萨斯大崩溃：饥荒爆发，流民四起
            crisisLevel = ProvinceCrisisLevel.MalthusianCollapse;
            publicMoraleDelta = _config.CollapseMoralePenalty;
            displacedRefugees = (int)(population * Math.Min(0.35, deficitRatio * 0.6));
            description = $"【土地超载 · 饥馑暴乱】{province.Name}人口({population})严重超过土地承载({effectiveCapacity})！世家兼并({clampedAristocracy:P0})，粮食缺口达{grainDeficit}石，流民四起！";
        }
        else if (deficitRatio > _config.SevereShortageDeficitThreshold || population > effectiveCapacity)
        {
            // 严重短缺
            crisisLevel = ProvinceCrisisLevel.SevereShortage;
            publicMoraleDelta = _config.SevereShortageMoralePenalty;
            displacedRefugees = (int)(population * 0.10);
            description = $"【粮饷匮乏 · 生民艰难】{province.Name}土地承载吃紧，粮食缺额{grainDeficit}石，民生不安。";
        }
        else if (deficitRatio > 0.0 || clampedAristocracy > _config.MildPressureAristocracyThreshold)
        {
            // 轻微承载压力
            crisisLevel = ProvinceCrisisLevel.MildPressure;
            publicMoraleDelta = _config.MildPressureMoralePenalty;
            description = $"【兼并隐忧】{province.Name}产粮尚能自给，但豪强兼并渐重，隐匿田产。";
        }
        else
        {
            // 充裕稳定
            crisisLevel = ProvinceCrisisLevel.StableAndAbundant;
            publicMoraleDelta = _config.StableAbundantMoraleBoost;
            description = $"【沃野千里 · 仓廪充实】{province.Name}水土丰茂，土地承载充裕，百姓安居乐业。";
        }

        return new ProvinceCarryingReport(
            ProvinceId: province.Id,
            ProvinceName: province.Name,
            Population: population,
            LandCarryingCapacity: effectiveCapacity,
            AristocracyLandRatio: clampedAristocracy,
            GrainYield: totalGrainYield,
            GrainRequired: grainRequired,
            GrainDeficit: grainDeficit,
            CrisisLevel: crisisLevel,
            PublicMoraleDelta: publicMoraleDelta,
            DisplacedRefugees: displacedRefugees,
            Description: description);
    }
}
