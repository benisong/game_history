using System;
using System.Collections.Generic;
using DonghanEngine.Core.Health;

namespace DonghanEngine.Core.Settlement;

/// <summary>
/// 纯领域服务：将每旬各领域计算结果编译为古典起居注卡片幻灯片（单一职责）
/// </summary>
public sealed class TurnSettlementSlideService : ITurnSettlementSlideService
{
    private readonly IImperialHealthService _healthService;

    public TurnSettlementSlideService(IImperialHealthService? healthService = null)
    {
        _healthService = healthService ?? new ImperialHealthService();
    }

    public TurnSettlementPackage CompileTurnSettlementPackage(GameState state, IReadOnlyList<SettlementSlide>? extraSlides = null)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));

        var slides = new List<SettlementSlide>();
        string dateText = $"{state.Year}年{state.Month}月{(state.Xun == 1 ? "上旬" : state.Xun == 2 ? "中旬" : "下旬")}";

        // 1. 宏观天时与起居状态卡
        var diag = _healthService.GetPhysicianDiagnosis(state);
        slides.Add(new SettlementSlide(
            Kind: SettlementSlideKind.General,
            Title: "【太史令起居注 · 旬日更迭】",
            HeaderSubtitle: $"{dateText} ｜ 驻跸：{state.CurrentLocation}",
            BodyText: $"太史令捧册录奏。陛下精神脉象为【{diag.MentalStateDescription}】，皇权维稳于 {state.ImperialPower} 点，太仓存银 {state.Treasury} 万钱，西园内帑 {state.PrivateTreasury} 万钱。",
            SummaryTag: "天时更替",
            BulletPoints: new[]
            {
                $"精神脉象：{diag.MentalStateDescription}",
                $"国库太仓：{state.Treasury} 万钱",
                $"西园内帑：{state.PrivateTreasury} 万钱",
                $"万民归心：{state.PopularSupport} 点"
            }));

        // 2. 禁军与军饷发放卡
        slides.Add(new SettlementSlide(
            Kind: SettlementSlideKind.MilitaryPayroll,
            Title: "【西园校尉部 · 军饷与士气】",
            HeaderSubtitle: $"禁军规模：{state.WestGardenArmy.Size} 人 ｜ 士气：{state.WestGardenArmy.Morale}",
            BodyText: $"本旬西园新军例行发放粮饷，三军士气维持在 {state.WestGardenArmy.Morale} 点。{(state.WestGardenArmy.Morale < 30 ? "士气低落，隐有怨声，需内帑犒赏！" : "军伍整肃，可堪一战。")}",
            SummaryTag: "禁军军势",
            BulletPoints: new[]
            {
                $"在册甲士：{state.WestGardenArmy.Size} 人",
                $"军心士气：{state.WestGardenArmy.Morale} 点"
            }));

        // 3. 若有额外各领域结算卡片（代办成效、特务密报、重大战役）直接并入
        if (extraSlides != null && extraSlides.Count > 0)
        {
            slides.AddRange(extraSlides);
        }

        return new TurnSettlementPackage(
            Year: state.Year,
            Month: state.Month,
            Xun: state.Xun,
            DateText: dateText,
            Slides: slides.AsReadOnly(),
            SnapshotState: state);
    }
}
