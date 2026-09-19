using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 189 年 4 月灵帝大病与宫廷鸩毒弑君因果（单一职责）
/// </summary>
public sealed class EmperorSuccessionCrisisEvaluator : IEmperorSuccessionCrisisEvaluator
{
    public EmperorSuccessionCrisisResult Evaluate(GameState state)
    {
        bool heJinPlotting = state.Npcs.TryGetValue("he_jin", out var hj) &&
                             hj.IsActive && !hj.IsHostile &&
                             hj.Favorability < 25 &&
                             hj.Ambition >= 60 &&
                             hj.Power >= 70;

        bool zhangRangPlotting = state.Npcs.TryGetValue("zhang_rang", out var zr) &&
                                 zr.IsActive && !zr.IsHostile &&
                                 zr.Favorability < 25 &&
                                 zr.Ambition >= 70 &&
                                 zr.Power >= 50;

        // 1. 何进鸩杀路线
        if (heJinPlotting)
        {
            if (state.Health <= 35)
            {
                return new EmperorSuccessionCrisisResult(
                    EmperorCrisisCause.HeJinAssassination,
                    EmperorSurvivalOutcome.DiedFromPoisonOrIllness,
                    "【惊天弑君】大将军何进指使内侍进奉鸩汤，灵帝暴崩！",
                    "【大变】大将军何进忌惮天子削权，密令御医暗下鸩毒，灵帝龙体衰竭暴崩于嘉德殿！何进何后遂立少子辩，总揽朝政！",
                    HealthDelta: -100,
                    ImperialPowerDelta: -50,
                    CulpritPowerDelta: 20,
                    CulpritNpcId: "he_jin",
                    CulpritExposed: false);
            }
            else
            {
                return new EmperorSuccessionCrisisResult(
                    EmperorCrisisCause.HeJinAssassination,
                    EmperorSurvivalOutcome.SurvivedAndDiscovered,
                    "【鸩毒事泄】天子挺过鸩毒！西园亲军查实何进弑君逆谋！",
                    "【密谋】大将军何进指使御医暗投剧毒，天子龙体硬挺不堕，西园上军校尉蹇硕捕斩行凶御医，何进弑君逆谋大白于天下！",
                    HealthDelta: -25,
                    ImperialPowerDelta: 25,
                    CulpritPowerDelta: -30,
                    CulpritNpcId: "he_jin",
                    CulpritExposed: true);
            }
        }

        // 2. 十常侍张让鸩杀路线
        if (zhangRangPlotting)
        {
            if (state.Health <= 35)
            {
                return new EmperorSuccessionCrisisResult(
                    EmperorCrisisCause.EunuchAssassination,
                    EmperorSurvivalOutcome.DiedFromPoisonOrIllness,
                    "【深宫逆伦】十常侍张让等恐被诛戮，铤而走险鸩杀天子！",
                    "【弑逆】十常侍张让见失宠势蹙，恐遭籍没清算，暗投奇毒于御药，灵帝暴毙于南宫，中官诈称天子托孤！",
                    HealthDelta: -100,
                    ImperialPowerDelta: -50,
                    CulpritPowerDelta: 20,
                    CulpritNpcId: "zhang_rang",
                    CulpritExposed: false);
            }
            else
            {
                return new EmperorSuccessionCrisisResult(
                    EmperorCrisisCause.EunuchAssassination,
                    EmperorSurvivalOutcome.SurvivedAndDiscovered,
                    "【中官弑君事发】天子大病初愈，震怒彻查十常侍鸩药大案！",
                    "【逆乱】张让等暗下鸩毒意图弑君立幼，天子龙体转安，亲御六军围搜内省，中官弑君罪证确凿，天下震骇！",
                    HealthDelta: -25,
                    ImperialPowerDelta: 30,
                    CulpritPowerDelta: -40,
                    CulpritNpcId: "zhang_rang",
                    CulpritExposed: true);
            }
        }

        // 3. 纯生理大病路线（好感正常，但天子本身 Health 极低）
        if (state.Health <= 20)
        {
            return new EmperorSuccessionCrisisResult(
                EmperorCrisisCause.NaturalIllness,
                EmperorSurvivalOutcome.DiedFromPoisonOrIllness,
                "【龙驭宾天】灵帝大病不治，驾崩于南宫嘉德殿！",
                "【崩殂】灵帝沉疴不起，年三十三崩于南宫嘉德殿。大将军何进、十常侍各怀异心，天下危疑！",
                HealthDelta: -100,
                ImperialPowerDelta: -40,
                CulpritPowerDelta: 0,
                CulpritNpcId: string.Empty,
                CulpritExposed: false);
        }

        // 4. 忠诚稳固、龙体安康（平安度过，延寿亲政！）
        return new EmperorSuccessionCrisisResult(
            EmperorCrisisCause.SafeAndHealthy,
            EmperorSurvivalOutcome.PeacefulRecovery,
            "【龙体康泰 · 天命延年】灵帝安然跨过三十三岁关口，临朝亲政！",
            "【中兴】灵帝起居如常，内廷肃然，宿卫严整，外戚中官莫敢生觊觎之心，天子亲御紫宸殿，开启延年亲政大典！",
            HealthDelta: 10,
            ImperialPowerDelta: 15,
            CulpritPowerDelta: 0,
            CulpritNpcId: string.Empty,
            CulpritExposed: false);
    }
}
