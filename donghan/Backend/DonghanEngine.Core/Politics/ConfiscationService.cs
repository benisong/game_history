using System;
using System.Collections.Generic;
using DonghanEngine.Core;

namespace DonghanEngine.Core.Politics;

/// <summary>
/// 纯领域服务：执行抄家断案与关联关系网络惩罚（单一职责）
/// </summary>
public sealed class ConfiscationService : IConfiscationService
{
    public ConfiscationExecutionResult ConfiscateTarget(GameState state, string targetNpcId)
    {
        if (string.IsNullOrWhiteSpace(targetNpcId) || !state.Npcs.TryGetValue(targetNpcId, out var targetNpc))
        {
            return new ConfiscationExecutionResult(
                Success: false,
                TargetNpcId: targetNpcId,
                TargetName: "未知官员",
                GoldSeized: 0,
                GrainSeized: 0,
                ImperialPowerDelta: 0,
                PublicMoraleDelta: 0,
                AffiliatedLoyaltyPenalties: new Dictionary<string, int>(),
                NarrativeTitle: "【查抄受阻】查无此官！",
                ChronicleText: "有司按籍索隐，未获其人。");
        }

        // 1. 没收隐匿财产（根据贪腐度、隐匿财富与权势计算）
        int baseStash = Math.Max(targetNpc.StashedWealth, 500);
        int corruptionBonus = targetNpc.Corruption * 80;
        int powerBonus = targetNpc.Power * 40;
        int goldSeized = baseStash + corruptionBonus + powerBonus;
        int grainSeized = (targetNpc.Corruption * 150) + 1000;

        // 执行国库注入与目标注销/收监
        state.Treasury = Math.Clamp(state.Treasury + goldSeized, 0, 999999);
        targetNpc.StashedWealth = 0;
        targetNpc.Power = 0;
        targetNpc.Favorability = 0;
        targetNpc.IsActive = false;
        targetNpc.DeathReason = $"【籍没查抄】因巨贪弄权被天子下明诏籍没家产，收押廷尉。";

        // 2. 皇权立威与民心提振
        int imperialPowerDelta = 8; // 严明纲纪立威
        int publicMoraleDelta = 12; // 惩贪除暴百姓称快
        state.ImperialPower = Math.Clamp(state.ImperialPower + imperialPowerDelta, 0, 100);

        // 3. 关联派系与关系网络惩罚 (Affiliated Loyalty Penalty)
        var penalties = new Dictionary<string, int>();
        string targetFaction = targetNpc.Faction;

        foreach (var (npcId, npc) in state.Npcs)
        {
            if (npcId == targetNpcId || !npc.IsActive) continue;

            // 同派系/同宗门阀官员：忠诚暴跌 (人人自危)
            if (!string.IsNullOrEmpty(targetFaction) && npc.Faction == targetFaction)
            {
                int penalty = -25;
                npc.AdjustFavorability(penalty);
                penalties[npcId] = penalty;
            }
            // 其他中立或异党官员：忠诚微降 (伴君如伴虎之戒备)
            else
            {
                int penalty = -5;
                npc.AdjustFavorability(penalty);
                penalties[npcId] = penalty;
            }
        }

        string narrativeTitle = $"【籍没巨贪 · 抄家立威】查抄{targetNpc.Name}家产！获金{goldSeized}贯！";
        string chronicleText = $"【籍没】天子赫然震怒，命御史中丞与廷尉缇骑围抄{targetNpc.Name}府邸，起获私藏金帛{goldSeized}贯、良田大庇，尽数籍没入太仓！百姓聚观街巷，无不称颂天子圣明！然朝中{targetFaction}党羽人人自危，深怀戒惧。";

        state.AddToChronicle(chronicleText);

        return new ConfiscationExecutionResult(
            Success: true,
            TargetNpcId: targetNpcId,
            TargetName: targetNpc.Name,
            GoldSeized: goldSeized,
            GrainSeized: grainSeized,
            ImperialPowerDelta: imperialPowerDelta,
            PublicMoraleDelta: publicMoraleDelta,
            AffiliatedLoyaltyPenalties: penalties,
            NarrativeTitle: narrativeTitle,
            ChronicleText: chronicleText);
    }
}
