using System;

namespace DonghanEngine.Core.Events;

/// <summary>
/// 纯领域评估器：评估 184 年 10 月广宗曲阳大捷与主将冤狱因果（单一职责）
/// </summary>
public sealed class PacificationBattleEvaluator : IPacificationBattleEvaluator
{
    public PacificationBattleResult Evaluate(GameState state)
    {
        bool luZhiAvailable = state.Npcs.TryGetValue("lu_zhi", out var luZhi) && luZhi.IsActive && !luZhi.IsHostile;
        bool huangfuAvailable = state.Npcs.TryGetValue("huangfu_song", out var huangfu) && huangfu.IsActive && !huangfu.IsHostile;
        bool zhangRangPowerful = state.Npcs.TryGetValue("zhang_rang", out var zhangRang) && zhangRang.IsActive && zhangRang.Power >= 40;

        // 1. 若卢植在朝/领军：
        if (luZhiAvailable && luZhi != null)
        {
            // 若灵帝对卢植好感高 (>=60) 或 阉党张让权势已被打压 (<40) -> 卢植未受中官构陷，广宗直破张角！
            if (luZhi.Favorability >= 60 || !zhangRangPowerful)
            {
                return new PacificationBattleResult(
                    PacificationOutcome.LuZhiTriumph,
                    "lu_zhi",
                    "【广宗大捷】北中郎将卢植围广宗，连破贼垒！斩张角首级！",
                    "【大捷】北中郎将卢植用兵如神，筑垒围广宗，大破黄巾主力，张角病死贼众皆降！天子诏加卢植尚书令！",
                    ImperialPowerDelta: 10,
                    PopularSupportDelta: 15,
                    JizhouPacified: true,
                    LuZhiImprisoned: false);
            }
            else
            {
                // 史实路线：小黄门左丰索贿不成，中官进谗，卢植被槛车征还下狱，皇甫嵩接任
                return new PacificationBattleResult(
                    PacificationOutcome.LuZhiFramed,
                    "huangfu_song",
                    "【广宗曲阳捷报】卢植蒙冤征还，左中郎将皇甫嵩接任大破曲阳！",
                    "【战报】小黄门左丰巡军索贿，卢植拒之被构陷下狱。天子改命皇甫嵩接帅印，夜袭曲阳斩张梁张宝，斩首十万级，积尸为京观！",
                    ImperialPowerDelta: 5,
                    PopularSupportDelta: 10,
                    JizhouPacified: true,
                    LuZhiImprisoned: true);
            }
        }

        // 2. 卢植不在/阵亡，皇甫嵩直接挂帅
        if (huangfuAvailable)
        {
            return new PacificationBattleResult(
                PacificationOutcome.HuangfuSongTriumph,
                "huangfu_song",
                "【曲阳大捷】左中郎将皇甫嵩斩张梁张宝，平定河北！",
                "【平乱】皇甫嵩率汉军主力连破曲阳、广宗，积尸为京观，河北黄巾主力彻底瓦解！天子拜皇甫嵩为左车骑将军。",
                ImperialPowerDelta: 8,
                PopularSupportDelta: 12,
                JizhouPacified: true,
                LuZhiImprisoned: false);
        }

        // 3. 汉军无名将可用
        return new PacificationBattleResult(
            PacificationOutcome.RebelsPersist,
            string.Empty,
            "【河北军危】朝廷无大将可用，冀州黄巾贼势未息！",
            "【危局】朝中缺统军名将，广宗久攻不下，张角余部据城固守，河北战事陷入胶着！",
            ImperialPowerDelta: -5,
            PopularSupportDelta: -5,
            JizhouPacified: false,
            LuZhiImprisoned: false);
    }
}
