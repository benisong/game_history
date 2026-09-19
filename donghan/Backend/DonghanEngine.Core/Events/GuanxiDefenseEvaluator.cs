using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 190 年 6 月关西军团整饬与董卓防务对峙因果（单一职责）
/// </summary>
public sealed class GuanxiDefenseEvaluator : IGuanxiDefenseEvaluator
{
    public GuanxiDefenseResult Evaluate(GameState state)
    {
        bool huangfuLoyalAndStrong = state.Npcs.TryGetValue("huangfu_song", out var hfs) &&
                                     hfs.IsActive && hfs.Favorability >= 60 && hfs.Power >= 60;
        bool hasTreasuryToAppease = state.Treasury >= 1000;

        // 1. 皇甫嵩坐镇关西且忠诚强盛：三辅金汤，西凉铁骑不敢妄动
        if (huangfuLoyalAndStrong)
        {
            return new GuanxiDefenseResult(
                GuanxiDefenseOutcome.FirmBorderDeterrence,
                "【关西锁钥 · 威镇西陲】皇甫嵩严整三辅防务，董卓大军受阻于郿坞！",
                "【边防】初平元年六月，左将军皇甫嵩奉天子密诏严饬右扶风、函谷关防务，结营三辅。董卓虽握凉州重兵，惮于皇甫威名与朝廷正统，退保郿坞不敢轻动！",
                ImperialPowerDelta: 10,
                HuangfuSongPowerDelta: 10,
                HuangfuSongLoyaltyDelta: 15,
                DongZhuoPowerDelta: -10,
                DongZhuoFavorDelta: -5,
                TreasuryCost: 500,
                PopularSupportDelta: 5);
        }

        // 2. 皇甫嵩势弱但国库充裕：朝廷以虚衔金帛羁縻安抚董卓
        if (hasTreasuryToAppease)
        {
            return new GuanxiDefenseResult(
                GuanxiDefenseOutcome.BribeAndAppease,
                "【金帛羁縻 · 虚衔定边】天子加封董卓为前将军，厚赐西凉将士！",
                "【绥靖】朝廷降旨封董卓为前将军、兼领并州牧，颁赐金帛万匹以犒西凉士卒。董卓奉诏受封，暂罢西进之念，关西局势稍安。",
                ImperialPowerDelta: 0,
                HuangfuSongPowerDelta: 0,
                HuangfuSongLoyaltyDelta: 0,
                DongZhuoPowerDelta: 10,
                DongZhuoFavorDelta: 20,
                TreasuryCost: 1000,
                PopularSupportDelta: 0);
        }

        // 3. 边防废弛且府库空虚：董卓试探性劫掠三辅，关西告急
        return new GuanxiDefenseResult(
            GuanxiDefenseOutcome.BorderBorderSkirmish,
            "【西凉犯境 · 三辅告急】董卓部曲抄掠关西诸县，天子震怒！",
            "【兵警】关西守备虚竭，董卓纵容部曲以就食为名抄掠三辅，百姓流离，天子降诏严切戒备！",
            ImperialPowerDelta: -10,
            HuangfuSongPowerDelta: -5,
            HuangfuSongLoyaltyDelta: -10,
            DongZhuoPowerDelta: 15,
            DongZhuoFavorDelta: -20,
            TreasuryCost: 0,
            PopularSupportDelta: -10);
    }
}
