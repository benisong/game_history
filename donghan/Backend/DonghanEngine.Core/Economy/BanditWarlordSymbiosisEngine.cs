using System;
using DonghanEngine.Core;
using DonghanEngine.Core.Balance;

namespace DonghanEngine.Core.Economy;

/// <summary>
/// 纯领域服务：流民转化流寇、反哺地缘军阀收编部曲引擎（单一职责）
/// 支持 IInitializableBalance&lt;BanditBalanceConfig&gt; 接口，供超级控制工具动态调参
/// </summary>
public sealed class BanditWarlordSymbiosisEngine : IBanditWarlordSymbiosisEngine, IInitializableBalance<BanditBalanceConfig>
{
    private BanditBalanceConfig _config;

    public BanditWarlordSymbiosisEngine(BanditBalanceConfig? config = null)
    {
        _config = config ?? new BanditBalanceConfig();
    }

    public void InitializeConfig(BanditBalanceConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public BanditBalanceConfig GetConfig() => _config;

    public BanditSpilloverResult EvaluateBanditSpillover(GameState state, string provinceId, int displacedRefugees)
    {
        if (displacedRefugees <= 0)
        {
            return new BanditSpilloverResult(
                ProvinceId: provinceId,
                BanditScale: 0,
                AbsorbingWarlordId: string.Empty,
                EliteTroopTypeName: "无",
                TroopsAbsorbed: 0,
                WarlordPowerGained: 0,
                NarrativeTitle: "【海内承平】无流民动乱。",
                ChronicleText: "境内晏然。");
        }

        string warlordId;
        string eliteTroopName;
        string provinceName = state.Provinces.TryGetValue(provinceId, out var p) ? p.Name : provinceId;

        switch (provinceId)
        {
            case "qingzhou":
            case "yanzhou":
                warlordId = "cao_cao";
                eliteTroopName = "青州兵";
                break;
            case "youzhou":
            case "jizhou":
                warlordId = "gongsun_zan";
                eliteTroopName = "白马义从";
                break;
            case "yangzhou":
            case "xuzhou":
                warlordId = "sun_ce";
                eliteTroopName = "丹阳兵";
                break;
            case "liangzhou":
                warlordId = "ma_teng";
                eliteTroopName = "西凉铁骑";
                break;
            case "bingzhou":
                warlordId = "zhang_yan"; // 黑山军
                eliteTroopName = "黑山勇士";
                break;
            default:
                warlordId = "local_warlord";
                eliteTroopName = "地方部曲";
                break;
        }

        int absorbedTroops = (displacedRefugees * _config.AbsorbedTroopsPercent) / 100;
        int powerGained = Math.Max(_config.MinPowerGain, absorbedTroops / _config.PowerGainDivisor);

        string warlordDisplayName = warlordId;
        if (state.Npcs.TryGetValue(warlordId, out var warlord))
        {
            warlord.AdjustPower(powerGained);
            warlordDisplayName = warlord.Name;
        }

        string title = $"【流寇反哺 · 诸侯坐大】{provinceName}爆发流民潮！{warlordDisplayName}借机收编【{eliteTroopName}】{absorbedTroops}人！";
        string chronicle = $"【乱象】{provinceName}因土地超载与兼并爆发流民动乱，流民数万沦为贼寇。诸侯【{warlordDisplayName}】以“平贼安民”为由大肆招纳流民精壮，收编为精锐【{eliteTroopName}】共{absorbedTroops}人，势力大振！朝廷地方尾大不掉之势渐成。";

        state.AddToChronicle(chronicle);

        return new BanditSpilloverResult(
            ProvinceId: provinceId,
            BanditScale: displacedRefugees,
            AbsorbingWarlordId: warlordId,
            EliteTroopTypeName: eliteTroopName,
            TroopsAbsorbed: absorbedTroops,
            WarlordPowerGained: powerGained,
            NarrativeTitle: title,
            ChronicleText: chronicle);
    }
}
