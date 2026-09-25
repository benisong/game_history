using System;
using DonghanEngine.Core;
using DonghanEngine.Core.Balance;

namespace DonghanEngine.Core.Economy;

/// <summary>
/// 纯领域服务：国家/世家土地产权、战乱焦土与世家出资赎田系统（单一职责）
/// 支持 IInitializableBalance&lt;LandBalanceConfig&gt; 接口，供超级控制工具动态调参
/// </summary>
public sealed class LandOwnershipService : ILandOwnershipService, IInitializableBalance<LandBalanceConfig>
{
    private LandBalanceConfig _config;

    public LandOwnershipService(LandBalanceConfig? config = null)
    {
        _config = config ?? new LandBalanceConfig();
    }

    public void InitializeConfig(LandBalanceConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public LandBalanceConfig GetConfig() => _config;

    public PostWarLandResolutionResult ResolveWarLandScorching(GameState state, string provinceId, string victorFactionId, bool isImperialDirectArmy)
    {
        if (!state.Provinces.TryGetValue(provinceId, out var province))
        {
            return new PostWarLandResolutionResult(
                ProvinceId: provinceId,
                ProvinceName: "未知州郡",
                NewlyScorchedLand: 0,
                StateLandGained: 0,
                GentryLandLost: 0,
                VictorFactionId: victorFactionId,
                NarrativeTitle: "【土地清算受阻】查无此州郡！",
                ChronicleText: "查无此州郡。");
        }

        // 1. 战乱爆发并平定后，世家庄园与坞堡受损，约 35% 世家土地被打烂沦为焦土无主地
        int gentryLost = (province.GentryControlledLand * _config.WarGentryLossRatioPercent) / 100;
        province.GentryControlledLand = Math.Max(0, province.GentryControlledLand - gentryLost);

        // 2. 焦土化：设立半年（6个月）不产粮倒计时
        province.ScorchedLand += gentryLost;
        province.ScorchedMonthsRemaining = _config.ScorchedMonthsDuration;

        int stateGained = 0;
        string title;
        string chronicle;

        if (isImperialDirectArmy || victorFactionId == "court")
        {
            // 天子亲卫/官军平叛：战后无主地全数归国家/天子所有（进入国家官田/屯田储备池）
            stateGained = gentryLost;
            province.StateControlledLand += stateGained;

            title = $"【王师靖难 · 焦土归公】天子亲卫平定{province.Name}！世家私田{gentryLost}顷收归国家无主官田！";
            chronicle = $"【平乱】天子禁军与官军克复{province.Name}，兵燹之后，世家坞堡遭贼寇践踏，{gentryLost}顷兼并私田沦为无主废墟，划为战乱焦土，休耕{_config.ScorchedMonthsDuration}个月。朝廷顺理成章将无主田产尽数收归国家官田！世家欲求赎回须缴纳巨资。";
        }
        else
        {
            // 军阀混战胜利方占领：战后无主地归胜方诸侯所有
            title = $"【诸侯鏖兵 · 沃野焦土】{victorFactionId}占领{province.Name}！{gentryLost}顷战乱焦土归属胜者！";
            chronicle = $"【兼并】{victorFactionId}击破敌手夺占{province.Name}，兵火连绵，境内{gentryLost}顷田亩化为焦土，休耕{_config.ScorchedMonthsDuration}个月，战后归属{victorFactionId}整编。";
        }

        state.AddToChronicle(chronicle);

        return new PostWarLandResolutionResult(
            ProvinceId: provinceId,
            ProvinceName: province.Name,
            NewlyScorchedLand: gentryLost,
            StateLandGained: stateGained,
            GentryLandLost: gentryLost,
            VictorFactionId: victorFactionId,
            NarrativeTitle: title,
            ChronicleText: chronicle);
    }

    public void AdvanceScorchedLandMonthly(GameState state)
    {
        foreach (var (provinceId, province) in state.Provinces)
        {
            if (province.ScorchedMonthsRemaining > 0)
            {
                province.ScorchedMonthsRemaining--;
                if (province.ScorchedMonthsRemaining == 0 && province.ScorchedLand > 0)
                {
                    // 焦土期满：土地恢复肥力，正式转化为正常耕作土地
                    province.ScorchedLand = 0;
                    state.AddToChronicle($"【时和岁丰】{province.Name}战乱焦土休耕期满（{_config.ScorchedMonthsDuration}个月），荒芜田亩重新复耕，国家有效官田全面恢复产粮！");
                }
            }
        }
    }

    public LandRepurchaseResult RepurchaseStateLandByGentry(GameState state, string provinceId, int purchaseAmount)
    {
        if (!state.Provinces.TryGetValue(provinceId, out var province))
        {
            return new LandRepurchaseResult(
                Success: false,
                ProvinceId: provinceId,
                ProvinceName: "未知州郡",
                LandPurchased: 0,
                GoldPaidToTreasury: 0,
                RemainingStateLand: 0,
                GentryLoyaltyBoost: 0,
                NarrativeTitle: "【赎田受阻】查无此州郡！",
                ChronicleText: "查无此州郡。");
        }

        int maxCanPurchase = Math.Min(province.StateControlledLand, purchaseAmount);
        if (maxCanPurchase <= 0)
        {
            return new LandRepurchaseResult(
                Success: false,
                ProvinceId: provinceId,
                ProvinceName: province.Name,
                LandPurchased: 0,
                GoldPaidToTreasury: 0,
                RemainingStateLand: province.StateControlledLand,
                GentryLoyaltyBoost: 0,
                NarrativeTitle: "【赎田无效】该州无多余国家官田可供赎买！",
                ChronicleText: "无官田可赎。");
        }

        int goldPaid = (maxCanPurchase / 1000) * _config.RepurchaseGoldPerThousandLand;
        goldPaid = Math.Max(_config.RepurchaseMinGold, goldPaid);

        // 1. 土地产权转移：国家官田 -> 世家私田
        province.StateControlledLand -= maxCanPurchase;
        province.GentryControlledLand += maxCanPurchase;

        // 2. 资金进入国库
        state.Treasury = Math.Clamp(state.Treasury + goldPaid, 0, 999999);

        // 3. 世家感恩叩谢，忠诚提升
        int loyaltyBoost = _config.RepurchaseLoyaltyBoost;
        foreach (var (_, npc) in state.Npcs)
        {
            if (npc.IsActive && (npc.Faction == "豪强派" || npc.Faction == "清流派"))
            {
                npc.AdjustFavorability(loyaltyBoost);
            }
        }

        string title = $"【世家纳资 · 赎买官田】{province.Name}世家大族缴纳{goldPaid}万钱，向朝廷赎回祖业田亩{maxCanPurchase}顷！";
        string chronicle = $"【纳金赎田】{province.Name}名门望族具呈尚书台，情愿输纳巨金{goldPaid}万钱入太仓国库，请准赎回战乱无主之官田{maxCanPurchase}顷。天子御笔允准，朝廷得资充裕，士族阖族感恩戴德。";

        state.AddToChronicle(chronicle);

        return new LandRepurchaseResult(
            Success: true,
            ProvinceId: provinceId,
            ProvinceName: province.Name,
            LandPurchased: maxCanPurchase,
            GoldPaidToTreasury: goldPaid,
            RemainingStateLand: province.StateControlledLand,
            GentryLoyaltyBoost: loyaltyBoost,
            NarrativeTitle: title,
            ChronicleText: chronicle);
    }

    public int CollectQuarterlyLandTax(GameState state, bool applyToTreasury = true)
    {
        int totalTax = 0;

        foreach (var (pId, p) in state.Provinces)
        {
            if (p.IsRebelling) continue; // 叛乱州郡无法征税

            // 1. 国家控制土地：编户齐民，全额征税
            int stateLandTax = (p.StateControlledLand / 1000) * _config.TaxPerThousandStateLand;

            // 2. 世家控制土地：隐匿田产，少收 40% (即按 60% 征收)
            int gentryLandTax = (int)((p.GentryControlledLand / 1000) * _config.TaxPerThousandStateLand * _config.GentryTaxDiscountRatio);

            // 焦土土地：休耕不产粮不纳税
            int provinceTax = stateLandTax + gentryLandTax;
            totalTax += provinceTax;
        }

        if (applyToTreasury && totalTax > 0)
        {
            state.Treasury = Math.Clamp(state.Treasury + totalTax, 0, 999999);
            state.AddToChronicle($"【季税入库】大司农清核十三州官民田产，本季征收田赋金{totalTax}万钱入太仓国库（国家官田全额纳赋，世家私田依例少收四成）。");
        }

        return totalTax;
    }
}
